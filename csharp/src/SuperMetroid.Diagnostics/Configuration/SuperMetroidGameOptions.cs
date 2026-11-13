using SuperMetroid.Core;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Map;

namespace SuperMetroid.Diagnostics.Configuration;

/// <summary>
/// The tester's game options: startup and testing conveniences around the retail game, as the options file and input
/// recordings store them. The game receives them as <see cref="GameplayAssists"/> (<see cref="ToAssists"/>).
/// </summary>
/// <remarks>
/// Keeping them in one named type makes every departure from normal retail startup visible where a host composes a game
/// session. The property names are the options file's keys and must not change.
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

    /// <summary>Prevents gameplay damage from reducing Samus below one energy.</summary>
    /// <remarks>
    /// Damage still applies, so energy loss, hit reactions, and collision behavior remain available for
    /// testing; a lethal zero-energy result becomes one. Shinesparks also bypass their low-energy cutoff
    /// and continue draining down to one, so this testing mode never requires an energy refill to use
    /// them. Collisions still stop them.
    /// </remarks>
    public bool Invincibility { get; init; }

    /// <summary>Keeps each unlocked consumable ammo type at one or more units.</summary>
    /// <remarks>
    /// Missiles, super missiles, and power bombs still consume normally. An unlocked type which reaches
    /// zero is raised to one. A type whose maximum remains zero is still locked and is never granted by
    /// this option.
    /// </remarks>
    public bool InfiniteAmmo { get; init; }

    /// <summary>Grants and fills the complete collectible inventory (<see cref="TesterAssistDefinitions.Loadout"/>) on player initialization; normal saves retain it.</summary>
    public bool GrantAllEquipment { get; init; }

    /// <summary>Opens the Tourian statue passage without changing boss defeat flags.</summary>
    public bool UnlockTourian { get; init; }

    /// <summary>Resets bosses and persistent enemy encounters outside Tourian (<see cref="TesterAssistDefinitions.EncounterReset"/>) when loading a regular save.</summary>
    public bool ResetBossesOnLoad { get; init; }

    /// <summary>Lets Ceres and Zebes escape countdowns run normally, but holds them at 00:01.00 instead of expiring.</summary>
    public bool PreventEscapeTimeout { get; init; }

    /// <summary>
    /// Optional total minutes, 0 through <see cref="SuperMetroid.Core.Progression.GameTimeState.LongestPlayMinutes"/>, the
    /// play times the game clock reaches, used only for ending time/reward selection; null preserves actual playtime and
    /// the saved play time is never changed.
    /// </summary>
    public int? EndingTimeOverrideMinutes { get; init => field = value is { } minutes ? Ensure.BetweenInclusive(0, SuperMetroid.Core.Progression.GameTimeState.LongestPlayMinutes, minutes) : null; }

    /// <summary>Temporary map visibility used by the HUD and pause-map presentations.</summary>
    /// <remarks>
    /// This convenience is evaluated only while drawing. It never changes exploration
    /// bits or area-map acquisition flags, so returning to <see cref="MapRevealMode.None"/>
    /// immediately restores the game's saved visibility.
    /// </remarks>
    public MapRevealMode MapReveal { get; init; }

    /// <summary>The assists these options ask the game for; attract demos play unassisted whatever they ask.</summary>
    public GameplayAssists ToAssists() => new()
    {
        SamusCannotDie = Invincibility,
        AmmoNeverRunsOut = InfiniteAmmo,
        EscapeTimerHoldsAtOneSecond = PreventEscapeTimeout,
        MapReveal = MapReveal,
        TourianStatuesOpen = UnlockTourian,
        GrantedLoadout = GrantAllEquipment ? TesterAssistDefinitions.Loadout : null,
        EncounterResetOnLoad = ResetBossesOnLoad ? TesterAssistDefinitions.EncounterReset : null,
        SkipOpeningCinematic = SkipOpeningCinematic,
        EndingClearTimeMinutes = EndingTimeOverrideMinutes,
    };
}
