using ZeroCool.Core.Catalog;
using ZeroCool.Core.Models;

namespace ZeroCool.Core.Layout;

public sealed record ContainerPreset(string Label, double Liters);

public sealed record ContainerRecommendation(
    int RecommendedPlants,
    double MinLitersPerPlant,
    bool IsTooSmall,
    string Summary);

/// <summary>Container size presets and plants-per-container guidance.</summary>
public static class ContainerGuidance
{
    public const double LitersPerGallon = 3.78541;
    public const double DefaultVolumeLiters = 5.0 * LitersPerGallon;

    public static readonly IReadOnlyList<ContainerPreset> Presets =
    [
        new("1 gal (3.8 L)", 1 * LitersPerGallon),
        new("3 gal (11.4 L)", 3 * LitersPerGallon),
        new("5 gal (18.9 L)", 5 * LitersPerGallon),
        new("10 gal (37.9 L)", 10 * LitersPerGallon),
        new("15 gal (56.8 L)", 15 * LitersPerGallon),
        new("25 gal (94.6 L)", 25 * LitersPerGallon),
    ];

    public static double ToGallons(double liters) => liters / LitersPerGallon;
    public static double FromGallons(double gallons) => gallons * LitersPerGallon;

    public static ContainerRecommendation Recommend(double? containerVolumeLiters, Spacing? spacing)
    {
        var vol = containerVolumeLiters is > 0 ? containerVolumeLiters.Value : DefaultVolumeLiters;
        var (perSqFt, spanW, spanH) = PlantResolver.RecommendedSquareFoot(spacing);

        double minLiters;
        if (spanW * spanH >= 4 || spacing?.InRowInches is >= 20)
            minLiters = 5.0 * LitersPerGallon;      // ~19 L (tomato, squash)
        else if (spanW * spanH >= 2 || spacing?.InRowInches is >= 14)
            minLiters = 3.0 * LitersPerGallon;      // ~11.4 L (pepper, eggplant, broccoli)
        else if (perSqFt <= 1)
            minLiters = 2.0 * LitersPerGallon;      // ~7.6 L (basil, dill, marigold)
        else if (perSqFt <= 4)
            minLiters = 1.0 * LitersPerGallon;      // ~3.8 L (lettuce, chard)
        else
            minLiters = 0.4 * LitersPerGallon;      // ~1.5 L (carrots, radishes)

        var gal = Math.Round(ToGallons(vol), 1);
        var minGal = Math.Round(ToGallons(minLiters), 1);

        if (vol + 0.05 < minLiters)
        {
            return new ContainerRecommendation(
                0,
                minLiters,
                true,
                $"This {gal} gal container is small for this plant — at least {minGal} gal recommended.");
        }

        var count = Math.Clamp((int)Math.Floor((vol + 0.05) / minLiters), 1, 24);
        var noun = count == 1 ? "plant" : "plants";
        return new ContainerRecommendation(
            count,
            minLiters,
            false,
            $"Fits ~{count} {noun} in this {gal} gal container.");
    }
}

