namespace ZeroCool.Core.Catalog;

/// <summary>Resolves plant/feature icons to bundled SVG paths under ZeroCool.UI/wwwroot/icons/.</summary>
public static class IconCatalog
{
    public const string BasePath = "_content/ZeroCool.UI/icons/";

    private static readonly Dictionary<string, string> ByKey = new(StringComparer.OrdinalIgnoreCase)
    {
        // Vegetables
        ["tomato"] = "openmoji-svg/1F345-tomato.svg",
        ["🍅"] = "openmoji-svg/1F345-tomato.svg",
        ["bell-pepper"] = "openmoji-svg/1FAD1-bell-pepper.svg",
        ["pepper"] = "openmoji-svg/1FAD1-bell-pepper.svg",
        ["🫑"] = "openmoji-svg/1FAD1-bell-pepper.svg",
        ["hot-pepper"] = "openmoji-svg/1F336-hot-pepper.svg",
        ["🌶️"] = "openmoji-svg/1F336-hot-pepper.svg",
        ["carrot"] = "openmoji-svg/1F955-carrot.svg",
        ["🥕"] = "openmoji-svg/1F955-carrot.svg",
        ["radish"] = "openmoji-svg/1F955-carrot.svg",
        ["beet"] = "openmoji-svg/1F955-carrot.svg",
        ["turnip"] = "openmoji-svg/1F955-carrot.svg",
        ["lettuce"] = "openmoji-svg/1F96C-leafy-green.svg",
        ["spinach"] = "openmoji-svg/1F96C-leafy-green.svg",
        ["kale"] = "openmoji-svg/1F96C-leafy-green.svg",
        ["swiss-chard"] = "openmoji-svg/1F96C-leafy-green.svg",
        ["celery"] = "openmoji-svg/1F96C-leafy-green.svg",
        ["arugula"] = "openmoji-svg/1F96C-leafy-green.svg",
        ["🥬"] = "openmoji-svg/1F96C-leafy-green.svg",
        ["broccoli"] = "openmoji-svg/1F966-broccoli.svg",
        ["cauliflower"] = "openmoji-svg/1F966-broccoli.svg",
        ["brussels-sprouts"] = "openmoji-svg/1F966-broccoli.svg",
        ["🥦"] = "openmoji-svg/1F966-broccoli.svg",
        ["cabbage"] = "game-icons/cabbage.svg",
        ["cucumber"] = "openmoji-svg/1F952-cucumber.svg",
        ["zucchini"] = "openmoji-svg/1F952-cucumber.svg",
        ["winter-squash"] = "openmoji-svg/1F952-cucumber.svg",
        ["okra"] = "openmoji-svg/1F952-cucumber.svg",
        ["🥒"] = "openmoji-svg/1F952-cucumber.svg",
        ["eggplant"] = "openmoji-svg/1F346-eggplant.svg",
        ["🍆"] = "openmoji-svg/1F346-eggplant.svg",
        ["sweet-corn"] = "openmoji-svg/1F33D-ear-of-corn.svg",
        ["corn"] = "openmoji-svg/1F33D-ear-of-corn.svg",
        ["🌽"] = "openmoji-svg/1F33D-ear-of-corn.svg",
        ["potato"] = "openmoji-svg/1F954-potato.svg",
        ["sweet-potato"] = "openmoji-svg/1F954-potato.svg",
        ["🥔"] = "openmoji-svg/1F954-potato.svg",
        ["onion"] = "openmoji-svg/1F9C5-onion.svg",
        ["🧅"] = "openmoji-svg/1F9C5-onion.svg",
        ["garlic"] = "openmoji-svg/1F9C4-garlic.svg",
        ["🧄"] = "openmoji-svg/1F9C4-garlic.svg",
        ["leek"] = "game-icons/leek.svg",
        ["bush-bean"] = "openmoji-svg/1F331-seedling.svg",
        ["pole-bean"] = "openmoji-svg/1F331-seedling.svg",
        ["pea"] = "openmoji-svg/1F331-seedling.svg",
        ["asparagus"] = "openmoji-svg/1F331-seedling.svg",
        ["seedling"] = "openmoji-svg/1F331-seedling.svg",
        ["🌱"] = "openmoji-svg/1F331-seedling.svg",

        // Herbs
        ["basil"] = "openmoji-svg/1F33F-herb.svg",
        ["dill"] = "openmoji-svg/1F33F-herb.svg",
        ["parsley"] = "openmoji-svg/1F33F-herb.svg",
        ["cilantro"] = "openmoji-svg/1F33F-herb.svg",
        ["mint"] = "openmoji-svg/1F33F-herb.svg",
        ["rosemary"] = "openmoji-svg/1F33F-herb.svg",
        ["thyme"] = "openmoji-svg/1F33F-herb.svg",
        ["oregano"] = "openmoji-svg/1F33F-herb.svg",
        ["chives"] = "openmoji-svg/1F33F-herb.svg",
        ["sage"] = "openmoji-svg/1F33F-herb.svg",
        ["herb"] = "openmoji-svg/1F33F-herb.svg",
        ["🌿"] = "openmoji-svg/1F33F-herb.svg",

        // Fruits & Berries (non-tree)
        ["strawberry"] = "openmoji-svg/1F353-strawberry.svg",
        ["raspberry"] = "openmoji-svg/1F353-strawberry.svg",
        ["blackberry"] = "openmoji-svg/1F353-strawberry.svg",
        ["🍓"] = "openmoji-svg/1F353-strawberry.svg",
        ["blueberry"] = "openmoji-svg/1F347-grapes.svg",
        ["grape"] = "openmoji-svg/1F347-grapes.svg",
        ["🍇"] = "openmoji-svg/1F347-grapes.svg",
        ["watermelon"] = "openmoji-svg/1F952-cucumber.svg",
        ["cantaloupe"] = "openmoji-svg/1F952-cucumber.svg",
        ["mushroom"] = "openmoji-svg/1F344-mushroom.svg",
        ["🍄"] = "openmoji-svg/1F344-mushroom.svg",

        // Flowers
        ["marigold"] = "openmoji-svg/1F33C-blossom.svg",
        ["nasturtium"] = "openmoji-svg/1F33C-blossom.svg",
        ["cosmos"] = "openmoji-svg/1F33C-blossom.svg",
        ["🌼"] = "openmoji-svg/1F33C-blossom.svg",
        ["sunflower"] = "openmoji-svg/1F33B-sunflower.svg",
        ["🌻"] = "openmoji-svg/1F33B-sunflower.svg",
        ["rose"] = "openmoji-svg/1F339-rose.svg",
        ["peony"] = "openmoji-svg/1F339-rose.svg",
        ["🌹"] = "openmoji-svg/1F339-rose.svg",
        ["tulip"] = "openmoji-svg/1F337-tulip.svg",
        ["🌷"] = "openmoji-svg/1F337-tulip.svg",
        ["hibiscus"] = "openmoji-svg/1F33A-hibiscus.svg",
        ["zinnia"] = "openmoji-svg/1F33A-hibiscus.svg",
        ["echinacea"] = "openmoji-svg/1F33A-hibiscus.svg",
        ["🌺"] = "openmoji-svg/1F33A-hibiscus.svg",
        ["lavender"] = "iconmonks-svg/flower-fill.svg",
        ["clematis"] = "iconmonks-svg/flower-lotus-fill.svg",
        ["sweet-pea"] = "iconmonks-svg/flower-fill.svg",

        // Ornamentals & Groundcovers
        ["hosta"] = "iconmonks-svg/bush-fill.svg",
        ["fountain-grass"] = "game-icons/grass.svg",
        ["creeping-thyme"] = "openmoji-svg/1F340-four-leaf-clover.svg",
        ["white-clover"] = "openmoji-svg/1F340-four-leaf-clover.svg",
        ["sweet-alyssum"] = "openmoji-svg/1F338-cherry-blossom.svg",
        ["clover"] = "openmoji-svg/1F340-four-leaf-clover.svg",
        ["🍀"] = "openmoji-svg/1F340-four-leaf-clover.svg",

        // Yard features
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
