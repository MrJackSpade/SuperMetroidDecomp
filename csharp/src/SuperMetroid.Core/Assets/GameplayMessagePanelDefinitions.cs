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

    /// <summary>The closed set of item messages with editable four-row instruction bodies.</summary>
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

    /// <summary>Exposes the seven large item-instruction messages in their published asset order.</summary>
    public static ReadOnlySpan<GameplayMessageId> MessageIds => SupportedMessageIds;

    /// <summary>Classifies the binding used to patch each supported panel's controller glyph.</summary>
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
            GameplayMessageId.None or GameplayMessageId.EnergyTank or GameplayMessageId.VariaSuit or
            GameplayMessageId.SpringBall or GameplayMessageId.MorphBall or GameplayMessageId.ScrewAttack or
            GameplayMessageId.HiJumpBoots or GameplayMessageId.SpaceJump or GameplayMessageId.ChargeBeam or
            GameplayMessageId.IceBeam or GameplayMessageId.WaveBeam or GameplayMessageId.SpazerBeam or
            GameplayMessageId.PlasmaBeam or GameplayMessageId.MapDataAccessCompleted or
            GameplayMessageId.EnergyRechargeCompleted or GameplayMessageId.MissileRechargeCompleted or
            GameplayMessageId.SaveConfirmation or GameplayMessageId.SaveCompleted or GameplayMessageId.ReserveTank or
            GameplayMessageId.GravitySuit or GameplayMessageId.GunshipSaveConfirmation => GameplayMessagePanelButtonBinding.None,
            _ => throw new ArgumentOutOfRangeException(nameof(messageId), messageId, "Undefined gameplay message."),
        };
}
