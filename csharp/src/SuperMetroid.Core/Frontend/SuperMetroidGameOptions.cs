using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Map;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The option values a <see cref="SuperMetroidGame"/> session reads: startup and testing conveniences around the
/// retail game, supplied by the host when it composes the session.
/// </summary>
/// <remarks>
/// Keeping them in one named type makes every departure from normal retail startup visible at the point where a game
/// session is constructed. How a host stores them (an options file, a settings menu, a recording) is the host's.
/// </remarks>
public sealed record SuperMetroidGameOptions
{
    /// <summary>
    /// Skips the story sequence between accepting the options screen and arriving at Ceres.
    /// </summary>
    /// <remarks>
    /// The title sequence, file-select screen, and game-options screen still run normally.
    /// The skip enters the same new-game setup state used when the cinematic finishes; it
    /// does not synthesize controller input or construct a special debug room.
    /// </remarks>
    public bool SkipOpeningCinematic { get; init; }

    /// <summary>Prevents emulated gameplay damage from reducing Samus below one energy.</summary>
    /// <remarks>
    /// This is a host-side testing convenience, not cartridge state. Damage routines still
    /// execute, so energy loss, hit reactions, and collision behavior remain available for
    /// debugging; the runtime changes a lethal zero-energy result to one. Shinesparks
    /// also bypass their low-energy cutoff and continue draining down to one, so this
    /// testing mode never requires an energy refill to use them. Collisions still stop them.
    /// </remarks>
    public bool Invincibility { get; init; }

    /// <summary>Keeps each unlocked consumable ammo type at one or more units.</summary>
    /// <remarks>
    /// Missiles, super missiles, and power bombs still consume normally. At the runtime
    /// frame boundary, an unlocked type which reached zero is raised to one. A type whose
    /// maximum remains zero is still locked and is never granted by this host option.
    /// </remarks>
    public bool InfiniteAmmo { get; init; }

    /// <summary>Grants and fills the complete collectible inventory on player initialization; normal saves retain it.</summary>
    public bool GrantAllEquipment { get; init; }

    /// <summary>Opens the Tourian statue passage without changing boss defeat flags.</summary>
    public bool UnlockTourian { get; init; }

    /// <summary>Resets bosses and persistent enemy encounters outside Tourian when loading a regular save.</summary>
    public bool ResetBossesOnLoad { get; init; }

    /// <summary>Lets Ceres and Zebes escape countdowns run normally, but holds them at 00:01.00 instead of expiring.</summary>
    public bool PreventEscapeTimeout { get; init; }

    /// <summary>
    /// Optional total minutes, 0 through 65535 (the hosts' options file and recordings accept up to 5999), used only for ending
    /// time/reward selection; null preserves actual playtime and SRAM is never changed.
    /// </summary>
    public int? EndingTimeOverrideMinutes { get; init => field = value is { } minutes ? Ensure.BetweenInclusive(0, ushort.MaxValue, minutes) : null; }

    /// <summary>Temporary map visibility used by the HUD and pause-map presentations.</summary>
    /// <remarks>
    /// This host convenience is evaluated only while drawing. It never changes exploration
    /// bits or area-map acquisition flags, so returning to <see cref="MapRevealMode.None"/>
    /// immediately restores the cartridge's saved visibility.
    /// </remarks>
    public MapRevealMode MapReveal { get; init; }

    /// <summary>
    /// The runtime's share of these options. Attract playback is retail behavior, so it receives
    /// <see cref="HostGameplayOverrides.None"/> whatever the host configured (#633).
    /// </summary>
    public HostGameplayOverrides RuntimeOverrides(bool forAttractDemo) => forAttractDemo
        ? HostGameplayOverrides.None
        : new HostGameplayOverrides
        {
            PlayerInvincibilityEnabled = Invincibility,
            InfiniteAmmoEnabled = InfiniteAmmo,
            MapRevealMode = MapReveal,
            PreventEscapeTimeout = PreventEscapeTimeout,
            GrantAllEquipment = GrantAllEquipment,
            UnlockTourian = UnlockTourian,
        };
}
