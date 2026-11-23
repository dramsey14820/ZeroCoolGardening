using ZeroCool.Core.Models;

namespace ZeroCool.Core.Layout;

/// <summary>Pure grid placement rules for the yard (1 cell = 1 ft). ActiveCells == null means the full rectangle is active.</summary>
public static class GridRules
{
    public readonly record struct Item(string Id, int X, int Y, int Width, int Height);

    public static IEnumerable<Item> Items(GardenPlan p) =>
        p.Beds.Select(b => new Item(b.Id, b.X, b.Y, b.Width, b.Height))
              .Concat(p.Features.Select(f => new Item(f.Id, f.X, f.Y, f.Width, f.Height)));

    public static HashSet<(int X, int Y)>? ActiveSet(GardenPlan p) =>
        p.ActiveCells?.Select(c => (c.X, c.Y)).ToHashSet();

    public static bool InBounds(GardenPlan p, int x, int y) => x >= 0 && y >= 0 && x < p.GridWidth && y < p.GridHeight;

    public static bool IsActive(GardenPlan p, int x, int y, HashSet<(int X, int Y)>? set = null)
    {
        if (!InBounds(p, x, y)) return false;
        set ??= ActiveSet(p);
        return set is null || set.Contains((x, y));
    }

    public static bool CanPlace(GardenPlan p, string? ignoreId, int x, int y, int w, int h)
    {
        if (w < 1 || h < 1) return false;
        var set = ActiveSet(p);
        for (var cy = y; cy < y + h; cy++)
            for (var cx = x; cx < x + w; cx++)
                if (!IsActive(p, cx, cy, set)) return false;

        foreach (var i in Items(p))
        {
            if (i.Id == ignoreId) continue;
            var overlap = x < i.X + i.Width && i.X < x + w && y < i.Y + i.Height && i.Y < y + h;
            if (overlap) return false;
        }
        return true;
    }

    public static (int X, int Y)? FindFreeSpot(GardenPlan p, int w, int h)
    {
        for (var y = 0; y + h <= p.GridHeight; y++)
            for (var x = 0; x + w <= p.GridWidth; x++)
                if (CanPlace(p, null, x, y, w, h)) return (x, y);
        return null;
    }

    /// <summary>Paint a yard cell active/inactive. Returns false if the cell is covered by a bed/feature (cannot be removed) or out of bounds.</summary>
    public static bool SetActive(GardenPlan p, int x, int y, bool active)
    {
        if (!InBounds(p, x, y)) return false;
        if (p.ActiveCells is null)
        {
            if (active) return true;
            p.ActiveCells = Enumerable.Range(0, p.GridHeight)
                .SelectMany(cy => Enumerable.Range(0, p.GridWidth).Select(cx => new GridCell { X = cx, Y = cy })).ToList();
        }
        var existing = p.ActiveCells.FirstOrDefault(c => c.X == x && c.Y == y);
        if (active)
        {
            if (existing is null) p.ActiveCells.Add(new GridCell { X = x, Y = y });
            return true;
        }
        var covered = Items(p).Any(i => x >= i.X && x < i.X + i.Width && y >= i.Y && y < i.Y + i.Height);
        if (covered) return false;
        if (existing is not null) p.ActiveCells.Remove(existing);
        return true;
    }

    /// <summary>Resize the yard grid, dropping items that no longer fit and trimming painted cells.</summary>
    public static void Resize(GardenPlan p, int width, int height)
    {
        p.GridWidth = Math.Clamp(width, 1, 200);
        p.GridHeight = Math.Clamp(height, 1, 200);
        p.ActiveCells?.RemoveAll(c => c.X >= p.GridWidth || c.Y >= p.GridHeight);
        p.Beds.RemoveAll(b => b.X + b.Width > p.GridWidth || b.Y + b.Height > p.GridHeight);
        p.Features.RemoveAll(f => f.X + f.Width > p.GridWidth || f.Y + f.Height > p.GridHeight);
    }
}

