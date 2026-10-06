using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>File-copy arrow compositions $82:CDCD/CE15/CE5D/CECD and placement $82:BABA-BB2F.</summary>
public static class FileCopyArrowDefinitions
{
    /// <summary>$81:98E6: initial delay before the first arrow palette rotation.</summary>
    public const int InitialPaletteDelay = 8;
    /// <summary>$81:9A09: subsequent arrow palette rotation delay.</summary>
    internal const int PaletteDelay = 4;
    /// <summary>$81:9A0F: OBJ palette 1, color 1 (CGRAM index 145).</summary>
    internal const int FirstColor = 145;
    /// <summary>$81:9A17-9A27: rotate seven colors, preserving transparent color zero.</summary>
    internal const int ColorCount = 7;
    /// <summary>$82:BB0E: all file-copy arrows use screen X = $14.</summary>
    internal const ushort OriginX = 20;

    internal static (int Shape, ushort Y) Select(int source, int destination)
    {
        if ((uint)source >= 3 || (uint)destination >= 3 || source == destination)
            throw new ArgumentOutOfRangeException(nameof(destination));
        int distance = Math.Abs(source - destination);
        int shape = (distance == 2 ? 2 : 0) + (source > destination ? 1 : 0);
        return (shape, (ushort)(distance == 2 ? 104 : 88 + 32 * Math.Min(source, destination)));
    }

    internal static ReadOnlySpan<CompiledSpritePart> Parts(int shape) => Shapes[shape];
    private static CompiledSpritePart Part(int x, int y, int tile, bool flipY) => new(
        SnesSpritemapXWord.Create(x, false), unchecked((byte)y),
        SnesObjAttributeWord.Create(tile, 1, 3, flipY ? SnesTileFlipFlags.Vertical : 0), true);

    // Native sprite order is retained, including the interleaved shaft and arrowhead tiles.
    private static readonly CompiledSpritePart[][] Shapes =
    [
        [
            Part(2, -16, 0xcf, false),
            Part(-6, -16, 0xce, false),
            Part(2, -24, 0xbf, false),
            Part(-6, -24, 0xbe, false),
            Part(-6, 8, 0xb9, false),
            Part(-6, 16, 0xc9, false),
            Part(2, 16, 0xcb, false),
            Part(2, 8, 0xbb, false),
            Part(-14, 8, 0xb8, false),
            Part(-14, 0, 0xbc, false),
            Part(-6, 0, 0xbd, false),
            Part(-14, -16, 0xcd, false),
            Part(-6, -8, 0xbd, false),
            Part(-14, -8, 0xbc, false),
        ],
        [
            Part(-14, -16, 0xb8, true),
            Part(-14, -8, 0xbc, true),
            Part(-6, -8, 0xbd, true),
            Part(-6, -24, 0xc9, true),
            Part(-6, -16, 0xb9, true),
            Part(2, 8, 0xcf, true),
            Part(-6, 8, 0xce, true),
            Part(2, 16, 0xbf, true),
            Part(-6, 16, 0xbe, true),
            Part(-14, 8, 0xcd, true),
            Part(-6, 0, 0xbd, true),
            Part(-14, 0, 0xbc, true),
            Part(2, -24, 0xcb, true),
            Part(2, -16, 0xbb, true),
        ],
        [
            Part(-6, 8, 0xbd, false),
            Part(-14, 8, 0xbc, false),
            Part(-6, 0, 0xbd, false),
            Part(-14, 0, 0xbc, false),
            Part(-6, -8, 0xbd, false),
            Part(-14, -8, 0xbc, false),
            Part(-6, -16, 0xbd, false),
            Part(-14, -16, 0xbc, false),
            Part(-14, 24, 0xb8, false),
            Part(-14, 16, 0xbc, false),
            Part(-6, 16, 0xbd, false),
            Part(-6, 32, 0xc9, false),
            Part(-6, 24, 0xb9, false),
            Part(2, -32, 0xcf, false),
            Part(-6, -32, 0xce, false),
            Part(2, -40, 0xbf, false),
            Part(-6, -40, 0xbe, false),
            Part(-14, -32, 0xcd, false),
            Part(-6, -24, 0xbd, false),
            Part(-14, -24, 0xbc, false),
            Part(2, 32, 0xcb, false),
            Part(2, 24, 0xbb, false),
        ],
        [
            Part(-6, -16, 0xbd, true),
            Part(-14, -16, 0xbc, true),
            Part(-6, -8, 0xbd, true),
            Part(-14, -8, 0xbc, true),
            Part(-6, 0, 0xbd, true),
            Part(-14, 0, 0xbc, true),
            Part(-6, 8, 0xbd, true),
            Part(-14, 8, 0xbc, true),
            Part(-14, -32, 0xb8, true),
            Part(-14, -24, 0xbc, true),
            Part(-6, -24, 0xbd, true),
            Part(-6, -40, 0xc9, true),
            Part(-6, -32, 0xb9, true),
            Part(2, 24, 0xcf, true),
            Part(-6, 24, 0xce, true),
            Part(2, 32, 0xbf, true),
            Part(-6, 32, 0xbe, true),
            Part(-14, 24, 0xcd, true),
            Part(-6, 16, 0xbd, true),
            Part(-14, 16, 0xbc, true),
            Part(2, -40, 0xcb, true),
            Part(2, -32, 0xbb, true),
        ],
    ];
}
