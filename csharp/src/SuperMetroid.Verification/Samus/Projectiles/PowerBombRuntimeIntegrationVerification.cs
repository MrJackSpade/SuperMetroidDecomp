using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>Checks that the shared desktop runtime renderer applies the active Power Bomb window.</summary>
internal static class PowerBombRuntimeIntegrationVerification
{
    /// <summary>
    /// Proves that the shared runtime renderer used by the desktop includes the active
    /// bank-$88 Power Bomb color-math window, rather than merely testing that compositor
    /// in isolation.
    /// </summary>
    internal static void VerifyPowerBombRuntimeRendererIntegration()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        SuperMetroidAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        SelectedRoom room = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(
            bus,
            RoomHeaderDefinitions.AlphaPowerBomb.Pointer);
        AssertEqual(
            new RoomIdentity(AreaId.Brinstar, 0x26),
            room.Identity,
            "Power Bomb renderer integration retail room");

        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.FrameBoundary.BeginAcceptedFrame(controller1Input: 0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(
            RoomHeaderDefinitions.AlphaPowerBomb,
            camera: new Position16<RoomPixels>(0, 0));
        runtime.FrameBoundary.BeginAcceptedFrame(controller1Input: 0);

        RgbaPixel[] baseline = FrontendScenePixelsAccess.Pixels(runtime.PublishDisplayFrame()).ToArray();
        SamusPowerBombExplosionState explosion = runtime.BombProjectiles.PowerBombExplosion;
        explosion.Arm();
        explosion.Spawn(
            new(PowerBombRuntimeVerificationDefinitions.ScreenCenterX, PowerBombRuntimeVerificationDefinitions.ScreenCenterY));
        explosion.StepFrame();
        AssertEqual(PowerBombExplosionPhase.Inactive, explosion.RenderedPhase,
            "HDMA allocation setup does not draw the first flash prematurely");
        explosion.StepFrame();
        // The window table is read during display, but the pre-instruction's COLDATA
        // write reaches the PPU only with the following accepted NMI (#1307).
        runtime.FrameBoundary.BeginAcceptedFrame(controller1Input: 0);

        GameplayRenderSnapshot ppu = runtime.Display.DisplayedGameplayScrolls;
        ScanlineColorAddRenderLayer expected = GameplayEffectCapture.CapturePowerBombColorMath(
                PowerBombRenderSnapshot.Capture(explosion), ppu.Layer1Position)
            ?? throw new InvalidOperationException("The active Power Bomb captured no window.");
        AssertTrue(expected.Windows.ToArray().Any(window => window != ColorAddWindow.Empty),
            "Power Bomb fixture produces a visible first-flash window");
        LayeredScene published = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
        ScanlineColorAddRenderLayer window = published.LayersOf<ScanlineColorAddRenderLayer>().Single();
        AssertTrue(window.Windows.SequenceEqual(expected.Windows),
            "the published gameplay frame carries the bank-$88 Power Bomb window");
        RgbaPixel[] actual = FrontendScenePixelsAccess.Pixels(published);
        AssertTrue(CountPowerBombFrameDifferences(baseline, actual) > 0,
            "shared runtime rendering shows the Power Bomb window");

        Console.WriteLine(
            "  Power Bomb runtime: shared desktop rendering includes the active bank-$88 window.");
    }

    /// <summary>Counts pixels changed between equal-size rendered frames.</summary>
    private static int CountPowerBombFrameDifferences(
        ReadOnlySpan<RgbaPixel> first,
        ReadOnlySpan<RgbaPixel> second)
    {
        AssertEqual(first.Length, second.Length, "Power Bomb comparison frame lengths");
        int count = 0;
        for (int pixel = 0; pixel < first.Length; pixel++)
            count += first[pixel] != second[pixel] ? 1 : 0;
        return count;
    }
}
