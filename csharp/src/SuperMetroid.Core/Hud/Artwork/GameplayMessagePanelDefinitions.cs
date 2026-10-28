using SuperMetroid.Core.Rooms;


namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>Schema, layout and closed identities for editable large gameplay-message panels.</summary>
public static class GameplayMessagePanelDefinitions
{
    /// <summary>Supported large-panel JSON schema version.</summary>
    public const int Version = 2;
    /// <summary>Canonical editable large-panel asset file name.</summary>
    public const string FileName = "gameplay-message-panels.json";
    /// <summary>Number of content rows between the shared top and bottom borders.</summary>
    public const int ContentRows = 4;
    /// <summary>Number of tilemap words in the four-row panel body.</summary>
    public const int ContentWords = ContentRows * GameplayMessageBoxDefinitions.Layout.TilemapWidth;
    /// <summary>Total tilemap words in the bordered six-row message panel.</summary>
    public const int TilemapWords =
        (ContentRows + GameplayMessageBoxDefinitions.Layout.BorderRows) *
        GameplayMessageBoxDefinitions.Layout.TilemapWidth;
    /// <summary>Transparent columns retained at the left edge of the title row.</summary>
    public const int OuterLeftColumns = 3;
    /// <summary>Transparent columns retained at the right edge of the title row.</summary>
    public const int OuterRightColumns = 3;
    /// <summary>Columns available to the title and panel body between the outer margins.</summary>
    public const int VisibleColumns = GameplayMessageBoxDefinitions.Layout.TilemapWidth -
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

    /// <summary>The binding used to patch each message's controller glyph: shoot or run for the large item panels that
    /// show one, <see cref="GameplayMessagePanelButtonBinding.None"/> for every other message.</summary>
    public static IReadOnlyDictionary<GameplayMessageId, GameplayMessagePanelButtonBinding> ButtonBindings { get; } =
        new Dictionary<GameplayMessageId, GameplayMessagePanelButtonBinding>
    {
        [GameplayMessageId.MissileTank] = GameplayMessagePanelButtonBinding.Shoot,
        [GameplayMessageId.SuperMissileTank] = GameplayMessagePanelButtonBinding.Shoot,
        [GameplayMessageId.PowerBombTank] = GameplayMessagePanelButtonBinding.Shoot,
        [GameplayMessageId.GrappleBeam] = GameplayMessagePanelButtonBinding.Shoot,
        [GameplayMessageId.Bombs] = GameplayMessagePanelButtonBinding.Shoot,
        [GameplayMessageId.XrayScope] = GameplayMessagePanelButtonBinding.Run,
        [GameplayMessageId.SpeedBooster] = GameplayMessagePanelButtonBinding.Run,
        [GameplayMessageId.None] = GameplayMessagePanelButtonBinding.None,
        [GameplayMessageId.EnergyTank] = GameplayMessagePanelButtonBinding.None,
        [GameplayMessageId.VariaSuit] = GameplayMessagePanelButtonBinding.None,
        [GameplayMessageId.SpringBall] = GameplayMessagePanelButtonBinding.None,
        [GameplayMessageId.MorphBall] = GameplayMessagePanelButtonBinding.None,
        [GameplayMessageId.ScrewAttack] = GameplayMessagePanelButtonBinding.None,
        [GameplayMessageId.HiJumpBoots] = GameplayMessagePanelButtonBinding.None,
        [GameplayMessageId.SpaceJump] = GameplayMessagePanelButtonBinding.None,
        [GameplayMessageId.ChargeBeam] = GameplayMessagePanelButtonBinding.None,
        [GameplayMessageId.IceBeam] = GameplayMessagePanelButtonBinding.None,
        [GameplayMessageId.WaveBeam] = GameplayMessagePanelButtonBinding.None,
        [GameplayMessageId.SpazerBeam] = GameplayMessagePanelButtonBinding.None,
        [GameplayMessageId.PlasmaBeam] = GameplayMessagePanelButtonBinding.None,
        [GameplayMessageId.MapDataAccessCompleted] = GameplayMessagePanelButtonBinding.None,
        [GameplayMessageId.EnergyRechargeCompleted] = GameplayMessagePanelButtonBinding.None,
        [GameplayMessageId.MissileRechargeCompleted] = GameplayMessagePanelButtonBinding.None,
        [GameplayMessageId.SaveConfirmation] = GameplayMessagePanelButtonBinding.None,
        [GameplayMessageId.SaveCompleted] = GameplayMessagePanelButtonBinding.None,
        [GameplayMessageId.ReserveTank] = GameplayMessagePanelButtonBinding.None,
        [GameplayMessageId.GravitySuit] = GameplayMessagePanelButtonBinding.None,
        [GameplayMessageId.GunshipSaveConfirmation] = GameplayMessagePanelButtonBinding.None,
    };
}
