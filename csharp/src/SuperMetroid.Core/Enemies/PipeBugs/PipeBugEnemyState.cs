using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>
/// Typed state of the three Pipe Bug families. Each property belongs to the family its summary names; the others never
/// read it.
/// </summary>
public sealed class PipeBugEnemyState
{
    /// <summary>Starts the state of the Pipe Bug set up in <paramref name="slot"/>, recording which family it is.</summary>
    internal PipeBugEnemyState(RoomEnemySlot slot)
    {
        DefinitionAtInitialization = slot.DefinitionId;
    }

    /// <summary>The animation the bug plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<PipeBugSegment> Playback { get; set; }

    /// <summary>The Pipe Bug family this state was set up for, one of the three that share it.</summary>
    public EnemyDefinitionId DefinitionAtInitialization { get; }

    /// <summary>
    /// Whether a Brinstar Pipe Bug flies left, set from Samus's side of the bug every update it waits with Samus in its
    /// vertical band.
    /// </summary>
    public bool BrinstarFliesLeft { get; internal set; }

    /// <summary>The current action.</summary>
    public PipeBugAction? Action { get; internal set; }

    /// <summary>Gets the whole-pixel spawn position restored after leaving the screen.</summary>
    public Position16<RoomPixels> SpawnPosition { get; internal set; }
    /// <summary>
    /// The highest point of a Brinstar Pipe Bug's emergence, sixteen pixels above its spawn point; only Y is
    /// meaningful. The bug keeps rising while it is still below this point.
    /// </summary>
    public Position16<RoomPixels> EmergenceTop { get; internal set; }
    /// <summary>The Brinstar animation main AI asks for.</summary>
    public PipeBugSegment RequestedAnimation { get; internal set; }
    /// <summary>The Brinstar animation currently installed; a request for it installs nothing.</summary>
    public PipeBugSegment InstalledAnimation { get; internal set; }

    /// <summary>
    /// A Norfair formation member's stagger count, the updates it has risen and waited. It returns to zero only when the
    /// member departs on reaching its <see cref="StaggerTarget"/>, so a member reset off screen keeps its count into the
    /// next rise; it never passes the largest target by more than its rise, far short of where the game's
    /// count would go round (#1846).
    /// </summary>
    public int FormationCounter { get; internal set; }

    /// <summary>Counts one formation update.</summary>
    internal void CountFormationTick() => FormationCounter++;

    /// <summary>
    /// Whether a Norfair member's count has reached its <see cref="StaggerTarget"/>, judged as the game does by
    /// their difference wrapped to sixteen bits and read as signed.
    /// </summary>
    internal bool HasReachedStaggerTarget => FormationCounter >= StaggerTarget;

    /// <summary>
    /// A Brinstar Pipe Bug's respawn countdown, in AI updates: started at 48 when it leaves the screen, and expiring
    /// when a decrement reaches zero.
    /// </summary>
    public int RespawnDelay { get; internal set; }
    /// <summary>The speed, and its negation, this Norfair bug flies at: the spawn settings' speed.</summary>
    public EnemyLinearSpeedRow FlightSpeeds { get; internal set; }
    /// <summary>The point at which a Norfair member's initial rise ended; only Y is meaningful. Its stagger moves are measured from it.</summary>
    public Position16<RoomPixels> RiseEnd { get; internal set; }
    /// <summary>The formation count, 72 through 136, at which this Norfair member departs.</summary>
    public int StaggerTarget { get; internal set; }
    /// <summary>Gets the action selected for this Norfair member after rising.</summary>
    public PipeBugAction NorfairPostRiseAction { get; internal set; }
    /// <summary>A yellow Pipe Bug's flight speed in pixels per update, the spawn settings' speed: added flying right and negated flying left.</summary>
    public SignedSixteenSixteen FlightSpeed { get; internal set; }
    /// <summary>Gets the yellow Pipe Bug's countdown before straight flight begins.</summary>
    public int EmergenceDelay { get; internal set; }
    /// <summary>
    /// The whole-mark place along the speed curve a yellow Pipe Bug's arc reads, from the curve's start through the
    /// curve's last mark. The rising phase moves it back from mark 40 and ends when it would pass the curve's start; the returning
    /// phase moves it back on.
    /// </summary>
    public QuadraticSpeedCurvePlace ArcCounter { get; internal set; }
    /// <summary>Whether a yellow Pipe Bug's arc is in its rising phase rather than its returning one.</summary>
    public bool ArcRising { get; internal set; }
    /// <summary>Gets whether a yellow Pipe Bug has completed its one authored arc.</summary>
    public bool ArcCompleted { get; internal set; }
}
