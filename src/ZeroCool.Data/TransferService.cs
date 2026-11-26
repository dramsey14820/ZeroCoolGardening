using System.Text.Json;
using ZeroCool.Core.Models;

namespace ZeroCool.Data;

public sealed class GardenBackup
{
    public int SchemaVersion { get; set; } = 1;
    public DateTimeOffset ExportedUtc { get; set; } = DateTimeOffset.UtcNow;
    public AppSettings Settings { get; set; } = new();
    public UserCatalog UserCatalog { get; set; } = new();
    public List<GardenPlan> Plans { get; set; } = [];
    public List<GardenPlan> Templates { get; set; } = [];
}

/// <summary>JSON import/export for single garden plans and full workspace backups, plus cross-plan plant cleanup.</summary>
public sealed class TransferService(PlanRepository plans, PlantCatalogRepository catalog, SettingsRepository settings)
{
    public string ExportPlanJson(GardenPlan plan) =>
        JsonSerializer.Serialize(plan, JsonDefaults.Options);

    /// <summary>Imports a single GardenPlan JSON (assigns a fresh ID so it never overwrites an existing plan).</summary>
    public async Task<GardenPlan> ImportPlanJsonAsync(string json, bool? forceTemplate = null, CancellationToken ct = default)
    {
        var plan = JsonSerializer.Deserialize<GardenPlan>(json, JsonDefaults.Options)
            ?? throw new InvalidDataException("Invalid garden plan JSON.");
        if (string.IsNullOrWhiteSpace(plan.Name)) plan.Name = "Imported Garden";
        plan.Id = Guid.NewGuid().ToString("N");
        if (forceTemplate is bool t) plan.IsTemplate = t;
        plan.GridWidth = Math.Clamp(plan.GridWidth, 1, 200);
        plan.GridHeight = Math.Clamp(plan.GridHeight, 1, 200);
        await plans.SaveAsync(plan, ct);
        return plan;
    }

    public async Task<string> ExportBackupJsonAsync(CancellationToken ct = default)
    {
        var backup = new GardenBackup
        {
            Settings = await settings.LoadAsync(ct),
            UserCatalog = await catalog.LoadUserAsync(ct),
            Plans = (await plans.ListAsync(templates: false, ct)).ToList(),
            Templates = (await plans.ListAsync(templates: true, ct)).ToList(),
        };
        return JsonSerializer.Serialize(backup, JsonDefaults.Options);
    }

    public async Task<(int PlansImported, int TemplatesImported, int CustomPlantsImported)> ImportBackupJsonAsync(string json, CancellationToken ct = default)
    {
        var backup = JsonSerializer.Deserialize<GardenBackup>(json, JsonDefaults.Options)
            ?? throw new InvalidDataException("Invalid backup JSON.");

        var pCount = 0;
        foreach (var p in backup.Plans)
        {
            p.Id = Guid.NewGuid().ToString("N");
            p.IsTemplate = false;
            await plans.SaveAsync(p, ct);
            pCount++;
        }

        var tCount = 0;
        foreach (var t in backup.Templates)
        {
            t.Id = Guid.NewGuid().ToString("N");
            t.IsTemplate = true;
            await plans.SaveAsync(t, ct);
            tCount++;
        }

        var cCount = 0;
        foreach (var plant in backup.UserCatalog.Plants)
        {
            await catalog.UpsertAsync(plant, ct);
            cCount++;
        }

        return (pCount, tCount, cCount);
    }

    /// <summary>Counts how many placements across all plans use a given plant (and optional variety).</summary>
    public async Task<int> CountPlantUsagesAsync(string plantId, string? varietyId = null, CancellationToken ct = default)
    {
        var allPlans = (await plans.ListAsync(false, ct)).Concat(await plans.ListAsync(true, ct));
        return allPlans
            .SelectMany(p => p.Beds)
            .SelectMany(b => b.Plantings)
            .Count(pl => pl.PlantId == plantId && (varietyId is null || pl.VarietyId == varietyId));
    }

    /// <summary>Removes placements referencing a deleted plant (or variety) across all plans and templates.</summary>
    public async Task<int> RemovePlantUsagesAsync(string plantId, string? varietyId = null, CancellationToken ct = default)
    {
        var total = 0;
        foreach (var isTemplate in new[] { false, true })
        {
            foreach (var plan in await plans.ListAsync(isTemplate, ct))
            {
                var removed = 0;
                foreach (var bed in plan.Beds)
                    removed += bed.Plantings.RemoveAll(pl => pl.PlantId == plantId && (varietyId is null || pl.VarietyId == varietyId));
                if (removed > 0)
                {
                    await plans.SaveAsync(plan, ct);
                    total += removed;
                }
            }
        }
        return total;
    }
}

