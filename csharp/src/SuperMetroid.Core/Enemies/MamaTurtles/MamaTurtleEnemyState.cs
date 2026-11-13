using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>The state of the Mama Turtle.</summary>
public sealed class MamaTurtleEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<MamaTurtleSegment> Playback { get; set; }

    /// <summary>The current Mama Turtle behaviour.</summary>
    public MamaTurtleAction? Action { get; internal set; }

    /// <summary>Signed 16.16 velocity per AI update.</summary>
    public FixedVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>Wake counter; each Baby touch or shot decrements it, and the Mama wakes once it reaches zero.</summary>
    public int AsleepFlag { get; internal set; }

    /// <summary>Gets the phase-specific countdown used by peak and transition states.</summary>
    public int ActionTimer { get; internal set; }

    /// <summary>
    /// Signed 16.16 horizontal acceleration per hovering update, toward Samus's side when hovering starts; a wall
    /// reversal negates it with the game's quirk of negating the whole and fraction parts separately.
    /// </summary>
    public SignedSixteenSixteen XAcceleration { get; internal set; }
}
