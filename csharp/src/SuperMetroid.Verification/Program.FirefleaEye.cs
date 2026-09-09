using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyFirefleaEye()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xb6ee);
        AssertEqual(RoomFxType.Fireflea, runtime.RoomLayer3Fx.Type, "reported room uses Fireflea darkness");
        var eye = runtime.Enemies.Slots[0];
        AssertEqual((ushort)0xe6ff, eye.EnemyDefinitionPointer, "reported rock eye is retail Fune/Namihe family");
        ushort cameraX = 368, cameraY = 608;
        runtime.Enemies.StepFrame(cameraX, cameraY, false, runtime.Samus, level: runtime.LevelData);
        var oam = new OamBuffer();
        oam.BeginFrame();
        runtime.Enemies.DrawLayers(oam, cameraX, cameraY, 0, 7);
        oam.FinalizeFrame();
        AssertTrue(oam.LastFinalizedSpriteCount > 0, "retail rock eye emits sprites");
        var memory = PpuMemorySnapshot.Capture(runtime.Vram, runtime.Cgram, oam);
        var source = GameplayDisplayCapture.CaptureOrdinaryBase(runtime);
        var actualDarkness = (XrayGameplayRenderLayer)GameplayDisplayCapture.TryCaptureFrame(runtime)!.Layers[0];
        AssertEqual((byte)0xa2, (byte)actualDarkness.ColorMath, "reported room captures the native OBJ-excluding blend mask");
        var baseLayer = (OrdinaryGameplayRenderLayer)source.Layers[0];
        // Isolate the actual room OBJ pixels on a black backdrop, retaining the
        // retail graphics/palette and the production source-aware color-math kernel.
        var palette = memory.Cgram.ToArray();
        palette[0] = 0;
        memory = new(memory.Vram, palette, memory.Oam, memory.ModeledSpriteCount);
        var objects = new OrdinaryGameplayRenderLayer(baseLayer.Registers with { MainScreenLayers = SnesMainScreenLayers.Obj });
        Rgba32[]? bright = null;
        foreach (byte darkness in new byte[] { 0, 6, 12, 18, 24, 25, 31 })
        {
            var closed = Enumerable.Repeat(new XrayWindowLine(255, 0), 224).ToArray();
            var layer = new XrayGameplayRenderLayer(objects, closed, false,
                actualDarkness.ColorMath, false, darkness, darkness, darkness);
            var pixels = SoftwareLayeredSnapshotRenderer.Render(new(memory, new RenderLayer[] { layer }, source.ObjectSelection, source.Brightness));
            bright ??= pixels;
            AssertTrue(pixels.SequenceEqual(bright), "native A2 darkness leaves retail eye OBJ colors unchanged");
            AssertTrue(pixels.Any(pixel => pixel.R != 0 || pixel.G != 0 || pixel.B != 0),
                "comparison contains visible colored eye pixels, not an empty frame");
        }
        Console.WriteLine("Lower Norfair Fireflea eye: native sprite colors remain unchanged across seven darkness shades.");
    }
}
