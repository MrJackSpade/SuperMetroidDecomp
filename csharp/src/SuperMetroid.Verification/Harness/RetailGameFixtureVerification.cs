using SuperMetroid.Core;
using SuperMetroid.Core.Frontend;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Verification.Harness;

/// <summary>Constructs installed retail states used by focused frontend and instruction verifications.</summary>
internal static class RetailGameFixtureVerification
{
    /// <summary>Creates a game host from a single retail import over <paramref name="saves"/>, with optional assists and raster rendering.</summary>
    internal static SuperMetroidGame CreateRetailGameFixture(SuperMetroid.Core.Saves.SaveSlotStore saves,
        SuperMetroid.Core.Gameplay.GameplayAssists? assists = null, bool renderGameplayFrames = true,
        SuperMetroidGameHostServices? hostServices = null, SharedRandom? random = null) =>
        InstalledFixtures.CreateGame(saves, assists, renderGameplayFrames, hostServices, random);

    /// <summary>
    /// A game in main gameplay in a Brinstar room, its Samus standing under her normal movement mode with the new game's
    /// health and no reserves.
    /// </summary>
    internal static (SuperMetroidGame Game, SuperMetroidRuntime Runtime, SamusState Samus) CreateMainGameplayFixture()
    {
        var random = new SharedRandom();
        var game = global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Frontend.SuperMetroidGame>([typeof(global::SuperMetroid.Core.Saves.SaveSlotStore), typeof(global::SuperMetroid.Core.Gameplay.GameplayAssists), typeof(global::SuperMetroid.Core.Frontend.SuperMetroidGameHostServices), typeof(global::SuperMetroid.Core.Compatibility.SharedRandom)], new object?[] { new SuperMetroid.Core.Saves.SaveSlotStore(InstalledFixtures.Maps), null, SuperMetroidGameHostServices.Default, random });
        SuperMetroidRuntime runtime = RetailRuntimeFixtureVerification.CreateRetailRuntimeFixture(random: random, audio: game.AudioForVerification);
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.AlphaPowerBomb);
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.PoseId = SamusPoseId.FacingRightNormalPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        game.AdoptRuntime(runtime);
        game.Session.Enter(SuperMetroidGameState.MainGameplay);
        return (game, runtime, samus);
    }

    /// <summary>Creates a Ceres destruction state waiting on <paramref name="audio"/>'s music, with optional cinematic artwork.</summary>
    internal static CeresDestructionCinematicState CreateRetailDestructionFixture(AudioQueues audio, ImportIntroCinematicArtwork? artwork = null) =>
        InstalledFixtures.CreateDestruction(audio, artwork);

    /// <summary>Creates an ending state with supplied game time, inventory, audio, and language selection.</summary>
    internal static EndingCreditsState CreateRetailEndingFixture(AudioQueues audio, ushort gameTimeHours, ushort gameTimeMinutes,
        EndingInventorySnapshot inventory = default, TextLanguage language = TextLanguage.English) =>
        InstalledFixtures.CreateEnding(audio, gameTimeHours, gameTimeMinutes, inventory, language);
    /// <summary>Creates an intro state with optionally supplied audio and installed visual catalogs.</summary>
    internal static IntroCinematicState CreateRetailIntroFixture(AudioQueues? audio = null,
        ImportIntroCinematicArtwork? characterArtwork = null,
        InstalledSamusBodyArtwork? samusBodyArtwork = null) =>
        InstalledFixtures.CreateIntro(audio, characterArtwork, samusBodyArtwork);
}
