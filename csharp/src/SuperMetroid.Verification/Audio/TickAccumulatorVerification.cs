using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;

namespace SuperMetroid.Verification.Audio;

/// <summary>
/// Confirms the driver's tick accumulator keeps the overflow-and-remainder behaviour the eight-bit
/// timers had, and that the sound-effect timer built on it still polls on every eighth driver tick.
/// </summary>
internal static class TickAccumulatorVerification
{
    /// <summary>Driver ticks between sound-effect polls: the period divided by the per-tick step.</summary>
    private const int TicksPerPoll = TickAccumulator.Period / SpcPortDefinitions.SoundEffectTimerStep;

    /// <summary>Updates walked when checking the poll cadence.</summary>
    private const int CadenceFrames = 20;

    /// <summary>
    /// Checks the wrap at the period boundary (overflow reported, remainder carried), a sum just under
    /// the period (no overflow), rejection of an out-of-range phase or step, and the production port
    /// model's poll on exactly every eighth driver tick across frame boundaries.
    /// </summary>
    internal static void VerifyTickAccumulator()
    {
        (TickAccumulator exact, bool exactOverflow) = TickAccumulator.FromPhase(224).Advance(32);
        AssertTrue(exactOverflow, "a sum reaching the period overflows");
        AssertEqual(0, exact.Phase, "a sum of exactly one period leaves no remainder");

        (TickAccumulator carried, bool carriedOverflow) = TickAccumulator.FromPhase(250).Advance(16);
        AssertTrue(carriedOverflow, "a sum past the period overflows");
        AssertEqual(10, carried.Phase, "the remainder past the period carries into the next tick");

        (TickAccumulator under, bool underOverflow) = TickAccumulator.FromPhase(0).Advance(TickAccumulator.Period - 1);
        AssertTrue(!underOverflow, "a sum one short of the period does not overflow");
        AssertEqual(TickAccumulator.Period - 1, under.Phase, "a sum under the period is kept whole");

        AssertThrows<ArgumentOutOfRangeException>(() => TickAccumulator.FromPhase(TickAccumulator.Period), "a phase must lie inside one period");
        AssertThrows<ArgumentOutOfRangeException>(() => TickAccumulator.FromPhase(0).Advance(-1), "a step cannot be negative");

        var ports = new SpcCommandPorts();
        Span<SpcDriverTick> schedule = stackalloc SpcDriverTick[SpcCommandPorts.MaximumTicksPerFrame];
        int tickNumber = 0;
        int polls = 0;
        for (int frame = 0; frame < CadenceFrames; frame++)
        {
            int count = ports.AdvanceFrame(schedule);
            foreach (SpcDriverTick tick in schedule[..count])
            {
                tickNumber += tick.Ticks;
                AssertEqual(tickNumber % TicksPerPoll == 0, tick.SoundEffectsPolled,
                    $"driver tick {tickNumber} polls the sound-effect ports only on every eighth tick");
                if (tick.SoundEffectsPolled)
                    polls++;
            }
        }
        AssertEqual(tickNumber / TicksPerPoll, polls, "one poll per eight driver ticks across frame boundaries");
    }
}
