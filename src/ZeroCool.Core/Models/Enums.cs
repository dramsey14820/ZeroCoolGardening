namespace ZeroCool.Core.Models;

public enum BedType { Row, SquareFoot, Container }

public enum RowOrientation { Horizontal, Vertical }

public enum ContainerShape { Round, Rectangular, GrowBag }

public enum PlantCategory { Vegetable, Herb, Fruit, Flower, Ornamental, Groundcover, Vine }

public enum WarningType { ToxicToPets, ToxicToHumans, ToxicToChildren, SkinIrritant, Allergen, Invasive, Thorny, UnsafeParts, Other }

public enum WarningSeverity { Info, Caution, Danger }

public enum UnitSystem { Imperial, Metric }

public enum CalendarEventType { StartIndoors, DirectSow, Transplant, Harvest }

/// <summary>Anchor for a calendar rule offset.</summary>
public enum FrostAnchor { LastSpringFrost, FirstFallFrost }

