using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>File-copy arrow compositions $82:CDCD/CE15/CE5D/CECD and placement $82:BABA-BB2F.</summary>
public static class FileCopyArrowDefinitions
{
    /// <summary>$81:98E6: initial delay before the first arrow palette rotation.</summary>
    public const int InitialPaletteDelay = 8;
    /// <summary>$81:9A09: subsequent arrow palette rotation delay.</summary>
    internal const int PaletteDelay = 4;
    /// <summary>The sprite row whose colours the arrows rotate.</summary>
    internal const int SpriteRow = 1;
    /// <summary>The first colour of the row the arrows rotate.</summary>
    internal const int FirstColour = 1;
    /// <summary>$81:9A17-9A27: rotate seven colors, preserving transparent color zero.</summary>
    internal const int ColorCount = 7;
    /// <summary>$82:BB0E: all file-copy arrows use screen X = $14.</summary>
    internal const ushort OriginX = 20;

    /// <summary>Chooses one of four up/down, short/long arrows and its screen Y from two distinct file-slot indexes.</summary>
    internal static FileCopyArrowPlacement Select(int source, int destination)
    {
        if ((uint)source >= 3 || (uint)destination >= 3 || source == destination)
            throw new ArgumentOutOfRangeException(nameof(destination));
        int distance = Math.Abs(source - destination);
        int shape = (distance == 2 ? 2 : 0) + (source > destination ? 1 : 0);
        return new FileCopyArrowPlacement(shape, (ushort)(distance == 2 ? 104 : 88 + 32 * Math.Min(source, destination)));
    }

    /// <summary>The file-copy arrow shapes: short and long, downward and upward.</summary>
    internal const int ShapeCount = 4;

    /// <summary>The sheet frame showing arrow shape <paramref name="shape"/>.</summary>
    public static string FrameName(int shape) => $"CopyArrow.{shape}";

    /// <summary>Exposes the selected native OAM part order for the four file-copy arrow shapes.</summary>
    internal static ReadOnlySpan<CompiledSpritePart> Parts(int shape) => Shapes[shape];
    /// <summary>Builds one 16-pixel OBJ part in palette three, optionally flipping it vertically for an upward arrow.</summary>
    private static CompiledSpritePart Part(Offset16<SpritePixels> offset, int tile, SnesTileFlipFlags flip) => new(
        SnesSpritemapXWord.Create(offset.X, false), SnesSpritemapYByte.Create(offset.Y),
        SnesObjAttributeWord.Create(tile, 1, 3, flip), true);

