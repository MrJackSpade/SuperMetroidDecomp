namespace SuperMetroid.Core.Assets;

/// <summary>Compiled layout identities and timing boundaries for the scrolling staff credits.</summary>
public static class CreditsPresentationDefinitions
{
    public const int Version = 1;
    public const string FileName = "ending-credits.json";
    public const int TilemapWidth = 32;
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

    private static readonly CreditRoles Roles = new();
    public static IReadOnlyList<CreditsLineDefinition> Lines => Roles;

    private sealed class CreditRoles : IReadOnlyList<CreditsLineDefinition>
    {
        public int Count => 67;
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
        public IEnumerator<CreditsLineDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

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
