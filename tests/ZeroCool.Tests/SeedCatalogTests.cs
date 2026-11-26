using ZeroCool.Core.Catalog;
using ZeroCool.Core.Models;
using ZeroCool.Data;

namespace ZeroCool.Tests;

public class SeedCatalogTests
{
    [Fact]
    public void SeedCatalog_HasAtLeast50PlantsCoveringAllCategoriesWithValidReferences()
    {
        var plants = PlantCatalogRepository.LoadSeed();
        Assert.True(plants.Count >= 50, $"Expected at least 50 plants, got {plants.Count}");

        // Unique plant IDs
        var ids = plants.Select(p => p.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        Assert.Equal(plants.Count, ids.Count);

        // Unique variety IDs
        var allVarieties = plants.SelectMany(p => p.Varieties).ToList();
        Assert.True(allVarieties.Count >= 100, $"Expected at least 100 varieties, got {allVarieties.Count}");
        Assert.Equal(allVarieties.Count, allVarieties.Select(v => v.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count());

        // Covers all 7 non-tree categories
        foreach (var cat in Enum.GetValues<PlantCategory>())
            Assert.Contains(plants, p => p.Category == cat);

        foreach (var p in plants)
        {
            Assert.False(string.IsNullOrWhiteSpace(p.Name), $"Missing name on {p.Id}");
            Assert.NotNull(p.Spacing);
            Assert.NotNull(p.Zones);
            Assert.NotEmpty(p.Zones!);
            Assert.NotNull(p.CalendarRules);
            Assert.NotEmpty(p.CalendarRules!);
            Assert.NotNull(p.Sources);
            Assert.NotEmpty(p.Sources!);
            Assert.DoesNotContain(p.Sources!, s => s.Contains("STARTER DATA", StringComparison.OrdinalIgnoreCase));

            foreach (var c in p.Companions)
                Assert.True(ids.Contains(c), $"Plant '{p.Id}' references unknown companion '{c}'");
            foreach (var a in p.Antagonists)
                Assert.True(ids.Contains(a), $"Plant '{p.Id}' references unknown antagonist '{a}'");

            // Every seed plant resolves to a bundled SVG icon
            Assert.NotNull(IconCatalog.ResolveSvgUrl(p.Icon, p.Id));
        }
    }
}

