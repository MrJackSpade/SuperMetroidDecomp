using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// The import's palette write layout of the palette-FX programs (#1298, #1526): each layout of runs of written colours and skipped colour entries a <see cref="PaletteFxRecord"/> writes, which each record run of <see cref="ColourEffectRecordOrder"/> names and the import reads to locate each record's colours
/// and to lay its shade over the inks its group recolours. The import keys the colours it reads by
/// <see cref="SuperMetroid.AssetExtraction.Fx.PaletteFx.PaletteFxColorKey"/>; the game sees only their shades (#1583). The palette-FX program checks compare each layout with the original records.
/// </summary>
internal static class PaletteFxRecordDefinitions
{
    /// <summary>A record that writes 15 colours.</summary>
    internal static readonly PaletteFxRecordPart[] Writes15 = [new(false, 15)];

    /// <summary>A record that writes 2 colours.</summary>
    internal static readonly PaletteFxRecordPart[] Writes2 = [new(false, 2)];

    /// <summary>A record that writes 4 colours.</summary>
    internal static readonly PaletteFxRecordPart[] Writes4 = [new(false, 4)];

    /// <summary>A record that writes 1 colours.</summary>
    internal static readonly PaletteFxRecordPart[] Writes1 = [new(false, 1)];

    /// <summary>A record that writes 3 colours.</summary>
    internal static readonly PaletteFxRecordPart[] Writes3 = [new(false, 3)];

    /// <summary>A record that writes 8 colours.</summary>
    internal static readonly PaletteFxRecordPart[] Writes8 = [new(false, 8)];

    /// <summary>A record that writes 16 colours.</summary>
    internal static readonly PaletteFxRecordPart[] Writes16 = [new(false, 16)];

    /// <summary>A record that writes 7 colours.</summary>
    internal static readonly PaletteFxRecordPart[] Writes7 = [new(false, 7)];

    /// <summary>A record that writes 3, then skips 9, then writes 1 colours.</summary>
    internal static readonly PaletteFxRecordPart[] Writes3Skips9Writes1 = [new(false, 3), new(true, 9), new(false, 1)];

    /// <summary>A record that writes 3, then skips 4, then writes 2 colours.</summary>
    internal static readonly PaletteFxRecordPart[] Writes3Skips4Writes2 = [new(false, 3), new(true, 4), new(false, 2)];

    /// <summary>A record that writes 3, then skips 8, then writes 2 colours.</summary>
    internal static readonly PaletteFxRecordPart[] Writes3Skips8Writes2 = [new(false, 3), new(true, 8), new(false, 2)];

    /// <summary>A record that writes 1, then skips 3, then writes 7 colours.</summary>
    internal static readonly PaletteFxRecordPart[] Writes1Skips3Writes7 = [new(false, 1), new(true, 3), new(false, 7)];

    /// <summary>A record that writes 6 colours.</summary>
    internal static readonly PaletteFxRecordPart[] Writes6 = [new(false, 6)];

    /// <summary>A record that writes 6, then skips 4, then writes 1 colours.</summary>
    internal static readonly PaletteFxRecordPart[] Writes6Skips4Writes1 = [new(false, 6), new(true, 4), new(false, 1)];

    /// <summary>A record that writes 3, then skips 4, then writes 4, then skips 2, then writes 1 colours.</summary>
    internal static readonly PaletteFxRecordPart[] Writes3Skips4Writes4Skips2Writes1 = [new(false, 3), new(true, 4), new(false, 4), new(true, 2), new(false, 1)];

    /// <summary>A record that writes 11 colours.</summary>
    internal static readonly PaletteFxRecordPart[] Writes11 = [new(false, 11)];

    /// <summary>A record that writes 5 colours.</summary>
    internal static readonly PaletteFxRecordPart[] Writes5 = [new(false, 5)];
}
