using System.Net.Http.Json;

namespace ECommerce.API.Helpers;

/// <summary>
/// Push notification to the owner's phone when an order is placed, through ntfy (https://ntfy.sh, free, no account).
/// Off until Ntfy:Topic is set. The topic is the secret, so the message carries no customer name, phone or address.
/// A failure here never touches the order: it is sent in the background and every error is swallowed and logged.
/// </summary>
public class OrderNotifier
{
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _config;
    private readonly ILogger<OrderNotifier> _logger;

    public OrderNotifier(IHttpClientFactory http, IConfiguration config, ILogger<OrderNotifier> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    public bool Enabled => !string.IsNullOrWhiteSpace(_config["Ntfy:Topic"]);

    public void NewOrder(int orderId, int books, decimal total, string governorate)
    {
        if (!Enabled) return;
        var title = $"طلب جديد #{orderId}";   // the cart icon comes from the "shopping_cart" tag
        var message = $"{books} {(books >= 3 && books <= 10 ? "كتب" : "كتاب")} — {total:0.##} جنيه — {governorate}";
        _ = Task.Run(() => SendAsync(title, message));
    }

    /// <summary>Sends a message and reports whether the server accepted it (used by the order hook and the test).</summary>
    public async Task<bool> SendAsync(string title, string message)
    {
        try
        {
            var server = (_config["Ntfy:Server"] ?? "https://ntfy.sh").TrimEnd('/');
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8));
            using var req = new HttpRequestMessage(HttpMethod.Post, server + "/")
            {
                Content = JsonContent.Create(new
                {
                    topic = _config["Ntfy:Topic"],
                    title,
                    message,
                    priority = 4,                                         // "high": sound and vibration
                    tags = new[] { "shopping_cart" },
                    click = _config["Ntfy:ClickUrl"] ?? "https://qaro2a.com/AdminOrders"
                })
            };
            var token = _config["Ntfy:Token"];
            if (!string.IsNullOrWhiteSpace(token)) req.Headers.Authorization = new("Bearer", token);
            using var res = await _http.CreateClient("ntfy").SendAsync(req, cts.Token);
            if (!res.IsSuccessStatusCode) _logger.LogWarning("ntfy answered {Status}", (int)res.StatusCode);
            return res.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("ntfy notification failed: {Message}", ex.Message);
            return false;
        }
    }
}
