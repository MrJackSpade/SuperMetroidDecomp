namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The Speed Booster running cadence the game never authored (#1473), timed by what Ceres Ridley's getaway left behind
/// (#1338). The game authors five boost cadences, one per boost stage, and picks the next one from what the boost's echo
/// sound request reports. When that request finds five sounds already queued, it reports a sixth cadence that does not
/// exist: the run's first frame after the advance is timed by a delay read past the authored cadences, and the boost's
/// cadence countdown restarts at zero, the value read past the authored restart values. Later frames take the full
/// stage's own cadence. The delays read past the authored cadences are what the getaway's wing animation leaves behind
/// when it redraws the floor: before any getaway they are all zero, and each wing frame the getaway draws replaces them
/// with that frame's ten delays for the rest of the session. The capsule's frames leave them as they were.
/// </summary>
/// <remarks>
/// Owned by the session (<c>FrontendSession</c>), which outlives every gameplay runtime as the original's memory does.
/// Recorded during the enemy pass, when the getaway's floor update advances the wing animation (every eighth frame of
/// the frame counter); consulted by Samus's running animation after movement, when the boost stage advances to full
/// while the echo finds the sound queue full, for the delay of the run's first frame. Evidence: the
/// original's sixth cadence entry (<c>$91:0303</c>) lies in the floor update queue the getaway's descriptors fill, and
/// the restart table's sixth entry reads the zero facing and movement bytes of the first pose record. The #1338
/// inventory of every queuer found that only the getaway's wing frames, queued after the capsule's two descriptors,
/// reach that far, and that nothing but power-on clears it; <c>--speed-booster-stage-five-residue</c> rebuilds the
/// queue the getaway leaves from the retail cartridge and confirms the delays. Every recording that starts a new game
/// passes through the getaway, so its delays hold from there on; the unauthored cadence itself is reached only when the
/// echo meets a full sound queue.
/// </remarks>
[CompatibilityService("bounded over-read: a full sound queue sends the Speed Booster run onto a cadence the game never authored, timed by what Ceres Ridley's getaway left behind")]
public sealed class UnauthoredBoostCadence
{
    /// <summary>The running frames the cadence times; the run's loop closes after the last.</summary>
    public const int FrameCount = 10;

    /// <summary>The cadence countdown the boost restarts with on this cadence.</summary>
    public const int CountdownAfterAdvance = 0;

    /// <summary>The longest delay, in ticks, one running frame of the cadence can take.</summary>
    public const int LongestDelay = byte.MaxValue;

    /// <summary>The ten delays, by running frame: all zero until the getaway first draws its wings.</summary>
    private int[] delays = new int[FrameCount];

    /// <summary>
    /// Takes the delays a wing frame of the getaway leaves behind as it redraws the floor; they time the cadence until
    /// the next wing frame.
    /// </summary>
    /// <param name="wingDelays">The wing frame's ten delays, by running frame, each 0 through <see cref="LongestDelay"/> ticks.</param>
    /// <exception cref="InvalidDataException">The delays are not ten, or one lies outside a running frame's range.</exception>
    public void TakeDelaysLeftByWings(IReadOnlyList<int> wingDelays)
    {
        Ensure.NotNull(wingDelays);
        if (wingDelays.Count != FrameCount)
            throw new InvalidDataException($"The unauthored Speed Booster cadence takes {FrameCount} delays; the wings left {wingDelays.Count}.");
        foreach (int delay in wingDelays)
        {
            if (delay is < 0 or > LongestDelay)
                throw new InvalidDataException($"A running delay of {delay} ticks lies outside 0 through {LongestDelay}.");
        }
        delays = [.. wingDelays];
    }

    /// <summary>The delay, 0 through <see cref="LongestDelay"/> ticks, of running frame <paramref name="frame"/>.</summary>
    /// <param name="frame">The running frame, zero through <see cref="FrameCount"/> less one.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is not one of the ten running frames.</exception>
    public int Delay(int frame) => delays[Ensure.BetweenInclusive(0, FrameCount - 1, frame)];
}
