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
        /// <summary>The wait the gunship's saving sound plays for.</summary>
        public const int GunshipSavingSoundFrames = 160;
        /// <summary>Maximum half-height reached by the expanding message window.</summary>
        public const int MaximumRadiusPixels = 24;
        /// <summary>Per-step change in message-window radius.</summary>
        public const int RadiusStepPixels = 2;
        /// <summary>Minimum updates an item-acquisition message remains displayed.</summary>
        internal const int ItemMinimumDisplayFrames = 360;
        /// <summary>Minimum updates a station message remains displayed.</summary>
        public const int StationMinimumDisplayFrames = 10;
    }

    /// <summary>
    /// Each message's definition (#2422, #2542): the updates it stays displayed at least once its window has opened, before
    /// it takes input, which the save confirmations do not wait; what it asks the player to confirm; and the binding its
    /// panel names the button of.
    /// </summary>
    public static IdentityTable<GameplayMessageId, GameplayMessageDefinition> Messages { get; } =
        new(new Dictionary<GameplayMessageId, GameplayMessageDefinition>
    {
        [GameplayMessageId.EnergyTank] = new(Timing.ItemMinimumDisplayFrames, GameplayMessageConfirmation.None, null),
        [GameplayMessageId.MissileTank] = new(Timing.ItemMinimumDisplayFrames, GameplayMessageConfirmation.None, GameplayMessagePanelButtonBinding.Shoot),
        [GameplayMessageId.SuperMissileTank] = new(Timing.ItemMinimumDisplayFrames, GameplayMessageConfirmation.None, GameplayMessagePanelButtonBinding.Shoot),
        [GameplayMessageId.PowerBombTank] = new(Timing.ItemMinimumDisplayFrames, GameplayMessageConfirmation.None, GameplayMessagePanelButtonBinding.Shoot),
        [GameplayMessageId.GrappleBeam] = new(Timing.ItemMinimumDisplayFrames, GameplayMessageConfirmation.None, GameplayMessagePanelButtonBinding.Shoot),
        [GameplayMessageId.XrayScope] = new(Timing.ItemMinimumDisplayFrames, GameplayMessageConfirmation.None, GameplayMessagePanelButtonBinding.Run),
        [GameplayMessageId.VariaSuit] = new(Timing.ItemMinimumDisplayFrames, GameplayMessageConfirmation.None, null),
        [GameplayMessageId.SpringBall] = new(Timing.ItemMinimumDisplayFrames, GameplayMessageConfirmation.None, null),
        [GameplayMessageId.MorphBall] = new(Timing.ItemMinimumDisplayFrames, GameplayMessageConfirmation.None, null),
        [GameplayMessageId.ScrewAttack] = new(Timing.ItemMinimumDisplayFrames, GameplayMessageConfirmation.None, null),
        [GameplayMessageId.HiJumpBoots] = new(Timing.ItemMinimumDisplayFrames, GameplayMessageConfirmation.None, null),
        [GameplayMessageId.SpaceJump] = new(Timing.ItemMinimumDisplayFrames, GameplayMessageConfirmation.None, null),
        [GameplayMessageId.SpeedBooster] = new(Timing.ItemMinimumDisplayFrames, GameplayMessageConfirmation.None, GameplayMessagePanelButtonBinding.Run),
        [GameplayMessageId.ChargeBeam] = new(Timing.ItemMinimumDisplayFrames, GameplayMessageConfirmation.None, null),
        [GameplayMessageId.IceBeam] = new(Timing.ItemMinimumDisplayFrames, GameplayMessageConfirmation.None, null),
        [GameplayMessageId.WaveBeam] = new(Timing.ItemMinimumDisplayFrames, GameplayMessageConfirmation.None, null),
        [GameplayMessageId.SpazerBeam] = new(Timing.ItemMinimumDisplayFrames, GameplayMessageConfirmation.None, null),
        [GameplayMessageId.PlasmaBeam] = new(Timing.ItemMinimumDisplayFrames, GameplayMessageConfirmation.None, null),
        [GameplayMessageId.Bombs] = new(Timing.ItemMinimumDisplayFrames, GameplayMessageConfirmation.None, GameplayMessagePanelButtonBinding.Shoot),
        [GameplayMessageId.MapDataAccessCompleted] = new(Timing.StationMinimumDisplayFrames, GameplayMessageConfirmation.None, null),
        [GameplayMessageId.EnergyRechargeCompleted] = new(Timing.StationMinimumDisplayFrames, GameplayMessageConfirmation.None, null),
        [GameplayMessageId.MissileRechargeCompleted] = new(Timing.StationMinimumDisplayFrames, GameplayMessageConfirmation.None, null),
        [GameplayMessageId.SaveConfirmation] = new(null, GameplayMessageConfirmation.Save, null),
        [GameplayMessageId.SaveCompleted] = new(Timing.StationMinimumDisplayFrames, GameplayMessageConfirmation.None, null),
        [GameplayMessageId.ReserveTank] = new(Timing.ItemMinimumDisplayFrames, GameplayMessageConfirmation.None, null),
        [GameplayMessageId.GravitySuit] = new(Timing.ItemMinimumDisplayFrames, GameplayMessageConfirmation.None, null),
        [GameplayMessageId.GunshipSaveConfirmation] = new(null, GameplayMessageConfirmation.SaveThenCompletionNotice, null),
    });

    /// <summary>The definition of <paramref name="messageId"/>.</summary>
    public static GameplayMessageDefinition Message(GameplayMessageId messageId) => Messages[messageId];

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
    }
}
