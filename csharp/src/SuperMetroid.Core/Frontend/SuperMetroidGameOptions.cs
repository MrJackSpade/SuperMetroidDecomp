using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Host-selected behavior for a complete <see cref="SuperMetroidGame"/> session.
/// </summary>
/// <remarks>
/// These are deliberately not SNES registers or translated WRAM fields. They describe the
/// few conveniences supplied by the C# host around the cartridge-authentic state machine.
/// Keeping them in one named type makes every departure from normal retail startup visible
/// at the point where a game session is constructed.
/// </remarks>
public sealed record SuperMetroidGameOptions
{
    /// <summary>Prefer hardware rendering; report startup failure before selecting the software reference backend.</summary>
    public RendererSelection Renderer { get; init; } = RendererSelection.Auto;
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

    /// <summary>Preserves a separate recovery state after each completed door transition.</summary>
    public bool DoorTransitionAutosave { get; init; } = true;

    /// <summary>Lets Ceres and Zebes escape countdowns run normally, but holds them at 00:01.00 instead of expiring.</summary>
    public bool PreventEscapeTimeout { get; init; }

    /// <summary>Optional total minutes used only for ending time/reward selection; null preserves actual playtime and SRAM is never changed.</summary>
    public ushort? EndingTimeOverrideMinutes { get; init; }

    /// <summary>Temporary map visibility used by the HUD and pause-map presentations.</summary>
    /// <remarks>
    /// This host convenience is evaluated only while drawing. It never changes exploration
    /// bits or area-map acquisition flags, so returning to <see cref="MapRevealMode.None"/>
    /// immediately restores the cartridge's saved visibility.
    /// </remarks>
    public MapRevealMode MapReveal { get; init; }

    /// <summary>Whether the desktop host creates the SPC/DSP mixer and Windows device.</summary>
    public bool AudioEnabled { get; init; } = true;

    /// <summary>Host PCM gain after cartridge mixing, from zero (mute) to 100.</summary>
    public int MasterVolumePercent { get; init; } = 100;

    /// <summary>Whether recoverable desktop-host errors are filed as GitHub issues.</summary>
    /// <remarks>
    /// This development convenience is deliberately disabled by default. When enabled, the
    /// desktop host prints the complete failure locally, queues a deduplicated issue through
    /// the authenticated GitHub CLI, and attempts the next emulated frame. It is not part of
    /// cartridge state and is therefore not serialized into deterministic input recordings.
    /// </remarks>
    public bool ReportErrorsToGitHub { get; init; }

    /// <summary>GitHub <c>owner/repository</c> receiving automatic private error reports.</summary>
    public string GitHubErrorRepository { get; init; } = "MrJackSpade/SuperMetroidDecomp";

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
