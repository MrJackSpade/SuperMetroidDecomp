using System.Collections.Immutable;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Hud;

/// <summary>
/// The gameplay message box's definitions (#1416, #1466, #2542): where its window shows, its waits, the window's opening
/// steps and display durations, what each message asks the player to confirm, and which button its item panels name.
/// </summary>
public static class GameplayMessageBoxDefinitions
{
    /// <summary>Where the message box shows on the screen.</summary>
    public static class Layout
    {
        /// <summary>The screen line the message's picture is centred on, and around which the message window expands.</summary>
        public const int WindowCenterY = 124;
    }

    /// <summary>Authored waits, window-animation steps, and minimum display durations.</summary>
    public static class Timing
    {
        /// <summary>The gunship's saving sound waits 160 updates.</summary>
        public const int GunshipSavingSoundFrames = 160;
        /// <summary>Maximum half-height reached by the expanding message window.</summary>
        public const int MaximumRadiusPixels = 24;
        /// <summary>Per-step change in message-window radius.</summary>
        public const int RadiusStepPixels = 2;
        /// <summary>Minimum updates an item-acquisition message remains displayed.</summary>
        public const int ItemMinimumDisplayFrames = 360;
        /// <summary>Minimum updates a station message remains displayed.</summary>
        public const int StationMinimumDisplayFrames = 10;

        /// <summary>
        /// The updates each message stays displayed at least once its window has opened, before it takes input (#2422): an
        /// item's description holds for <see cref="ItemMinimumDisplayFrames"/>, the four completion notices for
        /// <see cref="StationMinimumDisplayFrames"/>. The save confirmations take input as soon as their window opens and
        /// are not listed.
        /// </summary>
        public static IReadOnlyDictionary<GameplayMessageId, int> MinimumDisplayFrames { get; } = new Dictionary<GameplayMessageId, int>
        {
            [GameplayMessageId.EnergyTank] = ItemMinimumDisplayFrames,
            [GameplayMessageId.MissileTank] = ItemMinimumDisplayFrames,
            [GameplayMessageId.SuperMissileTank] = ItemMinimumDisplayFrames,
            [GameplayMessageId.PowerBombTank] = ItemMinimumDisplayFrames,
            [GameplayMessageId.GrappleBeam] = ItemMinimumDisplayFrames,
            [GameplayMessageId.XrayScope] = ItemMinimumDisplayFrames,
            [GameplayMessageId.VariaSuit] = ItemMinimumDisplayFrames,
            [GameplayMessageId.SpringBall] = ItemMinimumDisplayFrames,
            [GameplayMessageId.MorphBall] = ItemMinimumDisplayFrames,
            [GameplayMessageId.ScrewAttack] = ItemMinimumDisplayFrames,
            [GameplayMessageId.HiJumpBoots] = ItemMinimumDisplayFrames,
            [GameplayMessageId.SpaceJump] = ItemMinimumDisplayFrames,
            [GameplayMessageId.SpeedBooster] = ItemMinimumDisplayFrames,
            [GameplayMessageId.ChargeBeam] = ItemMinimumDisplayFrames,
            [GameplayMessageId.IceBeam] = ItemMinimumDisplayFrames,
            [GameplayMessageId.WaveBeam] = ItemMinimumDisplayFrames,
            [GameplayMessageId.SpazerBeam] = ItemMinimumDisplayFrames,
            [GameplayMessageId.PlasmaBeam] = ItemMinimumDisplayFrames,
            [GameplayMessageId.Bombs] = ItemMinimumDisplayFrames,
            [GameplayMessageId.ReserveTank] = ItemMinimumDisplayFrames,
            [GameplayMessageId.GravitySuit] = ItemMinimumDisplayFrames,
            [GameplayMessageId.MapDataAccessCompleted] = StationMinimumDisplayFrames,
            [GameplayMessageId.EnergyRechargeCompleted] = StationMinimumDisplayFrames,
            [GameplayMessageId.MissileRechargeCompleted] = StationMinimumDisplayFrames,
            [GameplayMessageId.SaveCompleted] = StationMinimumDisplayFrames,
        };
    }

