using ZeroCool.Core.Catalog;
using ZeroCool.Core.Models;

namespace ZeroCool.Core.Layout;

public enum AdjacencyRelation { Companion, Antagonist }

/// <summary>
/// A highlighted edge strictly between two adjacent planted cells (or adjacent rows).
/// Coordinates are in bed-local SVG units (feet).
/// </summary>
public sealed record AdjacencyEdge(
    double X1,
    double Y1,
    double X2,
    double Y2,
    AdjacencyRelation Relation,
    string PlantAId,
    string PlantBId,
    string Tooltip);

/// <summary>Pure companion/antagonist adjacency engine for square-foot and row beds.</summary>
public static class AdjacencyEngine
{
    public static AdjacencyRelation? RelationBetween(Plant a, Plant b)
    {
        if (a.Id == b.Id) return null;

        // Antagonist takes precedence if either plant lists the other
        if (a.Antagonists.Contains(b.Id, StringComparer.OrdinalIgnoreCase) ||
            b.Antagonists.Contains(a.Id, StringComparer.OrdinalIgnoreCase))
            return AdjacencyRelation.Antagonist;

        if (a.Companions.Contains(b.Id, StringComparer.OrdinalIgnoreCase) ||
            b.Companions.Contains(a.Id, StringComparer.OrdinalIgnoreCase))
            return AdjacencyRelation.Companion;

        return null;
    }

    /// <summary>
    /// Evaluates 4-connected cell neighbors in a square-foot bed and returns edges strictly between affected adjacent cells.
    /// Supports multi-cell spans without highlighting non-adjacent edges.
    /// </summary>
    public static IReadOnlyList<AdjacencyEdge> EvaluateSquareFoot(Bed bed, IReadOnlyList<Plant> catalog)
    {
        var byId = catalog.ToDictionary(p => p.Id, StringComparer.OrdinalIgnoreCase);
        var grid = new Dictionary<(int X, int Y), Planting>();

        foreach (var p in bed.Plantings)
        {
            var sw = Math.Max(1, p.SpanWidth);
            var sh = Math.Max(1, p.SpanHeight);
            for (var dy = 0; dy < sh; dy++)
                for (var dx = 0; dx < sw; dx++)
                    grid[(p.X + dx, p.Y + dy)] = p;
        }

        var edges = new List<AdjacencyEdge>();

        foreach (var ((x, y), a) in grid)
        {
            if (!byId.TryGetValue(a.PlantId, out var plantA)) continue;

            // Check right neighbor (x+1, y) -> shared vertical edge at x+1, from y to y+1
            if (grid.TryGetValue((x + 1, y), out var right) && right.Id != a.Id && byId.TryGetValue(right.PlantId, out var plantB))
            {
                if (RelationBetween(plantA, plantB) is { } rel)
                {
                    edges.Add(new AdjacencyEdge(
                        x + 1, y, x + 1, y + 1,
                        rel, plantA.Id, plantB.Id,
                        TooltipFor(plantA.Name, plantB.Name, rel)));
                }
            }

            // Check bottom neighbor (x, y+1) -> shared horizontal edge at y+1, from x to x+1
            if (grid.TryGetValue((x, y + 1), out var down) && down.Id != a.Id && byId.TryGetValue(down.PlantId, out var plantC))
            {
                if (RelationBetween(plantA, plantC) is { } rel)
                {
                    edges.Add(new AdjacencyEdge(
                        x, y + 1, x + 1, y + 1,
                        rel, plantA.Id, plantC.Id,
                        TooltipFor(plantA.Name, plantC.Name, rel)));
                }
            }
        }

        return edges;
    }

    /// <summary>Evaluates adjacent planted rows in a row bed, returning divider edges between affected rows.</summary>
    public static IReadOnlyList<AdjacencyEdge> EvaluateRows(Bed bed, IReadOnlyList<Plant> catalog, RowLayoutResult layout)
    {
        var byId = catalog.ToDictionary(p => p.Id, StringComparer.OrdinalIgnoreCase);
        var byRow = bed.Plantings.GroupBy(p => p.Y).ToDictionary(g => g.Key, g => g.First());
        var horiz = bed.RowOrientation == RowOrientation.Horizontal;
        var edges = new List<AdjacencyEdge>();

        for (var r = 0; r < layout.RowCount - 1; r++)
        {
            if (!byRow.TryGetValue(r, out var a) || !byRow.TryGetValue(r + 1, out var b)) continue;
            if (!byId.TryGetValue(a.PlantId, out var plantA) || !byId.TryGetValue(b.PlantId, out var plantB)) continue;
            if (RelationBetween(plantA, plantB) is not { } rel) continue;

            var mid = (layout.RowPositionsFeet[r] + layout.RowPositionsFeet[r + 1]) / 2.0;
            edges.Add(horiz
                ? new AdjacencyEdge(0.1, mid, bed.Width - 0.1, mid, rel, plantA.Id, plantB.Id, TooltipFor(plantA.Name, plantB.Name, rel))
                : new AdjacencyEdge(mid, 0.1, mid, bed.Height - 0.1, rel, plantA.Id, plantB.Id, TooltipFor(plantA.Name, plantB.Name, rel)));
        }

        return edges;
    }

    private static string TooltipFor(string nameA, string nameB, AdjacencyRelation rel) =>
        rel == AdjacencyRelation.Companion
            ? $"Companions: {nameA} & {nameB} grow well together."
            : $"Conflict: avoid planting {nameA} next to {nameB}.";
}

