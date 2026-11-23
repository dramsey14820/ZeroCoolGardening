namespace ZeroCool.Core.Models;

/// <summary>Spacing from a seed packet / guide. Values in inches.</summary>
public class Spacing
{
    public double? InRowInches { get; set; }
    public double? BetweenRowsInches { get; set; }
    /// <summary>Plants per 1 sq ft cell in square-foot gardening (1, 4, 9, 16...).</summary>
    public int? PlantsPerSquareFoot { get; set; }
    /// <summary>Cells (columns x rows) this plant occupies in square-foot beds.</summary>
    public int CellSpanWidth { get; set; } = 1;
    public int CellSpanHeight { get; set; } = 1;
}

public class PlantWarning
{
    public WarningType Type { get; set; }
    public WarningSeverity Severity { get; set; } = WarningSeverity.Caution;
    public string Text { get; set; } = "";
}

/// <summary>Timing of a calendar event in weeks relative to a frost date (negative = before).</summary>
public class CalendarRule
{
    public CalendarEventType Event { get; set; }
    public FrostAnchor Anchor { get; set; } = FrostAnchor.LastSpringFrost;
    public int WeeksOffsetStart { get; set; }
    public int WeeksOffsetEnd { get; set; }
    public int? SuccessionIntervalWeeks { get; set; }
}

/// <summary>Fields shared by Plant and Variety. On a Variety, null/empty means "inherit from Plant".</summary>
public class PlantInfoBase
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "";
    public string? Icon { get; set; }
    public string? Description { get; set; }
    public int? DaysToMaturity { get; set; }
    public int? DaysToGermination { get; set; }
    public string? Sun { get; set; }
    public string? Water { get; set; }
    public List<int>? Zones { get; set; }
    public Spacing? Spacing { get; set; }
    public List<string>? HealthBenefits { get; set; }
    public List<PlantWarning>? Warnings { get; set; }
    public List<CalendarRule>? CalendarRules { get; set; }
    public List<string>? Sources { get; set; }
    public string? Notes { get; set; }
    public bool IsUserDefined { get; set; }
}

public class Variety : PlantInfoBase { }

public class Plant : PlantInfoBase
{
    public PlantCategory Category { get; set; }
    public bool Edible { get; set; }
    public string? EdibleParts { get; set; }
    public List<string> PreferredSoils { get; set; } = new();
    public List<string> Companions { get; set; } = new();
    public List<string> Antagonists { get; set; } = new();
    public List<Variety> Varieties { get; set; } = new();
}

