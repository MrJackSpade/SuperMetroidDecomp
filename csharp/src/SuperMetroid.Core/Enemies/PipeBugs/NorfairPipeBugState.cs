using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>
/// A Norfair Pipe Bug's state, one Gamet of a five-bug formation (#1845): it waits in its pipe with the rest, rises past
/// Samus, staggers to its place in the formation and departs toward Samus when its count reaches its role's target.
/// </summary>
public sealed class NorfairPipeBugState
{
    /// <summary>What the enemy's population entry told its species, which it starts from and reads for its life (#2554).</summary>
    internal NorfairPipeBugSpawnSettings Settings { get; }

    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    /// <param name="settings">What the enemy's population entry told its species.</param>
    internal NorfairPipeBugState(NorfairPipeBugAction action, NorfairPipeBugSpawnSettings settings)
    {
        Settings = Ensure.NotNull(settings);
        Action = action;
    }

    /// <summary>The animation the bug plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<PipeBugSegment> Playback { get; set; }

    /// <summary>The current action, one of the Norfair actions.</summary>
    public NorfairPipeBugAction Action { get; internal set; }

    /// <summary>The way a departed member flies (#2165), chosen toward Samus as it departs.</summary>
    public HorizontalDirection FlightDirection { get; internal set; }

    /// <summary>The whole-pixel spawn position restored when the bug leaves the screen.</summary>
    public Position16<RoomPixels> SpawnPosition { get; internal set; }

    /// <summary>
    /// The member's wait before it departs, lasting its <see cref="Role"/>'s target: its age is the updates it has risen
    /// and waited. It starts again only when the member departs, so a member reset off screen keeps its count into the
    /// next rise (#1846).
    /// </summary>
    public UpdateCountdown Departure { get; private set; } = UpdateCountdown.Ended();

    /// <summary>Counts one formation update.</summary>
    internal void CountFormationTick() => Departure.Step();

    /// <summary>Whether the member's wait has reached its <see cref="Role"/>'s target.</summary>
    internal bool HasReachedStaggerTarget => Departure.IsOver;

    /// <summary>Starts the member's wait again, as it departs.</summary>
    internal void RestartDeparture() => Departure.Restart(Role.StaggerTarget);

    /// <summary>The room Y at which the member's rise ended. Its stagger moves are measured from it.</summary>
    public int RiseEndY { get; internal set; }

    /// <summary>
    /// The member's part in its formation (#1840, #2071): the stagger it runs after rising, how far that moves it and the
    /// formation count at which it departs. Its formation assigns it as it launches the rise; a member not yet launched
    /// has none, and reading it then fails.
    /// </summary>
    /// <exception cref="InvalidOperationException">The member's formation has not launched it.</exception>
    internal NorfairPipeBugRole Role
    {
        get => field ?? throw new InvalidOperationException("A Norfair Pipe Bug's role is assigned when its formation launches it.");
        set
        {
            field = Ensure.NotNull(value);
            Departure.LastFor(value.StaggerTarget);
        }
    }
}