    /// <summary>Four native sprite orders, including interleaved shaft and arrowhead tiles for short/long and upward/downward arrows.</summary>
    private static readonly CompiledSpritePart[][] Shapes =
    [
        [
            Part(new(2, -16), 0xcf, SnesTileFlipFlags.None),
            Part(new(-6, -16), 0xce, SnesTileFlipFlags.None),
            Part(new(2, -24), 0xbf, SnesTileFlipFlags.None),
            Part(new(-6, -24), 0xbe, SnesTileFlipFlags.None),
            Part(new(-6, 8), 0xb9, SnesTileFlipFlags.None),
            Part(new(-6, 16), 0xc9, SnesTileFlipFlags.None),
            Part(new(2, 16), 0xcb, SnesTileFlipFlags.None),
            Part(new(2, 8), 0xbb, SnesTileFlipFlags.None),
            Part(new(-14, 8), 0xb8, SnesTileFlipFlags.None),
            Part(new(-14, 0), 0xbc, SnesTileFlipFlags.None),
            Part(new(-6, 0), 0xbd, SnesTileFlipFlags.None),
            Part(new(-14, -16), 0xcd, SnesTileFlipFlags.None),
            Part(new(-6, -8), 0xbd, SnesTileFlipFlags.None),
            Part(new(-14, -8), 0xbc, SnesTileFlipFlags.None),
        ],
        [
            Part(new(-14, -16), 0xb8, SnesTileFlipFlags.Vertical),
            Part(new(-14, -8), 0xbc, SnesTileFlipFlags.Vertical),
            Part(new(-6, -8), 0xbd, SnesTileFlipFlags.Vertical),
            Part(new(-6, -24), 0xc9, SnesTileFlipFlags.Vertical),
            Part(new(-6, -16), 0xb9, SnesTileFlipFlags.Vertical),
            Part(new(2, 8), 0xcf, SnesTileFlipFlags.Vertical),
            Part(new(-6, 8), 0xce, SnesTileFlipFlags.Vertical),
            Part(new(2, 16), 0xbf, SnesTileFlipFlags.Vertical),
            Part(new(-6, 16), 0xbe, SnesTileFlipFlags.Vertical),
            Part(new(-14, 8), 0xcd, SnesTileFlipFlags.Vertical),
            Part(new(-6, 0), 0xbd, SnesTileFlipFlags.Vertical),
            Part(new(-14, 0), 0xbc, SnesTileFlipFlags.Vertical),
            Part(new(2, -24), 0xcb, SnesTileFlipFlags.Vertical),
            Part(new(2, -16), 0xbb, SnesTileFlipFlags.Vertical),
        ],
        [
            Part(new(-6, 8), 0xbd, SnesTileFlipFlags.None),
            Part(new(-14, 8), 0xbc, SnesTileFlipFlags.None),
            Part(new(-6, 0), 0xbd, SnesTileFlipFlags.None),
            Part(new(-14, 0), 0xbc, SnesTileFlipFlags.None),
            Part(new(-6, -8), 0xbd, SnesTileFlipFlags.None),
            Part(new(-14, -8), 0xbc, SnesTileFlipFlags.None),
            Part(new(-6, -16), 0xbd, SnesTileFlipFlags.None),
            Part(new(-14, -16), 0xbc, SnesTileFlipFlags.None),
            Part(new(-14, 24), 0xb8, SnesTileFlipFlags.None),
            Part(new(-14, 16), 0xbc, SnesTileFlipFlags.None),
            Part(new(-6, 16), 0xbd, SnesTileFlipFlags.None),
            Part(new(-6, 32), 0xc9, SnesTileFlipFlags.None),
            Part(new(-6, 24), 0xb9, SnesTileFlipFlags.None),
            Part(new(2, -32), 0xcf, SnesTileFlipFlags.None),
            Part(new(-6, -32), 0xce, SnesTileFlipFlags.None),
            Part(new(2, -40), 0xbf, SnesTileFlipFlags.None),
            Part(new(-6, -40), 0xbe, SnesTileFlipFlags.None),
            Part(new(-14, -32), 0xcd, SnesTileFlipFlags.None),
            Part(new(-6, -24), 0xbd, SnesTileFlipFlags.None),
            Part(new(-14, -24), 0xbc, SnesTileFlipFlags.None),
            Part(new(2, 32), 0xcb, SnesTileFlipFlags.None),
            Part(new(2, 24), 0xbb, SnesTileFlipFlags.None),
        ],
        [
            Part(new(-6, -16), 0xbd, SnesTileFlipFlags.Vertical),
            Part(new(-14, -16), 0xbc, SnesTileFlipFlags.Vertical),
            Part(new(-6, -8), 0xbd, SnesTileFlipFlags.Vertical),
            Part(new(-14, -8), 0xbc, SnesTileFlipFlags.Vertical),
            Part(new(-6, 0), 0xbd, SnesTileFlipFlags.Vertical),
            Part(new(-14, 0), 0xbc, SnesTileFlipFlags.Vertical),
            Part(new(-6, 8), 0xbd, SnesTileFlipFlags.Vertical),
            Part(new(-14, 8), 0xbc, SnesTileFlipFlags.Vertical),
            Part(new(-14, -32), 0xb8, SnesTileFlipFlags.Vertical),
            Part(new(-14, -24), 0xbc, SnesTileFlipFlags.Vertical),
            Part(new(-6, -24), 0xbd, SnesTileFlipFlags.Vertical),
            Part(new(-6, -40), 0xc9, SnesTileFlipFlags.Vertical),
            Part(new(-6, -32), 0xb9, SnesTileFlipFlags.Vertical),
            Part(new(2, 24), 0xcf, SnesTileFlipFlags.Vertical),
            Part(new(-6, 24), 0xce, SnesTileFlipFlags.Vertical),
            Part(new(2, 32), 0xbf, SnesTileFlipFlags.Vertical),
            Part(new(-6, 32), 0xbe, SnesTileFlipFlags.Vertical),
            Part(new(-14, 24), 0xcd, SnesTileFlipFlags.Vertical),
            Part(new(-6, 16), 0xbd, SnesTileFlipFlags.Vertical),
            Part(new(-14, 16), 0xbc, SnesTileFlipFlags.Vertical),
            Part(new(2, -40), 0xcb, SnesTileFlipFlags.Vertical),
            Part(new(2, -32), 0xbb, SnesTileFlipFlags.Vertical),
        ],
    ];
}
