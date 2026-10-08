using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Schema, layout and closed identities for editable large gameplay-message panels.</summary>
public static class GameplayMessagePanelDefinitions
{
    /// <summary>Supported large-panel JSON schema version.</summary>
    public const int Version = 1;
    /// <summary>Canonical editable large-panel asset file name.</summary>
    public const string FileName = "gameplay-message-panels.json";
    /// <summary>Number of content rows between the shared top and bottom borders.</summary>
    public const int ContentRows = 4;
    /// <summary>Number of tilemap words in the four-row panel body.</summary>
    public const int ContentWords = ContentRows * GameplayMessageRomData.Layout.TilemapWidth;
    /// <summary>Total tilemap words in the bordered six-row message panel.</summary>
    public const int TilemapWords =
        (ContentRows + GameplayMessageRomData.Layout.BorderRows) *
        GameplayMessageRomData.Layout.TilemapWidth;
    /// <summary>Transparent columns retained at the left edge of the title row.</summary>
    public const int OuterLeftColumns = 3;
    /// <summary>Transparent columns retained at the right edge of the title row.</summary>
    public const int OuterRightColumns = 3;
    /// <summary>Columns available to the title and panel body between the outer margins.</summary>
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

    /// <summary>Gets the seven large item-instruction messages in their published asset order.</summary>
    public static ReadOnlySpan<GameplayMessageId> MessageIds => SupportedMessageIds;

    /// <summary>Gets the controller action whose glyph slot is patched in a large message panel.</summary>
    /// <param name="messageId">Message identity to classify.</param>
    /// <returns>The shoot or run binding, or <see cref="GameplayMessagePanelButtonBinding.None"/> for another message.</returns>
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
    /// <summary>The message has no large-panel controller-glyph slot.</summary>
    None,
    /// <summary>The panel displays the configured shoot-button glyph.</summary>
    Shoot,
    /// <summary>The panel displays the configured run-button glyph.</summary>
    Run,
}
