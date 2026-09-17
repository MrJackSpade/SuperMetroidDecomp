using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Schema and cartridge glyph identities for editable one-row gameplay messages.</summary>
public static class GameplayMessageTitleDefinitions
{
    public const int Version = 1;
    public const string FileName = "gameplay-message-titles.json";
    public const int OuterLeftColumns = 6;
    public const int OuterRightColumns = 7;
    public const int VisibleColumns = 19;
    public const int TilemapWords = GameplayMessageRomData.Layout.TilemapWidth * 3;
    public const ushort TransparentWord = 0x000e;
    public const ushort PriorityWord = 0x2000;
    public const int SpaceCharacter = 0x04e;
    public const int HyphenCharacter = 0x0cf;
    public const int UppercaseACharacter = 0x0e0;
    public const int UppercaseZCharacter = 0x0f9;
    public const int PeriodCharacter = 0x0fa;

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

    public static ReadOnlySpan<GameplayMessageId> MessageIds => SupportedMessageIds;

    public static bool IsSupportedGlyph(char character) => character is ' ' or '-' or '.' or >= 'A' and <= 'Z';
}
