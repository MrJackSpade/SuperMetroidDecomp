namespace SuperMetroid.Core.Assets;

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
    /// <summary>Unattributed Font-3 blank tile word $007F, used for empty rows and spaces without applying a line's palette bits.</summary>
    public const ushort BlankWord = 0x007f;
    /// <summary>Left shift placing a line's three-bit palette selector into native BG word bits 10-12.</summary>
    public const int PaletteShift = 10;
    /// <summary>Largest accepted BG palette selector, seven; this is a palette-row identity, not an RGB value.</summary>
    public const int MaximumPalette = 7;
    /// <summary>Font-3 tile used by the upper half of the credits ampersand.</summary>
    public const ushort LargeAmpersandTop = 0x007b;
    /// <summary>Font-3 tile used by the lower half of the credits ampersand.</summary>
    public const ushort LargeAmpersandBottom = 0x007c;
    /// <summary>Font-3 tile used by the upper half of the credits period.</summary>
    public const ushort LargePeriodTop = 0x007d;
    /// <summary>Font-3 tile used by the lower half of the credits period.</summary>
    public const ushort LargePeriodBottom = 0x007e;

    /// <summary>Shared calculator for the fixed ordered role metadata exposed by <see cref="Lines"/>.</summary>
    private static readonly CreditRoles Roles = new();
    /// <summary>Calculated read-only view of all 67 required credit roles in native order, with fixed one-/two-row styles and preceding blank-row counts; wording remains editable content.</summary>
    public static IReadOnlyList<CreditsLineDefinition> Lines => Roles;

    /// <summary>Provides generated role identifiers, font styles, and spacing for the fixed credits sequence.</summary>
    private sealed class CreditRoles : IReadOnlyList<CreditsLineDefinition>
    {
        /// <summary>Gets the number of required staff-credit roles.</summary>
        public int Count => 67;

        /// <summary>Builds the metadata for one role at its fixed position in the native credits order.</summary>
        /// <param name="index">Zero-based role position from 0 through 66.</param>
        /// <returns>The stable role key, heading/name style, and preceding blank-row count.</returns>
        /// <exception cref="IndexOutOfRangeException">The index is outside the required role sequence.</exception>
        public CreditsLineDefinition this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new IndexOutOfRangeException();
                string id = RoleId(index);
                bool heading = id.EndsWith("-heading", StringComparison.Ordinal);
                int blankRows = index == 0 ? InitialBlankRows : index == 17 ? 0
                    : heading ? SectionBlankRows : InterLineBlankRows;
                return new(id, heading ? CreditsLineStyle.Small : CreditsLineStyle.Large, blankRows);
            }
        }
        /// <summary>Enumerates role definitions in their native scrolling order.</summary>
        /// <returns>An enumerator yielding each of the 67 calculated role definitions.</returns>
        public IEnumerator<CreditsLineDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>Maps a fixed credits position to its stable, editable-document role key.</summary>
    /// <param name="index">Zero-based position in the 67-line role sequence.</param>
    /// <returns>The case-sensitive key required for that position.</returns>
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

    /// <summary>Maps one of the eight programming-staff slots to its role-key stem.</summary>
    /// <param name="index">Zero-based programming slot; values outside the seven named leads select the assistant role.</param>
    /// <returns>The role-key stem used for the corresponding heading and name.</returns>
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
    /// <summary>Encodes one supported credits glyph as an unattributed Font-3 tile word, before line palette bits are applied.</summary>
    /// <param name="character">Space or uppercase A-Z; large text additionally accepts ampersand and period. No case conversion or punctuation substitution is performed.</param>
    /// <param name="style">Small heading or large name/credit style.</param>
    /// <param name="bottom">Selects the lower tile row of a large glyph; nonblank small glyphs require false.</param>
    /// <returns>The character's tile index, or <see cref="BlankWord"/> for a space; no palette, priority, or flip attributes are added.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The nonblank character, style, or requested glyph half has no supported credits encoding.</exception>
    public static ushort CompileGlyph(char character, CreditsLineStyle style,
        bool bottom = false) => (style, character, bottom) switch
    {
        (_, ' ', _) => BlankWord,
        (CreditsLineStyle.Small, >= 'A' and <= 'Z', false) =>
            unchecked((ushort)(character - 'A')),
        (CreditsLineStyle.Large, '&', false) => LargeAmpersandTop,
        (CreditsLineStyle.Large, '&', true) => LargeAmpersandBottom,
        (CreditsLineStyle.Large, '.', false) => LargePeriodTop,
        (CreditsLineStyle.Large, '.', true) => LargePeriodBottom,
        (CreditsLineStyle.Large, >= 'A' and <= 'Z', _) =>
            EndingTextDefinitions.CompileGlyph(
                character, EndingTextStyle.CopyrightLarge, bottom),
        _ => throw new ArgumentOutOfRangeException(nameof(character), character,
            style == CreditsLineStyle.Small
                ? "Small credits text supports spaces and A-Z."
                : "Large credits text supports spaces, A-Z, ampersand, and period."),
    };

    /// <summary>Decodes the low ten-bit character selector of a native credits word, ignoring palette, priority, and flip attributes.</summary>
    /// <param name="word">Native BG tilemap word containing the Font-3 character selector.</param>
    /// <param name="style">The one-row small or two-row large font layout owning the selector.</param>
    /// <param name="bottom">Whether the selector belongs to the lower row of a large glyph; nonblank small glyphs require false.</param>
    /// <returns>The represented glyph; large decoding also recognizes Font-3 copyright digits through the shared ending-text decoder, although authored credits compilation excludes digits.</returns>
    /// <exception cref="InvalidDataException">The selector does not have a supported mapping for the requested style and glyph half.</exception>
    public static char DecodeGlyph(ushort word, CreditsLineStyle style,
        bool bottom = false)
    {
        ushort tile = (ushort)(word & 0x03ff);
        if (tile == BlankWord)
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
                tile, EndingTextStyle.CopyrightLarge, bottom);
        }
        throw new InvalidDataException(
            $"Credits {style} tile word ${word:X4} has no safe UTF-8 mapping.");
    }

    /// <summary>Native cartridge identities used only by extraction and parity verification.</summary>
    public static class Native
    {
        /// <summary>$97:EEFF, compressed 128-row credits source tilemap.</summary>
        public const int Tilemap = 0x97eeff;
        /// <summary>Expanded credits source size at $7F:0000..1FFF.</summary>
        public const int TilemapBytes = 0x2000;
        /// <summary>$8C:D91B, beginning of the retail credits row instruction stream.</summary>
        public const ushort InitialInstruction = 0xd91b;
        /// <summary>Bank containing the retail credits row instruction stream.</summary>
        public const byte InstructionBank = 0x8c;
        /// <summary>High-word-bit mask distinguishing bank-$8B callback commands from duration/source-row records in the credits stream.</summary>
        public const ushort CommandBit = 0x8000;
        /// <summary><c>Instruction_CreditsObject_TimerInY</c> at $8B:9A17.</summary>
        public const ushort SetTimer = 0x9a17;
        /// <summary><c>Instruction_CreditsObject_DecrementTimer_GotoYIfNonZero</c> at $8B:9A0D.</summary>
        public const ushort DecrementTimerAndGoto = 0x9a0d;
        /// <summary><c>CreditsObject_Func2</c> at $8B:F6FE, which ends the scrolling credits.</summary>
        public const ushort EndCredits = 0xf6fe;
        /// <summary><c>Instruction_CreditsObject_Delete</c> at $8B:99FE.</summary>
        public const ushort Delete = 0x99fe;
        /// <summary>Host extraction bound of 4096 interpreted stream operations, including repeated loop commands; separate from the expected 520 output rows.</summary>
        public const int MaximumOperations = 4096;
    }
}

/// <summary>Mutually exclusive Font-3 glyph layouts used by the fixed staff-credit roles.</summary>
public enum CreditsLineStyle : byte
{
    /// <summary>One 8-pixel-high tile row per glyph, used for section headings; authored text accepts spaces and uppercase letters.</summary>
    Small,
    /// <summary>Two vertically stacked tile rows per 8-by-16-pixel glyph, used for names and other credits; authored text additionally accepts ampersand and period.</summary>
    Large,
}

/// <summary>Compiled role, font style, and spacing for one line of editable staff-credit content.</summary>
/// <param name="Id">Stable case-sensitive role key required at this ordered document position; does not contain the editable displayed wording.</param>
/// <param name="Style">Glyph layout determining whether the line occupies one or two BG tile rows.</param>
/// <param name="BlankRowsBefore">Number of complete 32-cell blank tile rows inserted before the line; a scrolling-layout count, not an update or frame delay.</param>
public readonly record struct CreditsLineDefinition(
    string Id,
    CreditsLineStyle Style,
    int BlankRowsBefore);
