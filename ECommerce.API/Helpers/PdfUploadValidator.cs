using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ECommerce.API.Helpers
{
    /// <summary>
    /// Checks an uploaded manuscript before it is stored: real PDF structure, no active content,
    /// then an antivirus scan (Windows Defender) when it is available on the host.
    /// </summary>
    public static class PdfUploadValidator
    {
        public const long MaxBytes = 5 * 1024 * 1024;

        // PDF name objects that run code, launch programs, hide payloads or submit data.
        private static readonly HashSet<string> Forbidden = new(StringComparer.Ordinal)
        {
            "JavaScript", "JS", "Launch", "EmbeddedFile", "EmbeddedFiles", "RichMedia", "XFA",
            "SubmitForm", "ImportData", "GoToE", "Encrypt"
        };

        private static readonly Regex NameToken = new(@"/([^\s/<>\[\](){}%]+)", RegexOptions.Compiled);
        private static readonly Regex HexEscape = new(@"#([0-9A-Fa-f]{2})", RegexOptions.Compiled);

        /// <summary>Returns null when the file is safe, otherwise a reason (English, Arabic).</summary>
        public static async Task<(string En, string Ar)?> ValidateAsync(IFormFile file, string privateFolder, ILogger logger, CancellationToken ct)
        {
            if (file.Length <= 0) return ("The PDF file is empty.", "ملف PDF فارغ.");
            if (file.Length > MaxBytes) return ("PDF file must be 5 MB or less.", "حجم ملف PDF يجب ألا يتجاوز 5 ميجابايت.");

            var ext = Path.GetExtension(file.FileName ?? "")?.ToLowerInvariant();
            if (ext != ".pdf") return ("Only PDF files are allowed.", "الملفات المسموحة PDF فقط.");

            var ct0 = file.ContentType?.ToLowerInvariant() ?? "";
            if (ct0 != "application/pdf" && ct0 != "application/octet-stream" && ct0 != "application/x-pdf")
                return ("Only PDF files are allowed.", "الملفات المسموحة PDF فقط.");

            byte[] data;
            await using (var ms = new MemoryStream((int)file.Length))
            {
                await file.CopyToAsync(ms, ct);
                data = ms.ToArray();
            }

            // real PDF: the signature must be at byte 0 (rejects exe/zip/html polyglots) and the file must end properly
            if (data.Length < 16 || data[0] != '%' || data[1] != 'P' || data[2] != 'D' || data[3] != 'F' || data[4] != '-')
                return ("The file is not a valid PDF.", "الملف ليس PDF سليم.");
            var tail = Encoding.Latin1.GetString(data, Math.Max(0, data.Length - 2048), Math.Min(2048, data.Length));
            if (!tail.Contains("%%EOF", StringComparison.Ordinal))
                return ("The PDF file is damaged or incomplete.", "ملف PDF تالف أو غير مكتمل.");

            // active content: decode #xx escapes inside names first so "/J#61vaScript" cannot slip through
            var text = Encoding.Latin1.GetString(data);
            foreach (Match m in NameToken.Matches(text))
            {
                var name = HexEscape.Replace(m.Groups[1].Value, h => ((char)Convert.ToInt32(h.Groups[1].Value, 16)).ToString());
                if (Forbidden.Contains(name))
                {
                    logger.LogWarning("Publish upload rejected: forbidden PDF object /{Name} in {File}", name, file.FileName);
                    return ("The PDF contains scripts, attachments or encryption, which are not allowed. Export a plain PDF and try again.",
                            "الملف يحتوي على أكواد أو مرفقات أو تشفير وهذا غير مسموح. صدّر نسخة PDF عادية وحاول مرة أخرى.");
                }
            }

            // antivirus (best effort): scan a quarantined copy that lives outside the web root
            Directory.CreateDirectory(privateFolder);
            var temp = Path.Combine(privateFolder, $"scan-{Guid.NewGuid():N}.pdf");
            try
            {
                await File.WriteAllBytesAsync(temp, data, ct);
                var verdict = await DefenderScanAsync(temp, logger, ct);
                if (verdict == false)
                {
                    logger.LogWarning("Publish upload rejected by antivirus: {File}", file.FileName);
                    return ("The file was flagged by the antivirus scan and was rejected.", "تم رفض الملف لأن فحص مضاد الفيروسات اكتشف به تهديدًا.");
                }
            }
            finally
            {
                try { File.Delete(temp); } catch { /* the scanner may still hold it; the folder is private anyway */ }
            }
            return null;
        }

        /// <summary>true = clean, false = threat found, null = scanner unavailable (fail open, structural checks above still apply).</summary>
        private static async Task<bool?> DefenderScanAsync(string path, ILogger logger, CancellationToken ct)
        {
            if (!OperatingSystem.IsWindows()) return null;
            try
            {
                var exe = FindMpCmdRun();
                if (exe == null) { logger.LogWarning("Windows Defender (MpCmdRun) not found: antivirus scan skipped."); return null; }
                using var p = Process.Start(new ProcessStartInfo(exe, $"-Scan -ScanType 3 -DisableRemediation -File \"{path}\"")
                {
                    UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true
                });
                if (p == null) return null;
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(TimeSpan.FromSeconds(30));
                try { await p.WaitForExitAsync(cts.Token); }
                catch (OperationCanceledException) { try { p.Kill(true); } catch { } logger.LogWarning("Antivirus scan timed out; skipped."); return null; }
                if (p.ExitCode == 0) return true;
                if (p.ExitCode == 2) return false;
                logger.LogWarning("Antivirus scan returned exit code {Code}; skipped.", p.ExitCode);
                return null;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Antivirus scan failed to run; skipped.");
                return null;
            }
        }

        private static string? FindMpCmdRun()
        {
            var classic = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Windows Defender", "MpCmdRun.exe");
            if (File.Exists(classic)) return classic;
            var platform = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Microsoft", "Windows Defender", "Platform");
            if (!Directory.Exists(platform)) return null;
            return Directory.GetDirectories(platform).OrderByDescending(d => d, StringComparer.Ordinal)
                .Select(d => Path.Combine(d, "MpCmdRun.exe")).FirstOrDefault(File.Exists);
        }

        private static readonly Regex PhoneRx = new(@"^\+?[0-9\s\-()]{7,20}$", RegexOptions.Compiled);
        public static bool IsValidPhone(string? s) => s != null && PhoneRx.IsMatch(s) && s.Count(char.IsDigit) >= 7;
    }
}
