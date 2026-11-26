using ZeroCool.Core.Layout;
using ZeroCool.Core.Models;

namespace ZeroCool.Tests;

public class RowAndContainerTests
{
    [Fact]
    public void RowLayout_ComputesRowsAndPlantsFromSeedPacketSpacing()
    {
        // 4 ft x 8 ft bed, horizontal rows: 48" run, 96" cross
        // Between rows = 24" -> 4 rows; In-row = 12" -> 4 plants/row => 16 total
        var r = RowLayoutEngine.Compute(4, 8, betweenRowsInches: 24, inRowInches: 12, RowOrientation.Horizontal);
        Assert.Equal(4, r.RowCount);
        Assert.Equal(4, r.PlantsPerRow);
        Assert.Equal(16, r.TotalPlants);
        Assert.Equal(4, r.RowPositionsFeet.Count);
        Assert.Equal(4, r.PlantPositionsFeet.Count);
        Assert.All(r.RowPositionsFeet, y => Assert.InRange(y, 0, 8));
        Assert.All(r.PlantPositionsFeet, x => Assert.InRange(x, 0, 4));
    }

    [Fact]
    public void RowLayout_VerticalOrientationSwapsAxes()
    {
        // 4 ft x 8 ft bed, vertical rows: 96" run, 48" cross
        // Between rows = 12" -> 4 rows; In-row = 24" -> 4 plants/row
        var r = RowLayoutEngine.Compute(4, 8, betweenRowsInches: 12, inRowInches: 24, RowOrientation.Vertical);
        Assert.Equal(4, r.RowCount);
        Assert.Equal(4, r.PlantsPerRow);
        Assert.All(r.RowPositionsFeet, x => Assert.InRange(x, 0, 4));
        Assert.All(r.PlantPositionsFeet, y => Assert.InRange(y, 0, 8));
    }

    [Fact]
    public void RowLayout_AtLeastOneRowAndOnePlantForLargeSpacing()
    {
        var r = RowLayoutEngine.Compute(2, 2, betweenRowsInches: 48, inRowInches: 48);
        Assert.Equal(1, r.RowCount);
        Assert.Equal(1, r.PlantsPerRow);
        Assert.Equal(1.0, r.RowPositionsFeet[0], 3);
        Assert.Equal(1.0, r.PlantPositionsFeet[0], 3);
    }

    [Fact]
    public void ContainerGuidance_LargePlantNeedsLargeContainer()
    {
        var tomatoSpacing = new Spacing { InRowInches = 24, BetweenRowsInches = 36, CellSpanWidth = 2, CellSpanHeight = 2 };

        var small = ContainerGuidance.Recommend(ContainerGuidance.FromGallons(1), tomatoSpacing);
        Assert.True(small.IsTooSmall);
        Assert.Equal(0, small.RecommendedPlants);

        var fiveGal = ContainerGuidance.Recommend(ContainerGuidance.FromGallons(5), tomatoSpacing);
        Assert.False(fiveGal.IsTooSmall);
        Assert.Equal(1, fiveGal.RecommendedPlants);

        var tenGal = ContainerGuidance.Recommend(ContainerGuidance.FromGallons(10), tomatoSpacing);
        Assert.Equal(2, tenGal.RecommendedPlants);
    }

    [Fact]
    public void ContainerGuidance_SmallRootCropFitsManyInFiveGallons()
    {
        var carrotSpacing = new Spacing { InRowInches = 3, BetweenRowsInches = 12, PlantsPerSquareFoot = 16 };
        var fiveGal = ContainerGuidance.Recommend(ContainerGuidance.FromGallons(5), carrotSpacing);
        Assert.False(fiveGal.IsTooSmall);
        Assert.True(fiveGal.RecommendedPlants >= 10);
    }
}

