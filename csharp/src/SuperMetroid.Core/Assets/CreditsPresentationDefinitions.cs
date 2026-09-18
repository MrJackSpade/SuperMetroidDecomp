namespace SuperMetroid.Core.Assets;

/// <summary>Compiled layout identities and timing boundaries for the scrolling staff credits.</summary>
public static class CreditsPresentationDefinitions
{
    public const int Version = 1;
    public const string FileName = "ending-credits.json";
    public const int TilemapWidth = 32;
    public const int TilemapHeight = 32;
    public const int InitialBlankRows = 8;
    public const int SectionBlankRows = 16;
    public const int InterLineBlankRows = 1;
    public const int TrailingBlankRows = 35;
    public const int ExpectedCompiledRows = 520;
    public const ushort BlankWord = 0x007f;
    public const int PaletteShift = 10;
    public const int MaximumPalette = 7;
    /// <summary>Font-3 tile used by the upper half of the credits ampersand.</summary>
    public const ushort LargeAmpersandTop = 0x007b;
    /// <summary>Font-3 tile used by the lower half of the credits ampersand.</summary>
    public const ushort LargeAmpersandBottom = 0x007c;
    /// <summary>Font-3 tile used by the upper half of the credits period.</summary>
    public const ushort LargePeriodTop = 0x007d;
    /// <summary>Font-3 tile used by the lower half of the credits period.</summary>
    public const ushort LargePeriodBottom = 0x007e;

    public static IReadOnlyList<CreditsLineDefinition> Lines { get; } =
    [
        Small("staff-heading", InitialBlankRows),
        Small("producer-heading"),
        Large("producer-name"),
        Small("director-heading"),
        Large("director-name"),
        Small("background-designers-heading"),
        Large("background-designer-1"),
        Large("background-designer-2"),
        Large("background-designer-3"),
        Small("object-designers-heading"),
        Large("object-designer-1"),
        Large("object-designer-2"),
        Small("samus-original-designer-heading"),
        Large("samus-original-designer-name"),
        Small("samus-designer-heading"),
        Large("samus-designer-name"),
        Small("sound-program-heading"),
        Small("sound-effects-heading", blankRowsBefore: 0),
        Large("sound-programmer-name"),
        Small("music-composers-heading"),
        Large("music-composer-1"),
        Large("music-composer-2"),
        Small("program-director-heading"),
        Large("program-director-name"),
        Small("system-coordinator-heading"),
        Large("system-coordinator-name"),
        Small("system-programmer-heading"),
        Large("system-programmer-name"),
        Small("samus-programmer-heading"),
        Large("samus-programmer-name"),
        Small("event-programmer-heading"),
        Large("event-programmer-name"),
        Small("enemy-programmer-heading"),
        Large("enemy-programmer-name"),
        Small("map-programmer-heading"),
        Large("map-programmer-name"),
        Small("assistant-programmer-heading"),
        Large("assistant-programmer-name"),
        Small("coordinators-heading"),
        Large("coordinator-1"),
        Large("coordinator-2"),
        Small("printed-art-work-heading"),
        Large("printed-art-work-1"),
        Large("printed-art-work-2"),
        Large("printed-art-work-3"),
        Large("printed-art-work-4"),
        Large("printed-art-work-5"),
        Large("printed-art-work-6"),
        Small("special-thanks-heading"),
        Large("special-thanks-01"),
        Large("special-thanks-02"),
        Large("special-thanks-03"),
        Large("special-thanks-04"),
        Large("special-thanks-05"),
        Large("special-thanks-06"),
        Large("special-thanks-07"),
        Large("special-thanks-08"),
        Large("special-thanks-09"),
        Large("special-thanks-10"),
        Large("special-thanks-11"),
        Large("special-thanks-12"),
        Large("special-thanks-13"),
        Large("special-thanks-14"),
        Large("special-thanks-15"),
        Large("special-thanks-r-and-d"),
        Small("general-manager-heading"),
        Large("general-manager-name"),
    ];

    private static CreditsLineDefinition Small(string id,
        int blankRowsBefore = SectionBlankRows) =>
        new(id, CreditsLineStyle.Small, blankRowsBefore);

    private static CreditsLineDefinition Large(string id) =>
        new(id, CreditsLineStyle.Large, InterLineBlankRows);

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
        public const ushort CommandBit = 0x8000;
        /// <summary><c>Instruction_CreditsObject_TimerInY</c> at $8B:9A17.</summary>
        public const ushort SetTimer = 0x9a17;
        /// <summary><c>Instruction_CreditsObject_DecrementTimer_GotoYIfNonZero</c> at $8B:9A0D.</summary>
        public const ushort DecrementTimerAndGoto = 0x9a0d;
        /// <summary><c>CreditsObject_Func2</c> at $8B:F6FE, which ends the scrolling credits.</summary>
        public const ushort EndCredits = 0xf6fe;
        /// <summary><c>Instruction_CreditsObject_Delete</c> at $8B:99FE.</summary>
        public const ushort Delete = 0x99fe;
        public const int MaximumOperations = 4096;
    }
}

public enum CreditsLineStyle : byte
{
    Small,
    Large,
}

public readonly record struct CreditsLineDefinition(
    string Id,
    CreditsLineStyle Style,
    int BlankRowsBefore);
