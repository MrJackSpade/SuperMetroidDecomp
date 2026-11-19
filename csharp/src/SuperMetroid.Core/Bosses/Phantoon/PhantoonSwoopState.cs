namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's swoop at Samus (#1933): whether a shot has asked for one, and the swoop's velocity and the target it chases.
/// A swoop's start sets its velocity and target before its first update reads them.
/// </summary>
public sealed class PhantoonSwoopState
{
    /// <summary>Whether a shot during his vulnerable window has asked him to swoop once the window closes.</summary>
    public bool Requested { get; internal set; }

    /// <summary>The swoop's velocity in pixels an update.</summary>
    public PixelVelocity<RoomPixels> Velocity { get; internal set; }

    /// <summary>
    /// The X the swoop chases, which sweeps out by two pixels an update to its turn at
    /// <see cref="PhantoonMotionDefinitions.SwoopTargetTurnX"/> and back to zero.
    /// </summary>
    public int TargetX { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>Whether the target has reached its turn and is sweeping back toward zero.</summary>
    public bool TargetReturning { get; internal set; }
}
