using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>The state of one Alcoon.</summary>
public sealed class AlcoonEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal AlcoonEnemyState(AlcoonAction action) => Action = action;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<AlcoonSegment> Playback { get; set; }

    /// <summary>The current behaviour.</summary>
    public AlcoonAction Action { get; internal set; }

    /// <summary>The way it walks and faces, airborne and firing too.</summary>
    public AlcoonWalk WalkingDirection { get; internal set; }

    /// <summary>
    /// Its vertical velocity in pixels per update: a jump and a retreat each start at their own upward speed, and its
    /// acceleration pulls it back down.
    /// </summary>
    public SignedSixteenSixteen VerticalVelocity { get; internal set; }

    /// <summary>
    /// The population's whole-pixel spawn position. Y is restored after the initialization floor search and after
    /// retreat, leaving where it lies within a pixel vertically unchanged; X enforces the walking range and is restored when
    /// hiding completes.
    /// </summary>
    public Position16<RoomPixels> SpawnPosition { get; internal set; }

    /// <summary>
    /// The floor row the complete initialization jump lands on (#2531): Samus must come within the emerging distance of it,
    /// vertically, to wake the Alcoon.
    /// </summary>
    public int LandingY { get; internal set; }

    /// <summary>The walk cycles, each of four strides, the Alcoon still walks before another fire volley, counted as each completes.</summary>
    public UpdateCountdown WalkCyclesBeforeVolley { get; internal set; } = UpdateCountdown.Ended();
}
