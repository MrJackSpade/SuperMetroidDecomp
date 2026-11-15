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

    /// <summary>
    /// The way it walks and faces, airborne and firing too; null from the start of each jump until it picks the side Samus
    /// is on, while a movement step moves it nowhere and it faces right.
    /// </summary>
    public HorizontalDirection? WalkingDirection { get; internal set; }

    /// <summary>
    /// Its vertical velocity in pixels per update: a jump starts at 12 pixels up and a retreat at 4 pixels up, and its
    /// acceleration pulls it back down.
    /// </summary>
    public SignedSixteenSixteen VerticalVelocity { get; internal set; }

    /// <summary>
    /// The population's whole-pixel spawn position. Y is restored after the initialization floor search and after
    /// retreat, leaving the live Y subposition unchanged; X enforces the 112-pixel walking range and is restored when
    /// hiding completes.
    /// </summary>
    public Position16<RoomPixels> SpawnPosition { get; internal set; }

    /// <summary>Vertical velocity increment per acceleration step; jump setup makes it half a pixel downward.</summary>
    public SignedSixteenSixteen YAcceleration { get; internal set; }

    /// <summary>The floor-aligned position the complete initialization jump lands at: the spawn X and the landing Y.</summary>
    public Position16<RoomPixels> LandingPosition { get; internal set; }

    /// <summary>Number of four-step walking cycles remaining before another fire volley.</summary>
    public int StepCounter { get; internal set; }
}
