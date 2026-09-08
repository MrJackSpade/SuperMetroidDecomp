using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static void VerifyAndroidHostPolicies()
    {
        VerifyQueuedPcmSink();
        var stalled = HostFrameDeadline.AfterFrame(0, 0.060);
        AssertTrue(stalled.Rebased, "60ms audio stall must not retain catch-up debt indefinitely");
        AssertTrue(stalled.WaitSeconds > 0, "rebase must not immediately publish another catch-up frame");
        AssertTrue(!HostFrameDeadline.AfterFrame(0, 0.025).Rebased, "ordinary scheduler jitter retains fractional deadline");
        double deadline = 0, time = 0;
        for (int frame = 0; frame < 600; frame++)
        {
            time += 0.008;
            var next = HostFrameDeadline.AfterFrame(deadline, time);
            AssertTrue(!next.Rebased, "steady eight-millisecond work never rebases");
            time += next.WaitSeconds;
            deadline = next.NextDeadline;
        }
        AssertTrue(Math.Abs(time - 10) < 0.000001, "600 normal host frames retain exact ten-second cadence");
        AssertEqual(new DisplayViewport(28, 172, 1024, 896), DisplayViewport.IntegerPixels(1080, 1240), "Retroid upright integer viewport");
        AssertEqual(new DisplayViewport(108, 92, 1024, 896), DisplayViewport.IntegerPixels(1240, 1080), "Retroid rotated integer viewport");
        AssertEqual(new DisplayViewport(28, 124, 1024, 896), DisplayViewport.IntegerPixels(1080, 1144), "insets reduce usable surface before scaling");
        AssertEqual(new DisplayViewport(0, 0, 256, 224), DisplayViewport.IntegerPixels(256, 224), "native exact fit");
        AssertEqual(0, DisplayViewport.IntegerPixels(255, 224).Width, "too small never crops or stretches");
        AssertThrows<ArgumentOutOfRangeException>(() => DisplayViewport.IntegerPixels(-1, 224), "negative viewport rejected");

        var input = new FrameInputLatch();
        input.Set(1, SnesButton.A);
        input.Set(1, SnesButton.None);
        AssertEqual((ushort)SnesButton.A, input.Sample(), "sub-frame tap reaches simulation");
        AssertEqual((ushort)0, input.Sample(), "sub-frame tap releases on next frame");
        input.Set(1, SnesButton.Left);
        input.Set(2, SnesButton.Left);
        input.Set(1, SnesButton.None);
        AssertEqual((ushort)SnesButton.Left, input.Sample(), "axis release cannot clear another held producer");
        AssertEqual((ushort)SnesButton.Left, input.Sample(), "sustained hold is not a pulse");
        input.Set(3, SnesButton.Start);
        input.Clear();
        AssertEqual((ushort)0, input.Sample(), "focus loss clears pending and held input");
        Console.WriteLine("PASS Android host integer viewport and input policies");
    }
}
