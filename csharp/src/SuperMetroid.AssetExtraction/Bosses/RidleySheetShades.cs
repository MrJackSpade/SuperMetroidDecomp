using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Bosses.Ridley.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// The shades of the Ridley encounters (#1512), which the import pre-colours their sprite sheets and their rooms'
/// pictures in, from each encounter's installed colours over the rows and colour groups its loads write: the door's,
/// container's and Baby's rows, Ridley's own row, and the room's tile groups the alarm, warning, retreat, floor zoom and
/// Norfair's reveal recolour.
/// </summary>
internal static class RidleySheetShades
{
    /// <summary>The colours of one row or colour group.</summary>
    private const int Group = 16;

    /// <summary>The first ink of the door's and container's row, which the Baby's row follows.</summary>
    private const int StartFirstInk = 2 * Group;

    /// <summary>The first ink of the Baby's row.</summary>
    private const int BabyRow = 3 * Group;

    /// <summary>The first ink of the row Ridley's body shadow fades in.</summary>
    private const int ShadowRow = 1 * Group;

    /// <summary>The first ink of Ridley's own row, and of a room picture's sixth tile group.</summary>
    private const int OwnRow = 7 * Group;

    /// <summary>The colour of Ridley's own row his eyes start at.</summary>
    private const int EyeFirstColour = 12;

    /// <summary>The first ink of a room picture's first tile group.</summary>
    private const int TilesFirst = 2 * Group;

    /// <summary>The first ink of a room picture's fourth tile group.</summary>
    private const int TilesFourth = 5 * Group;

    /// <summary>The first ink of a room picture's fifth tile group.</summary>
    private const int TilesFifth = 6 * Group;

    /// <summary>The first ink of the HUD's effect colours, which the warning copies from with the HUD's own.</summary>
    private const int HudEffects = 1 * Group;

    /// <summary>The shades of both encounters' sprite sheets and the objects they spawn.</summary>
    /// <exception cref="InvalidDataException">An encounter's colours are not installed.</exception>
    internal static IReadOnlyList<SheetShade> Sprites(CeresRidleyColorCatalog? ceres, BossSourceArtwork bosses)
    {
        Ensure.NotNull(bosses);
        return [.. CeresSprites(ceres ?? throw Missing("Ceres Ridley")), .. NorfairSprites(bosses.NorfairRidleyColors ?? throw Missing("Norfair Ridley"))];
    }

    /// <summary>The shades of both encounters' rooms' pictures, whose standard colours are <paramref name="standard"/>.</summary>
    /// <exception cref="InvalidDataException">An encounter's colours are not installed.</exception>
    internal static IReadOnlyList<SheetShade> Room(AreaMapPresentation maps, BossSourceArtwork bosses, ReadOnlySpan<RgbColor> standard)
    {
        Ensure.NotNull(maps);
        Ensure.NotNull(bosses);
        return [.. CeresRoom(InstalledAreaMapPresentation.CeresRidleyColorsOf(maps), InstalledAreaMapPresentation.CeresRidleyFloorColorsOf(maps), standard),
            .. NorfairRoom(bosses.NorfairRidleyColors ?? throw Missing("Norfair Ridley"))];
    }

    /// <summary>
    /// The shades of Ceres Ridley's sprites: the door's, container's and Baby's start colours, his cleared own row, his
    /// eye and body fades, his health bands, the colours his retreat shares, and the Baby's shades.
    /// </summary>
    internal static IReadOnlyList<SheetShade> CeresSprites(CeresRidleyColorCatalog ceres)
    {
        Ensure.NotNull(ceres);
        var shades = new List<SheetShade>
        {
            SheetShade.Run(RidleyShades.CeresStart, StartFirstInk, Colours(CeresRidleyColorFormat.StartColorCount, ceres.selection.Start.Resolve)),
            SheetShade.Run(RidleyShades.CeresHidden, OwnRow + 1, new RgbColor[Group - 1]),
            SheetShade.Run(RidleyShades.RetreatShared, OwnRow + 1, Colours(CeresRidleyColorFormat.RetreatSharedColorCount, ceres.ResolveRetreatShared)),
        };
        for (int shade = 0; shade < CeresRidleyColorFormat.EyeFadeShadeCount; shade++)
        {
            int shown = shade;
            shades.Add(SheetShade.Run(RidleyShades.EyeFade(shown), OwnRow + EyeFirstColour,
                Colours(CeresRidleyColorFormat.EyeFadeColorCount, colour => ceres.selection.EyeFade.Resolve(shown, colour))));
        }
        for (int shade = 0; shade < CeresRidleyPaletteDefinitions.BodyFadeShadeCount; shade++)
        {
            int shown = shade;
            RgbColor[] body = Colours(CeresRidleyColorFormat.BodyFadeColorCount, colour => ceres.selection.BodyFade.Resolve(shown, colour));
            shades.Add(new SheetShade(RidleyShades.BodyFade(shown),
                [.. Enumerable.Range(ShadowRow + 1, body.Length), .. Enumerable.Range(OwnRow + 1, body.Length)], [.. body, .. body]));
        }
        for (int band = 0; band < CeresRidleyColorFormat.HealthBandCount; band++)
        {
            int shown = band;
            shades.Add(SheetShade.Run(RidleyShades.Health(shown), OwnRow + 1,
                Colours(CeresRidleyColorFormat.HealthColorCount, colour => ceres.selection.Health.Resolve(shown, colour))));
        }
        foreach (CeresBabyShade baby in Enum.GetValues<CeresBabyShade>())
            shades.Add(SheetShade.Run(RidleyShades.Baby(baby), BabyRow + 1,
                Colours(CeresRidleyColorFormat.BabyColorCount, colour => ceres.selection.Baby.Resolve(baby, colour))));
        return shades;
    }

