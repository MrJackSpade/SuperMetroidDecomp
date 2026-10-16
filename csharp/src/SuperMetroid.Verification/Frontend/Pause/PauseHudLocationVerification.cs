using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Map;
namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Checks the gameplay minimap marker's colors and position through paused HUD updates.</summary>
internal static class PauseHudLocationVerification
{
    /// <summary>Compares native minimap pixels with pause rendering and captured packets for both blink phases.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for the pause palette and HUD tile graphics")]
    internal static void VerifyPauseHudLocation()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        var cart = CartridgeImportSource.Require(bus);
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        var maps = RetailPresentationFixture();
        var samus = runtime.Player.IntroducedSamus;
        samus.Position = samus.Position.WithWhole(new(0x200, 0x300));
        var expectedColors = new SnesCgram();
        for (int i = 0; i < 256; i++)
            expectedColors.SetColor(i, Bgr555.FromWord(checked((ushort)(RomDataReader.ReadWordFixedBank(cart, 0xb6f000 + i * 2)))));

        foreach (byte phase in new byte[] { 0, 8 })
        {
            runtime.Hud.UpdateMinimap(bus, runtime.System, new MinimapFrame { AreaIndex = AreaId.Crateria, RoomMap = new(28, 1), RoomSize = new(0x90, 0x50), Samus = samus.Position.Whole, NmiFrameCounter = phase, PresentationMap = maps.Get(AreaId.Crateria) });
            ushort center = runtime.Hud.Tiles[60];
            AssertEqual(phase == 0 ? 0xbc26 : 0xa826, center, "independent native minimap capture");
            AssertEqual(phase == 0 ? 7 : 2, (center >> 10) & 7,
                "native $90:AB4A blink selects palette seven only in its on phase");
            runtime.Hud.QueueUpload(bus, runtime.VramWrites);
            runtime.NmiHandler.RunNmi(0, true);
            var pause = CreateRetailPauseFixture(bus, new PauseMenuEntry { Samus = samus, System = runtime.System, AreaIndex = AreaId.Crateria, RoomMapPosition = new(28, 1), GameplayVram = runtime.Vram });
            var nativeVram = new SnesVram();
            nativeVram.LoadBytes(0x8000, RomDataReader.ReadFixedBank(cart, 0x9ab200, 0x2000));
            nativeVram.LoadBytes(0xb000, runtime.Vram.Bytes.Slice(0xb000, 0x100));
            var native = SnesBgTilemapRenderer.Render2Bpp(nativeVram, expectedColors,
                0x5800, 0x4000, 4, true, true);
            var game = new SuperMetroidGame(bus);
            game.Session.runtime = runtime;
            game.Session.pauseMenu = pause;
            game.GameState = SuperMetroidGameState.PausedB;
            for (int frame = 0; frame < 24; frame++)
            {
                game.StepCaptured(0, frame + 1, 1);
                var actual = pause.Render();
                var packet = SoftwareLayeredSnapshotRenderer.Render(pause.CaptureRenderSnapshot());
                int opaque = 0;
                for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++)
                {
                    var expected = native[(16 + y) * 256 + 224 + x];
                    if (expected.A == 0) continue;
                    opaque++;
                    int index = (15 + y) * 256 + 224 + x;
                    AssertEqual(expected, actual[index], $"phase {phase} frame {frame} retained HUD position pixel {x},{y}");
                    AssertEqual(expected, packet[index], "snapshot retains the same HUD position color");
                }
                AssertTrue(opaque > 0, "current location test examines opaque native pixels");

            }
            Console.WriteLine($"Pause HUD location: phase {phase}, tile ${center:X4}, native pixels retained through 24 paused HUD/NMI updates.");
        }
    }
}
