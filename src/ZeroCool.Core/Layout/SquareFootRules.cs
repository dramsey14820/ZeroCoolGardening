using ZeroCool.Core.Models;

namespace ZeroCool.Core.Layout;

/// <summary>Pure placement rules for plantings inside a square-foot bed (1 cell = 1 ft x 1 ft).</summary>
public static class SquareFootRules
{
    public static bool InBounds(Bed bed, int x, int y, int spanW = 1, int spanH = 1) =>
        spanW >= 1 && spanH >= 1 && x >= 0 && y >= 0 && x + spanW <= bed.Width && y + spanH <= bed.Height;

    public static bool CanPlace(Bed bed, string? ignorePlantingId, int x, int y, int spanW, int spanH)
    {
        if (!InBounds(bed, x, y, spanW, spanH)) return false;

        foreach (var p in bed.Plantings)
        {
            if (p.Id == ignorePlantingId) continue;
            var pw = Math.Max(1, p.SpanWidth);
            var ph = Math.Max(1, p.SpanHeight);
            var overlap = x < p.X + pw && p.X < x + spanW && y < p.Y + ph && p.Y < y + spanH;
            if (overlap) return false;
        }
        return true;
    }

    public static (int X, int Y)? FindFreeSpot(Bed bed, int spanW, int spanH)
    {
        for (var y = 0; y + spanH <= bed.Height; y++)
            for (var x = 0; x + spanW <= bed.Width; x++)
                if (CanPlace(bed, null, x, y, spanW, spanH)) return (x, y);
        return null;
    }

    public static Planting? PlantingAt(Bed bed, int x, int y) =>
        bed.Plantings.FirstOrDefault(p =>
            x >= p.X && x < p.X + Math.Max(1, p.SpanWidth) &&
            y >= p.Y && y < p.Y + Math.Max(1, p.SpanHeight));

    /// <summary>Removes any plantings that fall outside the bed after a bed resize.</summary>
    public static int TrimOutside(Bed bed) =>
        bed.Plantings.RemoveAll(p => !InBounds(bed, p.X, p.Y, Math.Max(1, p.SpanWidth), Math.Max(1, p.SpanHeight)));
}

