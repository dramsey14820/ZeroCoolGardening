using ZeroCool.Core.Models;

namespace ZeroCool.Core.Layout;

public sealed record RowLayoutResult(
    int RowCount,
    int PlantsPerRow,
    int TotalPlants,
    double BetweenRowsInches,
    double InRowInches,
    IReadOnlyList<double> RowPositionsFeet,
    IReadOnlyList<double> PlantPositionsFeet);

/// <summary>Pure row-gardening calculator: bed dimensions + seed-packet spacing -> rows and plant positions.</summary>
public static class RowLayoutEngine
{
    public const double DefaultBetweenRowsInches = 18.0;
    public const double DefaultInRowInches = 12.0;

    public static RowLayoutResult Compute(
        double bedWidthFeet,
        double bedHeightFeet,
        double? betweenRowsInches,
        double? inRowInches,
        RowOrientation orientation = RowOrientation.Horizontal)
    {
        var w = Math.Max(0.5, bedWidthFeet);
        var h = Math.Max(0.5, bedHeightFeet);
        var between = betweenRowsInches is > 0 ? betweenRowsInches.Value : DefaultBetweenRowsInches;
        var inRow = inRowInches is > 0 ? inRowInches.Value : DefaultInRowInches;

        var runFeet = orientation == RowOrientation.Horizontal ? w : h;
        var crossFeet = orientation == RowOrientation.Horizontal ? h : w;

        var rowCount = Math.Max(1, (int)Math.Floor((crossFeet * 12.0) / between));
        var plantsPerRow = Math.Max(1, (int)Math.Floor((runFeet * 12.0) / inRow));

        var rowPositions = CenteredPositions(crossFeet, rowCount, between / 12.0);
        var plantPositions = CenteredPositions(runFeet, plantsPerRow, inRow / 12.0);

        return new RowLayoutResult(
            rowCount,
            plantsPerRow,
            rowCount * plantsPerRow,
            between,
            inRow,
            rowPositions,
            plantPositions);
    }

    private static IReadOnlyList<double> CenteredPositions(double spanFeet, int count, double stepFeet)
    {
        if (count <= 1) return [spanFeet / 2.0];
        var used = (count - 1) * stepFeet;
        var start = Math.Max(stepFeet / 2.0, (spanFeet - used) / 2.0);
        // Clamp if rounding pushed the last item beyond the bed
        if (start + used > spanFeet)
        {
            var margin = spanFeet / (count * 2.0);
            var step = (spanFeet - 2.0 * margin) / (count - 1);
            return Enumerable.Range(0, count).Select(i => margin + i * step).ToList();
        }
        return Enumerable.Range(0, count).Select(i => start + i * stepFeet).ToList();
    }
}

