using ZeroCool.Core.Models;
using ZeroCool.Data;

namespace ZeroCool.Tests;

public class StorageTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "zc-" + Guid.NewGuid().ToString("N"));
    private LocalFileStore Store => new(_dir);

    public void Dispose() { if (Directory.Exists(_dir)) Directory.Delete(_dir, true); }

    [Fact]
    public async Task FileStore_RoundTrips_And_LeavesNoTempFile()
    {
        var s = Store;
        await s.WriteAsync("a/b.json", "{}");
        await s.WriteAsync("a/b.json", "{\"x\":1}");
        Assert.Equal("{\"x\":1}", await s.ReadAsync("a/b.json"));
        Assert.Empty(Directory.GetFiles(Path.Combine(_dir, "a"), "*.tmp"));
    }

    [Theory]
    [InlineData("../evil.json")]
    [InlineData("..\\evil.json")]
    public async Task FileStore_RejectsPathTraversal(string path)
    {
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Store.WriteAsync(path, "x"));
    }

    [Fact]
    public async Task Plans_SaveListDelete_And_Template_Clone()
    {
        var repo = new PlanRepository(Store);
        var plan = new GardenPlan { Name = "Backyard" };
        plan.Beds.Add(new Bed { Name = "Bed 1", Type = BedType.SquareFoot, Soil = "Loam" });
        plan.Beds[0].Plantings.Add(new Planting { PlantId = "tomato", VarietyId = "tomato-roma", SpanWidth = 2, SpanHeight = 2 });
        await repo.SaveAsync(plan);

        var loaded = await repo.GetAsync(plan.Id, false);
        Assert.NotNull(loaded);
        Assert.Equal("tomato-roma", loaded!.Beds[0].Plantings[0].VarietyId);
        Assert.Equal(BedType.SquareFoot, loaded.Beds[0].Type);

        var template = await repo.CloneAsync(plan, asTemplate: true, "My Template");
        Assert.True(template.IsTemplate);
        Assert.NotEqual(plan.Id, template.Id);
        Assert.Single(await repo.ListAsync(templates: true));
        Assert.Single(await repo.ListAsync(templates: false));

        Assert.True(await repo.DeleteAsync(plan.Id, false));
        Assert.Empty(await repo.ListAsync(false));
    }

    [Fact]
    public async Task Plans_RejectUnsafeIds()
    {
        var repo = new PlanRepository(Store);
        await Assert.ThrowsAsync<ArgumentException>(() => repo.GetAsync("../x", false));
    }

    [Fact]
    public async Task Settings_DefaultThenPersist()
    {
        var repo = new SettingsRepository(Store);
        Assert.Equal(UnitSystem.Imperial, (await repo.LoadAsync()).Units);
        await repo.SaveAsync(new AppSettings { UsdaZone = 7, Units = UnitSystem.Metric });
        var s = await repo.LoadAsync();
        Assert.Equal(7, s.UsdaZone);
        Assert.Equal(UnitSystem.Metric, s.Units);
    }

    [Fact]
    public async Task Catalog_SeedLoads_WithVarieties_And_Warnings()
    {
        var all = await new PlantCatalogRepository(Store).GetAllAsync();
        var tomato = all.Single(p => p.Id == "tomato");
        Assert.Equal(2, tomato.Varieties.Count);
        Assert.Contains(tomato.Warnings!, w => w.Type == WarningType.ToxicToPets);
        Assert.Equal(2, tomato.Spacing!.CellSpanWidth);
    }

    [Fact]
    public async Task Catalog_AddEditDelete_Reset()
    {
        var repo = new PlantCatalogRepository(Store);

        var custom = new Plant { Id = "my-pepper", Name = "Pepper", IsUserDefined = true, Category = PlantCategory.Vegetable };
        custom.Varieties.Add(new Variety { Id = "my-jalapeno", Name = "Jalapeño", IsUserDefined = true });
        await repo.UpsertAsync(custom);
        Assert.Contains(await repo.GetAllAsync(), p => p.Id == "my-pepper" && p.Varieties.Count == 1);

        var tomato = (await repo.GetAllAsync()).Single(p => p.Id == "tomato");
        tomato.Name = "Tomato (edited)";
        await repo.UpsertAsync(tomato);
        Assert.Equal("Tomato (edited)", (await repo.GetAllAsync()).Single(p => p.Id == "tomato").Name);

        await repo.ResetToDefaultAsync("tomato");
        Assert.Equal("Tomato", (await repo.GetAllAsync()).Single(p => p.Id == "tomato").Name);

        await repo.DeleteAsync("tomato");
        Assert.DoesNotContain(await repo.GetAllAsync(), p => p.Id == "tomato");
        await repo.ResetToDefaultAsync("tomato");
        Assert.Contains(await repo.GetAllAsync(), p => p.Id == "tomato");

        await repo.DeleteAsync("my-pepper");
        Assert.DoesNotContain(await repo.GetAllAsync(), p => p.Id == "my-pepper");
    }

    [Fact]
    public async Task AutoSaver_Debounces_And_Flushes()
    {
        var saved = new List<string>();
        await using var saver = new AutoSaver<string>(v => { lock (saved) saved.Add(v); return Task.CompletedTask; }, TimeSpan.FromMilliseconds(100));
        saver.Schedule("a"); saver.Schedule("b"); saver.Schedule("c");
        await Task.Delay(400);
        Assert.Equal(new[] { "c" }, saved);

        saver.Schedule("d");
        await saver.FlushAsync();
        Assert.Equal(new[] { "c", "d" }, saved);
    }
}

