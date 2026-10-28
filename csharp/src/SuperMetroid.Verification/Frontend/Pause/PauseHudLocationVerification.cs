using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Fx;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Progression;
namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Checks the gameplay minimap marker's colors and position through paused HUD updates.</summary>
internal static class PauseHudLocationVerification
{
    /// <summary>Checks that pause retains the native minimap tile and its native inks for both blink phases.</summary>
    internal static void VerifyPauseHudLocation()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        var cart = CartridgeImportSource.Require(bus);
        var random = new SharedRandom();
        var runtime = CreateRetailRuntimeFixture(random: random);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        var maps = RetailPresentationFixture();
        var samus = runtime.Player.IntroducedSamus;
        samus.Position = samus.Position.WithWhole(new(0x200, 0x300));

        foreach (byte phase in new byte[] { 0, 8 })
        {
            runtime.Hud.UpdateMinimap(runtime.System, new MinimapFrame { AreaIndex = AreaId.Crateria, RoomMap = new(28, 1), RoomSize = new(0x90, 0x50), Samus = samus.Position.Whole, FrameCounter = phase, PresentationMap = maps.Get(AreaId.Crateria) });
            ushort center = runtime.Hud.Tiles[60].PackedWord;
            AssertEqual(phase == 0 ? 0xbc26 : 0xa826, center, "independent native minimap capture");
            AssertEqual(phase == 0 ? 7 : 2, (center >> 10) & 7,
                "native $90:AB4A blink selects palette seven only in its on phase");
            runtime.FrameBoundary.BeginAcceptedFrame(0);
            var pause = CreateRetailPauseFixture(new PauseMenuEntry { Samus = samus, System = runtime.System, AreaIndex = AreaId.Crateria, RoomMapPosition = new(28, 1), GameplayLayer3 = runtime.AcceptedLayer3Plane });
            // The centre tile sits in the HUD's mutable rows, which follow the static top row on BG3.
            const int centerWord = Layer3Plane.Columns + 60;
            // A BG3 cell's palette selects four of the first CGRAM inks; the pause screen loads them from $B6:F000.
            int inks = ((center >> 10) & 7) * 4;
            RgbColor[] centerColors = [.. Enumerable.Range(inks + 1, 3).Select(color => RgbColor.FromWord(checked(RomDataReader.ReadWordFixedBank(cart, 0xb6f000 + color * 2))))];
            var game = new SuperMetroid.Core.Frontend.SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore(), null, SuperMetroid.Core.Frontend.SuperMetroidGameHostServices.Default, random);
            game.AdoptRuntime(runtime);
            game.Session.Show(pause);
            game.GameState = SuperMetroidGameState.PausedB;
            for (int frame = 0; frame < 24; frame++)
            {
                game.StepCaptured(0, frame + 1, 1);
                AssertEqual(center, pause.screen.Tilemaps.Bg3[centerWord].PackedWord, $"phase {phase} frame {frame} retained HUD position tile");
                for (int color = 0; color < centerColors.Length; color++)
                    AssertEqual(centerColors[color], pause.screen.Palette.Colors[inks + 1 + color], $"phase {phase} frame {frame} HUD position ink {color + 1}");
            }
            Console.WriteLine($"Pause HUD location: phase {phase}, tile ${center:X4}, tile and native inks retained through 24 paused HUD/NMI updates.");
        }
    }
}
