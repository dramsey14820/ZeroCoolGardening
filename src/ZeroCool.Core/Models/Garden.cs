namespace ZeroCool.Core.Models;

public class AppSettings
{
    public int SchemaVersion { get; set; } = 1;
    public int? UsdaZone { get; set; }
    public UnitSystem Units { get; set; } = UnitSystem.Imperial;
    public DateOnly? LastSpringFrostOverride { get; set; }
    public DateOnly? FirstFallFrostOverride { get; set; }
}

public class GridCell
{
    public int X { get; set; }
    public int Y { get; set; }
}

public class Planting
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string PlantId { get; set; } = "";
    public string? VarietyId { get; set; }
    /// <summary>Bed-local cell/position (square-foot: cell column/row; row beds: row index/slot).</summary>
    public int X { get; set; }
    public int Y { get; set; }
    public int SpanWidth { get; set; } = 1;
    public int SpanHeight { get; set; } = 1;
    public int? Count { get; set; }
    public DateOnly? PlantedOn { get; set; }
    public DateOnly? TransplantedOn { get; set; }
    public DateOnly? HarvestedOn { get; set; }
}

public class Bed
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "";
    public BedType Type { get; set; } = BedType.SquareFoot;
    /// <summary>Position and size on the yard grid, in cells (1 cell = 1 ft).</summary>
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; } = 4;
    public int Height { get; set; } = 4;
    public int RotationDegrees { get; set; }
    public string? Soil { get; set; }
    public string? Fertilizer { get; set; }
    public double? ContainerVolumeLiters { get; set; }
    public List<Planting> Plantings { get; set; } = new();
}

public class GardenFeature
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Kind { get; set; } = "paver";
    public string? Icon { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; } = 1;
    public int Height { get; set; } = 1;
}

public class GardenPlan
{
    public int SchemaVersion { get; set; } = 1;
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "New Garden";
    public bool IsTemplate { get; set; }
    public int GridWidth { get; set; } = 20;
    public int GridHeight { get; set; } = 20;
    /// <summary>Cells that are part of the yard. Null = the full rectangle; an empty list = no active cells.</summary>
    public List<GridCell>? ActiveCells { get; set; }
    public List<Bed> Beds { get; set; } = new();
    public List<GardenFeature> Features { get; set; } = new();
    public DateTimeOffset ModifiedUtc { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>User edits to the plant catalog, layered over the built-in seed data.</summary>
public class UserCatalog
{
    public int SchemaVersion { get; set; } = 1;
    public List<Plant> Plants { get; set; } = new();
    /// <summary>Ids of built-in plants the user deleted (hidden).</summary>
    public List<string> HiddenPlantIds { get; set; } = new();
}

