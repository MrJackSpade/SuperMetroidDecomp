using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

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
    /// many cartridge damage producers. Enemy, projectile, liquid, and scripted routines
    /// therefore retain their ordinary damage, collision, and animation side effects. Only
    /// a lethal zero-energy result is raised to one before the frontend can enter death.
    /// </remarks>
    public bool PlayerInvincibilityEnabled { get; internal set; }

    /// <summary>
    /// Host testing switch that keeps every unlocked consumable ammo type above zero.
    /// </summary>
    /// <remarks>
    /// Cartridge routines still subtract ammunition and publish their ordinary projectile,
    /// HUD, and sound side effects. The frame-exit guard only raises a zero count to one when
    /// the corresponding maximum is nonzero, so it cannot unlock an unavailable item.
    /// </remarks>
    public bool InfiniteAmmoEnabled { get; internal set; }

    /// <summary>Host testing option: both escape countdowns stop at one second, without changing their earlier timing.</summary>
    public bool PreventEscapeTimeout { get; internal set; }

    /// <summary>Nonpersistent host map visibility used by gameplay HUD updates.</summary>
    public MapRevealMode MapRevealMode { get; internal set; }

    /// <summary>
    /// Host-readable equivalent of the native Moonwalk options word consumed by
    /// `$91:F88C`. False preserves the default turn substitution; true admits the six
    /// stable movement-type-$10 poses selected by the unchanged ROM input tables.
    /// </summary>
    public bool MoonwalkEnabled { get; set; }

    /// <summary>
    /// WRAM <c>$09EA</c>. Native projectile-data teardown clears the selected HUD icon
    /// when this word is nonzero; retaining it beside Moonwalk keeps both special-options
    /// words available to their actual gameplay owners.
    /// </summary>
    public bool IconCancelEnabled { get; set; }

    /// <summary>
    /// Live copies of WRAM <c>$09B2-$09BE</c>. The host supplies physical controller bits;
    /// accepted gameplay NMIs translate them once at the same conceptual seam where native
    /// routines compare input with these configurable action words.
    /// </summary>
    public ControllerBindings ControllerBindings { get; set; } = ControllerBindings.Default;
}
