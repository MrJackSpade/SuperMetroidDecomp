using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Fx.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// The shades of the Torizos (#1512, #2236), which the import pre-colours their sheets in: the colours both load over their
/// orb-and-flame and statue-shading rows and each settled colouring of a Torizo's body and belly over its two rows. Once a
/// Torizo's fight starts, his belly's glow writes each record of its palette-FX program over his belly's glowing colours,
/// so the import walks that program and pre-colours each record it shows there, and the glowing colours in each colouring
/// a fade runs toward. A fade between colourings is a typed fade between these shades, never pre-coloured.
/// </summary>
internal static partial class TorizoSheetShades
{
    /// <summary>The first ink of a Torizo's body and belly: the first of the two rows they draw in.</summary>
    private const int BodyFirstInk = 1 * LoadedSetColours.RowColours;

    /// <summary>The first ink of the orb-and-flame row.</summary>
    private const int OrbsFirstInk = 3 * LoadedSetColours.RowColours;

    /// <summary>The first ink of the statue-shading row.</summary>
    private const int ShadingFirstInk = 7 * LoadedSetColours.RowColours;

    /// <summary>
    /// The first ink the belly glow recolours: its program's colour index names the tenth colour of the first of the
    /// Torizo's two body rows.
    /// </summary>
    private const int BellyGlowFirstInk = BodyFirstInk + 9;

    /// <summary>The ink after the last the belly glow may recolour: the end of that row.</summary>
    private const int BellyGlowEndInk = BodyFirstInk + LoadedSetColours.RowColours;

    /// <summary>The colours of a Torizo's body and belly together.</summary>
    private const int BodyColours = 2 * LoadedSetColours.RowColours;

    /// <summary>The colourings a fade runs toward: black, and each statue's waking colours.</summary>
    private static readonly TorizoLook[] FadeTargets =
        [new(TorizoLookKind.Black), new(TorizoLookKind.Normal), new(TorizoLookKind.Golden)];

    /// <summary>
    /// Every shade of a Torizo's sheet, with Golden Torizo's health colours from <paramref name="auxiliary"/> and the
    /// records of the belly glow <paramref name="glow"/> from <paramref name="paletteFx"/>.
    /// </summary>
    /// <exception cref="InvalidDataException">A glow record has no installed colour, or writes past the body's first row.</exception>
    internal static IReadOnlyList<SheetShade> Of(EnemyAuxiliaryColorCatalog auxiliary, IPaletteFxColorSource paletteFx, ColourEffectId glow)
    {
        Ensure.NotNull(auxiliary);
        Ensure.NotNull(paletteFx);
        var shades = new List<SheetShade>
        {
            // The shared colours are the orb-and-flame row's run, then the statue-shading row's.
            SheetShade.Combined(TorizoShades.Shared,
            [
                SheetShade.Run(TorizoShades.Shared, OrbsFirstInk, TorizoPaletteColours.Shared[..LoadedSetColours.RowColours]),
                SheetShade.Run(TorizoShades.Shared, ShadingFirstInk, TorizoPaletteColours.Shared[LoadedSetColours.RowColours..]),
            ]),
        };
        TorizoLook[] settled =
        [
            new(TorizoLookKind.BombStatue), new(TorizoLookKind.GoldenInitial), new(TorizoLookKind.Normal), new(TorizoLookKind.Golden),
            new(TorizoLookKind.White), new(TorizoLookKind.Black),
            .. GoldenTorizoHealthBandDefinitions.HighestFirst.Reverse().Select(TorizoLook.GoldenHealth),
        ];
        foreach (TorizoLook look in settled)
            shades.Add(SheetShade.Run(TorizoShades.Body(look), BodyFirstInk, Colours(auxiliary, look)));
        shades.AddRange(BellyGlow(auxiliary, paletteFx, glow));
        return shades;
    }

