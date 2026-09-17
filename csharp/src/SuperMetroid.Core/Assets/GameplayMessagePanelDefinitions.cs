using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Schema, layout and closed identities for editable large gameplay-message panels.</summary>
public static class GameplayMessagePanelDefinitions
{
    public const int Version = 1;
    public const string FileName = "gameplay-message-panels.json";
    public const int ContentRows = 4;
    public const int ContentWords = ContentRows * GameplayMessageRomData.Layout.TilemapWidth;
    public const int TilemapWords =
        (ContentRows + GameplayMessageRomData.Layout.BorderRows) *
        GameplayMessageRomData.Layout.TilemapWidth;
    public const int OuterLeftColumns = 3;
    public const int OuterRightColumns = 3;
    public const int VisibleColumns = GameplayMessageRomData.Layout.TilemapWidth -
        OuterLeftColumns - OuterRightColumns;

    private static readonly GameplayMessageId[] SupportedMessageIds =
    [
        GameplayMessageId.MissileTank,
        GameplayMessageId.SuperMissileTank,
        GameplayMessageId.PowerBombTank,
        GameplayMessageId.GrappleBeam,
        GameplayMessageId.XrayScope,
        GameplayMessageId.SpeedBooster,
        GameplayMessageId.Bombs,
    ];

    public static ReadOnlySpan<GameplayMessageId> MessageIds => SupportedMessageIds;

    public static GameplayMessagePanelButtonBinding ButtonBinding(GameplayMessageId messageId) =>
        messageId switch
        {
            GameplayMessageId.MissileTank or
            GameplayMessageId.SuperMissileTank or
            GameplayMessageId.PowerBombTank or
            GameplayMessageId.GrappleBeam or
            GameplayMessageId.Bombs => GameplayMessagePanelButtonBinding.Shoot,
            GameplayMessageId.XrayScope or
            GameplayMessageId.SpeedBooster => GameplayMessagePanelButtonBinding.Run,
            _ => GameplayMessagePanelButtonBinding.None,
        };
}

/// <summary>Compiled controller binding represented by a panel's safe button-glyph slot.</summary>
public enum GameplayMessagePanelButtonBinding : byte
{
    None,
    Shoot,
    Run,
}
