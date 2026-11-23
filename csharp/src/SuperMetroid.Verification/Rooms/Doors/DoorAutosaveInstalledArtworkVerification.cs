using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hud;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.Diagnostics.Sessions;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>
/// A debugger state captures a game drawing the installed content its store was built over (#2656), as the desktop host
/// composes every session: the game binds the installation's artwork, Samus's body placements among it, and its door
/// autosave captures the game holding them. The first door of a player's session threw, as the capture could not hold the
/// installed body placements.
/// </summary>
internal static class DoorAutosaveInstalledArtworkVerification
{
    /// <summary>Captures a gameplay state of a game bound to the installed content, and restores it.</summary>
    internal static void VerifyDoorAutosaveCapturesInstalledArtwork()
    {
        string parent = Path.GetFullPath("csharp/test-temp");
        Directory.CreateDirectory(parent);
        string directory = Directory.CreateDirectory(Path.Combine(parent, "door-autosave-installed-" + Guid.NewGuid().ToString("N"))).FullName;
        try
        {
            var random = new SharedRandom();
            var clips = RepositoryInstallation.Installation.LoadAudioClips().Clips;
            var content = new InstalledSessionContent(RepositoryInstallation.Artwork, clips);
            var game = CreateAudibleRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(InstalledFixtures.Maps), clips,
                renderGameplayFrames: false, random: random);
            var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true, random: random, audio: game.AudioForVerification);
            runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus(runtime.Room.LoadedScope);
            game.AdoptRuntime(runtime);
            // The host's session draws the installed content, as the desktop host composes it.
            GameSessionComposer.Bind(game, content, storage: null);
            AssertTrue(ReferenceEquals(RepositoryInstallation.Artwork.SamusBodyArt, runtime.Artwork.SamusArtwork.Body),
                "the session's Samus draws the installed body placements");
            var identity = GameContentIdentity.Create(new string('A', 64), new string('B', 64), new string('C', 64), BuildIdentity.Version);
            using var store = DebuggerSaveStateStore.ForInstalledGame(directory, null, identity, content);
            DebuggerSaveStateMetadata saved = store.Save(DebuggerStateFormat.AutomaticSlot, game);
            AssertEqual(game.FrameNumber, saved.FrameNumber, "the capture holds the game's frame");
            var loaded = store.Load(DebuggerStateFormat.AutomaticSlot);
            AssertTrue(ReferenceEquals(RepositoryInstallation.Artwork.SamusBodyArt,
                    loaded.Game.GameplayRuntimeForVerification.Artwork.SamusArtwork.Body),
                "the restored Samus draws the installed body placements the capture named");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
        Console.WriteLine("  Door autosave: a game drawing the installed content is captured and restored.");
    }

    /// <summary>Runs the installed-artwork door autosave check.</summary>
    [VerificationCommand("--door-autosave-installed-artwork")]
    internal static int DoorAutosaveInstalledArtworkCommand()
    {
        Suite(nameof(VerifyDoorAutosaveCapturesInstalledArtwork), VerifyDoorAutosaveCapturesInstalledArtwork);
        return 0;
    }
}
