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
    /// <summary>Each record's layout, by record.</summary>
    private static readonly int[] RecordLayouts =
    [
        .. Repeated(0, 8),
        .. Repeated(1, 8),
        .. Repeated(2, 8),
        1,
        1,
        3,
        3,
        .. Repeated(1, 14),
        .. Repeated(4, 30),
        .. Repeated(3, 14),
        .. Repeated(5, 7),
        .. Repeated(0, 61),
        .. Repeated(3, 15),
        .. Repeated(5, 6),
        .. Repeated(3, 10),
        .. Repeated(0, 16),
        .. Repeated(6, 16),
        .. Repeated(5, 10),
        .. Repeated(6, 41),
        .. Repeated(5, 8),
        .. Repeated(4, 12),
        .. Repeated(0, 48),
        .. Repeated(1, 8),
        .. Repeated(5, 13),
        .. Repeated(4, 28),
        .. Repeated(5, 14),
        .. Repeated(8, 10),
        .. Repeated(9, 16),
        .. Repeated(10, 48),
        .. Repeated(5, 4),
        .. Repeated(2, 4),
        .. Repeated(5, 8),
        .. Repeated(11, 11),
        .. Repeated(12, 14),
        .. Repeated(2, 14),
        .. Repeated(13, 14),
        .. Repeated(14, 14),
        .. Repeated(4, 30),
        .. Repeated(7, 14),
        .. Repeated(15, 11),
        .. Repeated(16, 11),
    ];

    /// <summary>The distinct layouts.</summary>
    private static readonly PaletteFxRecordPart[][] Layouts =
    [
        [new(false, 15)],
        [new(false, 2)],
        [new(false, 4)],
        [new(false, 1)],
        [new(false, 3)],
        [new(false, 8)],
        [new(false, 16)],
        [new(false, 7)],
        [new(false, 3), new(true, 9), new(false, 1)],
        [new(false, 3), new(true, 4), new(false, 2)],
        [new(false, 3), new(true, 8), new(false, 2)],
        [new(false, 1), new(true, 3), new(false, 7)],
        [new(false, 6)],
        [new(false, 6), new(true, 4), new(false, 1)],
        [new(false, 3), new(true, 4), new(false, 4), new(true, 2), new(false, 1)],
        [new(false, 11)],
        [new(false, 5)],
    ];

    /// <summary>How <paramref name="record"/> writes its colours.</summary>
    internal static IReadOnlyList<PaletteFxRecordPart> Layout(PaletteFxRecord record) => Layouts[RecordLayouts[record.Record]];
}
