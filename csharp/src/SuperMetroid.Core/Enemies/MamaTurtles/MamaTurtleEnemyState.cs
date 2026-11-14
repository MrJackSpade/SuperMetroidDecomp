using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>The state of the Mama Turtle.</summary>
public sealed class MamaTurtleEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<MamaTurtleSegment> Playback { get; set; }

    /// <summary>The current Mama Turtle behaviour.</summary>
    public MamaTurtleAction? Action { get; internal set; }

    /// <summary>Signed 1/65536-pixel velocity per AI update.</summary>
    public FixedVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>Whether Mama is asleep in her shell; the first touch or shot on one of her babies wakes her.</summary>
    public bool Asleep { get; internal set; }

    /// <summary>Gets the phase-specific countdown used by peak and transition states.</summary>
    public int ActionTimer { get; internal set; }

    /// <summary>
    /// Signed 1/65536-pixel horizontal acceleration per hovering update, toward Samus's side when hovering starts; a wall
    /// reversal negates it with the game's quirk of negating the whole and fraction parts separately.
    /// </summary>
    public SignedSixteenSixteen XAcceleration { get; internal set; }
}
