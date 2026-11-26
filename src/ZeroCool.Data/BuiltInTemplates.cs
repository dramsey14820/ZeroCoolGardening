using ZeroCool.Core.Layout;
using ZeroCool.Core.Models;

namespace ZeroCool.Data;

/// <summary>Starter garden templates users can clone to start a new plan immediately.</summary>
public static class BuiltInTemplates
{
    public static IReadOnlyList<GardenPlan> All { get; } =
    [
        new GardenPlan
        {
            Id = "tpl-kitchen-sf",
            Name = "Starter Kitchen Square-Foot Garden",
            IsTemplate = true,
            GridWidth = 12,
            GridHeight = 10,
            Beds =
            [
                new Bed
                {
                    Id = "tpl-bed-1", Name = "Main Raised Bed", Type = BedType.SquareFoot,
                    X = 1, Y = 1, Width = 4, Height = 4, Soil = "Raised-bed mix", Fertilizer = "Compost",
                    Plantings =
                    [
                        new Planting { PlantId = "tomato", VarietyId = "tomato-roma", X = 0, Y = 0, SpanWidth = 2, SpanHeight = 2, Count = 1 },
                        new Planting { PlantId = "basil", VarietyId = "basil-genovese", X = 2, Y = 0, SpanWidth = 1, SpanHeight = 1, Count = 1 },
                        new Planting { PlantId = "marigold", VarietyId = "marigold-french", X = 2, Y = 1, SpanWidth = 1, SpanHeight = 1, Count = 1 },
                        new Planting { PlantId = "carrot", VarietyId = "carrot-nantes", X = 0, Y = 2, SpanWidth = 1, SpanHeight = 1, Count = 16 },
                    ]
                },
                new Bed
                {
                    Id = "tpl-bed-2", Name = "Herb Bed", Type = BedType.SquareFoot,
                    X = 7, Y = 1, Width = 4, Height = 4, Soil = "Loam", Fertilizer = "Compost"
                }
            ],
            Features =
            [
                new GardenFeature { Kind = "paver", X = 5, Y = 2, Width = 2, Height = 1 },
                new GardenFeature { Kind = "paver", X = 5, Y = 4, Width = 2, Height = 1 },
            ]
        },
        new GardenPlan
        {
            Id = "tpl-row-backyard",
            Name = "Backyard Row Garden",
            IsTemplate = true,
            GridWidth = 16,
            GridHeight = 12,
            Beds =
            [
                new Bed
                {
                    Id = "tpl-row-1", Name = "Veggie Rows", Type = BedType.Row,
                    X = 1, Y = 1, Width = 10, Height = 6, Soil = "Loam", Fertilizer = "Aged manure",
                    RowOrientation = RowOrientation.Horizontal, BetweenRowsInches = 18, InRowSpacingInches = 12,
                    Plantings =
                    [
                        new Planting { PlantId = "carrot", VarietyId = "carrot-nantes", X = 0, Y = 0, Count = 10 },
                        new Planting { PlantId = "tomato", VarietyId = "tomato-cherokee-purple", X = 0, Y = 1, Count = 10 },
                        new Planting { PlantId = "basil", VarietyId = "basil-genovese", X = 0, Y = 2, Count = 10 },
                    ]
                }
            ]
        },
        new GardenPlan
        {
            Id = "tpl-patio-containers",
            Name = "Patio Container Trio",
            IsTemplate = true,
            GridWidth = 10,
            GridHeight = 6,
            Beds =
            [
                new Bed
                {
                    Id = "tpl-pot-1", Name = "Tomato Pot", Type = BedType.Container,
                    X = 1, Y = 2, Width = 2, Height = 2, Soil = "Raised-bed mix",
                    ContainerShape = ContainerShape.GrowBag, ContainerVolumeLiters = ContainerGuidance.FromGallons(10),
                    Plantings = [new Planting { PlantId = "tomato", VarietyId = "tomato-roma", Count = 1 }]
                },
                new Bed
                {
                    Id = "tpl-pot-2", Name = "Basil Pot", Type = BedType.Container,
                    X = 4, Y = 2, Width = 2, Height = 2, Soil = "Raised-bed mix",
                    ContainerShape = ContainerShape.Round, ContainerVolumeLiters = ContainerGuidance.FromGallons(3),
                    Plantings = [new Planting { PlantId = "basil", VarietyId = "basil-genovese", Count = 1 }]
                },
                new Bed
                {
                    Id = "tpl-pot-3", Name = "Marigold Box", Type = BedType.Container,
                    X = 7, Y = 2, Width = 2, Height = 2, Soil = "Loam",
                    ContainerShape = ContainerShape.Rectangular, ContainerVolumeLiters = ContainerGuidance.FromGallons(3),
                    Plantings = [new Planting { PlantId = "marigold", VarietyId = "marigold-french", Count = 1 }]
                }
            ]
        }
    ];
}

