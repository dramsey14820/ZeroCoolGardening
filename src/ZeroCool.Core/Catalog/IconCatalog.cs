namespace ZeroCool.Core.Catalog;

/// <summary>Resolves plant/feature icons to bundled SVG paths under ZeroCool.UI/wwwroot/icons/.</summary>
public static class IconCatalog
{
    public const string BasePath = "_content/ZeroCool.UI/icons/";

    private static readonly Dictionary<string, string> ByKey = new(StringComparer.OrdinalIgnoreCase)
    {
        ["tomato"] = "openmoji-svg/1F345-tomato.svg",
        ["🍅"] = "openmoji-svg/1F345-tomato.svg",
        ["carrot"] = "openmoji-svg/1F955-carrot.svg",
        ["🥕"] = "openmoji-svg/1F955-carrot.svg",
        ["basil"] = "openmoji-svg/1F33F-herb.svg",
        ["dill"] = "openmoji-svg/1F33F-herb.svg",
        ["herb"] = "openmoji-svg/1F33F-herb.svg",
        ["🌿"] = "openmoji-svg/1F33F-herb.svg",
        ["marigold"] = "openmoji-svg/1F33C-blossom.svg",
        ["🌼"] = "openmoji-svg/1F33C-blossom.svg",
        ["eggplant"] = "openmoji-svg/1F346-eggplant.svg",
        ["🍆"] = "openmoji-svg/1F346-eggplant.svg",
        ["corn"] = "openmoji-svg/1F33D-ear-of-corn.svg",
        ["🌽"] = "openmoji-svg/1F33D-ear-of-corn.svg",
        ["broccoli"] = "openmoji-svg/1F966-broccoli.svg",
        ["🥦"] = "openmoji-svg/1F966-broccoli.svg",
        ["potato"] = "openmoji-svg/1F954-potato.svg",
        ["🥔"] = "openmoji-svg/1F954-potato.svg",
        ["cucumber"] = "openmoji-svg/1F952-cucumber.svg",
        ["🥒"] = "openmoji-svg/1F952-cucumber.svg",
        ["lettuce"] = "openmoji-svg/1F96C-leafy-green.svg",
        ["spinach"] = "openmoji-svg/1F96C-leafy-green.svg",
        ["kale"] = "openmoji-svg/1F96C-leafy-green.svg",
        ["🥬"] = "openmoji-svg/1F96C-leafy-green.svg",
        ["pepper"] = "openmoji-svg/1FAD1-bell-pepper.svg",
        ["🫑"] = "openmoji-svg/1FAD1-bell-pepper.svg",
        ["hot-pepper"] = "openmoji-svg/1F336-hot-pepper.svg",
        ["🌶️"] = "openmoji-svg/1F336-hot-pepper.svg",
        ["garlic"] = "openmoji-svg/1F9C4-garlic.svg",
        ["🧄"] = "openmoji-svg/1F9C4-garlic.svg",
        ["onion"] = "openmoji-svg/1F9C5-onion.svg",
        ["🧅"] = "openmoji-svg/1F9C5-onion.svg",
        ["strawberry"] = "openmoji-svg/1F353-strawberry.svg",
        ["🍓"] = "openmoji-svg/1F353-strawberry.svg",
        ["grape"] = "openmoji-svg/1F347-grapes.svg",
        ["🍇"] = "openmoji-svg/1F347-grapes.svg",
        ["mushroom"] = "openmoji-svg/1F344-mushroom.svg",
        ["🍄"] = "openmoji-svg/1F344-mushroom.svg",
        ["rose"] = "openmoji-svg/1F339-rose.svg",
        ["🌹"] = "openmoji-svg/1F339-rose.svg",
        ["sunflower"] = "openmoji-svg/1F33B-sunflower.svg",
        ["🌻"] = "openmoji-svg/1F33B-sunflower.svg",
        ["tulip"] = "openmoji-svg/1F337-tulip.svg",
        ["🌷"] = "openmoji-svg/1F337-tulip.svg",
        ["hibiscus"] = "openmoji-svg/1F33A-hibiscus.svg",
        ["🌺"] = "openmoji-svg/1F33A-hibiscus.svg",
        ["clover"] = "openmoji-svg/1F340-four-leaf-clover.svg",
        ["🍀"] = "openmoji-svg/1F340-four-leaf-clover.svg",
        ["seedling"] = "openmoji-svg/1F331-seedling.svg",
        ["🌱"] = "openmoji-svg/1F331-seedling.svg",
        ["cabbage"] = "game-icons/cabbage.svg",
        ["leek"] = "game-icons/leek.svg",
        ["paver"] = "game-icons/stone-pile.svg",
        ["fence"] = "iconmonks-svg/fence-fill.svg",
        ["greenhouse"] = "iconmonks-svg/greenhouse-fill.svg",
        ["sprinkler"] = "iconmonks-svg/sprinkler-fill.svg",
        ["birdhouse"] = "iconmonks-svg/birdhouse-fill.svg",
    };

    /// <summary>Returns the static web asset URL for a bundled SVG icon, or null if none matches.</summary>
    public static string? ResolveSvgUrl(string? icon, string? fallbackKey = null)
    {
        if (!string.IsNullOrWhiteSpace(icon))
        {
            if (icon.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
                return icon.StartsWith(BasePath, StringComparison.OrdinalIgnoreCase) ? icon : BasePath + icon.TrimStart('/');
            if (ByKey.TryGetValue(icon.Trim(), out var rel))
                return BasePath + rel;
        }
        if (!string.IsNullOrWhiteSpace(fallbackKey) && ByKey.TryGetValue(fallbackKey.Trim(), out var relByKey))
            return BasePath + relByKey;
        return null;
    }
}

