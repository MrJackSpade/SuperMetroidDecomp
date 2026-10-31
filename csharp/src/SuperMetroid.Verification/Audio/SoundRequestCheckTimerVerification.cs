namespace SuperMetroid.Verification.Audio;

/// <summary>
/// Confirms the sound request-check timer keeps the overflow-and-remainder behaviour of the sound program's eight-bit
/// counter, forms a multi-period step's increment within one period, and that the sound acceptance clock built on it
/// checks the requests on every eighth clock step.
/// </summary>
internal static class SoundRequestCheckTimerVerification
{
    /// <summary>Clock steps between request checks: the period divided by the per-step increment.</summary>
    private const int StepsPerCheck = SoundRequestCheckTimer.Period / SoundClockTiming.CheckTimerIncrement;

    /// <summary>Updates walked when checking the check cadence.</summary>
    private const int CadenceFrames = 20;

    /// <summary>
    /// Checks the wrap at the period boundary (check reported, remainder carried), a sum just under the period (no
    /// check), a step spanning more periods than one period holds, rejection of an out-of-range phase or step, and the
    /// production clock's check on exactly every eighth step across update boundaries.
    /// </summary>
    internal static void VerifySoundRequestCheckTimer()
    {
        (SoundRequestCheckTimer exact, bool exactChecks) = SoundRequestCheckTimerConstants.FromPhase(224).Advance(1);
        AssertTrue(exactChecks, "a sum reaching the period checks the requests");
        AssertEqual(0, exact.Phase, "a sum of exactly one period leaves no remainder");

        (SoundRequestCheckTimer carried, bool carriedChecks) = SoundRequestCheckTimerConstants.FromPhase(250).Advance(1);
        AssertTrue(carriedChecks, "a sum past the period checks the requests");
        AssertEqual(26, carried.Phase, "the remainder past the period carries into the next step");

        (SoundRequestCheckTimer under, bool underChecks) = SoundRequestCheckTimerConstants.FromPhase(0).Advance(StepsPerCheck - 1);
        AssertTrue(!underChecks, "a sum one increment short of the period does not check the requests");
        AssertEqual(SoundRequestCheckTimer.Period - SoundClockTiming.CheckTimerIncrement, under.Phase, "a sum under the period is kept whole");

        (SoundRequestCheckTimer formed, bool formedChecks) = SoundRequestCheckTimerConstants.FromPhase(0).Advance(StepsPerCheck + 1);
        AssertTrue(!formedChecks, "a step spanning more than a period's increments forms its increment within one period");
        AssertEqual(SoundClockTiming.CheckTimerIncrement, formed.Phase, "only the increment past a whole period is added");

        AssertThrows<ArgumentOutOfRangeException>(() => SoundRequestCheckTimerConstants.FromPhase(SoundRequestCheckTimer.Period), "a phase must lie inside one period");
        AssertThrows<ArgumentOutOfRangeException>(() => SoundRequestCheckTimerConstants.FromPhase(0).Advance(-1), "a step cannot span negative periods");

        SoundAcceptanceClock<SoundEffectId> clock = new SoundAcceptanceClock<SoundEffectId>();
        Span<SoundClockStep<SoundEffectId>> schedule = stackalloc SoundClockStep<SoundEffectId>[SoundClockTiming.MaximumStepsPerUpdate];
        int stepNumber = 0;
        int checks = 0;
        for (int frame = 0; frame < CadenceFrames; frame++)
        {
            int count = clock.Advance(schedule);
            foreach (SoundClockStep<SoundEffectId> step in schedule[..count])
            {
                stepNumber += step.Steps;
                AssertEqual(stepNumber % StepsPerCheck == 0, step.ChecksRequests,
                    $"clock step {stepNumber} checks the sound requests only on every eighth step");
                if (step.ChecksRequests)
                    checks++;
            }
        }
        AssertEqual(stepNumber / StepsPerCheck, checks, "one check per eight clock steps across update boundaries");
    }
}
