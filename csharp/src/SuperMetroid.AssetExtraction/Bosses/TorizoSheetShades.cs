using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// The shades of the Torizos (#1512), which the import pre-colours their sheets in: the colours both load over their
/// orb-and-flame and statue-shading rows, each settled colouring of a Torizo's body and belly over its two rows, and each
/// run of counter values a fade takes from one toward another, as the shared gradual colour change moves each colour one
/// step from the colour shown. A fade's run starts past zero, as the counter's zero step moves nothing, and at any value
/// through its last: a hurt colouring written mid-fade restarts the fade from that colouring at the counter value it
/// stands at.
/// </summary>
internal static class TorizoSheetShades
{
    /// <summary>The first ink of a Torizo's body and belly: the first of the two rows they draw in.</summary>
    private const int BodyFirstInk = 1 * LoadedSetColours.RowColours;

    /// <summary>The first ink of the orb-and-flame row.</summary>
    private const int OrbsFirstInk = 3 * LoadedSetColours.RowColours;

    /// <summary>The first ink of the statue-shading row.</summary>
    private const int ShadingFirstInk = 7 * LoadedSetColours.RowColours;

    /// <summary>The colours of a Torizo's body and belly together.</summary>
    private const int BodyColours = 2 * LoadedSetColours.RowColours;

    /// <summary>The last band of Golden Torizo's health colours.</summary>
    private const int LastHealthBand = 7;

    /// <summary>The colourings a fade runs toward: black, and each statue's waking colours.</summary>
    private static readonly TorizoLook[] FadeTargets =
        [new(TorizoLookKind.Black), new(TorizoLookKind.Normal), new(TorizoLookKind.Golden)];

    /// <summary>Every shade of a Torizo's sheet, with Golden Torizo's health colours from <paramref name="auxiliary"/>.</summary>
    internal static IReadOnlyList<SheetShade> Of(EnemyAuxiliaryColorCatalog auxiliary)
    {
        Ensure.NotNull(auxiliary);
        var shades = new List<SheetShade>
        {
            new(TorizoShades.Shared,
                [.. Enumerable.Range(OrbsFirstInk, LoadedSetColours.RowColours), .. Enumerable.Range(ShadingFirstInk, LoadedSetColours.RowColours)],
                TorizoPaletteColours.Shared.ToArray()),
        };
        TorizoLook[] settled =
        [
            new(TorizoLookKind.BombStatue), new(TorizoLookKind.GoldenInitial), new(TorizoLookKind.Normal), new(TorizoLookKind.Golden),
            new(TorizoLookKind.White), new(TorizoLookKind.Black),
            .. Enumerable.Range(0, LastHealthBand + 1).Select(band => new TorizoLook(TorizoLookKind.GoldenHealth, band)),
        ];
        foreach (TorizoLook look in settled)
            shades.Add(SheetShade.Run(TorizoShades.Body(look), BodyFirstInk, Colours(auxiliary, look)));
        foreach (TorizoLook from in settled)
        {
            RgbColor[] start = Colours(auxiliary, from);
            foreach (TorizoLook to in FadeTargets)
            {
                RgbColor[] goal = Colours(auxiliary, to);
                for (int first = 1; first <= TorizoPaletteDefinitions.FadeDenominator + 1; first++)
                {
                    for (int last = first; last <= TorizoPaletteDefinitions.FadeDenominator + 1; last++)
                        shades.Add(SheetShade.Run(TorizoShades.Fading(from, to, first, last), BodyFirstInk, Faded(start, goal, first, last)));
                }
            }
        }
        return shades;
    }

    /// <summary>The body's and belly's colours of <paramref name="look"/>, body first.</summary>
    private static RgbColor[] Colours(EnemyAuxiliaryColorCatalog auxiliary, TorizoLook look) => look.Kind switch
    {
        TorizoLookKind.BombStatue => TorizoPaletteColours.BombInitial.ToArray(),
        TorizoLookKind.GoldenInitial => TorizoPaletteColours.GoldenInitial.ToArray(),
        TorizoLookKind.Normal => TorizoPaletteColours.Normal.ToArray(),
        TorizoLookKind.Golden => TorizoPaletteColours.Golden.ToArray(),
        TorizoLookKind.White => [.. Enumerable.Repeat(RgbColor.White, BodyColours)],
        TorizoLookKind.Black => new RgbColor[BodyColours],
        TorizoLookKind.GoldenHealth =>
        [
            .. Enumerable.Range(0, LoadedSetColours.RowColours).Select(colour => auxiliary.Resolve(EnemyAuxiliaryPalette.GoldenTorizoBody, look.Band, colour)),
            .. Enumerable.Range(0, LoadedSetColours.RowColours).Select(colour => auxiliary.Resolve(EnemyAuxiliaryPalette.GoldenTorizoBelly, look.Band, colour)),
        ],
        _ => throw new InvalidDataException($"Undefined Torizo colouring {look.Kind}."),
    };

    /// <summary>Each of <paramref name="start"/>'s colours after the fade's steps toward <paramref name="goal"/>'s of counter values <paramref name="first"/> through <paramref name="last"/>.</summary>
    private static RgbColor[] Faded(RgbColor[] start, RgbColor[] goal, int first, int last)
    {
        var faded = new RgbColor[start.Length];
        for (int ink = 0; ink < faded.Length; ink++)
        {
            RgbColor current = start[ink];
            for (int step = first; step <= last; step++)
                current = GradualColourFade.Step(step, current, goal[ink], TorizoPaletteDefinitions.FadeDenominator);
            faded[ink] = current;
        }
        return faded;
    }
}
