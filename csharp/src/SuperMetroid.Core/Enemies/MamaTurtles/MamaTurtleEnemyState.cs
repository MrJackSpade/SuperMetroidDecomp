using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>The state of the Mama Turtle.</summary>
public sealed class MamaTurtleEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal MamaTurtleEnemyState(MamaTurtleAction action) => Action = action;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<MamaTurtleSegment> Playback { get; set; }

    /// <summary>The current Mama Turtle behaviour.</summary>
    public MamaTurtleAction Action { get; internal set; }

    /// <summary>Mama's velocity, in pixels an update.</summary>
    public FixedVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>Whether Mama is dormant in her shell; the first touch or shot on one of her babies wakes her.</summary>
    public bool Asleep { get; internal set; }

    /// <summary>Gets the phase-specific countdown used by peak and transition states.</summary>
    public int ActionTimer { get; internal set; }

    /// <summary>
    /// Mama's sideways acceleration while she hovers, in pixels an update each update, toward Samus's side when hovering
    /// starts; a wall turns it round (<see cref="MamaTurtleWallReversal"/>).
    /// </summary>
    public SignedSixteenSixteen XAcceleration { get; internal set; }
}
