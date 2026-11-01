using SuperMetroid.Core.Input;
using SuperMetroid.Core.Map;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// Host gameplay options for the session: invincibility, infinite ammo, escape-timeout prevention, map reveal,
/// moonwalk, icon cancel and controller bindings.
/// </summary>
internal sealed class GameplayOptions
{

    /// <summary>
    /// Host testing switch that prevents Samus from dropping below one energy.
    /// </summary>
    /// <remarks>
    /// The guard deliberately surrounds the complete frame instead of branching inside the
    /// many damage producers. Enemy, projectile, liquid, and scripted routines
    /// therefore retain their ordinary damage, collision, and animation side effects. Only
    /// a lethal zero-energy result is raised to one before the frontend can enter death.
    /// </remarks>
    public bool PlayerInvincibilityEnabled { get; internal set; }

    /// <summary>
    /// Host testing switch that keeps every unlocked consumable ammo type above zero.
    /// </summary>
    /// <remarks>
    /// Game routines still subtract ammunition and publish their ordinary projectile,
    /// HUD, and sound side effects. The frame-exit guard only raises a zero count to one when
    /// the corresponding maximum is nonzero, so it cannot unlock an unavailable item.
    /// </remarks>
    public bool InfiniteAmmoEnabled { get; internal set; }

    /// <summary>Host testing option: both escape countdowns stop at one second, without changing their earlier timing.</summary>
    public bool PreventEscapeTimeout { get; internal set; }

    /// <summary>Nonpersistent host map visibility used by gameplay HUD updates.</summary>
    public MapRevealMode MapRevealMode { get; internal set; }

    /// <summary>
    /// Host-readable equivalent of the game's Moonwalk option, read when
    /// Samus turns. False preserves the default turn substitution; true admits the six
    /// stable moonwalk poses selected by the unchanged input tables.
    /// </summary>
    public bool MoonwalkEnabled { get; set; }

    /// <summary>
    /// The Icon Cancel option. Projectile-data teardown clears the selected HUD icon
    /// when this word is nonzero; retaining it beside Moonwalk keeps both special-options
    /// words available to their actual gameplay owners.
    /// </summary>
    public bool IconCancelEnabled { get; set; }

    /// <summary>
    /// The live controller bindings. The host supplies physical controller buttons; each
    /// gameplay frame boundary translates them once, at the same point where gameplay
    /// compares input with these configurable actions.
    /// </summary>
    public ControllerBindings ControllerBindings { get; set; } = ControllerBindings.Default;
}
