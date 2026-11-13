using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>
/// Checks that the bands Mother Brain's rise shows (#1514) are captured and later removed through production state
/// transitions: her body shows only between the upper band and the floor, and the floor's band hides the actors.
/// </summary>
internal static class MotherBrainAscentCaptureVerification
{
    /// <summary>Verifies what every gameplay line shows during the rise, the completion reset, and snapshot independence after state changes.</summary>
    internal static int VerifyMotherBrainAscentCapture(SuperMetroidRuntime runtime)
    {
        var state = runtime.Enemies.MotherBrainEncounter.MotherBrain;
        // A frame shows the room's backgrounds once an accepted NMI has latched them.
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AssertTrue(Layer().Bands.IsEmpty, "No ascent bands before the rise starts");
        (state ?? throw new InvalidOperationException("Verification requires state.")).PhaseTimer = 0;
        SuperMetroid.Core.Bosses.MotherBrain.MotherBrainAscent.PrepareMotherBrainForRising(state);
        AssertTrue(state.RisingFloorClipActive, "Production rising setup starts the floor's clip");
        var captured = Layer();
        AssertEqual(3, captured.Bands.Length, "The rise shows three bands below the HUD");
        for (int y = 32; y < 224; y++)
            AssertEqual(y < 56 ? ScreenLayers.Foreground | ScreenLayers.Sprites
                    : y < 216 ? ScreenLayers.Foreground | ScreenLayers.Background | ScreenLayers.Sprites
                    : ScreenLayers.Foreground,
                captured.ShownLayersAt(y), $"What line {y} shows while Mother Brain rises");
        state.Body.Position = state.Body.Position.WithWholeY(0xbd);
        runtime.Enemies.MotherBrainBehavior.Ascent.RaiseMotherBrain(state);
        AssertTrue(!state.RisingFloorClipActive, "Production ascent completion removes the floor's clip");
        AssertTrue(Layer().Bands.IsEmpty, "Completed ascent shows the same layers on every line");
        AssertEqual(ScreenLayers.Foreground, captured.ShownLayersAt(223), "Captured frame retains its own bands after the state changes");
        Console.WriteLine("Mother Brain ascent capture: actual room, production start/end and every line's shown layers passed.");
        return 0;
        GameplaySceneLayer Layer() => (GameplaySceneLayer)GameplayDisplayCapture.CaptureOrdinaryBase(runtime.PublishDisplayFrame()).Layers[0];
    }
}
