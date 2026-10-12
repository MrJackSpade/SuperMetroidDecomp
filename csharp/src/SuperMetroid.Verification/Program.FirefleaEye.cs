using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using static SuiteTimingVerification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static RetailRuntimeFixtureVerification;

internal static class FirefleaEyeVerification
{
    internal static void VerifyFirefleaEye()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xb6ee);
        AssertEqual(RoomFxType.Fireflea, runtime.RoomLayer3Fx.Type, "reported room uses Fireflea darkness");
        var eye = runtime.Enemies.SlotTable.Slots[0];
        AssertEqual(EnemyDefinitionId.Fune, eye.EnemyDefinitionPointer, "reported rock eye is retail Fune/Namihe family");
        ushort cameraX = 368, cameraY = 608;
        runtime.Enemies.StepFrame(new RoomEnemyFrame { Camera = new(cameraX, cameraY), TimeIsFrozen = false, Samus = runtime.Player.Samus, Level = runtime.Room.LevelData });
        var oam = new OamBuffer();
        oam.BeginFrame();
        runtime.Enemies.Drawing.DrawLayers(oam, new(cameraX, cameraY), 0, 7);
        oam.FinalizeFrame();
        AssertTrue(oam.LastFinalizedSpriteCount > 0, "retail rock eye emits sprites");
        var memory = PpuMemorySnapshot.Capture(runtime.Vram, runtime.Cgram, oam);
        var source = GameplayDisplayCapture.CaptureOrdinaryBase(runtime);
        var actualDarkness = (GameplayColorMathRenderLayer)GameplayDisplayCapture.TryCaptureFrame(runtime)!.Layers[0];
        AssertEqual((byte)0xa2, (byte)actualDarkness.ColorMath, "reported room captures the native OBJ-excluding blend mask");
        var baseLayer = (OrdinaryGameplayRenderLayer)source.Layers[0];
        // Isolate the actual room OBJ pixels on a black backdrop, retaining the
        // retail graphics/palette and the production source-aware color-math kernel.
        var palette = memory.Cgram.ToArray();
        palette[0] = Bgr555.Black;
        memory = new(memory.Vram, palette, memory.Oam, memory.ModeledSpriteCount);
        var objects = new OrdinaryGameplayRenderLayer(baseLayer.Registers with { MainScreenLayers = SnesMainScreenLayers.Obj });
        Rgba32[]? bright = null;
        foreach (byte darkness in new byte[] { 0, 6, 12, 18, 24, 25, 31 })
        {
            var closed = Enumerable.Repeat(new XrayWindowLine(255, 0), 224).ToArray();
            var layer = new GameplayColorMathRenderLayer(objects, closed, false, new ColorMathRegisters { ColorMath = actualDarkness.ColorMath, AddSubscreen = false, FixedRed = darkness, FixedGreen = darkness, FixedBlue = darkness });
            var pixels = SoftwareLayeredSnapshotRenderer.Render(new(memory, new RenderLayer[] { layer }, source.ObjectSelection, source.Brightness));
            bright ??= pixels;
            AssertTrue(pixels.SequenceEqual(bright), "native A2 darkness leaves retail eye OBJ colors unchanged");
            AssertTrue(pixels.Any(pixel => pixel.R != 0 || pixel.G != 0 || pixel.B != 0),
                "comparison contains visible colored eye pixels, not an empty frame");
        }
        Console.WriteLine("Lower Norfair Fireflea eye: native sprite colors remain unchanged across seven darkness shades.");
    }

    [VerificationCommand("--fireflea-eye")]
    internal static int FirefleaEyeCommand()
    {
        Suite(nameof(VerifyFirefleaEye), () => VerifyFirefleaEye());
        return 0;
    }
}
