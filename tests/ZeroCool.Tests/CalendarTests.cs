using ZeroCool.Core.Calendar;
using ZeroCool.Core.Models;
using ZeroCool.Data;

namespace ZeroCool.Tests;

public class CalendarTests
{
    [Fact]
    public void BuildSchedule_ComputesFrostBasedWindowsAndVarietyHarvestDate()
    {
        var catalog = PlantCatalogRepository.LoadSeed();
        var settings = new AppSettings { UsdaZone = 6 }; // Last spring frost: Apr 15
        var plan = new GardenPlan { Name = "Backyard" };
        var bed = new Bed { Name = "Bed 1" };
        // Cherokee Purple has DaysToMaturity = 85 (overriding Tomato's 75)
        bed.Plantings.Add(new Planting { Id = "p1", PlantId = "tomato", VarietyId = "tomato-cherokee-purple" });
        plan.Beds.Add(bed);

        var tasks = PlantingCalendarEngine.BuildSchedule([plan], catalog, settings, 2026);

        var indoors = Assert.Single(tasks, t => t.EventType == CalendarEventType.StartIndoors);
        Assert.Equal(new DateOnly(2026, 4, 15).AddDays(-56), indoors.StartDate); // -8 weeks
        Assert.Equal("Tomato — Cherokee Purple", indoors.DisplayName);

        var transplant = Assert.Single(tasks, t => t.EventType == CalendarEventType.Transplant);
        Assert.Equal(new DateOnly(2026, 4, 22), transplant.StartDate); // +1 week

        var harvest = Assert.Single(tasks, t => t.EventType == CalendarEventType.Harvest);
        Assert.Equal(new DateOnly(2026, 4, 22).AddDays(85), harvest.StartDate);
    }

    [Fact]
    public void BuildSchedule_ActualTransplantDateShiftsEstimatedHarvest()
    {
        var catalog = PlantCatalogRepository.LoadSeed();
        var settings = new AppSettings { UsdaZone = 6 };
        var plan = new GardenPlan { Name = "Backyard" };
        var bed = new Bed { Name = "Bed 1" };
        bed.Plantings.Add(new Planting
        {
            Id = "p1", PlantId = "tomato", VarietyId = "tomato-roma", // 78 days
            TransplantedOn = new DateOnly(2026, 5, 1)
        });
        plan.Beds.Add(bed);

        var tasks = PlantingCalendarEngine.BuildSchedule([plan], catalog, settings, 2026);
        var harvest = Assert.Single(tasks, t => t.EventType == CalendarEventType.Harvest);
        Assert.Equal(new DateOnly(2026, 5, 1).AddDays(78), harvest.StartDate);
    }

    [Fact]
    public void ExportIcs_ProducesValidCalendarWithEvents()
    {
        var catalog = PlantCatalogRepository.LoadSeed();
        var settings = new AppSettings { UsdaZone = 6 };
        var plan = new GardenPlan { Name = "Kitchen" };
        plan.Beds.Add(new Bed { Name = "Bed 1", Plantings = [new Planting { Id = "c1", PlantId = "carrot" }] });

        var tasks = PlantingCalendarEngine.BuildSchedule([plan], catalog, settings, 2026);
        var ics = PlantingCalendarEngine.ExportIcs(tasks);

        Assert.Contains("BEGIN:VCALENDAR", ics);
        Assert.Contains("BEGIN:VEVENT", ics);
        Assert.Contains("SUMMARY:Direct sow: Carrot", ics);
        Assert.Contains("SUMMARY:Harvest: Carrot", ics);
        Assert.Contains("END:VCALENDAR", ics);
    }
}

