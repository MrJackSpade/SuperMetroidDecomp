using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn;
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
    /// <exception cref="InvalidDataException">His colours are not installed.</exception>
    internal static IReadOnlyList<SheetShade> Sprites(BossArtworkCatalog bosses)
    {
        SporeSpawnColorCatalog colours = Colours(bosses);
        var shades = new List<SheetShade> { SheetShade.Run(SporeSpawnShades.Spores, SporesRow * LoadedSetColours.RowColours, Frame(colours.ResolveSpore)) };
        for (int frame = 0; frame < SporeSpawnColorDefinitions.HealthFrameCount; frame++)
        {
            int shown = frame;
            shades.Add(SheetShade.Run(SporeSpawnShades.Health(shown), BodyRow * LoadedSetColours.RowColours, Frame(colour => colours.ResolveHealth(shown, colour))));
        }
        for (int step = 0; step < SporeSpawnColorDefinitions.DeathSpriteFrameCount; step++)
        {
            int shown = step;
            shades.Add(SheetShade.Run(SporeSpawnShades.DeathSprite(shown), BodyRow * LoadedSetColours.RowColours, Frame(colour => colours.ResolveDeathSprite(shown, colour))));
        }
        return shades;
    }

    /// <summary>The shades of his room's pictures: each step of his death fade over its level and background.</summary>
    /// <exception cref="InvalidDataException">His colours are not installed.</exception>
    internal static IReadOnlyList<SheetShade> Room(BossArtworkCatalog bosses)
    {
        SporeSpawnColorCatalog colours = Colours(bosses);
        var shades = new List<SheetShade>();
        for (int step = 0; step < SporeSpawnColorDefinitions.DeathSceneFrameCount; step++)
        {
            int shown = step;
            shades.Add(SheetShade.Run(SporeSpawnShades.DeathLevel(shown), LevelFirstInk, Frame(colour => colours.ResolveDeathLevel(shown, colour))));
            shades.Add(SheetShade.Run(SporeSpawnShades.DeathBackground(shown), BackgroundFirstInk, Frame(colour => colours.ResolveDeathBackground(shown, colour))));
        }
        return shades;
    }

    /// <summary>His installed colours.</summary>
    /// <exception cref="InvalidDataException">His colours are not installed.</exception>
    private static SporeSpawnColorCatalog Colours(BossArtworkCatalog bosses) =>
        bosses.SporeSpawnColors ?? throw new InvalidDataException("Spore Spawn is shaded from the installed Spore Spawn colours.");

    /// <summary>One frame's sixteen colours, each from <paramref name="colour"/>.</summary>
    private static RgbColor[] Frame(Func<int, RgbColor> colour) =>
        [.. Enumerable.Range(0, SporeSpawnColorDefinitions.ColorsPerFrame).Select(colour)];
}
