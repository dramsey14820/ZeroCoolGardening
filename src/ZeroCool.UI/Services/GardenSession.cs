using ZeroCool.Core.Models;
using ZeroCool.Core.Storage;
using ZeroCool.Data;

namespace ZeroCool.UI.Services;

/// <summary>Holds the plan being edited, applies mutations and autosaves after every change.</summary>
public sealed class GardenSession(PlanRepository plans, PlantCatalogRepository catalog, SettingsRepository settings) : IAsyncDisposable
{
    private AutoSaver<GardenPlan>? _saver;

    public GardenPlan? Plan { get; private set; }
    public IReadOnlyList<Plant> Plants { get; private set; } = Array.Empty<Plant>();
    public AppSettings Settings { get; private set; } = new();
    public string StatusText { get; private set; } = "";
    public event Action? Changed;

    public async Task<bool> LoadAsync(string id)
    {
        if (_saver is not null) await _saver.FlushAsync();
        Plan = await plans.GetAsync(id, template: false) ?? await plans.GetAsync(id, template: true);
        Plants = await catalog.GetAllAsync();
        Settings = await settings.LoadAsync();
        _saver = new AutoSaver<GardenPlan>(async p =>
        {
            await plans.SaveAsync(p);
            StatusText = "Saved";
            Changed?.Invoke();
        });
        _saver.SaveFailed += _ => { StatusText = "Save failed"; Changed?.Invoke(); };
        StatusText = Plan is null ? "" : "Saved";
        return Plan is not null;
    }

    /// <summary>Apply a change to the plan; it is autosaved shortly after.</summary>
    public void Mutate(Action<GardenPlan> change)
    {
        if (Plan is null || _saver is null) return;
        change(Plan);
        StatusText = "Saving…";
        _saver.Schedule(Plan);
        Changed?.Invoke();
    }

    public Task FlushAsync() => _saver?.FlushAsync() ?? Task.CompletedTask;

    public async Task SaveSettingsAsync(AppSettings updated)
    {
        await settings.SaveAsync(updated);
        Settings = updated;
        Changed?.Invoke();
    }

    public async ValueTask DisposeAsync()
    {
        if (_saver is not null) await _saver.DisposeAsync();
    }
}

