// Written by tools/movie-trace/trace_layouts.py from csharp/test-support/MovieReplay/ReplayTraceMemoryCatalog.cs.
// Do not edit: run the tool again.
namespace Game.Tests.Movies;

/// <summary>The layouts the committed movie traces are kept in: the runs of the cartridge's memory each record holds, in ascending order of address.</summary>
public static class TraceLayouts
{
    /// <summary>The runs of the layout named <paramref name="layout"/>: where each starts and how long it is.</summary>
    public static (int Start, int Length)[] Of(string layout) => layout switch
    {
        "movie-desync" => MovieDesync,
        "ridley-showcase" => RidleyShowcase,
        _ => throw new System.IO.InvalidDataException($"No movie trace layout is named \"{layout}\"."),
    };

    /// <summary>The layout "movie-desync": 60 runs, 1696 bytes a record.</summary>
    private static readonly (int Start, int Length)[] MovieDesync =
    [
        (0x05B6, 2), (0x05E5, 2), (0x0629, 39), (0x079B, 2), (0x090F, 8), (0x093F, 2),
        (0x0943, 5), (0x0998, 2), (0x099C, 2), (0x09A2, 8), (0x09C2, 22), (0x0A1C, 2),
        (0x0A94, 4), (0x0AF6, 8), (0x0B2C, 4), (0x0B42, 10), (0x0B64, 200), (0x0DBA, 6),
        (0x0F78, 30), (0x0FA8, 6), (0x0FB8, 30), (0x0FF8, 30), (0x1038, 30), (0x106A, 4),
        (0x1078, 30), (0x10B8, 30), (0x10F8, 30), (0x1138, 30), (0x1178, 30), (0x11B8, 30),
        (0x11F8, 30), (0x1238, 30), (0x1278, 30), (0x12B8, 30), (0x12F8, 30), (0x1338, 30),
        (0x1378, 30), (0x13B8, 30), (0x13F8, 30), (0x1438, 30), (0x1478, 30), (0x14B8, 30),
        (0x14F8, 30), (0x1538, 30), (0x1578, 30), (0x15B8, 30), (0x15F8, 30), (0x1638, 30),
        (0x1678, 30), (0x16B8, 30), (0x16F8, 30), (0x1738, 30), (0x1997, 36), (0x1A4B, 36),
        (0x1A93, 36), (0x1B23, 144), (0x1F51, 2), (0x2000, 2), (0x2020, 140), (0x8026, 2),
    ];

    /// <summary>The layout "ridley-showcase": 47 runs, 47050 bytes a record.</summary>
    private static readonly (int Start, int Length)[] RidleyShowcase =
    [
        (0x0370, 544), (0x0592, 2), (0x05B5, 3), (0x05E5, 2), (0x05F7, 2), (0x0797, 2),
        (0x079B, 2), (0x079F, 2), (0x07F7, 256), (0x090F, 8), (0x093F, 2), (0x0998, 2),
        (0x099C, 2), (0x09A2, 8), (0x09C0, 24), (0x0A04, 2), (0x0A0E, 2), (0x0A12, 2),
        (0x0A1C, 12), (0x0A42, 32), (0x0A66, 10), (0x0A76, 2), (0x0A94, 10), (0x0AA6, 8),
        (0x0AC8, 4), (0x0AD2, 123), (0x0B56, 384), (0x0CEE, 2), (0x0DA2, 34), (0x0DFE, 4),
        (0x0F78, 2048), (0x182C, 8), (0x18A8, 6), (0x1962, 2), (0x1974, 2), (0x1997, 648),
        (0x1C37, 560), (0x2000, 172), (0x7800, 62), (0xC000, 512), (0xC608, 192), (0xCD20, 1842),
        (0xD658, 360), (0xD820, 244), (0xDE1C, 320), (0xF380, 180), (0x10002, 38400),
    ];
}
