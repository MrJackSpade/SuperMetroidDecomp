using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Gameplay;
using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks the accepted-frame counters stored as ints: each returns to zero after its period exactly as the cartridge's
/// byte and word do, through one accepted frame and through a hardware wait that crosses the boundary, and the door
/// IRQ clears only the short counter.
/// </summary>
internal static class FrameClockWrapVerification
{
    /// <summary>Checks both counters' wraps, the door-IRQ clear and the import range.</summary>
    internal static void VerifyFrameClockWrap()
    {
        var clock = new FrameClock();
        clock.Import(255, 65535);
        clock.AdvanceAcceptedFrame();
        AssertEqual(0, clock.Counter8, "the short counter wraps from 255 to zero");
        AssertEqual(0, clock.Counter, "the long counter wraps from 65535 to zero");

        clock.Import(250, 65530);
        clock.AdvanceHardwareWaitFrames(300);
        AssertEqual(38, clock.Counter8, "a hardware wait crossing 256 keeps the short counter's byte wrap");
        AssertEqual(294, clock.Counter, "a hardware wait crossing 65536 keeps the long counter's word wrap");

        clock.ClearCounter8ForDoorIrq();
        AssertEqual(0, clock.Counter8, "the door IRQ clears the short counter");
        AssertEqual(294, clock.Counter, "the door IRQ leaves the long counter running");

        AssertThrows<ArgumentOutOfRangeException>(() => clock.Import(256, 0), "the short counter rejects a value past its byte");
        AssertThrows<ArgumentOutOfRangeException>(() => clock.Import(0, 65536), "the long counter rejects a value past its word");

        Console.WriteLine("Frame clock: both counters wrap at their periods and the door IRQ clears only the short one.");
    }

    /// <summary>Runs the focused frame clock wrap verification command.</summary>
    [VerificationCommand("--frame-clock-wrap")]
    internal static int FrameClockWrapCommand()
    {
        Suite(nameof(VerifyFrameClockWrap), () => VerifyFrameClockWrap());
        return 0;
    }
}
