using ZeroCool.Core.Models;

namespace ZeroCool.Core.Catalog;

/// <summary>Merged view of a Plant and an optional Variety, where any non-null variety field overrides the parent plant.</summary>
public sealed class ResolvedPlant
{
    public required Plant Plant { get; init; }
    public Variety? Variety { get; init; }

    public string PlantId => Plant.Id;
    public string? VarietyId => Variety?.Id;
    public string DisplayName => Variety is null ? Plant.Name : $"{Plant.Name} — {Variety.Name}";
    public string ShortName => Variety?.Name ?? Plant.Name;
    public string Icon => Variety?.Icon ?? Plant.Icon ?? "🌱";
    public string? Description => Variety?.Description ?? Plant.Description;
    public int? DaysToMaturity => Variety?.DaysToMaturity ?? Plant.DaysToMaturity;
    public int? DaysToGermination => Variety?.DaysToGermination ?? Plant.DaysToGermination;
    public string? Sun => Variety?.Sun ?? Plant.Sun;
    public string? Water => Variety?.Water ?? Plant.Water;
    public IReadOnlyList<int> Zones => Variety?.Zones is { Count: > 0 } vz ? vz : (Plant.Zones ?? []);
    public Spacing EffectiveSpacing => MergeSpacing(Plant.Spacing, Variety?.Spacing);
    public IReadOnlyList<string> HealthBenefits => MergeList(Plant.HealthBenefits, Variety?.HealthBenefits);
    public IReadOnlyList<PlantWarning> Warnings => MergeList(Plant.Warnings, Variety?.Warnings);
    public IReadOnlyList<CalendarRule> CalendarRules => Variety?.CalendarRules is { Count: > 0 } vr ? vr : (Plant.CalendarRules ?? []);
    public IReadOnlyList<string> Sources => MergeList(Plant.Sources, Variety?.Sources);
    public string? Notes => Variety?.Notes ?? Plant.Notes;

    public (int PlantsPerSquareFoot, int SpanWidth, int SpanHeight) SquareFootDefault =>
        PlantResolver.RecommendedSquareFoot(EffectiveSpacing);

    private static Spacing MergeSpacing(Spacing? p, Spacing? v)
    {
        if (p is null && v is null) return new Spacing();
        if (v is null) return p!;
        if (p is null) return v;
        return new Spacing
        {
            InRowInches = v.InRowInches ?? p.InRowInches,
            BetweenRowsInches = v.BetweenRowsInches ?? p.BetweenRowsInches,
            PlantsPerSquareFoot = v.PlantsPerSquareFoot ?? p.PlantsPerSquareFoot,
            CellSpanWidth = v.CellSpanWidth > 1 ? v.CellSpanWidth : p.CellSpanWidth,
            CellSpanHeight = v.CellSpanHeight > 1 ? v.CellSpanHeight : p.CellSpanHeight,
        };
    }

    private static IReadOnlyList<T> MergeList<T>(List<T>? parent, List<T>? child)
    {
        if (parent is not { Count: > 0 }) return child ?? [];
        if (child is not { Count: > 0 }) return parent;
        return parent.Concat(child).ToList();
    }
}

public static class PlantResolver
{
    public static ResolvedPlant? Resolve(IReadOnlyList<Plant> catalog, string plantId, string? varietyId = null)
    {
        var plant = catalog.FirstOrDefault(p => p.Id == plantId);
        if (plant is null) return null;
        var variety = string.IsNullOrEmpty(varietyId) ? null : plant.Varieties.FirstOrDefault(v => v.Id == varietyId);
        return new ResolvedPlant { Plant = plant, Variety = variety };
    }

    /// <summary>
    /// Computes recommended square-foot density and cell span from spacing info.
    /// Uses explicit PlantsPerSquareFoot / CellSpan when set, otherwise derives from seed-packet spacing in inches.
    /// </summary>
    public static (int PlantsPerSquareFoot, int SpanWidth, int SpanHeight) RecommendedSquareFoot(Spacing? spacing)
    {
        if (spacing is null) return (1, 1, 1);

        var spanW = Math.Max(1, spacing.CellSpanWidth);
        var spanH = Math.Max(1, spacing.CellSpanHeight);
        if (spanW > 1 || spanH > 1)
            return (Math.Max(1, spacing.PlantsPerSquareFoot ?? 1), spanW, spanH);

        if (spacing.PlantsPerSquareFoot is > 0)
            return (spacing.PlantsPerSquareFoot.Value, 1, 1);

        if (spacing.InRowInches is > 0)
        {
            var inRow = spacing.InRowInches.Value;
            if (inRow <= 3.5) return (16, 1, 1);
            if (inRow <= 4.5) return (9, 1, 1);
            if (inRow <= 6.5) return (4, 1, 1);
            if (inRow <= 12.5) return (1, 1, 1);

            var w = Math.Max(1, (int)Math.Ceiling(inRow / 12.0));
            var h = Math.Max(1, (int)Math.Ceiling((spacing.BetweenRowsInches ?? inRow) / 12.0));
            return (1, Math.Min(w, 6), Math.Min(h, 6));
        }

        return (1, 1, 1);
    }

    /// <summary>Sub-grid (columns x rows) for rendering N plants inside a single square-foot cell.</summary>
    public static (int Cols, int Rows) SubGrid(int count)
    {
        if (count <= 1) return (1, 1);
        if (count == 2) return (2, 1);
        if (count <= 4) return (2, 2);
        if (count <= 6) return (3, 2);
        if (count <= 9) return (3, 3);
        var cols = (int)Math.Ceiling(Math.Sqrt(count));
        var rows = (int)Math.Ceiling((double)count / cols);
        return (cols, rows);
    }
}

