using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static int VerifyMotherBrainAscentCapture(SuperMetroidRuntime runtime)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var state = runtime.Enemies.MotherBrain!;
        AssertTrue(Layer().MainScreenLayersByLine.IsEmpty, "No ascent mask before the native state starts it");
        state.FunctionTimer = 0;
        typeof(RoomEnemySystem).GetMethod("PrepareMotherBrainForRising", flags)!
            .Invoke(runtime.Enemies, [state]);
        AssertTrue(state.RisingHdmaActive, "Production rising setup activates HDMA");
        var captured = Layer();
        AssertEqual(192, captured.MainScreenLayersByLine.Length, "Capture owns each gameplay scanline");
        for (int y = 32; y < 224; y++)
            AssertEqual((ushort)(y < 56 ? 0x15 : y < 216 ? 0x13 : 0x05),
                captured.MainScreenLayersByLine[y - 32], $"Native $88:E73D main-screen value at line {y}");
        state.Body.YPosition = 0xbd;
        typeof(RoomEnemySystem).GetMethod("RaiseMotherBrain", flags)!
            .Invoke(runtime.Enemies, [state, (byte)0]);
        AssertTrue(!state.RisingHdmaActive, "Production ascent completion removes HDMA");
        AssertTrue(Layer().MainScreenLayersByLine.IsEmpty, "Completed ascent restores uniform main-screen selection");
        AssertEqual((ushort)5, captured.MainScreenLayersByLine[191], "Captured frame retains its own mask after the state changes");
        Console.WriteLine("Mother Brain ascent capture: actual room, production start/end and all native scanline masks passed.");
        return 0;
        OrdinaryGameplayRenderLayer Layer() => (OrdinaryGameplayRenderLayer)GameplayDisplayCapture.CaptureOrdinaryBase(runtime).Layers[0];
    }
}
