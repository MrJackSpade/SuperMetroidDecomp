using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Owtch;

/// <summary>
/// The state of an Owtch: its patrol state and bounds and how deep it is buried. It reads its patrol speed from its spawn
/// settings as it walks (#2439).
/// </summary>
public sealed class OwtchEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<OwtchSegment> Playback { get; set; }

    /// <summary>The patrol state: walking either way, sinking, underground or rising.</summary>
    public OwtchBehaviorState Behavior { get; internal set; }

    /// <summary>
    /// The Owtch's sink below its walking height, one pixel each sinking update: its age is the depth sunk, and it is over
    /// at <see cref="OwtchMovementDefinitions.MaximumBurialDepth"/>, where the Owtch goes underground.
    /// </summary>
    public UpdateCountdown Sink { get; internal set; } = UpdateCountdown.Ended();

    /// <summary>
    /// The Owtch's rise out of its burial, one pixel each rising update: its updates left are the depth still buried, and
    /// it is over as the Owtch surfaces. An Owtch rises only out of a burial, from the underground wait or its buried
    /// spawn, so the rise always starts at <see cref="OwtchMovementDefinitions.MaximumBurialDepth"/> (#1846).
    /// </summary>
    public UpdateCountdown Rise { get; internal set; } = UpdateCountdown.Ended();

    /// <summary>The underground wait, which ends on the update the Owtch starts rising.</summary>
    public UpdateCountdown UndergroundTimer { get; internal set; } = UpdateCountdown.Ended();

    /// <summary>The left end of the patrol: the spawn X less the travel distance.</summary>
    public int MinimumX { get; internal set; }

    /// <summary>The right end of the patrol: the spawn X plus the travel distance.</summary>
    public int MaximumX { get; internal set; }
}
