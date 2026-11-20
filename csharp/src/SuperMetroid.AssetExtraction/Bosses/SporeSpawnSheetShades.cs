using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.AssetExtraction.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// The shades of Spore Spawn (#1512), which the import pre-colours his sheets, the objects he spawns and his room's
/// pictures in: his spores' colours over the row his initialization loads them in, his body's health stages and death
/// fade over the row he draws in, and his death fade over the room's level and background colour groups.
/// </summary>
internal static class SporeSpawnSheetShades
{
    /// <summary>The sprite row his body and the objects he spawns draw in.</summary>
    private const int BodyRow = 1;

    /// <summary>The sprite row his initialization loads his spores' colours into.</summary>
    private const int SporesRow = 7;

    /// <summary>The first ink of a room picture's third tile colour group: the room's level, which his death fades.</summary>
    private const int LevelFirstInk = 64;

    /// <summary>The first ink of a room picture's sixth tile colour group: the room's background, which his death fades.</summary>
    private const int BackgroundFirstInk = 112;

    /// <summary>The shades of his sprites and the objects he spawns.</summary>
    internal static IReadOnlyList<SheetShade> Sprites(BossSourceArtwork bosses)
    {
        SporeSpawnColorCatalog colours = Colours(bosses);
        var shades = new List<SheetShade> { SheetShade.Run(SporeSpawnShades.Spores, SporesRow * LoadedSetColours.RowColours, Frame(colours.ResolveSpore)) };
        foreach (SporeSpawnHealthBand band in SporeSpawnHealthBandDefinitions.HighestFirst)
        {
            shades.Add(SheetShade.Run(SporeSpawnShades.Health(band), BodyRow * LoadedSetColours.RowColours,
                Frame(colour => colours.ResolveHealth(band, colour))));
        }
        foreach (SporeSpawnDeathFadeStep step in SporeSpawnDeathFadeStep.All)
            shades.Add(SheetShade.Run(SporeSpawnShades.DeathSprite(step), BodyRow * LoadedSetColours.RowColours, Frame(colour => colours.ResolveDeathSprite(step.Step, colour))));
        return shades;
    }

    /// <summary>The shades of his room's pictures: each step of his death fade over its level and background.</summary>
    internal static IReadOnlyList<SheetShade> Room(BossSourceArtwork bosses)
    {
        SporeSpawnColorCatalog colours = Colours(bosses);
        var shades = new List<SheetShade>();
        foreach (SporeSpawnDeathFadeStep step in SporeSpawnDeathFadeStep.All)
        {
            shades.Add(SheetShade.Run(SporeSpawnShades.DeathLevel(step), LevelFirstInk, Frame(colour => colours.ResolveDeathLevel(step.Step, colour))));
            shades.Add(SheetShade.Run(SporeSpawnShades.DeathBackground(step), BackgroundFirstInk, Frame(colour => colours.ResolveDeathBackground(step.Step, colour))));
        }
        return shades;
    }

    /// <summary>His installed colours.</summary>
    private static SporeSpawnColorCatalog Colours(BossSourceArtwork bosses) =>
        bosses.SporeSpawnColors;

    /// <summary>One frame's sixteen colours, each from <paramref name="colour"/>.</summary>
    private static RgbColor[] Frame(Func<int, RgbColor> colour) =>
        [.. Enumerable.Range(0, SporeSpawnColorFormat.ColorsPerFrame).Select(colour)];
}
