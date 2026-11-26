using System.Text;
using ZeroCool.Core.Catalog;
using ZeroCool.Core.Models;

namespace ZeroCool.Core.Calendar;

public sealed record CalendarTask(
    string PlanId,
    string PlanName,
    string BedId,
    string BedName,
    string PlantingId,
    string PlantId,
    string? VarietyId,
    string DisplayName,
    string Icon,
    CalendarEventType EventType,
    DateOnly StartDate,
    DateOnly EndDate,
    DateOnly? ActualDate,
    string Notes);

/// <summary>Computes start-indoors, direct-sow, transplant, and harvest windows from USDA zone frost dates and variety rules.</summary>
public static class PlantingCalendarEngine
{
    public static IReadOnlyList<CalendarTask> BuildSchedule(
        IReadOnlyList<GardenPlan> plans,
        IReadOnlyList<Plant> catalog,
        AppSettings settings,
        int year)
    {
        var (springFrost, fallFrost) = GrowingZoneCatalog.EffectiveFrostDates(settings, year);
        // For frost-free zones (11–13) or unset zone, use a mild spring reference so schedules still render
        var springAnchor = springFrost ?? new DateOnly(year, 4, 15);
        var fallAnchor = fallFrost ?? new DateOnly(year, 10, 15);

        var tasks = new List<CalendarTask>();

        foreach (var plan in plans)
        {
            foreach (var bed in plan.Beds)
            {
                foreach (var planting in bed.Plantings)
                {
                    var info = PlantResolver.Resolve(catalog, planting.PlantId, planting.VarietyId);
                    if (info is null) continue;

                    DateOnly? transplantStart = null;
                    DateOnly? directSowStart = null;
                    DateOnly? indoorsStart = null;
                    var hasExplicitHarvest = false;

                    foreach (var rule in info.CalendarRules)
                    {
                        var anchor = rule.Anchor == FrostAnchor.FirstFallFrost ? fallAnchor : springAnchor;
                        var s = anchor.AddDays(rule.WeeksOffsetStart * 7);
                        var e = anchor.AddDays(rule.WeeksOffsetEnd * 7);
                        if (e < s) (s, e) = (e, s);

                        if (rule.Event == CalendarEventType.Transplant) transplantStart ??= s;
                        if (rule.Event == CalendarEventType.DirectSow) directSowStart ??= s;
                        if (rule.Event == CalendarEventType.StartIndoors) indoorsStart ??= s;
                        if (rule.Event == CalendarEventType.Harvest) hasExplicitHarvest = true;

                        var note = rule.SuccessionIntervalWeeks is > 0
                            ? $"Repeat every {rule.SuccessionIntervalWeeks} weeks."
                            : "";

                        tasks.Add(new CalendarTask(
                            plan.Id, plan.Name, bed.Id, bed.Name, planting.Id,
                            info.PlantId, info.VarietyId, info.DisplayName, info.Icon,
                            rule.Event, s, e,
                            ActualFor(planting, rule.Event),
                            note));
                    }

                    if (!hasExplicitHarvest && info.DaysToMaturity is > 0)
                    {
                        var baseDate = planting.TransplantedOn
                            ?? planting.PlantedOn
                            ?? transplantStart
                            ?? directSowStart
                            ?? indoorsStart;

                        if (baseDate is DateOnly bd)
                        {
                            var harvestStart = bd.AddDays(info.DaysToMaturity.Value);
                            var harvestEnd = harvestStart.AddDays(14);
                            tasks.Add(new CalendarTask(
                                plan.Id, plan.Name, bed.Id, bed.Name, planting.Id,
                                info.PlantId, info.VarietyId, info.DisplayName, info.Icon,
                                CalendarEventType.Harvest, harvestStart, harvestEnd,
                                planting.HarvestedOn,
                                $"Estimated ~{info.DaysToMaturity.Value} days to maturity."));
                        }
                    }
                }
            }
        }

        return tasks
            .OrderBy(t => t.StartDate)
            .ThenBy(t => t.EventType)
            .ThenBy(t => t.PlanName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(t => t.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static DateOnly? ActualFor(Planting p, CalendarEventType ev) => ev switch
    {
        CalendarEventType.Transplant => p.TransplantedOn,
        CalendarEventType.Harvest => p.HarvestedOn,
        _ => p.PlantedOn
    };

    /// <summary>Generates an RFC 5545 iCalendar (.ics) file for the scheduled garden tasks.</summary>
    public static string ExportIcs(IReadOnlyList<CalendarTask> tasks, string calendarName = "ZeroCool Gardening")
    {
        var sb = new StringBuilder();
        sb.Append("BEGIN:VCALENDAR\r\n");
        sb.Append("VERSION:2.0\r\n");
        sb.Append("PRODID:-//ZeroCoolGardening//Planting Calendar//EN\r\n");
        sb.Append($"X-WR-CALNAME:{EscapeIcs(calendarName)}\r\n");

        var stamp = DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ");
        foreach (var t in tasks)
        {
            var start = t.ActualDate ?? t.StartDate;
            var endExclusive = (t.ActualDate ?? t.EndDate).AddDays(1);
            var summary = $"{FormatEvent(t.EventType)}: {t.DisplayName}";
            var desc = $"Garden: {t.PlanName} | Bed: {t.BedName}" + (string.IsNullOrEmpty(t.Notes) ? "" : $" | {t.Notes}");

            sb.Append("BEGIN:VEVENT\r\n");
            sb.Append($"UID:{t.PlantingId}-{t.EventType}-{start:yyyyMMdd}@zerocoolgardening\r\n");
            sb.Append($"DTSTAMP:{stamp}\r\n");
            sb.Append($"DTSTART;VALUE=DATE:{start:yyyyMMdd}\r\n");
            sb.Append($"DTEND;VALUE=DATE:{endExclusive:yyyyMMdd}\r\n");
            sb.Append($"SUMMARY:{EscapeIcs(summary)}\r\n");
            sb.Append($"DESCRIPTION:{EscapeIcs(desc)}\r\n");
            sb.Append("END:VEVENT\r\n");
        }

        sb.Append("END:VCALENDAR\r\n");
        return sb.ToString();
    }

    public static string FormatEvent(CalendarEventType ev) => ev switch
    {
        CalendarEventType.StartIndoors => "Start indoors",
        CalendarEventType.DirectSow => "Direct sow",
        CalendarEventType.Transplant => "Transplant",
        CalendarEventType.Harvest => "Harvest",
        _ => ev.ToString()
    };

    private static string EscapeIcs(string value) =>
        value.Replace("\\", "\\\\").Replace(";", "\\;").Replace(",", "\\,").Replace("\r\n", "\\n").Replace("\n", "\\n");
}

