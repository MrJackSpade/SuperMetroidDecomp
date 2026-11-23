using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>The state of the Mama Turtle.</summary>
public sealed partial class MamaTurtleEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal MamaTurtleEnemyState(MamaTurtleAction action) => Action = action;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<MamaTurtleSegment> Playback { get; set; }

    /// <summary>The current Mama Turtle behaviour.</summary>
    public MamaTurtleAction Action { get; internal set; }

    /// <summary>
    /// Mama's velocity, in pixels an update. Its horizontal part stays signed rather than a heading with a speed
    /// (#2473): her hover's acceleration can point against it, so it slows through zero and turns her round, and a wall
    /// reverses it and changes its size by a pixel an update (<see cref="MamaTurtleWallReversal"/>). Its sign is the
    /// way she moves, which her landing faces.
    /// </summary>
    public FixedVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>
    /// Whether Mama lies in her shell, dormant or just woken, whose contour her babies crawl over; before her first update
    /// she does not yet.
    /// </summary>
    internal bool LiesInShell => Action is MamaTurtleAction.Asleep or MamaTurtleAction.Woken;

    /// <summary>
    /// The first touch or shot on one of her babies wakes Mama in her shell (#2439); she leaves it on her next update that
    /// finds her there. A later touch or shot finds her already awake and does nothing.
    /// </summary>
    internal void Wake()
    {
        if (Action == MamaTurtleAction.Placed)
            Action = MamaTurtleAction.PlacedAndWoken;
        else if (Action == MamaTurtleAction.Asleep)
            Action = MamaTurtleAction.Woken;
    }

    /// <summary>Gets the phase-specific countdown used by peak and transition states.</summary>
    public UpdateCountdown ActionTimer { get; internal set; } = UpdateCountdown.Ended();

    /// <summary>
    /// Mama's sideways acceleration while she hovers, in pixels an update each update, toward Samus's side when hovering
    /// starts; a wall turns it round (<see cref="MamaTurtleWallReversal"/>). It stays signed rather than a direction with a
    /// size (#2473): the wall's reversal lowers it a further pixel after a hit at under a pixel an update leftward, which
    /// changes its size as well as its way.
    /// </summary>
    public SignedSixteenSixteen XAcceleration { get; internal set; }
}
