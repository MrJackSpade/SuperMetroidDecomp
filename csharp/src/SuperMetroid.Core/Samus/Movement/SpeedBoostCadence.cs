namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// The cadence a Speed Booster run animates by (#2553): one of the boost's authored stages, or the unauthored cadence a
/// full sound queue selects (<see cref="SuperMetroid.Core.Compatibility.UnauthoredBoostCadence"/>). Each gives the countdown
/// the boost restarts with and the delay of each running frame.
/// </summary>
internal abstract partial class SpeedBoostCadence
{
    /// <summary>The unauthored cadence, which the session's unauthored Speed Booster cadence times.</summary>
    internal static SpeedBoostCadence Unauthored { get; } = new UnauthoredStage();

    /// <summary>The countdown, 0 through 2, the boost restarts with on this cadence.</summary>
    internal abstract int CountdownAfterAdvance { get; }

    /// <summary>The cadence of an authored boost stage.</summary>
    /// <param name="timing">The stage's frame delays.</param>
    /// <param name="boost">The stage's restart.</param>
    internal static SpeedBoostCadence Authored(SamusRunningCadenceTiming timing, SamusBoostStageTiming boost) => new AuthoredStage(timing, boost);

    /// <summary>The delay, in ticks, of running frame <paramref name="frame"/> on this cadence.</summary>
    /// <param name="unauthored">The session's unauthored Speed Booster cadence, which times the unauthored cadence's frames.</param>
    /// <param name="frame">The running frame, zero through nine.</param>
    internal abstract int Delay(UnauthoredBoostCadence unauthored, int frame);

    /// <summary>An authored boost stage's cadence: its own delays and restart countdown.</summary>
    /// <param name="timing">The stage's frame delays.</param>
    /// <param name="boost">The stage's restart.</param>
    private sealed partial class AuthoredStage(SamusRunningCadenceTiming timing, SamusBoostStageTiming boost) : SpeedBoostCadence
    {
        /// <inheritdoc/>
        internal override int CountdownAfterAdvance => boost.CountdownAfterAdvance;

        /// <inheritdoc/>
        internal override int Delay(UnauthoredBoostCadence unauthored, int frame) => timing.Delays[frame];
    }

    /// <summary>The unauthored cadence, timed by the session's unauthored Speed Booster cadence.</summary>
    private sealed partial class UnauthoredStage : SpeedBoostCadence
    {
        /// <inheritdoc/>
        internal override int CountdownAfterAdvance => UnauthoredBoostCadence.CountdownAfterAdvance;

        /// <inheritdoc/>
        internal override int Delay(UnauthoredBoostCadence unauthored, int frame) => Ensure.NotNull(unauthored).Delay(frame);
    }
}
