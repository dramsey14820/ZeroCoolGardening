using ZeroCool.Core.Catalog;
using ZeroCool.Core.Layout;
using ZeroCool.Core.Models;

namespace ZeroCool.Tests;

public class SquareFootTests
{
    [Fact]
    public void PlantResolver_VarietyInheritsAndOverridesParentFields()
    {
        var plant = new Plant
        {
            Id = "tomato", Name = "Tomato", Icon = "🍅", DaysToMaturity = 75, Sun = "Full sun",
            Spacing = new Spacing { InRowInches = 24, BetweenRowsInches = 36, PlantsPerSquareFoot = 1, CellSpanWidth = 2, CellSpanHeight = 2 },
            HealthBenefits = ["Vitamin C"],
            Warnings = [new PlantWarning { Type = WarningType.ToxicToPets, Text = "Leaves toxic" }],
            Varieties =
            [
                new Variety
                {
                    Id = "tomato-roma", Name = "Roma", DaysToMaturity = 78,
                    HealthBenefits = ["Dense flesh for sauces"]
                }
            ]
        };

        var resolved = PlantResolver.Resolve([plant], "tomato", "tomato-roma")!;
        Assert.Equal("Tomato — Roma", resolved.DisplayName);
        Assert.Equal("🍅", resolved.Icon);
        Assert.Equal("Full sun", resolved.Sun);
        Assert.Equal(78, resolved.DaysToMaturity);
        Assert.Equal((1, 2, 2), resolved.SquareFootDefault);
        Assert.Equal(2, resolved.HealthBenefits.Count);
        Assert.Single(resolved.Warnings);
    }

    [Theory]
    [InlineData(3, null, 16, 1, 1)]
    [InlineData(4, null, 9, 1, 1)]
    [InlineData(6, null, 4, 1, 1)]
    [InlineData(12, null, 1, 1, 1)]
    [InlineData(24, 24.0, 1, 2, 2)]
    [InlineData(18, 36.0, 1, 2, 3)]
    public void RecommendedSquareFoot_DerivesFromSeedPacketSpacing(double inRow, double? betweenRows, int expectedPerSqFt, int expectedW, int expectedH)
    {
        var spacing = new Spacing { InRowInches = inRow, BetweenRowsInches = betweenRows };
        Assert.Equal((expectedPerSqFt, expectedW, expectedH), PlantResolver.RecommendedSquareFoot(spacing));
    }

    [Fact]
    public void SquareFootRules_MultiCellPlacementAndOverlap()
    {
        var bed = new Bed { Width = 4, Height = 4, Type = BedType.SquareFoot };
        Assert.True(SquareFootRules.CanPlace(bed, null, 0, 0, 2, 2));
        bed.Plantings.Add(new Planting { Id = "t1", PlantId = "tomato", X = 0, Y = 0, SpanWidth = 2, SpanHeight = 2 });

        Assert.False(SquareFootRules.CanPlace(bed, null, 1, 1, 1, 1));      // inside tomato's 2x2
        Assert.True(SquareFootRules.CanPlace(bed, null, 2, 0, 1, 1));       // adjacent ok
        Assert.False(SquareFootRules.CanPlace(bed, null, 3, 3, 2, 2));      // out of bounds
        Assert.True(SquareFootRules.CanPlace(bed, "t1", 0, 0, 3, 2));       // resizing self ok
        Assert.Equal("t1", SquareFootRules.PlantingAt(bed, 1, 1)?.Id);
        Assert.Null(SquareFootRules.PlantingAt(bed, 2, 0));
        Assert.Equal((2, 0), SquareFootRules.FindFreeSpot(bed, 2, 2));
    }

    [Fact]
    public void SquareFootRules_TrimOutsideOnBedShrink()
    {
        var bed = new Bed { Width = 4, Height = 4 };
        bed.Plantings.Add(new Planting { Id = "a", PlantId = "carrot", X = 0, Y = 0, SpanWidth = 1, SpanHeight = 1 });
        bed.Plantings.Add(new Planting { Id = "b", PlantId = "tomato", X = 2, Y = 2, SpanWidth = 2, SpanHeight = 2 });

        bed.Width = 3;
        bed.Height = 3;
        var removed = SquareFootRules.TrimOutside(bed);

        Assert.Equal(1, removed);
        Assert.Single(bed.Plantings);
        Assert.Equal("a", bed.Plantings[0].Id);
    }

    [Fact]
    public void IconCatalog_ResolvesSeedPlantsAndExplicitPaths()
    {
        Assert.EndsWith("openmoji-svg/1F345-tomato.svg", IconCatalog.ResolveSvgUrl("🍅", "tomato"));
        Assert.EndsWith("openmoji-svg/1F955-carrot.svg", IconCatalog.ResolveSvgUrl(null, "carrot"));
        Assert.EndsWith("game-icons/stone-pile.svg", IconCatalog.ResolveSvgUrl("game-icons/stone-pile.svg"));
        Assert.Null(IconCatalog.ResolveSvgUrl("❓", "unknown-plant"));
    }
}