    /// <summary>
    /// What each message asks the player to confirm (#2542): the save station's question asks whether to save, the gunship's
    /// asks the same and follows an accepted save with its own completion notice, and every other message asks nothing.
    /// </summary>
    public static IReadOnlyDictionary<GameplayMessageId, GameplayMessageConfirmation> Confirmations { get; } =
        new Dictionary<GameplayMessageId, GameplayMessageConfirmation>
    {
        [GameplayMessageId.EnergyTank] = GameplayMessageConfirmation.None,
        [GameplayMessageId.MissileTank] = GameplayMessageConfirmation.None,
        [GameplayMessageId.SuperMissileTank] = GameplayMessageConfirmation.None,
        [GameplayMessageId.PowerBombTank] = GameplayMessageConfirmation.None,
        [GameplayMessageId.GrappleBeam] = GameplayMessageConfirmation.None,
        [GameplayMessageId.XrayScope] = GameplayMessageConfirmation.None,
        [GameplayMessageId.VariaSuit] = GameplayMessageConfirmation.None,
        [GameplayMessageId.SpringBall] = GameplayMessageConfirmation.None,
        [GameplayMessageId.MorphBall] = GameplayMessageConfirmation.None,
        [GameplayMessageId.ScrewAttack] = GameplayMessageConfirmation.None,
        [GameplayMessageId.HiJumpBoots] = GameplayMessageConfirmation.None,
        [GameplayMessageId.SpaceJump] = GameplayMessageConfirmation.None,
        [GameplayMessageId.SpeedBooster] = GameplayMessageConfirmation.None,
        [GameplayMessageId.ChargeBeam] = GameplayMessageConfirmation.None,
        [GameplayMessageId.IceBeam] = GameplayMessageConfirmation.None,
        [GameplayMessageId.WaveBeam] = GameplayMessageConfirmation.None,
        [GameplayMessageId.SpazerBeam] = GameplayMessageConfirmation.None,
        [GameplayMessageId.PlasmaBeam] = GameplayMessageConfirmation.None,
        [GameplayMessageId.Bombs] = GameplayMessageConfirmation.None,
        [GameplayMessageId.MapDataAccessCompleted] = GameplayMessageConfirmation.None,
        [GameplayMessageId.EnergyRechargeCompleted] = GameplayMessageConfirmation.None,
        [GameplayMessageId.MissileRechargeCompleted] = GameplayMessageConfirmation.None,
        [GameplayMessageId.SaveConfirmation] = GameplayMessageConfirmation.Save,
        [GameplayMessageId.SaveCompleted] = GameplayMessageConfirmation.None,
        [GameplayMessageId.ReserveTank] = GameplayMessageConfirmation.None,
        [GameplayMessageId.GravitySuit] = GameplayMessageConfirmation.None,
        [GameplayMessageId.GunshipSaveConfirmation] = GameplayMessageConfirmation.SaveThenCompletionNotice,
    };

    /// <summary>Which button an item panel names for its action.</summary>
    public static class Buttons
    {
        /// <summary>The buttons a panel's glyph can show, in the order a binding is tested for them.</summary>
        public static ImmutableArray<ControllerButton> GlyphOrder { get; } =
        [
            ControllerButton.A,
            ControllerButton.B,
            ControllerButton.X,
            ControllerButton.Y,
            ControllerButton.Select,
            ControllerButton.L,
            ControllerButton.R,
        ];

        /// <summary>
        /// The binding each message's panel names the button of: shoot or run for the large item panels that show one,
        /// <see cref="GameplayMessagePanelButtonBinding.None"/> for every other message.
        /// </summary>
        public static IReadOnlyDictionary<GameplayMessageId, GameplayMessagePanelButtonBinding> PanelBindings { get; } =
            new Dictionary<GameplayMessageId, GameplayMessagePanelButtonBinding>
        {
            [GameplayMessageId.MissileTank] = GameplayMessagePanelButtonBinding.Shoot,
            [GameplayMessageId.SuperMissileTank] = GameplayMessagePanelButtonBinding.Shoot,
            [GameplayMessageId.PowerBombTank] = GameplayMessagePanelButtonBinding.Shoot,
            [GameplayMessageId.GrappleBeam] = GameplayMessagePanelButtonBinding.Shoot,
            [GameplayMessageId.Bombs] = GameplayMessagePanelButtonBinding.Shoot,
            [GameplayMessageId.XrayScope] = GameplayMessagePanelButtonBinding.Run,
            [GameplayMessageId.SpeedBooster] = GameplayMessagePanelButtonBinding.Run,
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
}
