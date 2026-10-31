using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Hud;

/// <summary>
/// The gameplay message box's definitions (#1416, #1466): its rows and window placement, its waits and display durations,
/// the two colours it borrows while open, and which button its panels name.
/// </summary>
public static class GameplayMessageBoxDefinitions
{
    /// <summary>Message dimensions and window placement.</summary>
    public static class Layout
    {
        /// <summary>Width of a message in tile cells.</summary>
        public const int TilemapWidth = 32;
        /// <summary>Minimum supported message-box height in tile rows.</summary>
        public const int MinimumRows = 3;
        /// <summary>Maximum supported message-box height in tile rows.</summary>
        public const int MaximumRows = 6;
        /// <summary>Width and height of one tile in pixels.</summary>
        public const int TilePixels = 8;
        /// <summary>Vertical screen coordinate around which the message window expands.</summary>
        public const int WindowCenterY = 124;
    }

    /// <summary>Authored waits, window-animation steps, and minimum display durations.</summary>
    public static class Timing
    {
        /// <summary>The gunship's saving sound waits 160 updates.</summary>
        public const int GunshipSavingSoundFrames = 160;
        /// <summary>
        /// Updates gameplay pauses before the box starts to open: preparing the screen 2, clearing the box 1, writing the
        /// message 1, setting up the box and its window 2, and two more updates of music and sound.
        /// </summary>
        public const int OpeningPauseUpdates = 8;
        /// <summary>Updates before the gunship's second box starts to open, which needs no screen preparation or clearing.</summary>
        public const int SecondBoxOpeningPauseUpdates = 5;
        /// <summary>Clearing the closed box takes one update.</summary>
        public const int ClearingUpdates = 1;
        /// <summary>Updates gameplay stays paused after the box closes: clearing 1, restoring the screen 2, music and sound 2.</summary>
        public const int ClosingPauseUpdates = 5;
        /// <summary>Maximum half-height reached by the expanding message window.</summary>
        public const int MaximumRadiusPixels = 24;
        /// <summary>Per-step change in message-window radius.</summary>
        public const int RadiusStepPixels = 2;
        /// <summary>Minimum updates an item-acquisition message remains displayed.</summary>
        public const int ItemMinimumDisplayFrames = 360;
        /// <summary>Minimum updates a station message remains displayed.</summary>
        public const int StationMinimumDisplayFrames = 10;
    }

    /// <summary>The two colours the message box replaces while it is open.</summary>
    public static class Palette
    {
        /// <summary>The background set whose colours the message box's text shows.</summary>
        public const BackgroundColourSet TextSet = BackgroundColourSet.HudItemsAndEffects;
        /// <summary>The colour of the text set the light message colour replaces.</summary>
        public const int TemporaryLightColour = 9;
        /// <summary>The light colour installed for the message box.</summary>
        public static RgbColor TemporaryLightColor => new RgbColor(17, 29, 2);
        /// <summary>The colour of the text set the dark message colour replaces.</summary>
        public const int TemporaryDarkColour = 10;
        /// <summary>The dark colour installed for the message box.</summary>
        public static RgbColor TemporaryDarkColor => new(31, 0, 0);
    }

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
}
