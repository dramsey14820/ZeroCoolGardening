using ZeroCool.Core.Models;

namespace ZeroCool.Core.Catalog;

public sealed record GrowingZoneInfo(
    int Zone,
    string MinWinterTempF,
    string MinWinterTempC,
    int? LastSpringFrostMonth,
    int? LastSpringFrostDay,
    int? FirstFallFrostMonth,
    int? FirstFallFrostDay,
    string Notes)
{
    public DateOnly? LastSpringFrost(int year) =>
        LastSpringFrostMonth is int m && LastSpringFrostDay is int d ? new DateOnly(year, m, d) : null;

    public DateOnly? FirstFallFrost(int year) =>
        FirstFallFrostMonth is int m && FirstFallFrostDay is int d ? new DateOnly(year, m, d) : null;
}

/// <summary>
/// Offline USDA Hardiness Zone reference (Zones 1–13) with approximate average frost dates.
/// Users can override exact frost dates in Settings for their local microclimate.
/// </summary>
public static class GrowingZoneCatalog
{
    public static readonly IReadOnlyList<GrowingZoneInfo> Zones =
    [
        new(1,  "-60°F to -50°F", "-51°C to -46°C", 6, 15, 8, 15, "Subarctic; very short frost-free season (~60 days)."),
        new(2,  "-50°F to -40°F", "-46°C to -40°C", 6,  1, 9,  1, "Short cool season (~90 days); start warm crops indoors."),
        new(3,  "-40°F to -30°F", "-40°C to -34°C", 5, 15, 9, 15, "Short season (~120 days); hardy perennials & quick-maturing crops."),
        new(4,  "-30°F to -20°F", "-34°C to -29°C", 5, 10, 9, 25, "Cool northern climate (~135 days frost-free)."),
        new(5,  "-20°F to -10°F", "-29°C to -23°C", 4, 30, 10, 10, "Moderate northern climate (~160 days frost-free)."),
        new(6,  "-10°F to 0°F",   "-23°C to -18°C", 4, 15, 10, 20, "Four-season climate (~185 days frost-free)."),
        new(7,  "0°F to 10°F",    "-18°C to -12°C", 4,  5, 10, 30, "Long growing season (~205 days); spring and fall plantings."),
        new(8,  "10°F to 20°F",   "-12°C to -7°C",  3, 20, 11, 10, "Mild winters (~235 days frost-free); overwinter many greens."),
        new(9,  "20°F to 30°F",   "-7°C to -1°C",   2, 20, 11, 30, "Long warm season (~280 days); cool crops thrive in winter."),
        new(10, "30°F to 40°F",   "-1°C to 4°C",    1, 30, 12, 15, "Nearly frost-free (~320 days); year-round gardening."),
        new(11, "40°F to 50°F",   "4°C to 10°C",    null, null, null, null, "Frost-free tropical/subtropical; year-round growing."),
        new(12, "50°F to 60°F",   "10°C to 16°C",   null, null, null, null, "Frost-free tropical; year-round growing."),
        new(13, "60°F to 70°F",   "16°C to 21°C",   null, null, null, null, "Frost-free equatorial tropical; year-round growing."),
    ];

    public static GrowingZoneInfo? Get(int? zone) =>
        zone is int z ? Zones.FirstOrDefault(x => x.Zone == z) : null;

    public static (DateOnly? LastSpring, DateOnly? FirstFall) EffectiveFrostDates(AppSettings settings, int year)
    {
        var info = Get(settings.UsdaZone);
        var spring = settings.LastSpringFrostOverride is DateOnly s
            ? new DateOnly(year, s.Month, s.Day)
            : info?.LastSpringFrost(year);
        var fall = settings.FirstFallFrostOverride is DateOnly f
            ? new DateOnly(year, f.Month, f.Day)
            : info?.FirstFallFrost(year);
        return (spring, fall);
    }

    /// <summary>True if no zone is set, the plant has no zone restriction, or the plant supports the user's zone.</summary>
    public static bool IsSuitable(IReadOnlyList<int>? plantZones, int? userZone) =>
        userZone is null || plantZones is null || plantZones.Count == 0 || plantZones.Contains(userZone.Value);
}

