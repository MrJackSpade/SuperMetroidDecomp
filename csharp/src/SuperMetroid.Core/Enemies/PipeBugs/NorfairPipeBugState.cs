using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>
/// A Norfair Pipe Bug's state, one Gamet of a five-bug formation (#1845): it waits in its pipe with the rest, rises past
/// Samus, staggers to its place in the formation and departs toward Samus when its count reaches its role's target.
/// </summary>
public sealed class NorfairPipeBugState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal NorfairPipeBugState(PipeBugAction action) => Action = action;

    /// <summary>The animation the bug plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<PipeBugSegment> Playback { get; set; }

    /// <summary>The current action, one of the Norfair actions.</summary>
    public PipeBugAction Action { get; internal set; }

    /// <summary>The whole-pixel spawn position restored when the bug leaves the screen.</summary>
    public Position16<RoomPixels> SpawnPosition { get; internal set; }

    /// <summary>
    /// The member's stagger count, the updates it has risen and waited. It returns to zero only when the member departs
    /// on reaching its <see cref="StaggerTarget"/>, so a member reset off screen keeps its count into the next rise; it
    /// never passes the largest target by more than its rise, far short of where the game's count would go round (#1846).
    /// </summary>
    public int FormationCounter { get; internal set; }

    /// <summary>Counts one formation update.</summary>
    internal void CountFormationTick() => FormationCounter++;

    /// <summary>
    /// Whether the member's count has reached its <see cref="StaggerTarget"/>, judged as the game does by their difference
    /// wrapped to sixteen bits and read as signed.
    /// </summary>
    internal bool HasReachedStaggerTarget => FormationCounter >= StaggerTarget;

    /// <summary>The speed the bug flies at, the spawn settings' speed: added flying one way and negated flying the other.</summary>
    public EnemyLinearSpeed FlightSpeeds { get; internal set; }

    /// <summary>The room Y at which the member's rise ended. Its stagger moves are measured from it.</summary>
    public int RiseEndY { get; internal set; }

    /// <summary>The formation count, 88 through 120, at which the member departs: its role's (#1840).</summary>
    public int StaggerTarget { get; internal set; }

    /// <summary>The stagger the member runs after rising, its role's (#1840), or null before its formation assigns one.</summary>
    public PipeBugAction? PostRiseAction { get; internal set; }

    /// <summary>How far, in pixels, the member's stagger moves it from where its rise ended: its role's (#2071).</summary>
    public int StaggerOffset { get; internal set; }
}
