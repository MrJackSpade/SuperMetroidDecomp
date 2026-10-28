using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Compiled layout identities and timing boundaries for the scrolling staff credits.</summary>
public static class CreditsPresentationDefinitions
{
    /// <summary>Required schema revision of the editable ordered staff-credit line document.</summary>
    public const int Version = 1;
    /// <summary>JSON presentation filename containing selected wording, starting columns, and palettes for the fixed credit roles.</summary>
    public const string FileName = "ending-credits.json";
    /// <summary>Number of BG tile cells in each compiled credit row; text placement must fit within columns zero through 31.</summary>
    public const int TilemapWidth = 32;
    /// <summary>Eight blank tile rows preceding the first staff heading; this is a row count, not a frame delay.</summary>
    public const int InitialBlankRows = 8;
    /// <summary>Sixteen blank tile rows before a new section heading, except the initial heading and paired sound-effects heading.</summary>
    public const int SectionBlankRows = 16;
    /// <summary>One blank tile row before each non-heading line, separating large names within a section.</summary>
    public const int InterLineBlankRows = 1;
    /// <summary>Thirty-five blank tile rows following the final general-manager name, retaining the native scrolling tail.</summary>
    public const int TrailingBlankRows = 35;
    /// <summary>Total expanded scrolling stream length, 520 tile rows including headings, two-row names, and blank spacing; not the 128-row source tilemap size.</summary>
    public const int ExpectedCompiledRows = 520;
    /// <summary>The Font-3 blank tile, used for empty rows and spaces without applying a line's colour variant.</summary>
    public static BackgroundTile BlankTile { get; } = new(0x07f, 0, highPriority: false);
    /// <summary>Largest accepted colour variant, seven; this is a palette-row identity, not an RGB value.</summary>
    public const int MaximumPalette = 7;
    /// <summary>Font-3 tile used by the upper half of the credits ampersand.</summary>
    public const int LargeAmpersandTop = 0x007b;
    /// <summary>Font-3 tile used by the lower half of the credits ampersand.</summary>
    public const int LargeAmpersandBottom = 0x007c;
    /// <summary>Font-3 tile used by the upper half of the credits period.</summary>
    public const int LargePeriodTop = 0x007d;
    /// <summary>Font-3 tile used by the lower half of the credits period.</summary>
    public const int LargePeriodBottom = 0x007e;

    /// <summary>Fixed native role order without duplicating the 67 role descriptors in storage.</summary>
    private static readonly CreditRoles Roles = new();
    /// <summary>Calculated read-only view of all 67 required credit roles in native order, with fixed one-/two-row styles and preceding blank-row counts; wording remains editable content.</summary>
    public static IReadOnlyList<CreditsLineDefinition> Lines => Roles;

