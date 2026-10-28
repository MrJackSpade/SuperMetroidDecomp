using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>Checks that Mother Brain's rising HDMA screen mask is captured and later removed through production state transitions.</summary>
internal static class MotherBrainAscentCaptureVerification
{
    /// <summary>Verifies every native ascent scanline value, the completion reset, and snapshot independence after state changes.</summary>
    internal static int VerifyMotherBrainAscentCapture(SuperMetroidRuntime runtime)
    {
        var state = runtime.Enemies.MotherBrainEncounter.MotherBrain;
        // A frame shows the room's backgrounds once an accepted NMI has latched them.
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AssertTrue(Layer().MainScreenLayersByLine.IsEmpty, "No ascent mask before the native state starts it");
        (state ?? throw new InvalidOperationException("Verification requires state.")).FunctionTimer = WrappingCountdown.Start(0);
        runtime.Enemies.MotherBrainBehavior.Ascent.PrepareMotherBrainForRising(state);
        AssertTrue(state.RisingFloorClipActive, "Production rising setup activates HDMA");
        var captured = Layer();
        AssertEqual(192, captured.MainScreenLayersByLine.Length, "Capture owns each gameplay scanline");
        for (int y = 32; y < 224; y++)
            AssertEqual((ScreenLayers)(y < 56 ? 0x15 : y < 216 ? 0x13 : 0x05),
                captured.MainScreenLayersByLine[y - 32], $"Native $88:E73D main-screen value at line {y}");
        state.Body.Position = state.Body.Position.WithWholeY(0xbd);
        runtime.Enemies.MotherBrainBehavior.Ascent.RaiseMotherBrain(state);
        AssertTrue(!state.RisingFloorClipActive, "Production ascent completion removes HDMA");
        AssertTrue(Layer().MainScreenLayersByLine.IsEmpty, "Completed ascent restores uniform main-screen selection");
        AssertEqual((ScreenLayers)5, captured.MainScreenLayersByLine[191], "Captured frame retains its own mask after the state changes");
        Console.WriteLine("Mother Brain ascent capture: actual room, production start/end and all native scanline masks passed.");
        return 0;
        GameplaySceneLayer Layer() => (GameplaySceneLayer)GameplayDisplayCapture.CaptureOrdinaryBase(runtime.PublishDisplayFrame()).Layers[0];
    }
}
