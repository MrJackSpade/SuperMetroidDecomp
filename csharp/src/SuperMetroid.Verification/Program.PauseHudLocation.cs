using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyPauseHudLocation()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        var cart = CartridgeImportSource.Require(bus);
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        var maps = RetailPresentationFixture();
        var samus = runtime.Samus!;
        samus.XPosition = 0x200; samus.YPosition = 0x300;
        var expectedColors = new SnesCgram();
        for (int i = 0; i < 256; i++)
            expectedColors.SetColor(i, RomDataReader.ReadWordFixedBank(cart, 0xb6f000 + i * 2));
        Directory.CreateDirectory("csharp/test-temp/issue-1253-hud");
        foreach (byte phase in new byte[] { 0, 8 })
        {
            runtime.Hud.UpdateMinimap(bus, runtime.System, AreaId.Crateria, 28, 1,
                0x90, 0x50, samus.XPosition, samus.YPosition, phase,
                presentationMap: maps.Get(AreaId.Crateria));
            ushort center = runtime.Hud.Tiles[60];
            AssertEqual(phase == 0 ? 0xbc26 : 0xa826, center, "independent native minimap capture");
            AssertEqual(phase == 0 ? 7 : 2, (center >> 10) & 7,
                "native $90:AB4A blink selects palette seven only in its on phase");
            runtime.Hud.QueueUpload(bus, runtime.VramWrites);
            runtime.RunNmi(0, true);
            var pause = CreateRetailPauseFixture(bus, samus, runtime.System, AreaId.Crateria,
                28, 1, gameplayVram: runtime.Vram);
            var nativeVram = new SnesVram();
            nativeVram.LoadBytes(0x8000, RomDataReader.ReadFixedBank(cart, 0x9ab200, 0x2000));
            nativeVram.LoadBytes(0xb000, runtime.Vram.Bytes.Slice(0xb000, 0x100));
            var native = SnesBgTilemapRenderer.Render2Bpp(nativeVram, expectedColors,
                0x5800, 0x4000, 4, true, true);
            var game = new SuperMetroidGame(bus);
            typeof(SuperMetroidGame).GetField("runtime", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(game, runtime);
            typeof(SuperMetroidGame).GetField("pauseMenu", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(game, pause);
            typeof(SuperMetroidGame).GetProperty(nameof(SuperMetroidGame.GameState))!.SetValue(game, SuperMetroidGameState.PausedB);
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
                if (frame == 0)
                    PngWriterTooling.WriteRgba($"csharp/test-temp/issue-1253-hud/phase-{phase}.png", 256, 224, actual);
            }
            Console.WriteLine($"Pause HUD location: phase {phase}, tile ${center:X4}, native pixels retained through 24 paused HUD/NMI updates.");
        }
    }
}
