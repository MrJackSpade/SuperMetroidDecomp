using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Escape.Artwork;

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
    /// <summary>Every composition: the TIME label, then the digits 0 through 9.</summary>
    public static IReadOnlyList<EscapeTimerGlyph> Glyphs { get; } = Enum.GetValues<EscapeTimerGlyph>();
    /// <summary>Every anchor: the label origin, then the tens-digit origins of the three time pairs.</summary>
    public static IReadOnlyList<EscapeTimerAnchor> Anchors { get; } = Enum.GetValues<EscapeTimerAnchor>();

    /// <summary>The glyph that draws the decimal value <paramref name="digit"/>.</summary>
    /// <param name="digit">Decoded decimal value 0 through 9, not a packed-BCD byte.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="digit"/> is outside 0..9.</exception>
    public static EscapeTimerGlyph DigitGlyph(int digit) =>
        (uint)digit < DigitGlyphs.Length ? DigitGlyphs[digit] : throw new ArgumentOutOfRangeException(nameof(digit));

    /// <summary>The digit glyphs, indexed by the decimal value each draws.</summary>
    private static readonly EscapeTimerGlyph[] DigitGlyphs =
    [
        EscapeTimerGlyph.Digit0, EscapeTimerGlyph.Digit1, EscapeTimerGlyph.Digit2, EscapeTimerGlyph.Digit3, EscapeTimerGlyph.Digit4,
        EscapeTimerGlyph.Digit5, EscapeTimerGlyph.Digit6, EscapeTimerGlyph.Digit7, EscapeTimerGlyph.Digit8, EscapeTimerGlyph.Digit9,
    ];

    /// <summary>The decimal value a digit glyph draws.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="glyph"/> is the label.</exception>
    public static int DigitValue(EscapeTimerGlyph glyph) => glyph switch
    {
        EscapeTimerGlyph.Digit0 => 0,
        EscapeTimerGlyph.Digit1 => 1,
        EscapeTimerGlyph.Digit2 => 2,
        EscapeTimerGlyph.Digit3 => 3,
        EscapeTimerGlyph.Digit4 => 4,
        EscapeTimerGlyph.Digit5 => 5,
        EscapeTimerGlyph.Digit6 => 6,
        EscapeTimerGlyph.Digit7 => 7,
        EscapeTimerGlyph.Digit8 => 8,
        EscapeTimerGlyph.Digit9 => 9,
        EscapeTimerGlyph.Label => throw new ArgumentOutOfRangeException(nameof(glyph)),
        _ => throw new ArgumentOutOfRangeException(nameof(glyph)),
    };

    /// <summary>The JSON frame key of a glyph: Label, or Digit.0 through Digit.9.</summary>
    public static string FrameName(EscapeTimerGlyph glyph) => glyph switch
    {
        EscapeTimerGlyph.Label => LabelFrame,
        EscapeTimerGlyph.Digit0 => "Digit.0",
        EscapeTimerGlyph.Digit1 => "Digit.1",
        EscapeTimerGlyph.Digit2 => "Digit.2",
        EscapeTimerGlyph.Digit3 => "Digit.3",
        EscapeTimerGlyph.Digit4 => "Digit.4",
        EscapeTimerGlyph.Digit5 => "Digit.5",
        EscapeTimerGlyph.Digit6 => "Digit.6",
        EscapeTimerGlyph.Digit7 => "Digit.7",
        EscapeTimerGlyph.Digit8 => "Digit.8",
        EscapeTimerGlyph.Digit9 => "Digit.9",
        _ => throw new ArgumentOutOfRangeException(nameof(glyph)),
    };

    /// <summary>The JSON frame key of the glyph drawing the decimal value <paramref name="digit"/>.</summary>
    public static string DigitFrame(int digit) => FrameName(DigitGlyph(digit));

    /// <summary>The JSON key of an anchor, whose coordinates are pixel offsets from the live timer origin.</summary>
    public static string AnchorName(EscapeTimerAnchor anchor) => anchor switch
    {
        EscapeTimerAnchor.Label => "Label",
        EscapeTimerAnchor.Minutes => "Minutes",
        EscapeTimerAnchor.Seconds => "Seconds",
        EscapeTimerAnchor.Centiseconds => "Centiseconds",
        _ => throw new ArgumentOutOfRangeException(nameof(anchor)),
    };
    /// <summary>The last OBJ character a square part of <paramref name="size"/> pixels reads from <paramref name="tileNumber"/>.</summary>
    internal static int LastCharacterRead(int tileNumber, int size) =>
        tileNumber + (size / 8 - 1) * (SpriteArtwork.CharactersPerRow + 1);
    /// <summary>$80:9FEA..A05F: decimal glyphs are eight-pixel-wide, two-tile-high small OBJs.</summary>
    private const int GlyphWidth = 8;
    /// <summary>$80:9FF2: first upper decimal glyph tile; lower glyphs follow all ten upper glyphs.</summary>
    private const int FirstDigitTile = 0x1e0;
    /// <summary>OBJ character $1E0, where the timer's first character lands; timer artwork character 0.</summary>
    internal const int FirstCharacter = FirstDigitTile;
    /// <summary>$80:A065..A07A: twenty decimal tiles are followed by two separators, then three TIME label tiles.</summary>
    private const int DecimalDigitCount = 10;
    /// <summary>$80:A065..A06F: TIME uses three consecutive font tiles, emitted right-to-left.</summary>
    private const int LabelTileCount = EscapeTimerTileAtlasFormat.TileCount - 2 * DecimalDigitCount - 2;
    /// <summary>$80:9FEA..A07A: timer OBJs use highest sprite priority and inherit the selected timer palette.</summary>
    internal const int TimerPriority = 3;

    /// <summary>$80:9F7B/9F84/9F8D: three two-digit groups separated by one cell, centered across eight cells.</summary>
    internal static Offset16<OamPixels> DefaultAnchor(EscapeTimerAnchor anchor) => anchor switch
    {
        EscapeTimerAnchor.Label => new(0, 0),
        EscapeTimerAnchor.Minutes => GroupAnchor(0),
        EscapeTimerAnchor.Seconds => GroupAnchor(1),
        EscapeTimerAnchor.Centiseconds => GroupAnchor(2),
        _ => throw new ArgumentOutOfRangeException(nameof(anchor)),
    };

    /// <summary>The tens-digit origin of the <paramref name="group"/>th two-digit group.</summary>
    private static Offset16<OamPixels> GroupAnchor(int group) => new(checked((short)((3 * group * 2 - 7) * GlyphWidth / 2)), 0);

    /// <summary>Exact ordered native digit/label geometry. Font ink pixels remain owned by EscapeTimerTileAtlas.</summary>
    internal static SpriteComposition DefaultFrame(EscapeTimerGlyph glyph) => SpriteComposition.FromCalculated(DefaultParts(glyph));
    /// <summary>Selects the calculated TIME label or decimal glyph parts.</summary>
    internal static PartSequence DefaultParts(EscapeTimerGlyph glyph) =>
        new(glyph == EscapeTimerGlyph.Label ? -1 : DigitValue(glyph));
    /// <summary>Yields the native small-OBJ tile order for one digit or the TIME label and separators.</summary>
    internal readonly struct PartSequence(int digit) : IReadOnlyList<CompiledSpritePart>
    {
        /// <summary>Digits use two parts; the label uses three letters and two separator parts.</summary>
        public int Count => digit < 0 ? LabelTileCount + 2 : 2;
        /// <summary>Calculates one tile, offset, and priority from the selected native frame shape.</summary>
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
                return new(SnesSpritemapXWord.Create(x, false), SnesSpritemapYByte.Create((sbyte)y),
                    SnesObjAttributeWord.Create(tile, 0, TimerPriority), true);
            }
        }
        /// <summary>Visits parts in native spritemap order.</summary>
        public IEnumerator<CompiledSpritePart> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Exposes native part order to non-generic callers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
