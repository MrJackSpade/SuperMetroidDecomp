using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Native tile character indexes of the title punctuation; letters are the contiguous range at <see cref="GameplayMessageTitleDefinitions.FirstLetterTile"/>.</summary>
public enum GameplayMessageTitleGlyph : ushort
{
    /// <summary>Native tile character index for a space.</summary>
    Space = 0x04e,
    /// <summary>Native tile character index for a hyphen.</summary>
    Hyphen = 0x0cf,
    /// <summary>Native tile character index for a period.</summary>
    Period = 0x0fa,
    /// <summary>Native tile character index for a question mark.</summary>
    QuestionMark = 0x0fe,
}

/// <summary>Schema and cartridge glyph identities for editable one-row gameplay messages.</summary>
public static class GameplayMessageTitleDefinitions
{
    /// <summary>Supported gameplay-message title document schema revision.</summary>
    public const int Version = 1;
    /// <summary>Native tile character index of A; B through Z follow contiguously.</summary>
    public const ushort FirstLetterTile = 0x0e0;
    /// <summary>Letters in the contiguous A-Z tile range.</summary>
    public const int LetterCount = 26;

    /// <summary>True when <paramref name="tile"/> is one of the A-Z letter tiles.</summary>
    public static bool IsLetterTile(ushort tile) => (uint)(tile - FirstLetterTile) < LetterCount;
    /// <summary>JSON filename containing the editable one-row item and status titles.</summary>
    public const string FileName = "gameplay-message-titles.json";
    /// <summary>Number of transparent tilemap columns preceding the visible title.</summary>
    public const int OuterLeftColumns = 6;
    /// <summary>Number of transparent tilemap columns following the visible title.</summary>
    public const int OuterRightColumns = 7;
    /// <summary>Width in tilemap cells of the centered visible title region.</summary>
    public const int VisibleColumns = 19;
    /// <summary>Total words in the native three-row, 32-column message tilemap.</summary>
    public const int TilemapWords = GameplayMessageRomData.Layout.TilemapWidth * 3;
    /// <summary>Transparent character word used outside the visible title region.</summary>
    public const ushort TransparentWord = 0x000e;
    /// <summary>BG priority bit applied to compiled title glyph words.</summary>
    public const ushort PriorityWord = 0x2000;

    private static readonly GameplayMessageId[] SupportedMessageIds =
    [
        GameplayMessageId.EnergyTank,
        GameplayMessageId.VariaSuit,
        GameplayMessageId.SpringBall,
        GameplayMessageId.MorphBall,
        GameplayMessageId.ScrewAttack,
        GameplayMessageId.HiJumpBoots,
        GameplayMessageId.SpaceJump,
        GameplayMessageId.ChargeBeam,
        GameplayMessageId.IceBeam,
        GameplayMessageId.WaveBeam,
        GameplayMessageId.SpazerBeam,
        GameplayMessageId.PlasmaBeam,
        GameplayMessageId.SaveCompleted,
        GameplayMessageId.ReserveTank,
        GameplayMessageId.GravitySuit,
    ];

    /// <summary>Gets the 15 gameplay messages whose centered title text is editable.</summary>
    public static ReadOnlySpan<GameplayMessageId> MessageIds => SupportedMessageIds;

    /// <summary>Determines whether a character belongs to the supported title glyph alphabet.</summary>
    public static bool IsSupportedGlyph(char character) =>
        character is ' ' or '-' or '.' or '?' or >= 'A' and <= 'Z';
}
