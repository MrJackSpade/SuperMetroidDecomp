using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Gameplay;
using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks the frame counters stored as ints: each returns to zero after its period, through one shown frame and through
/// a door's music load that crosses the boundary, and a door's scroll restarts only the short counter.
/// </summary>
internal static class FrameClockWrapVerification
{
    /// <summary>A door music load that counts a fixed number of frames.</summary>
    /// <param name="frames">The frames the load counts.</param>
    private sealed class FixedMusicLoad(int frames) : IDoorMusicLoadFrames
    {
        /// <inheritdoc/>
        public int FramesCountedDuringMusicLoad() => frames;
    }

    /// <summary>Checks both counters' wraps, the door scroll's restart and the import range.</summary>
    internal static void VerifyFrameClockWrap()
    {
        var clock = new FrameClock();
        clock.Import(255, 65535);
        clock.AdvanceAcceptedFrame();
        AssertEqual(0, clock.ShortCounter, "the short counter wraps from 255 to zero");
        AssertEqual(0, clock.Counter, "the long counter wraps from 65535 to zero");

        clock.Import(250, 65530);
        clock.CountDoorMusicLoad(new FixedMusicLoad(300));
        AssertEqual(38, clock.ShortCounter, "a music load crossing 256 keeps the short counter's wrap");
        AssertEqual(294, clock.Counter, "a music load crossing 65536 keeps the long counter's wrap");

        clock.RestartShortCounter();
        AssertEqual(0, clock.ShortCounter, "the door scroll restarts the short counter");
        AssertEqual(294, clock.Counter, "the door scroll leaves the long counter running");

        AssertThrows<ArgumentOutOfRangeException>(() => clock.Import(256, 0), "the short counter rejects a value past its period");
        AssertThrows<ArgumentOutOfRangeException>(() => clock.Import(0, 65536), "the long counter rejects a value past its period");

        Console.WriteLine("Frame clock: both counters wrap at their periods and a door's scroll restarts only the short one.");
    }

    /// <summary>Runs the focused frame clock wrap verification command.</summary>
    [VerificationCommand("--frame-clock-wrap")]
    internal static int FrameClockWrapCommand()
    {
        Suite(nameof(VerifyFrameClockWrap), () => VerifyFrameClockWrap());
        return 0;
    }
}
