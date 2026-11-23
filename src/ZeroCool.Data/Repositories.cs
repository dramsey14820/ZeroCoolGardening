using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using ZeroCool.Core.Models;
using ZeroCool.Core.Storage;

namespace ZeroCool.Data;

public static class JsonDefaults
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };
}

public sealed class SettingsRepository(IFileStore store)
{
    private const string Path = "settings.json";

    public async Task<AppSettings> LoadAsync(CancellationToken ct = default)
    {
        var json = await store.ReadAsync(Path, ct);
        return json is null ? new AppSettings() : JsonSerializer.Deserialize<AppSettings>(json, JsonDefaults.Options) ?? new AppSettings();
    }

    public Task SaveAsync(AppSettings settings, CancellationToken ct = default) =>
        store.WriteAsync(Path, JsonSerializer.Serialize(settings, JsonDefaults.Options), ct);
}

/// <summary>Stores garden plans in plans/ and templates in templates/.</summary>
public sealed class PlanRepository(IFileStore store)
{
    private static string Folder(bool template) => template ? "templates" : "plans";
    private static string PathFor(string id, bool template) => $"{Folder(template)}/{SafeId(id)}.json";

    private static string SafeId(string id)
    {
        if (string.IsNullOrWhiteSpace(id) || id.Any(c => !(char.IsLetterOrDigit(c) || c is '-' or '_')))
            throw new ArgumentException("Invalid id.", nameof(id));
        return id;
    }

    public async Task<IReadOnlyList<GardenPlan>> ListAsync(bool templates, CancellationToken ct = default)
    {
        var result = new List<GardenPlan>();
        foreach (var file in await store.ListAsync(Folder(templates), ct))
        {
            var json = await store.ReadAsync(file, ct);
            if (json is null) continue;
            var plan = JsonSerializer.Deserialize<GardenPlan>(json, JsonDefaults.Options);
            if (plan is not null) result.Add(plan);
        }
        return result.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public async Task<GardenPlan?> GetAsync(string id, bool template, CancellationToken ct = default)
    {
        var json = await store.ReadAsync(PathFor(id, template), ct);
        return json is null ? null : JsonSerializer.Deserialize<GardenPlan>(json, JsonDefaults.Options);
    }

    public Task SaveAsync(GardenPlan plan, CancellationToken ct = default)
    {
        plan.ModifiedUtc = DateTimeOffset.UtcNow;
        return store.WriteAsync(PathFor(plan.Id, plan.IsTemplate), JsonSerializer.Serialize(plan, JsonDefaults.Options), ct);
    }

    public Task<bool> DeleteAsync(string id, bool template, CancellationToken ct = default) =>
        store.DeleteAsync(PathFor(id, template), ct);

    /// <summary>Copy a plan into a template (or a template into a new plan) with a new id.</summary>
    public async Task<GardenPlan> CloneAsync(GardenPlan source, bool asTemplate, string? newName = null, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(source, JsonDefaults.Options);
        var copy = JsonSerializer.Deserialize<GardenPlan>(json, JsonDefaults.Options)!;
        copy.Id = Guid.NewGuid().ToString("N");
        copy.IsTemplate = asTemplate;
        copy.Name = newName ?? source.Name;
        await SaveAsync(copy, ct);
        return copy;
    }
}

/// <summary>Built-in seed plants (embedded resource) merged with the user's edits in catalog.user.json.</summary>
public sealed class PlantCatalogRepository(IFileStore store)
{
    private const string UserPath = "catalog.user.json";

    public static List<Plant> LoadSeed()
    {
        var asm = typeof(PlantCatalogRepository).Assembly;
        var name = asm.GetManifestResourceNames().Single(n => n.EndsWith("plants.seed.json", StringComparison.OrdinalIgnoreCase));
        using var s = asm.GetManifestResourceStream(name)!;
        return JsonSerializer.Deserialize<List<Plant>>(s, JsonDefaults.Options) ?? new();
    }

    public async Task<UserCatalog> LoadUserAsync(CancellationToken ct = default)
    {
        var json = await store.ReadAsync(UserPath, ct);
        return json is null ? new UserCatalog() : JsonSerializer.Deserialize<UserCatalog>(json, JsonDefaults.Options) ?? new UserCatalog();
    }

    private Task SaveUserAsync(UserCatalog c, CancellationToken ct) =>
        store.WriteAsync(UserPath, JsonSerializer.Serialize(c, JsonDefaults.Options), ct);

    /// <summary>Effective catalog: seed minus hidden, with user plants overriding by id or added.</summary>
    public async Task<IReadOnlyList<Plant>> GetAllAsync(CancellationToken ct = default)
    {
        var user = await LoadUserAsync(ct);
        var map = LoadSeed().Where(p => !user.HiddenPlantIds.Contains(p.Id)).ToDictionary(p => p.Id);
        foreach (var p in user.Plants) map[p.Id] = p;
        return map.Values.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>Add or edit a plant (including its varieties). Edits of built-ins are stored as overrides.</summary>
    public async Task UpsertAsync(Plant plant, CancellationToken ct = default)
    {
        var user = await LoadUserAsync(ct);
        user.Plants.RemoveAll(p => p.Id == plant.Id);
        user.Plants.Add(plant);
        user.HiddenPlantIds.Remove(plant.Id);
        await SaveUserAsync(user, ct);
    }

    /// <summary>Delete a user plant, or hide a built-in plant.</summary>
    public async Task DeleteAsync(string plantId, CancellationToken ct = default)
    {
        var user = await LoadUserAsync(ct);
        user.Plants.RemoveAll(p => p.Id == plantId);
        if (LoadSeed().Any(p => p.Id == plantId) && !user.HiddenPlantIds.Contains(plantId))
            user.HiddenPlantIds.Add(plantId);
        await SaveUserAsync(user, ct);
    }

    /// <summary>Restore a built-in plant to its original data (drops override and un-hides).</summary>
    public async Task ResetToDefaultAsync(string plantId, CancellationToken ct = default)
    {
        var user = await LoadUserAsync(ct);
        user.Plants.RemoveAll(p => p.Id == plantId && LoadSeed().Any(s => s.Id == plantId));
        user.HiddenPlantIds.Remove(plantId);
        await SaveUserAsync(user, ct);
    }
}

