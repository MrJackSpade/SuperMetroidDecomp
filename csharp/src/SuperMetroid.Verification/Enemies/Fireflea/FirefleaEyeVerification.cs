using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
namespace SuperMetroid.Verification.Enemies.Fireflea;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Checks Lower Norfair Fireflea darkness around a retail Fune eye sprite.</summary>
internal static class FirefleaEyeVerification
{
    /// <summary>Confirms native color math excludes OBJ across seven fixed-color darkness shades.</summary>
    internal static void VerifyFirefleaEye()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.LNFireflea);
        AssertEqual(RoomFxType.Fireflea, runtime.RoomLayer3Fx.Type, "reported room uses Fireflea darkness");
        var eye = runtime.Enemies.SlotTable.Slots[0];
        AssertEqual(EnemyDefinitionId.Fune, eye.DefinitionId, "reported rock eye is retail Fune/Namihe family");
        ushort cameraX = 368, cameraY = 608;
        runtime.Enemies.StepFrame(new RoomEnemyFrame { Camera = new(cameraX, cameraY), TimeIsFrozen = false, Samus = runtime.Player.Samus, Level = runtime.Room.LevelData });
        // The display reads the state the NMI latches (#1307); latch the loaded room's darkness once, as its first frame would.
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        runtime.Enemies.Drawing.AdvanceLayers(new(cameraX, cameraY), 0, 7);
        runtime.Enemies.Drawing.DrawLayers(sprites, new(cameraX, cameraY), 0, 7);
        sprites.FinalizeFrame();
        AssertTrue(sprites.Draws.Length > 0, "retail rock eye emits sprites");
        var memory = FramePaintConstants.Capture(runtime.Colours.AsPaletteColors(), sprites);
        var source = GameplayDisplayCapture.CaptureOrdinaryBase(runtime.PublishDisplayFrame());
        var actualDarkness = (GameplayBlendLayer)GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0];
        AssertEqual(ColorBlend.Bg2 | ColorBlend.Backdrop | ColorBlend.Subtract, actualDarkness.Blend.Layers,
            "reported room darkens the background and backdrop, not the sprites");
        var baseLayer = (GameplaySceneLayer)source.Layers[0];
        // Isolate the actual room OBJ pixels on a black backdrop, retaining the
        // retail graphics/palette and the production source-aware color-math kernel.
        var palette = memory.Colors.ToArray();
        palette[0] = RgbColor.Black;
        memory = new(palette, memory.SpriteDraws);
        var objects = new GameplaySceneLayer(baseLayer.View with { MainScreenLayers = ScreenLayers.Sprites }, baseLayer.Rooms);
        RgbaPixel[]? bright = null;
        foreach (byte darkness in new byte[] { 0, 6, 12, 18, 24, 25, 31 })
        {
            var closed = Enumerable.Repeat(new XrayWindowLine(255, 0), 224).ToArray();
            var layer = new GameplayBlendLayer(objects, closed, false, new SceneBlend { Layers = actualDarkness.Blend.Layers, BlendsWithLayersBehind = false, FixedColor = new RgbColor(darkness, darkness, darkness) });
            var pixels = DisplayRendering.Render(new LayeredScene(memory, new DisplayLayer[] { layer }, source.Brightness));
            bright ??= pixels;
            AssertTrue(pixels.SequenceEqual(bright), "native A2 darkness leaves retail eye OBJ colors unchanged");
            AssertTrue(pixels.Any(pixel => pixel.R != 0 || pixel.G != 0 || pixel.B != 0),
                "comparison contains visible colored eye pixels, not an empty frame");
        }
        Console.WriteLine("Lower Norfair Fireflea eye: native sprite colors remain unchanged across seven darkness shades.");
    }

    /// <summary>Runs the focused retail eye and darkness-rendering check.</summary>
    [VerificationCommand("--fireflea-eye")]
    internal static int FirefleaEyeCommand()
    {
        Suite(nameof(VerifyFirefleaEye), () => VerifyFirefleaEye());
        return 0;
    }
}