    /// <summary>
    /// The belly glow's shades: each record its program shows over the glowing colours, and those colours in each
    /// colouring a fade runs toward, which a fade over the glow ends on.
    /// </summary>
    /// <exception cref="InvalidDataException">The glow shows no record, or its records recolour different colours.</exception>
    private static List<SheetShade> BellyGlow(EnemyAuxiliaryColorCatalog auxiliary, IPaletteFxColorSource paletteFx, ColourEffectId glow)
    {
        var shades = new List<SheetShade>();
        foreach ((ColourEffectColours colours, ColourEffectFrame record) in PaletteFxRecordShades.ShownBy([glow]))
        {
            if (colours != ColourEffectColours.TorizoBelly)
                throw new InvalidDataException($"The Torizo belly glow {glow} shows {colours}.");
            shades.Add(PaletteFxRecordShades.Shade(colours, record, new InkRange(BellyGlowFirstInk, BellyGlowEndInk), paletteFx));
        }
        int[] inks = shades.Count > 0 ? [.. shades[0].Inks] : throw new InvalidDataException($"The Torizo belly glow {glow} shows no record.");
        if (shades.Any(shade => !shade.Inks.SequenceEqual(inks)))
            throw new InvalidDataException($"The Torizo belly glow {glow}'s records recolour different colours.");
        foreach (TorizoLook to in FadeTargets)
        {
            RgbColor[] colouring = Colours(auxiliary, to);
            shades.Add(SheetShade.Of(TorizoShades.GlowingBelly(to), inks, ink => colouring[ink - BodyFirstInk]));
        }
        return shades;
    }

    /// <summary>The body's and belly's colours of <paramref name="look"/>, body first.</summary>
    private static RgbColor[] Colours(EnemyAuxiliaryColorCatalog auxiliary, TorizoLook look) => look.Band is { } band
        // Golden Torizo's health colouring shows its band's body and belly colours; the others are named by their kind.
        ?
        [
            .. Enumerable.Range(0, LoadedSetColours.RowColours).Select(colour => auxiliary.Resolve(EnemyAuxiliaryPalette.GoldenTorizoBody, HealthRow(band), colour)),
            .. Enumerable.Range(0, LoadedSetColours.RowColours).Select(colour => auxiliary.Resolve(EnemyAuxiliaryPalette.GoldenTorizoBelly, HealthRow(band), colour)),
        ]
        : look.Kind switch
    {
        TorizoLookKind.BombStatue => TorizoPaletteColours.BombInitial.ToArray(),
        TorizoLookKind.GoldenInitial => TorizoPaletteColours.GoldenInitial.ToArray(),
        TorizoLookKind.Normal => TorizoPaletteColours.Normal.ToArray(),
        TorizoLookKind.Golden => TorizoPaletteColours.Golden.ToArray(),
        TorizoLookKind.White => [.. Enumerable.Repeat(RgbColor.White, BodyColours)],
        TorizoLookKind.Black => new RgbColor[BodyColours],
        // A health colouring names its band, which the colours above read.
        TorizoLookKind.GoldenHealth or _ => throw new InvalidDataException($"Undefined Torizo colouring {look.Kind}."),
    };

    /// <summary>The row of Golden Torizo's installed health colours each band reads, one row per 2048 health.</summary>
    private static readonly Dictionary<GoldenTorizoHealthBand, int> HealthRows = new()
    {
        [GoldenTorizoHealthBandDefinitions.NearlyDead] = 0,
        [GoldenTorizoHealthBandDefinitions.GravelyWounded] = 1,
        [GoldenTorizoHealthBandDefinitions.BadlyWounded] = 2,
        [GoldenTorizoHealthBandDefinitions.Wounded] = 3,
        [GoldenTorizoHealthBandDefinitions.Hurt] = 4,
        [GoldenTorizoHealthBandDefinitions.Grazed] = 5,
        [GoldenTorizoHealthBandDefinitions.Unhurt] = 6,
    };

    /// <summary>The row of Golden Torizo's installed health colours <paramref name="band"/> reads.</summary>
    /// <exception cref="InvalidDataException"><paramref name="band"/> has no installed colours.</exception>
    private static int HealthRow(GoldenTorizoHealthBand band)
    {
        return HealthRows.TryGetValue(band, out int row) ? row : throw new InvalidDataException($"Golden Torizo has no installed health colours for band {band}.");
    }
}
