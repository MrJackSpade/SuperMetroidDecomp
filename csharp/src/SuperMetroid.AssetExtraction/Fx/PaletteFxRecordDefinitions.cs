using SuperMetroid.AssetExtraction.Fx.PaletteFx;
using static SuperMetroid.Core.DefinitionRun;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// The import's palette write layout of the palette-FX programs (#1298, #1526): each <see cref="PaletteFxRecord"/>'s runs of written colours and skipped colour entries, which the import reads to locate each record's colours
/// and to lay its shade over the inks its group recolours. The import keys the colours it reads by
/// <see cref="SuperMetroid.AssetExtraction.Fx.PaletteFx.PaletteFxColorKey"/>; the game sees only their shades (#1583). The palette-FX program checks compare each layout with the original records.
/// </summary>
internal static class PaletteFxRecordDefinitions
{
    /// <summary>A record that writes 15 colours.</summary>
    private static readonly PaletteFxRecordPart[] Writes15 = [new(false, 15)];

    /// <summary>A record that writes 2 colours.</summary>
    private static readonly PaletteFxRecordPart[] Writes2 = [new(false, 2)];

    /// <summary>A record that writes 4 colours.</summary>
    private static readonly PaletteFxRecordPart[] Writes4 = [new(false, 4)];

    /// <summary>A record that writes 1 colours.</summary>
    private static readonly PaletteFxRecordPart[] Writes1 = [new(false, 1)];

    /// <summary>A record that writes 3 colours.</summary>
    private static readonly PaletteFxRecordPart[] Writes3 = [new(false, 3)];

    /// <summary>A record that writes 8 colours.</summary>
    private static readonly PaletteFxRecordPart[] Writes8 = [new(false, 8)];

    /// <summary>A record that writes 16 colours.</summary>
    private static readonly PaletteFxRecordPart[] Writes16 = [new(false, 16)];

    /// <summary>A record that writes 7 colours.</summary>
    private static readonly PaletteFxRecordPart[] Writes7 = [new(false, 7)];

    /// <summary>A record that writes 3, then skips 9, then writes 1 colours.</summary>
    private static readonly PaletteFxRecordPart[] Writes3Skips9Writes1 = [new(false, 3), new(true, 9), new(false, 1)];

    /// <summary>A record that writes 3, then skips 4, then writes 2 colours.</summary>
    private static readonly PaletteFxRecordPart[] Writes3Skips4Writes2 = [new(false, 3), new(true, 4), new(false, 2)];

    /// <summary>A record that writes 3, then skips 8, then writes 2 colours.</summary>
    private static readonly PaletteFxRecordPart[] Writes3Skips8Writes2 = [new(false, 3), new(true, 8), new(false, 2)];

    /// <summary>A record that writes 1, then skips 3, then writes 7 colours.</summary>
    private static readonly PaletteFxRecordPart[] Writes1Skips3Writes7 = [new(false, 1), new(true, 3), new(false, 7)];

    /// <summary>A record that writes 6 colours.</summary>
    private static readonly PaletteFxRecordPart[] Writes6 = [new(false, 6)];

    /// <summary>A record that writes 6, then skips 4, then writes 1 colours.</summary>
    private static readonly PaletteFxRecordPart[] Writes6Skips4Writes1 = [new(false, 6), new(true, 4), new(false, 1)];

    /// <summary>A record that writes 3, then skips 4, then writes 4, then skips 2, then writes 1 colours.</summary>
    private static readonly PaletteFxRecordPart[] Writes3Skips4Writes4Skips2Writes1 = [new(false, 3), new(true, 4), new(false, 4), new(true, 2), new(false, 1)];

    /// <summary>A record that writes 11 colours.</summary>
    private static readonly PaletteFxRecordPart[] Writes11 = [new(false, 11)];

    /// <summary>A record that writes 5 colours.</summary>
    private static readonly PaletteFxRecordPart[] Writes5 = [new(false, 5)];

    /// <summary>Each record's layout, by record.</summary>
    private static readonly PaletteFxRecordPart[][] RecordLayouts =
    [
        .. Repeated(Writes15, 8),
        .. Repeated(Writes2, 8),
        .. Repeated(Writes4, 8),
        Writes2,
        Writes2,
        Writes1,
        Writes1,
        .. Repeated(Writes2, 14),
        .. Repeated(Writes3, 30),
        .. Repeated(Writes1, 14),
        .. Repeated(Writes8, 7),
        .. Repeated(Writes15, 61),
        .. Repeated(Writes1, 15),
        .. Repeated(Writes8, 6),
        .. Repeated(Writes1, 10),
        .. Repeated(Writes15, 16),
        .. Repeated(Writes16, 16),
        .. Repeated(Writes8, 10),
        .. Repeated(Writes16, 41),
        .. Repeated(Writes8, 8),
        .. Repeated(Writes3, 12),
        .. Repeated(Writes15, 48),
        .. Repeated(Writes2, 8),
        .. Repeated(Writes8, 13),
        .. Repeated(Writes3, 28),
        .. Repeated(Writes8, 14),
        .. Repeated(Writes3Skips9Writes1, 10),
        .. Repeated(Writes3Skips4Writes2, 16),
        .. Repeated(Writes3Skips8Writes2, 48),
        .. Repeated(Writes8, 4),
        .. Repeated(Writes4, 4),
        .. Repeated(Writes8, 8),
        .. Repeated(Writes1Skips3Writes7, 11),
        .. Repeated(Writes6, 14),
        .. Repeated(Writes4, 14),
        .. Repeated(Writes6Skips4Writes1, 14),
        .. Repeated(Writes3Skips4Writes4Skips2Writes1, 14),
        .. Repeated(Writes3, 30),
        .. Repeated(Writes7, 14),
        .. Repeated(Writes11, 11),
        .. Repeated(Writes5, 11),
    ];

    /// <summary>How <paramref name="record"/> writes its colours.</summary>
    internal static IReadOnlyList<PaletteFxRecordPart> Layout(PaletteFxRecord record) => RecordLayouts[record.Record];
}
