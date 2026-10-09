using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// World-area lettering at82:CC6B..CD66. Tiles use the contiguous alphabet6A+(letter-'A'),
/// with one small, unflipped, priority3 sprite per letter and live caller palette.
/// Single lines use Y=-4; Wrecked Ship draws SHIP at Y=0 before WRECKED at Y=-8.
/// Horizontal placement advances by the eight-pixel glyph cell, with authored line origins
/// and individual nonstandard advances. These are optical typography inputs: the same I/A
/// glyph pair advances7 in Crateria/Tourian and8 in Maridia, while Wrecked Ship's lower line
/// is deliberately staggered. A uniform font-metric rule would change those chosen layouts;
/// reciting their per-word choices in code would disguise the same authored composition.
/// </summary>
internal sealed class WorldMapLabelComposition
{
    /// <summary>
    /// Identifies the world area whose label text and line arrangement are rendered.
    /// </summary>
    private readonly ushort identity;

    /// <summary>
    /// X coordinate of the rightmost glyph in the upper label line, or the only line for single-line labels.
    /// </summary>
    private readonly int upperOrigin;

    /// <summary>
    /// X coordinate of the rightmost glyph in Wrecked Ship's lower <c>SHIP</c> line.
    /// </summary>
    private readonly int lowerOrigin;

    /// <summary>
    /// Per-glyph horizontal spacing overrides for layouts whose authored advance differs from the default eight-pixel cell.
    /// </summary>
    private readonly Dictionary<int, int>? letterAdvances;

    /// <summary>
    /// General compiled sprite composition used when the supplied parts do not match the supported world-label lettering pattern.
    /// </summary>
    private readonly SpriteComposition? authored;

    /// <summary>
    /// Stores either the compact label-layout parameters or the general composition fallback selected by <see cref="Compile"/>.
    /// </summary>
    /// <param name="identity">The world-area identifier used to choose the label text and line layout.</param>
    /// <param name="upperOrigin">The rightmost glyph's X offset on the upper or single line.</param>
    /// <param name="lowerOrigin">The rightmost glyph's X offset on Wrecked Ship's lower line.</param>
    /// <param name="letterAdvances">Optional per-glyph spacing values that preserve authored optical adjustments.</param>
    /// <param name="authored">A general sprite composition to draw when the parts cannot use the compact label representation.</param>
    private WorldMapLabelComposition(ushort identity, int upperOrigin, int lowerOrigin,
        Dictionary<int, int>? letterAdvances, SpriteComposition? authored)
    { this.identity = identity; this.upperOrigin = upperOrigin; this.lowerOrigin = lowerOrigin;
        this.letterAdvances = letterAdvances; this.authored = authored; }

    /// <summary>Area names are semantic text content; named cases select the map-label wording.</summary>
    private static string Text(ushort id) => id switch
    {
        MapSpriteDefinitions.WorldCrateria => "CRATERIA",
        MapSpriteDefinitions.WorldBrinstar => "BRINSTAR",
        MapSpriteDefinitions.WorldNorfair => "NORFAIR",
        MapSpriteDefinitions.WorldWreckedShip => "WRECKEDSHIP",
        MapSpriteDefinitions.WorldMaridia => "MARIDIA",
        MapSpriteDefinitions.WorldTourian => "TOURIAN",
        _ => throw new ArgumentOutOfRangeException(nameof(id)),
    };

    /// <summary>
    /// Returns the authored line Y offset for a glyph, separating Wrecked Ship's two words and centering other labels on one line.
    /// </summary>
    /// <param name="id">The world-area identifier controlling the label's line arrangement.</param>
    /// <param name="index">The glyph index in the rendered label sequence.</param>
    /// <returns>The glyph's vertical offset relative to the label origin.</returns>
    private static int VerticalOffset(ushort id, int index) =>
        id == MapSpriteDefinitions.WorldWreckedShip ? (index < 4 ? 0 : -8) : -4;

    /// <summary>
    /// Compacts recognized map-label sprite parts into text and spacing data, retaining the generic sprite compiler as a fallback.
    /// </summary>
    /// <param name="id">The world-area identity that determines the expected label text and line structure.</param>
    /// <param name="parts">The authored sprite parts whose tile sequence and layout are inspected.</param>
    /// <param name="name">The diagnostic name passed to the generic compiler when compact recognition fails.</param>
    /// <returns>A compact label composition for a recognized layout, or a general sprite composition otherwise.</returns>
    internal static WorldMapLabelComposition Compile(ushort id, SpriteVisualPart[] parts, string name)
    {
        string text = Text(id);
        bool regular = parts.Length == text.Length;
        for (int index = 0; regular && index < parts.Length; index++)
        {
            var part = parts[index];
            int tile = 0x6a + text[text.Length - 1 - index] - 'A';
            regular = part is not null && part.OffsetX is >= -256 and <= 255 &&
                part.OffsetY == VerticalOffset(id, index) && part.TileColumn == tile % 16 && part.TileRow == tile / 16 &&
                part.Size == 8 && part.Priority == 3 && part.Palette is null && !part.FlipX && !part.FlipY;
        }
        if (!regular) return new(id, 0, 0, null, MenuSpriteCompiler.Compile(parts, name));
        Dictionary<int, int>? advances = null;
        bool twoLines = id == MapSpriteDefinitions.WorldWreckedShip;
        for (int index = 0; index < parts.Length - 1; index++)
        {
            if (twoLines && index == 3) continue;
            int advance = parts[index].OffsetX - parts[index + 1].OffsetX;
            if (advance != 8) (advances ??= []).Add(index, advance);
        }
        return new(id, parts[^1].OffsetX, twoLines ? parts[3].OffsetX : 0, advances, null);
    }

    /// <summary>
    /// Adds this label's glyph sprites to OAM using the caller's screen origin and live palette selection.
    /// </summary>
    /// <param name="oam">The OAM buffer that receives the label sprites.</param>
    /// <param name="x">The screen-space X origin for the label.</param>
    /// <param name="y">The screen-space Y origin for the label.</param>
    /// <param name="paletteBits">The palette bits supplied by the current map-label caller.</param>
    internal void Draw(OamBuffer oam, ushort x, ushort y, ushort paletteBits)
    {
        if (authored is not null) { authored.DrawOnScreen(oam, x, y, paletteBits); return; }
        _ = SnesObjAttributeWord.FromPaletteBits(paletteBits);
        string text = Text(identity);
        for (int index = 0; index < text.Length; index++)
        {
            int tile = 0x6a + text[text.Length - 1 - index] - 'A';
            oam.AddOnScreenSpritePart(SnesSpritemapXWord.Create(HorizontalOffset(index, text.Length), false),
                unchecked((byte)VerticalOffset(identity, index)),
                SnesObjAttributeWord.Create(tile, 0, 3).WithPaletteBits(paletteBits), x, y);
        }
    }

    /// <summary>
    /// Computes a glyph's authored horizontal position from the line's rightmost origin and intervening advances.
    /// </summary>
    /// <param name="index">The glyph's index in the label text.</param>
    /// <param name="count">The total number of glyphs in the label.</param>
    /// <returns>The glyph's X offset relative to the supplied screen origin.</returns>
    private int HorizontalOffset(int index, int count)
    {
        bool bottom = identity == MapSpriteDefinitions.WorldWreckedShip && index < 4;
        int leftmostIndex = bottom ? 3 : count - 1;
        int x = bottom ? lowerOrigin : upperOrigin;
        for (int next = leftmostIndex - 1; next >= index; next--)
            x += letterAdvances is not null && letterAdvances.TryGetValue(next, out int advance) ? advance : 8;
        return x;
    }
}
