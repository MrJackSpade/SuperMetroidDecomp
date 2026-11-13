using SuperMetroid.Core.Frontend;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.InstalledArtworkBindingVerification;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.RepositoryInstallation;
namespace SuperMetroid.Verification.Bosses.Torizo;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Palettes;

/// <summary>Checks the Golden Torizo room-entry code, the entering beams' colours it leaves shown and the pictures of its beams.</summary>
internal static class GoldenTorizoCodeEntryVerification
{
    /// <summary>
    /// Confirms the native item grant, the entering Wave beam's colours the room load showed before it, and that the
    /// granted beams' projectiles have a projectile sheet to show (#1586).
    /// </summary>
    internal static void VerifyGoldenTorizoCodeEntry()
    {
        // Isolated SRAM; installed artwork is read-only and no player save is loaded.
        var installation = RepositoryInstallation.Installation;
        var game = new SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore(), new SuperMetroid.Core.Gameplay.GameplayAssists { SamusCannotDie = true }, SuperMetroidGameHostServices.Default with { GameplayFrames = RetainedGameplayFrames.Instance }, new SharedRandom());
        BindInstalledArtwork(installation)(game, false);
        game.InitializeDirectRoomVerification();
        var runtime = game.GameplayRuntimeForVerification;
        runtime.Player.IntroducedSamus.EquippedBeams = SamusBeamFlags.Wave;
        runtime.Player.IntroducedSamus.CollectedBeams = SamusBeamFlags.Wave;
        runtime.Controller1.Latch(GoldenTorizoCodeDefinitions.ControllerChord);
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.GoldenTorizo);
        AssertEqual(GoldenTorizoCodeDefinitions.Beams, runtime.Player.IntroducedSamus.EquippedBeams, "GT code retains exact native beam grant");
        AssertEqual(GoldenTorizoCodeDefinitions.Items, runtime.Player.IntroducedSamus.EquippedItems, "GT code retains exact native equipment grant");
        AssertEqual(SamusShades.Beam(SamusBeamCombination.Wave), runtime.Player.Colours.Beams,
            "the room load showed the entering Wave beam's colours before the code changed the equipment");
        // The granted Spazer, Plasma, Ice and Wave beams have no beam artwork of their own; their projectiles show the
        // Ice-Wave sheet, whose charged shot their uncharged beam fires, instead of the entering beam's.
        SpriteSheet grantedSheet = (runtime.Artwork.ProjectileSheets ?? throw new InvalidOperationException(
            "The Golden Torizo fixture binds the installed projectile sheets.")).For(SamusBeamCombinations.Of(GoldenTorizoCodeDefinitions.Beams));
        AssertTrue(ReferenceEquals(runtime.Artwork.SpriteSheets?.Get(SpriteSheetId.ProjectilesIceWave).Image, grantedSheet.Image),
            "the granted beams' projectiles are pictured from the Ice-Wave sheet");
        for (int frame = 0; frame < 3; frame++) game.Step(0);
        AssertEqual(SuperMetroidGameState.MainGameplay, game.GameState, "GT room entry resumes gameplay after code");
        Console.WriteLine("Golden Torizo code: real room initialization and three continuation frames succeed with the native grant.");
    }

    /// <summary>Runs the focused room-entry and artwork-continuation check.</summary>
    [VerificationCommand("--golden-torizo-code-entry")]
    internal static int GoldenTorizoCodeEntryCommand()
    {
     VerifyGoldenTorizoCodeEntry(); return 0; 
    }
}