    /// <summary>The shades of Norfair Ridley's sprites: the colours his initialization loads and his cleared own row.</summary>
    internal static IReadOnlyList<SheetShade> NorfairSprites(NorfairRidleyColorCatalog norfair)
    {
        Ensure.NotNull(norfair);
        return [SheetShade.Run(RidleyShades.NorfairStart, StartFirstInk, Colours(NorfairRidleyColorFormat.InitialColorCount, norfair.selection.Initial.ColorAt)),
            SheetShade.Run(RidleyShades.NorfairHidden, OwnRow + 1, new RgbColor[Group - 1])];
    }

    /// <summary>
    /// The shades of Ceres Ridley's room, whose standard colours are <paramref name="standard"/>: his retreat, the
    /// self-destruct warning and alarm, and the getaway floor's zoom.
    /// </summary>
    internal static IReadOnlyList<SheetShade> CeresRoom(CeresRidleyColorCatalog ceres, CeresRidleyFloorColorCatalog floor, ReadOnlySpan<RgbColor> standard)
    {
        Ensure.NotNull(ceres);
        Ensure.NotNull(floor);
        var shades = new List<SheetShade>
        {
            SheetShade.Run(RidleyShades.RetreatRoom, TilesFourth + 1, Colours(CeresRidleyColorFormat.RetreatBgColorCount, ceres.ResolveRetreatBg)),
            SheetShade.Run(RidleyShades.RetreatShared, TilesFirst + 1, Colours(CeresRidleyColorFormat.RetreatSharedColorCount, ceres.ResolveRetreatShared)),
            // The warning copies the HUD's effect colours one and three over the fourth tile group's and the HUD's own
            // over the fifth's.
            new(RidleyShades.Warning, [TilesFourth + 1, TilesFourth + 3, TilesFifth + 1, TilesFifth + 3],
                [standard[HudEffects + 1], standard[HudEffects + 3], standard[1], standard[3]]),
        };
        for (int step = 0; step < CeresRidleyColorFormat.AlarmStepCount; step++)
        {
            int shown = step;
            shades.Add(SheetShade.Run(RidleyShades.Alarm(shown), TilesFifth + 1,
                Colours(CeresRidleyColorFormat.AlarmColorCount, colour => ceres.selection.Alarm.Resolve(shown, colour))));
        }
        for (int step = 0; step < CeresRidleyPaletteDefinitions.GetawayZoomStepCount; step++)
        {
            int shown = step;
            shades.Add(SheetShade.Run(RidleyShades.FloorZoom(shown), TilesFourth + 1,
                Colours(CeresRidleyColorFormat.GetawayZoomColorCount, colour => CeresRidleyFloorShades.Resolve(floor.steps, shown, colour))));
        }
        return shades;
    }

    /// <summary>The shades of Norfair Ridley's arena: its sixth tile group cleared as he arrives, and each step of its reveal.</summary>
    internal static IReadOnlyList<SheetShade> NorfairRoom(NorfairRidleyColorCatalog norfair)
    {
        Ensure.NotNull(norfair);
        var shades = new List<SheetShade> { SheetShade.Run(RidleyShades.NorfairHidden, OwnRow + 1, new RgbColor[Group - 1]) };
        for (int step = 0; step < NorfairRidleyPaletteDefinitions.RevealStepCount; step++)
        {
            int shown = step;
            shades.Add(SheetShade.Run(RidleyShades.NorfairReveal(shown), OwnRow + 1,
                Colours(NorfairRidleyColorFormat.RevealColorCount, colour => norfair.selection.Reveal.ColorAt(shown, colour))));
        }
        return shades;
    }

    /// <summary><paramref name="count"/> colours, each from <paramref name="colour"/>.</summary>
    private static RgbColor[] Colours(int count, Func<int, RgbColor> colour) => [.. Enumerable.Range(0, count).Select(colour)];

    /// <summary>The error for an encounter whose colours are not installed.</summary>
    private static InvalidDataException Missing(string encounter) => new($"{encounter} is shaded from its installed colours.");
}
