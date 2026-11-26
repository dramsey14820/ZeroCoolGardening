using ZeroCool.Core.Models;
using ZeroCool.Data;

namespace ZeroCool.Tests;

public class M6Tests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "zc-m6-" + Guid.NewGuid().ToString("N"));
    private LocalFileStore Store => new(_dir);

    public void Dispose() { if (Directory.Exists(_dir)) Directory.Delete(_dir, true); }

    [Fact]
    public async Task BuiltInTemplates_CanBeClonedIntoNewPlan()
    {
        var repo = new PlanRepository(Store);
        var tpl = BuiltInTemplates.All[0];
        Assert.True(tpl.IsTemplate);
        Assert.NotEmpty(tpl.Beds);

        var plan = await repo.CloneAsync(tpl, asTemplate: false, "My Kitchen Garden");
        Assert.False(plan.IsTemplate);
        Assert.Equal("My Kitchen Garden", plan.Name);
        Assert.Equal(tpl.Beds.Count, plan.Beds.Count);
        Assert.Single(await repo.ListAsync(templates: false));
    }

    [Fact]
    public async Task TransferService_PlanExportImportRoundTrip()
    {
        var store = Store;
        var plans = new PlanRepository(store);
        var catalog = new PlantCatalogRepository(store);
        var settings = new SettingsRepository(store);
        var transfer = new TransferService(plans, catalog, settings);

        var original = new GardenPlan { Name = "Export Me", GridWidth = 14, GridHeight = 9 };
        original.Beds.Add(new Bed { Name = "Raised 1", Width = 4, Height = 4, Soil = "Loam" });

        var json = transfer.ExportPlanJson(original);
        var imported = await transfer.ImportPlanJsonAsync(json);

        Assert.NotEqual(original.Id, imported.Id);
        Assert.Equal("Export Me", imported.Name);
        Assert.Equal(14, imported.GridWidth);
        Assert.Single(imported.Beds);
        Assert.Single(await plans.ListAsync(false));
    }

    [Fact]
    public async Task TransferService_BackupExportImport_And_PlantUsageCleanup()
    {
        var store = Store;
        var plans = new PlanRepository(store);
        var catalog = new PlantCatalogRepository(store);
        var settings = new SettingsRepository(store);
        var transfer = new TransferService(plans, catalog, settings);

        var plan = new GardenPlan { Name = "Backyard" };
        var bed = new Bed { Name = "Bed 1" };
        bed.Plantings.Add(new Planting { PlantId = "tomato", VarietyId = "tomato-roma" });
        bed.Plantings.Add(new Planting { PlantId = "basil" });
        plan.Beds.Add(bed);
        await plans.SaveAsync(plan);

        Assert.Equal(1, await transfer.CountPlantUsagesAsync("tomato"));
        Assert.Equal(1, await transfer.CountPlantUsagesAsync("tomato", "tomato-roma"));

        var backupJson = await transfer.ExportBackupJsonAsync();
        Assert.Contains("Backyard", backupJson);

        var removed = await transfer.RemovePlantUsagesAsync("tomato");
        Assert.Equal(1, removed);
        Assert.Equal(0, await transfer.CountPlantUsagesAsync("tomato"));

        var (pCount, _, _) = await transfer.ImportBackupJsonAsync(backupJson);
        Assert.Equal(1, pCount);
        Assert.Equal(2, (await plans.ListAsync(false)).Count);
    }
}

