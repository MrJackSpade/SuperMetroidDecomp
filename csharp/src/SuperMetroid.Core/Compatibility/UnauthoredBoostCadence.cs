namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The Speed Booster running cadence the game never authored (#1473). The game authors five boost cadences, one per boost
/// stage, and picks the next one from what the boost's echo sound request reports. When that request finds five sounds
/// already queued, it reports a sixth cadence that does not exist: the run's first frame after the advance is timed by a
/// delay read past the authored cadences, one of the bytes the Ceres Ridley getaway's floor updates left behind, and the
/// boost's cadence countdown restarts at zero, the value read past the authored restart values. Later frames take the
/// full stage's own cadence.
/// </summary>
/// <remarks>
/// Consulted by Samus's running animation when the boost stage advances to full while the echo finds the sound queue
/// full, after movement in the update. Evidence: the original stage table's sixth entry (<c>$91:0303</c>) lies
/// in the floor update descriptors' residue and the restart table's sixth entry reads the zero facing and movement bytes of
/// the first pose record; the Ceres SMV's running stage-five reads established the residue delays (#1338).
/// </remarks>
[CompatibilityService("bounded over-read: a full sound queue sends the Speed Booster run onto a cadence the game never authored")]
public static class UnauthoredBoostCadence
{
    /// <summary>The running frames the cadence times; the run's loop closes after the last.</summary>
    public const int FrameCount = 10;

    /// <summary>The cadence countdown the boost restarts with on this cadence.</summary>
    public const int CountdownAfterAdvance = 0;

    /// <summary>The residue byte that times the cadence's first frame; each later frame reads the byte after.</summary>
    private const int FirstFrameResidueByte = 51;

    /// <summary>The delay, 0 through 255 ticks, of running frame <paramref name="frame"/>.</summary>
    /// <param name="residue">What the session's floor updates left behind.</param>
    /// <param name="frame">The running frame, zero through <see cref="FrameCount"/> less one.</param>
    /// <exception cref="InvalidOperationException">No residue is bound to the running Samus.</exception>
    public static int Delay(IFloorUpdateResidue? residue, int frame) =>
        (residue ?? throw new InvalidOperationException(
            "The unauthored Speed Booster cadence reads the floor update residue; bind the session's to this Samus."))
        .ByteAt(FirstFrameResidueByte + Ensure.BetweenInclusive(0, FrameCount - 1, frame));
}
