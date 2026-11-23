using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// The in-game options a gameplay runtime reads: Moonwalk, Icon Cancel and the controller bindings, as the options
/// menu and the loaded save set them.
/// </summary>
internal sealed partial class GameplayOptions
{
    /// <summary>
    /// The game's Moonwalk option, read when
    /// Samus turns. False preserves the default turn substitution; true admits the six
    /// stable moonwalk poses selected by the unchanged input transitions.
    /// </summary>
    public bool MoonwalkEnabled { get; set; }

    /// <summary>
    /// The Icon Cancel option: when it is set, a door's transition selects the beam on the HUD again as it clears Samus's
    /// projectiles.
    /// </summary>
    public bool IconCancelEnabled { get; set; }

    /// <summary>
    /// The live controller bindings. Physical controller buttons arrive each update; each
    /// gameplay frame boundary maps them once, at the same point where gameplay
    /// compares input with these configurable actions.
    /// </summary>
    public ControllerBindings ControllerBindings { get; set; } = ControllerBindings.Default;
}
