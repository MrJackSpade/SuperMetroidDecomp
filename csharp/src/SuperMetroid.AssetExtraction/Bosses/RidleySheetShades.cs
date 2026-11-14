using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Bosses.Ridley.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// The shades of the Ridley encounters (#1512), which the import pre-colours their sprite sheets and their rooms'
/// pictures in, from each encounter's installed colours over the rows and colour groups its loads write: the door's,
/// container's and Baby's rows, Ridley's own row, and the room's tile groups the retreat, floor zoom and Norfair's reveal
/// recolour; and the colours and alarm of the self-destruct warning's own written pictures (#1544).
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

    /// <summary>The first ink of the HUD's effect colours, which the warning's typed text shows two of.</summary>
    private const int HudEffects = 1 * Group;

    /// <summary>The colours of a tile group the warning's pictures show from the HUD's: its first and third.</summary>
    private static readonly int[] WarningColoursFromHud = [1, 3];

    /// <summary>The shades of both encounters' sprite sheets and the objects they spawn.</summary>
    /// <exception cref="InvalidDataException">An encounter's colours are not installed.</exception>
    internal static IReadOnlyList<SheetShade> Sprites(CeresRidleyColorCatalog? ceres, BossSourceArtwork bosses)
    {
        Ensure.NotNull(bosses);
        return [.. CeresSprites(ceres ?? throw Missing("Ceres Ridley")), .. NorfairSprites(bosses.NorfairRidleyColors ?? throw Missing("Norfair Ridley"))];
    }

    /// <summary>The shades of both encounters' rooms' pictures.</summary>
    /// <exception cref="InvalidDataException">An encounter's colours are not installed.</exception>
    internal static IReadOnlyList<SheetShade> Room(ImportAreaMapPresentation maps, BossSourceArtwork bosses)
    {
        Ensure.NotNull(maps);
        Ensure.NotNull(bosses);
        return [.. CeresRoom(maps.CeresRidleyColors, maps.CeresRidleyFloorColors),
            .. NorfairRoom(bosses.NorfairRidleyColors ?? throw Missing("Norfair Ridley"))];
    }

    /// <summary>
    /// The colours the self-destruct warning's written pictures show (#1544), whose room's standard colours are
    /// <paramref name="standard"/>: its own, the room's standard colours except where its typed text shows the HUD's
    /// effect colours one and three and its EMERGENCY title the HUD's own colours one and three, as the cartridge copied
    /// them before writing the warning. No other picture shows them.
    /// </summary>
    /// <exception cref="InvalidDataException">The standard colours are not a whole colour table.</exception>
    internal static RgbColor[] CeresWarningColours(ReadOnlySpan<RgbColor> standard)
    {
        if (standard.Length <= TilesFifth + Group - 1)
            throw new InvalidDataException($"The Ceres warning is coloured over a whole room colour table, not {standard.Length} colours.");
        RgbColor[] colours = standard.ToArray();
        foreach (int colour in WarningColoursFromHud)
        {
            colours[TilesFourth + colour] = standard[HudEffects + colour];
            colours[TilesFifth + colour] = standard[colour];
        }
        return colours;
    }

    /// <summary>The shades of the self-destruct warning's written pictures: its EMERGENCY title at each alarm step.</summary>
    internal static IReadOnlyList<SheetShade> CeresWarning(CeresRidleyColorCatalog ceres)
    {
        Ensure.NotNull(ceres);
        var shades = new List<SheetShade>();
        for (int step = 0; step < CeresRidleyColorFormat.AlarmStepCount; step++)
        {
            int shown = step;
            shades.Add(SheetShade.Run(RidleyShades.Alarm(shown), TilesFifth + 1,
                Colours(CeresRidleyColorFormat.AlarmColorCount, colour => ceres.ResolveAlarm(shown, colour))));
        }
        return shades;
    }

    /// <summary>
    /// The shades of Ceres Ridley's sprites: the door's, container's and Baby's start colours, his cleared own row, his
    /// eye and body fades, his health bands, and the Baby's shades.
    /// </summary>
    internal static IReadOnlyList<SheetShade> CeresSprites(CeresRidleyColorCatalog ceres)
    {
        Ensure.NotNull(ceres);
        var shades = new List<SheetShade>
        {
            SheetShade.Run(RidleyShades.CeresStart, StartFirstInk, Colours(CeresRidleyColorFormat.StartColorCount, ceres.ResolveStart)),
            SheetShade.Run(RidleyShades.CeresHidden, OwnRow + 1, new RgbColor[Group - 1]),
        };
        for (int shade = 0; shade < CeresRidleyColorFormat.EyeFadeShadeCount; shade++)
        {
            int shown = shade;
            shades.Add(SheetShade.Run(RidleyShades.EyeFade(shown), OwnRow + EyeFirstColour,
                Colours(CeresRidleyColorFormat.EyeFadeColorCount, colour => ceres.ResolveEyeFade(shown, colour))));
        }
        for (int shade = 0; shade < CeresRidleyShadeDefinitions.BodyFadeShadeCount; shade++)
        {
            int shown = shade;
            RgbColor[] body = Colours(CeresRidleyColorFormat.BodyFadeColorCount, colour => ceres.ResolveBodyFade(shown, colour));
            shades.Add(new SheetShade(RidleyShades.BodyFade(shown),
                [.. Enumerable.Range(ShadowRow + 1, body.Length), .. Enumerable.Range(OwnRow + 1, body.Length)], [.. body, .. body]));
        }
        for (int band = 0; band < CeresRidleyColorFormat.HealthBandCount; band++)
        {
            int shown = band;
            shades.Add(SheetShade.Run(RidleyShades.Health(shown), OwnRow + 1,
                Colours(CeresRidleyColorFormat.HealthColorCount, colour => ceres.ResolveHealth(shown, colour))));
        }
        foreach (CeresBabyShade baby in Enum.GetValues<CeresBabyShade>())
            shades.Add(SheetShade.Run(RidleyShades.Baby(baby), BabyRow + 1,
                Colours(CeresRidleyColorFormat.BabyColorCount, colour => ceres.ResolveBaby(baby, colour))));
        return shades;
    }

    /// <summary>The shades of Norfair Ridley's sprites: the colours his initialization loads and his cleared own row.</summary>
    internal static IReadOnlyList<SheetShade> NorfairSprites(NorfairRidleyColorCatalog norfair)
    {
        Ensure.NotNull(norfair);
        return [SheetShade.Run(RidleyShades.NorfairStart, StartFirstInk, Colours(NorfairRidleyColorFormat.InitialColorCount, norfair.ResolveInitial)),
            SheetShade.Run(RidleyShades.NorfairHidden, OwnRow + 1, new RgbColor[Group - 1])];
    }

    /// <summary>The shades of Ceres Ridley's room: his retreat and the getaway floor's zoom.</summary>
    internal static IReadOnlyList<SheetShade> CeresRoom(CeresRidleyColorCatalog ceres, CeresRidleyFloorColorCatalog floor)
    {
        Ensure.NotNull(ceres);
        Ensure.NotNull(floor);
        var shades = new List<SheetShade>
        {
            SheetShade.Run(RidleyShades.RetreatRoom, TilesFourth + 1, Colours(CeresRidleyColorFormat.RetreatBgColorCount, ceres.ResolveRetreatBg)),
            SheetShade.Run(RidleyShades.RetreatShared, TilesFirst + 1, Colours(CeresRidleyColorFormat.RetreatSharedColorCount, ceres.ResolveRetreatShared)),
        };
        for (int step = 0; step < CeresRidleyShadeDefinitions.GetawayZoomStepCount; step++)
        {
            int shown = step;
            shades.Add(SheetShade.Run(RidleyShades.FloorZoom(shown), TilesFourth + 1,
                Colours(CeresRidleyColorFormat.GetawayZoomColorCount, colour => floor.Resolve(shown, colour))));
        }
        return shades;
    }

    /// <summary>The shades of Norfair Ridley's arena: its sixth tile group cleared as he arrives, and each step of its reveal.</summary>
    internal static IReadOnlyList<SheetShade> NorfairRoom(NorfairRidleyColorCatalog norfair)
    {
        Ensure.NotNull(norfair);
        var shades = new List<SheetShade> { SheetShade.Run(RidleyShades.NorfairHidden, OwnRow + 1, new RgbColor[Group - 1]) };
        for (int step = 0; step < NorfairRidleyRevealDefinitions.RevealStepCount; step++)
        {
            int shown = step;
            shades.Add(SheetShade.Run(RidleyShades.NorfairReveal(shown), OwnRow + 1,
                Colours(NorfairRidleyColorFormat.RevealColorCount, colour => norfair.ResolveReveal(shown, colour))));
        }
        return shades;
    }

    /// <summary><paramref name="count"/> colours, each from <paramref name="colour"/>.</summary>
    private static RgbColor[] Colours(int count, Func<int, RgbColor> colour) => [.. Enumerable.Range(0, count).Select(colour)];

    /// <summary>The error for an encounter whose colours are not installed.</summary>
    private static InvalidDataException Missing(string encounter) => new($"{encounter} is shaded from its installed colours.");
}
