using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Stable names, extraction sources and stock anchors for the escape timer presentation.</summary>
public static class EscapeTimerPresentationDefinitions
{
    /// <summary>Supported JSON revision for timer compositions, palette, pixel anchors, and digit spacing; countdown behavior is not authored in this schema.</summary>
    public const int Version = 1;
    /// <summary>Installation-relative JSON layout filename, separate from the timer font's indexed PNG artwork.</summary>
    public const string FileName = "escape-timer.json";
    /// <summary>Maximum ordered OAM parts permitted in each label or digit composition; this is a per-frame schema limit, not reserved capacity for the whole timer.</summary>
    public const int MaximumParts = 128;

    /// <summary>$80:9FE8: first decimal digit's two-object, twelve-byte spritemap.</summary>
    private const ushort FirstDigitSpritemap = 0x9fe8;
    /// <summary>Resolves the $80:9FD4 decimal digit selection from consecutive two-object records.</summary>
    public static ushort DigitSpritemapPointer(int digit) => (uint)digit < 10
        ? (ushort)(FirstDigitSpritemap + digit * 12)
        : throw new ArgumentOutOfRangeException(nameof(digit));
    /// <summary>The five-part <c>TIME</c> label spritemap at <c>$80:A060</c>.</summary>
    public const int LabelSpritemap = 0x80a060;

    /// <summary>The timer renderer inherits its spritemap pointers from bank $80.</summary>
    public const int SpritemapBank = 0x800000;

    /// <summary>Canonical composition key for the TIME label and both separators, corresponding to the five-part Spritemap_Timer_TIME at $80:A060.</summary>
    public const string LabelFrame = "Label";
    /// <summary>Returns the canonical composition key for one decimal glyph shared by the minutes, seconds, and centiseconds pairs.</summary>
    /// <param name="digit">Decoded decimal value 0 through 9, not a packed-BCD byte.</param>
    /// <returns>The exact JSON frame key Digit.0 through Digit.9.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="digit"/> is outside 0..9.</exception>
    public static string DigitFrame(int digit) => (uint)digit < 10
        ? $"Digit.{digit}"
        : throw new ArgumentOutOfRangeException(nameof(digit));

    /// <summary>Required JSON anchor keys: the label origin and the tens-digit origins of the three time pairs; their coordinates are pixel offsets from the live timer origin.</summary>
    /// <remarks>The readonly field fixes the shared array reference, not its elements; callers must not modify this schema-name array.</remarks>
    public static readonly string[] AnchorNames = ["Label", "Minutes", "Seconds", "Centiseconds"];
    /// <summary>$80:9FEA..A05F: decimal glyphs are eight-pixel-wide, two-tile-high small OBJs.</summary>
    private const int GlyphWidth = 8;
    /// <summary>$80:9FF2: first upper decimal glyph tile; lower glyphs follow all ten upper glyphs.</summary>
    private const int FirstDigitTile = 0x1e0;
    /// <summary>$80:A065..A07A: twenty decimal tiles are followed by two separators, then three TIME label tiles.</summary>
    private const int DecimalDigitCount = 10;
    /// <summary>$80:A065..A06F: TIME uses three consecutive font tiles, emitted right-to-left.</summary>
    private const int LabelTileCount = EscapeTimerTileAtlasFormat.TileCount - 2 * DecimalDigitCount - 2;
    /// <summary>$80:9FEA..A07A: timer OBJs use highest sprite priority and inherit the selected timer palette.</summary>
    private const int TimerPriority = 3;

    /// <summary>$80:9F7B/9F84/9F8D: three two-digit groups separated by one cell, centered across eight cells.</summary>
    internal static MapLabelPoint DefaultAnchor(string name)
    {
        if (name == "Label") return new(0, 0);
        int group = name switch { "Minutes" => 0, "Seconds" => 1, "Centiseconds" => 2,
            _ => throw new ArgumentOutOfRangeException(nameof(name)) };
        return new((3 * group * 2 - 7) * GlyphWidth / 2, 0);
    }

    /// <summary>Exact ordered native digit/label geometry. Font ink pixels remain owned by EscapeTimerTileAtlas.</summary>
    internal static SpriteComposition DefaultFrame(string name) => SpriteComposition.FromCalculated(DefaultParts(name));
    internal static PartSequence DefaultParts(string name)
    {
        if (name == LabelFrame) return new(-1);
        if (name.StartsWith("Digit.", StringComparison.Ordinal) && int.TryParse(name.AsSpan(6), out int digit)
            && (uint)digit < DecimalDigitCount) return new(digit);
        throw new ArgumentOutOfRangeException(nameof(name));
    }
    internal readonly struct PartSequence(int digit) : IReadOnlyList<CompiledSpritePart>
    {
        public int Count => digit < 0 ? LabelTileCount + 2 : 2;
        public CompiledSpritePart this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
                int x, y, tile;
                if (digit >= 0)
                {
                    x = -GlyphWidth / 2;
                    y = -index * GlyphWidth;
                    tile = FirstDigitTile + digit + (1 - index) * DecimalDigitCount;
                }
                else if (index < LabelTileCount)
                {
                    x = -(index + 2) * GlyphWidth;
                    y = -2 * GlyphWidth;
                    tile = FirstDigitTile + 2 * DecimalDigitCount + 2 + LabelTileCount - 1 - index;
                }
                else
                {
                    int separator = Count - 1 - index;
                    x = (-2 + 3 * separator) * GlyphWidth;
                    y = -GlyphWidth;
                    tile = FirstDigitTile + 2 * DecimalDigitCount + separator;
                }
                return new(SnesSpritemapXWord.Create(x, false), unchecked((byte)(sbyte)y),
                    SnesObjAttributeWord.Create(tile, 0, TimerPriority), true);
            }
        }
        public IEnumerator<CompiledSpritePart> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
