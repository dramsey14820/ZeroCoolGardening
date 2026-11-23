using ZeroCool.Core.Layout;
using ZeroCool.Core.Models;

namespace ZeroCool.Tests;

public class GridRulesTests
{
    private static GardenPlan Plan(int w = 10, int h = 10) => new() { GridWidth = w, GridHeight = h };

    [Fact]
    public void CanPlace_InBounds_And_NoOverlap()
    {
        var p = Plan();
        p.Beds.Add(new Bed { Id = "a", X = 0, Y = 0, Width = 4, Height = 4 });
        Assert.False(GridRules.CanPlace(p, null, 3, 3, 2, 2));      // overlaps
        Assert.True(GridRules.CanPlace(p, null, 4, 0, 2, 2));       // adjacent ok
        Assert.True(GridRules.CanPlace(p, "a", 0, 0, 5, 5));        // moving/resizing itself ignores itself
        Assert.False(GridRules.CanPlace(p, null, 9, 9, 2, 2));      // out of bounds
        Assert.False(GridRules.CanPlace(p, null, -1, 0, 1, 1));
        Assert.False(GridRules.CanPlace(p, null, 0, 5, 0, 1));      // zero size
    }

    [Fact]
    public void Shape_PaintingCells_RestrictsPlacement()
    {
        var p = Plan(4, 4);
        Assert.True(GridRules.SetActive(p, 3, 3, false));
        Assert.NotNull(p.ActiveCells);
        Assert.Equal(15, p.ActiveCells!.Count);
        Assert.False(GridRules.IsActive(p, 3, 3));
        Assert.False(GridRules.CanPlace(p, null, 2, 2, 2, 2));
        Assert.True(GridRules.CanPlace(p, null, 0, 0, 2, 2));
        Assert.True(GridRules.SetActive(p, 3, 3, true));
        Assert.Equal(16, p.ActiveCells.Count);
    }

    [Fact]
    public void Shape_CannotRemoveCellUnderBed()
    {
        var p = Plan(4, 4);
        p.Beds.Add(new Bed { X = 0, Y = 0, Width = 2, Height = 2 });
        Assert.False(GridRules.SetActive(p, 1, 1, false));
        Assert.True(GridRules.IsActive(p, 1, 1));
    }

    [Fact]
    public void FindFreeSpot_SkipsOccupied_And_ReturnsNullWhenFull()
    {
        var p = Plan(4, 4);
        p.Beds.Add(new Bed { X = 0, Y = 0, Width = 2, Height = 4 });
        Assert.Equal((2, 0), GridRules.FindFreeSpot(p, 2, 2));
        p.Beds.Add(new Bed { X = 2, Y = 0, Width = 2, Height = 4 });
        Assert.Null(GridRules.FindFreeSpot(p, 1, 1));
    }

    [Fact]
    public void Resize_DropsItemsOutsideAndTrimsCells()
    {
        var p = Plan(10, 10);
        p.Beds.Add(new Bed { X = 0, Y = 0, Width = 2, Height = 2 });
        p.Beds.Add(new Bed { X = 8, Y = 8, Width = 2, Height = 2 });
        GridRules.SetActive(p, 9, 9, false);
        GridRules.Resize(p, 5, 5);
        Assert.Single(p.Beds);
        Assert.All(p.ActiveCells!, c => Assert.True(c.X < 5 && c.Y < 5));
    }
}
