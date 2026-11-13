using SuperMetroid.AssetExtraction.World.Artwork;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Frontend.CeresCinematics.CeresSaveStartupVerification;
using static SuperMetroid.Verification.Harness.InstalledArtworkBindingVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Saves;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Core.Frontend.CeresCinematics;
namespace SuperMetroid.Verification.Enemies.Gunship;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Artwork;

/// <summary>Checks extracted gunship sprite compositions and the first Landing Site fade-in draw.</summary>
internal static class GunshipInstalledCompositionsVerification
{
    /// <summary>
    /// Read the enemy compositions production import code extracted into the import data (#1638), then confirm only
    /// the reported gunship frames and landing draw.
    /// Native bytes are a reference oracle, never the gameplay bus.
    /// </summary>
    internal static void VerifyExtractedGunshipCompositions(string romPath)
    {
        var imported = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath(romPath));
        EnemySpritemapCatalog compositions = EnemyTileArtworkReader.Spritemaps(
            SuperMetroid.TestSupport.RepositoryInstallation.Verification.CartridgeImportContent.ImportData(GameInstallationLayout.EnemyTileDirectoryName));
        NativeEnemySpritemapDefinition[] shipFrames = GunshipVisualDefinitions.Frames();
        AssertEqual(GunshipVisualDefinitions.FrameCount, shipFrames.Length,
            "#1154 gunship exports its two hull and eleven pad compositions");
        foreach (NativeEnemySpritemapDefinition frame in shipFrames)
        {
            SpritePart[] native = EnemySpritemapCompiler.CompileParts(
                EnemySpritemapFiles.ExtractParts(imported, frame.Bank, frame.Pointer), frame.Name);
            AssertTrue(compositions.TryGetDisplay(frame.Bank, frame.Pointer, out var installed),
                $"#1154 installed gunship composition {frame.Name} is resolvable");
            AssertTrue(installed.SequenceEqual(native),
                $"#1154 {frame.Name} retains native offsets, sizes, tiles, priority and flips");
        }

        Console.WriteLine("#1154: all 13 gunship compositions match native OAM.");
    }

    /// <summary>
    /// #1154: reproduce the first Landing Site fade-in draw after the Ceres
    /// destruction handoff. Retain the actual initialized gameplay/HUD owner,
    /// as the reported escape-to-Landing-Site transition does. No replay is needed.
    /// </summary>
    internal static void VerifyGunshipLandingCompositions(GameInstallation installation, ImportEnemyArtwork? enemyArtwork = null)
    {
        // The repository's installation binds the process's shared catalogs; a fresh load would be the check's own cost.
        var game = new SuperMetroidGame(new SaveSlotStore());
        BindInstalledArtwork(installation, enemyArtwork)(game, true);
        game.BindArtwork(game.Artwork with
        {
            RoomObjectBlueDoorVisuals = installation.LoadRoomObjectBlueDoorVisuals(),
            RoomObjectElevatorPlatformVisuals = installation.LoadRoomObjectElevatorPlatformVisuals(),
        });
        SetState(game, SuperMetroidGameState.SetUpNewGame);
        game.StepCaptured(0);
        // Finish the new game's native loading waits; left pending, the Zebes landing load would resume them instead.
        while (game.Session.ResumesGameLoadingWait)
            game.StepCaptured(0);
        game.GameplayRuntimeForVerification.System.RecordBossesDefeated(AreaId.Ceres, DefeatedBosses.AreaBoss);
        game.GameplayRuntimeForVerification.System.LoadSavedStartupRoute(SaveLoadingGameState.CeresDestruction);
        SetState(game, SuperMetroidGameState.CeresGoesBoom);
        game.StepCaptured(0);
        var scene = game.Session.ScreenAs<CeresDestructionCinematicState>();
        (scene ?? throw new InvalidOperationException("Verification requires scene.")).Phase = CeresDestructionPhase.Finished;
        game.StepCaptured(0);
        game.StepCaptured(0);
        AssertEqual(AreaId.Crateria, game.GameplayRuntimeForVerification.Room.LoadedRoom.AreaIndex,
            "#1154 fixture reaches the reported Landing Site through the existing loader");
        // The landing's native loading waits publish black; only the subsequent
        // gameplay fade owns the gunship OAM draw that failed in the player report.
        while (game.Session.ResumesGameLoadingWait) game.StepCaptured(0);
        AssertEqual(SuperMetroidGameState.MainGameplayFadeIn, game.GameState,
            "#1154 fixture reaches the reported first fade-in frame");
        game.StepCaptured(0);
        Console.WriteLine("#1154: reported first Landing Site fade-in draw completes with installed enemy compositions (RAM-only gameplay).");
    }

    /// <summary>Runs the gunship's extracted compositions against the cartridge from the CLI.</summary>
    [VerificationCommand("--gunship-extracted-compositions")]
    internal static int GunshipExtractedCompositionsCommand()
    {
        Suite(nameof(VerifyExtractedGunshipCompositions), () => VerifyExtractedGunshipCompositions(RepositoryRomPath));
        return 0;
    }

    /// <summary>Runs the gunship's first Landing Site draw from the CLI.</summary>
    [VerificationCommand("--gunship-landing-compositions")]
    internal static int GunshipLandingCompositionsCommand()
    {
        Suite(nameof(VerifyGunshipLandingCompositions), () => VerifyGunshipLandingCompositions(RepositoryInstallation.Installation));
        return 0;
    }
}
