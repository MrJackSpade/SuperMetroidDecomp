using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Schema and cartridge glyph identities for editable one-row gameplay messages.</summary>
public static class GameplayMessageTitleDefinitions
{
    /// <summary>Supported gameplay-message title document schema revision.</summary>
    public const int Version = 1;
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
    /// <summary>Native tile character index for a space.</summary>
    public const int SpaceCharacter = 0x04e;
    /// <summary>Native tile character index for a hyphen.</summary>
    public const int HyphenCharacter = 0x0cf;
    /// <summary>First character index in the consecutive uppercase alphabet run.</summary>
    public const int UppercaseACharacter = 0x0e0;
    /// <summary>Last character index in the consecutive uppercase alphabet run.</summary>
    public const int UppercaseZCharacter = 0x0f9;
    /// <summary>Native tile character index for a period.</summary>
    public const int PeriodCharacter = 0x0fa;
    /// <summary>Native tile character index for a question mark.</summary>
    public const int QuestionMarkCharacter = 0x0fe;

    /// <summary>Ordered set of item-acquisition and status messages with editable centered title rows.</summary>
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
