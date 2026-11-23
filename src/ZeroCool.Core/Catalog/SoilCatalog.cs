namespace ZeroCool.Core.Catalog;

public readonly record struct SoilType(string Name, string Color);

/// <summary>Built-in soil and fertilizer choices (both optional on a bed; users can also type their own).</summary>
public static class SoilCatalog
{
    public const string UnspecifiedColor = "#a39580";

    public static readonly IReadOnlyList<SoilType> Soils = new SoilType[]
    {
        new("Loam", "#8b5a2b"),
        new("Sandy loam", "#b88c5a"),
        new("Sandy", "#d2b48c"),
        new("Clay", "#a0522d"),
        new("Silty", "#7a6a53"),
        new("Peat-based", "#4a3728"),
        new("Raised-bed mix", "#5c4033"),
        new("Compost-rich", "#3b2a1a"),
    };

    public static readonly IReadOnlyList<string> Fertilizers = new[]
    {
        "Compost", "Aged manure", "Balanced (10-10-10)", "High nitrogen", "High phosphorus", "Fish emulsion", "Bone meal", "Organic granular",
    };

    public static string ColorFor(string? soil) =>
        Soils.FirstOrDefault(s => string.Equals(s.Name, soil, StringComparison.OrdinalIgnoreCase)).Color ?? UnspecifiedColor;
}

