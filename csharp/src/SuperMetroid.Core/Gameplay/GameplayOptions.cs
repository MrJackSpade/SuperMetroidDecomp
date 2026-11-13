using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// The in-game options a gameplay runtime reads: Moonwalk, Icon Cancel and the controller bindings, as the options
/// menu and the loaded save set them.
/// </summary>
internal sealed class GameplayOptions
{
    /// <summary>
    /// The game's Moonwalk option, read when
    /// Samus turns. False preserves the default turn substitution; true admits the six
    /// stable moonwalk poses selected by the unchanged input tables.
    /// </summary>
    public bool MoonwalkEnabled { get; set; }

    /// <summary>
    /// The Icon Cancel option. Projectile-data teardown clears the selected HUD icon
    /// when this option is set; retaining it beside Moonwalk keeps both special options
    /// available to their actual gameplay owners.
    /// </summary>
    public bool IconCancelEnabled { get; set; }

    /// <summary>
    /// The live controller bindings. Physical controller buttons arrive each update; each
    /// gameplay frame boundary maps them once, at the same point where gameplay
    /// compares input with these configurable actions.
    /// </summary>
    public ControllerBindings ControllerBindings { get; set; } = ControllerBindings.Default;
}
