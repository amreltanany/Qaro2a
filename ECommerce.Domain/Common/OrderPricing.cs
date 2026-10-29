namespace ECommerce.Domain.Common;

public sealed record Governorate(string Key, string En, string Ar);

public static class OrderPricing
{
    public const decimal CairoFee = 50m;
    public const decimal OtherGovernoratesFee = 70m;

    /// <summary>Fee used when an order is created without a governorate (admin API): the standard rate.</summary>
    public const decimal DeliveryFee = OtherGovernoratesFee;

    public static readonly IReadOnlyList<Governorate> Governorates = new Governorate[]
    {
        new("cairo", "Cairo", "القاهرة"),
        new("giza", "Giza", "الجيزة"),
        new("alexandria", "Alexandria", "الإسكندرية"),
        new("qalyubia", "Qalyubia", "القليوبية"),
        new("port-said", "Port Said", "بورسعيد"),
        new("suez", "Suez", "السويس"),
        new("dakahlia", "Dakahlia", "الدقهلية"),
        new("sharqia", "Sharqia", "الشرقية"),
        new("gharbia", "Gharbia", "الغربية"),
        new("monufia", "Monufia", "المنوفية"),
        new("beheira", "Beheira", "البحيرة"),
        new("kafr-el-sheikh", "Kafr El Sheikh", "كفر الشيخ"),
        new("damietta", "Damietta", "دمياط"),
        new("ismailia", "Ismailia", "الإسماعيلية"),
        new("fayoum", "Fayoum", "الفيوم"),
        new("beni-suef", "Beni Suef", "بني سويف"),
        new("minya", "Minya", "المنيا"),
        new("assiut", "Assiut", "أسيوط"),
        new("sohag", "Sohag", "سوهاج"),
        new("qena", "Qena", "قنا"),
        new("luxor", "Luxor", "الأقصر"),
        new("aswan", "Aswan", "أسوان"),
        new("red-sea", "Red Sea", "البحر الأحمر"),
        new("new-valley", "New Valley", "الوادي الجديد"),
        new("matrouh", "Matrouh", "مطروح"),
        new("north-sinai", "North Sinai", "شمال سيناء"),
        new("south-sinai", "South Sinai", "جنوب سيناء"),
    };

    public static Governorate? Find(string? key) =>
        string.IsNullOrWhiteSpace(key) ? null : Governorates.FirstOrDefault(g => g.Key == key.Trim().ToLowerInvariant());

    /// <summary>Cairo 50 EGP, every other governorate 70 EGP. Computed on the server, never taken from the client.</summary>
    public static decimal FeeFor(Governorate g) => g.Key == "cairo" ? CairoFee : OtherGovernoratesFee;
}
