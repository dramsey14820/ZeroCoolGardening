using ZeroCool.Core.Catalog;
using ZeroCool.Core.Layout;
using ZeroCool.Core.Models;

namespace ZeroCool.Tests;

public class ZoneAndAdjacencyTests
{
    private static readonly IReadOnlyList<Plant> SampleCatalog =
    [
        new Plant { Id = "tomato", Name = "Tomato", Companions = ["basil", "carrot"], Antagonists = [], Zones = [3,4,5,6,7,8,9,10,11] },
        new Plant { Id = "basil",  Name = "Basil",  Companions = ["tomato"], Antagonists = [], Zones = [3,4,5,6,7,8,9,10,11] },
        new Plant { Id = "carrot", Name = "Carrot", Companions = ["tomato"], Antagonists = ["dill"], Zones = [3,4,5,6,7,8,9,10] },
        new Plant { Id = "dill",   Name = "Dill",   Companions = [], Antagonists = ["carrot", "tomato"], Zones = [3,4,5,6,7,8,9,10,11] },
    ];

    [Fact]
    public void SquareFootAdjacency_HighlightsOnlySharedEdgeBetweenAffectedCells()
    {
        var bed = new Bed { Width = 4, Height = 4, Type = BedType.SquareFoot };
        // 2x2 Tomato at (0,0)-(1,1)
        bed.Plantings.Add(new Planting { Id = "t", PlantId = "tomato", X = 0, Y = 0, SpanWidth = 2, SpanHeight = 2 });
        // 1x1 Basil at (2,0) -> touches ONLY the top-right cell (1,0) of Tomato along x=2, y=0..1
        bed.Plantings.Add(new Planting { Id = "b", PlantId = "basil", X = 2, Y = 0, SpanWidth = 1, SpanHeight = 1 });
        // 1x1 Dill at (0,2) -> touches ONLY the bottom-left cell (0,1) of Tomato along y=2, x=0..1
        bed.Plantings.Add(new Planting { Id = "d", PlantId = "dill", X = 0, Y = 2, SpanWidth = 1, SpanHeight = 1 });

        var edges = AdjacencyEngine.EvaluateSquareFoot(bed, SampleCatalog);

        Assert.Equal(2, edges.Count);

        var good = Assert.Single(edges, e => e.Relation == AdjacencyRelation.Companion);
        Assert.Equal((2, 0, 2, 1), (good.X1, good.Y1, good.X2, good.Y2));

        var bad = Assert.Single(edges, e => e.Relation == AdjacencyRelation.Antagonist);
        Assert.Equal((0, 2, 1, 2), (bad.X1, bad.Y1, bad.X2, bad.Y2));
    }

    [Fact]
    public void SquareFootAdjacency_IgnoresDiagonalsAndSamePlant()
    {
        var bed = new Bed { Width = 4, Height = 4, Type = BedType.SquareFoot };
        bed.Plantings.Add(new Planting { Id = "c1", PlantId = "carrot", X = 0, Y = 0 });
        bed.Plantings.Add(new Planting { Id = "c2", PlantId = "carrot", X = 1, Y = 0 }); // same plant
        bed.Plantings.Add(new Planting { Id = "d1", PlantId = "dill",   X = 1, Y = 1 }); // diagonal to c1, adjacent to c2!

        var edges = AdjacencyEngine.EvaluateSquareFoot(bed, SampleCatalog);
        // Only c2 (1,0) and d1 (1,1) share an edge at y=1, x=1..2
        var edge = Assert.Single(edges);
        Assert.Equal(AdjacencyRelation.Antagonist, edge.Relation);
        Assert.Equal((1, 1, 2, 1), (edge.X1, edge.Y1, edge.X2, edge.Y2));
    }

    [Fact]
    public void RowAdjacency_HighlightsBetweenAdjacentRowsOnly()
    {
        var bed = new Bed { Width = 4, Height = 6, Type = BedType.Row, BetweenRowsInches = 18, InRowSpacingInches = 12 };
        var layout = RowLayoutEngine.Compute(bed.Width, bed.Height, bed.BetweenRowsInches, bed.InRowSpacingInches);
        bed.Plantings.Add(new Planting { Id = "r0", PlantId = "carrot", Y = 0 });
        bed.Plantings.Add(new Planting { Id = "r1", PlantId = "dill", Y = 1 });
        bed.Plantings.Add(new Planting { Id = "r2", PlantId = "tomato", Y = 2 });
        bed.Plantings.Add(new Planting { Id = "r3", PlantId = "basil", Y = 3 });

        var edges = AdjacencyEngine.EvaluateRows(bed, SampleCatalog, layout);
        Assert.Equal(3, edges.Count);
        Assert.Equal(2, edges.Count(e => e.Relation == AdjacencyRelation.Antagonist)); // carrot-dill, dill-tomato
        Assert.Single(edges, e => e.Relation == AdjacencyRelation.Companion);          // tomato-basil
    }

    [Fact]
    public void GrowingZoneCatalog_LookupAndFrostOverrides()
    {
        var z6 = GrowingZoneCatalog.Get(6)!;
        Assert.Equal(new DateOnly(2026, 4, 15), z6.LastSpringFrost(2026));
        Assert.Equal(new DateOnly(2026, 10, 20), z6.FirstFallFrost(2026));

        var z11 = GrowingZoneCatalog.Get(11)!;
        Assert.Null(z11.LastSpringFrost(2026));

        var settings = new AppSettings { UsdaZone = 6, LastSpringFrostOverride = new DateOnly(2025, 5, 1) };
        var (spring, fall) = GrowingZoneCatalog.EffectiveFrostDates(settings, 2026);
        Assert.Equal(new DateOnly(2026, 5, 1), spring);
        Assert.Equal(new DateOnly(2026, 10, 20), fall);

        Assert.True(GrowingZoneCatalog.IsSuitable([3, 4, 5, 6], 6));
        Assert.False(GrowingZoneCatalog.IsSuitable([3, 4, 5, 6], 9));
        Assert.True(GrowingZoneCatalog.IsSuitable([3, 4, 5, 6], null));
    }
}