    /// <summary>Calculates role identity, glyph style, and spacing from native line order.</summary>
    private sealed class CreditRoles : IReadOnlyList<CreditsLineDefinition>
    {
        /// <summary>The 67 staff roles present in the retail credits stream.</summary>
        public int Count => 67;
        /// <summary>Assigns headings a small font and section spacing, except for the paired sound-effects heading.</summary>
        public CreditsLineDefinition this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
                string id = RoleId(index);
                bool heading = id.EndsWith("-heading", StringComparison.Ordinal);
                int blankRows = index == 0 ? InitialBlankRows : index == 17 ? 0
                    : heading ? SectionBlankRows : InterLineBlankRows;
                return new(id, heading ? CreditsLineStyle.Small : CreditsLineStyle.Large, blankRows);
            }
        }
        /// <summary>Yields all fixed role descriptors in displayed order.</summary>
        public IEnumerator<CreditsLineDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Adapts the ordered role sequence for non-generic enumeration.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>Maps a retail line ordinal to its stable editable-document role key.</summary>
    private static string RoleId(int index) => index switch
    {
        0 => "staff-heading",
        1 => "producer-heading",
        2 => "producer-name",
        3 => "director-heading",
        4 => "director-name",
        5 => "background-designers-heading",
        >= 6 and <= 8 => $"background-designer-{index - 5}",
        9 => "object-designers-heading",
        10 or 11 => $"object-designer-{index - 9}",
        12 => "samus-original-designer-heading",
        13 => "samus-original-designer-name",
        14 => "samus-designer-heading",
        15 => "samus-designer-name",
        16 => "sound-program-heading",
        17 => "sound-effects-heading",
        18 => "sound-programmer-name",
        19 => "music-composers-heading",
        20 or 21 => $"music-composer-{index - 19}",
        >= 22 and <= 37 => ProgrammingRole((index - 22) / 2) + (index % 2 == 0 ? "-heading" : "-name"),
        38 => "coordinators-heading",
        39 or 40 => $"coordinator-{index - 38}",
        41 => "printed-art-work-heading",
        >= 42 and <= 47 => $"printed-art-work-{index - 41}",
        48 => "special-thanks-heading",
        >= 49 and <= 63 => FormattableString.Invariant($"special-thanks-{index - 48:D2}"),
        64 => "special-thanks-r-and-d",
        65 => "general-manager-heading",
        _ => "general-manager-name",
    };

    /// <summary>Names the eight programming roles shared by their heading and name lines.</summary>
    private static string ProgrammingRole(int index) => index switch
    {
        0 => "program-director",
        1 => "system-coordinator",
        2 => "system-programmer",
        3 => "samus-programmer",
        4 => "event-programmer",
        5 => "enemy-programmer",
        6 => "map-programmer",
        _ => "assistant-programmer",
    };
    /// <summary>The Font-3 tile of one supported credits glyph, in colour variant zero before the line's variant is applied.</summary>
    /// <param name="character">Space or uppercase A-Z; large text additionally accepts ampersand and period. No case conversion or punctuation substitution is performed.</param>
    /// <param name="style">Small heading or large name/credit style.</param>
    /// <param name="bottom">Selects the lower tile row of a large glyph; nonblank small glyphs require false.</param>
    /// <returns>The character's tile, or <see cref="BlankTile"/> for a space; no colour variant, priority, or mirroring is added.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The nonblank character, style, or requested glyph half has no supported credits encoding.</exception>
    public static BackgroundTile CompileGlyph(char character, CreditsLineStyle style,
        bool bottom = false) => new CreditsGlyphKey(style, character, bottom) switch
    {
        (_, ' ', _) => BlankTile,
        (CreditsLineStyle.Small, >= 'A' and <= 'Z', false) => Plain(character - 'A'),
        (CreditsLineStyle.Large, '&', false) => Plain(LargeAmpersandTop),
        (CreditsLineStyle.Large, '&', true) => Plain(LargeAmpersandBottom),
        (CreditsLineStyle.Large, '.', false) => Plain(LargePeriodTop),
        (CreditsLineStyle.Large, '.', true) => Plain(LargePeriodBottom),
        (CreditsLineStyle.Large, >= 'A' and <= 'Z', _) =>
            EndingTextDefinitions.CompileGlyph(
                character, EndingTextStyle.CopyrightLarge, bottom),
        _ => throw new ArgumentOutOfRangeException(nameof(character), character,
            style == CreditsLineStyle.Small
                ? "Small credits text supports spaces and A-Z."
                : "Large credits text supports spaces, A-Z, ampersand, and period."),
    };

    /// <summary>Decodes the Font-3 tile a credits cell shows, ignoring its colour variant, priority and mirroring.</summary>
    /// <param name="cell">The placed tile.</param>
    /// <param name="style">The one-row small or two-row large font layout owning the selector.</param>
    /// <param name="bottom">Whether the selector belongs to the lower row of a large glyph; nonblank small glyphs require false.</param>
    /// <returns>The represented glyph; large decoding also recognizes Font-3 copyright digits through the shared ending-text decoder, although authored credits compilation excludes digits.</returns>
    /// <exception cref="InvalidDataException">The selector does not have a supported mapping for the requested style and glyph half.</exception>
    public static char DecodeGlyph(BackgroundTile cell, CreditsLineStyle style,
        bool bottom = false)
    {
        int tile = cell.Tile;
        if (tile == BlankTile.Tile)
            return ' ';
        if (style == CreditsLineStyle.Small && !bottom && tile <= 25)
            return (char)('A' + tile);
        if (style == CreditsLineStyle.Large)
        {
            if (tile == (bottom ? LargeAmpersandBottom : LargeAmpersandTop))
                return '&';
            if (tile == (bottom ? LargePeriodBottom : LargePeriodTop))
                return '.';
            return EndingTextDefinitions.DecodeGlyph(
                Plain(tile), EndingTextStyle.CopyrightLarge, bottom);
        }
        throw new InvalidDataException(
            $"Credits {style} tile {cell} has no safe UTF-8 mapping.");
    }

    /// <summary>Font-3 tile <paramref name="tile"/> in colour variant zero, the lower priority tier and unmirrored.</summary>
    private static BackgroundTile Plain(int tile) => new(tile, 0, highPriority: false);
}
