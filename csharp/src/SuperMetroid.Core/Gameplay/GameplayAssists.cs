using SuperMetroid.Core.Map;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// Assists that change how forgiving a session is without changing how the game plays out underneath them: Samus
/// cannot die, ammunition never runs out, the escape countdown holds at its last second, the map is revealed, the
/// Tourian statues stand open, a loadout is granted, defeated encounters return when a save is loaded, the opening
/// cinematic is skipped, and the ending reads a chosen clear time. <see cref="None"/> is the unassisted game.
/// </summary>
/// <remarks>
/// Every assist works at one seam around the game's own behaviour, so damage, firing, timers and room setup keep their
/// ordinary side effects. Attract-demo playback always runs unassisted.
/// </remarks>
public sealed record GameplayAssists
{
    /// <summary>The unassisted game: no assist is active.</summary>
    public static GameplayAssists None { get; } = new();

    /// <summary>
    /// Whether Samus cannot die: a lethal hit leaves her at one energy, and a shinespark keeps draining down to one
    /// energy instead of stopping at its low-energy cutoff. Collisions still end a shinespark.
    /// </summary>
    /// <remarks>
    /// Every damage source applies its ordinary amount, reaction and animation; only a zero-energy result at the end of
    /// the update is raised to one, before the game can enter death.
    /// </remarks>
    public bool SamusCannotDie { get; init; }

    /// <summary>
    /// Whether ammunition never runs out: each unlocked missile, super missile and power bomb count stays at one or
    /// more. A type whose maximum is zero is still locked and is never granted.
    /// </summary>
    /// <remarks>
    /// Firing still consumes ammunition and publishes its ordinary projectile, HUD and sound effects; the selected
    /// weapon stays selected after its last round (<see cref="AmmoLoanGuard"/>).
    /// </remarks>
    public bool AmmoNeverRunsOut { get; init; }

    /// <summary>Whether the Ceres and Zebes escape countdowns run normally but hold at one second instead of expiring.</summary>
    public bool EscapeTimerHoldsAtOneSecond { get; init; }

    /// <summary>
    /// How much of the map the HUD minimap and the pause map show. The reveal is drawn only: it never marks rooms
    /// explored or area maps collected, so returning to <see cref="MapRevealMode.None"/> restores the saved map.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The mode is not a named reveal.</exception>
    public MapRevealMode MapReveal
    {
        get;
        init => field = Enum.IsDefined(value) ? value : throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown map reveal mode.");
    }

    /// <summary>
    /// Whether the Tourian statues stand open: the statue room starts fully descended with its passage cleared, without
    /// setting any boss defeat or the Tourian-unlocked event.
    /// </summary>
    public bool TourianStatuesOpen { get; init; }

    /// <summary>
    /// The loadout granted once to each new Samus and again each time a save restores her inventory; <see langword="null"/>
    /// grants nothing. Later damage, ammunition use and equipment choices are kept until the next restore.
    /// </summary>
    public SamusLoadout? GrantedLoadout { get; init; }

    /// <summary>
    /// The encounters returned to their undefeated state when a regular save is loaded, after its progress is restored and
    /// before its room is chosen; <see langword="null"/> keeps the saved encounters. Room transitions never apply it.
    /// </summary>
    public SavedEncounterReset? EncounterResetOnLoad { get; init; }

    /// <summary>
    /// Whether accepting the options screen of a new game goes straight to the Ceres arrival, skipping the story
    /// sequence. The title, file-select and options screens still run, and the arrival is the same setup the finished
    /// cinematic reaches.
    /// </summary>
    public bool SkipOpeningCinematic { get; init; }

    /// <summary>
    /// The clear time, in total minutes from 0 through 65535, the ending reads to choose its time display and reward;
    /// <see langword="null"/> reads the actual play time. The saved play time is never changed.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The minutes are outside 0 through 65535.</exception>
    public int? EndingClearTimeMinutes
    {
        get;
        init => field = value is { } minutes ? Ensure.BetweenInclusive(0, ushort.MaxValue, minutes) : null;
    }
}
