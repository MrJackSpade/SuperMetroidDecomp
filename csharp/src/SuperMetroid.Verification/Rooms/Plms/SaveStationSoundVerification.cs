using SuperMetroid.Core.Samus.Drawing;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>#1390: confirming a save at a save station publishes the saving sound.</summary>
internal static class SaveStationSoundVerification
{
    /// <summary>
    /// Lands Samus on the Crateria save station, accepts the save with A, and requires library-one sound $2E
    /// ($84:AFF4) in the game's published sound queue while the save sequence runs.
    /// </summary>
    internal static void VerifySaveStationSound()
    {
        var random = new CartridgeRandom();
        var runtime = CreateRetailRuntimeFixture(random: random);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.CrateriaSave);
        int width = (runtime.Room.LevelData ?? throw new InvalidOperationException("The save room has no level data.")).Size.X;
        PlmSlot station = runtime.Room.LoadedScope.Plms.SlotTable.Slots.Single(slot =>
            slot.Active && slot.Family is StationPlms.StationPlmState { Kind: StationKind.Save });
        int column = station.BlockIndex % width;
        int row = station.BlockIndex / width;
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        samus.Position = FixedPosition<RoomPixels>.FromWhole(new(
            (ushort)(column * 16 + SaveStationTriggerGeometry.HorizontalProbeOffset + 8),
            (ushort)(row * 16 - samus.Kinematics.Radius.Y - 1)));

        var game = new SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore(RetailPresentationFixture()), null, 
            SuperMetroidGameHostServices.Default with { GameplayFrames = RetainedGameplayFrames.Instance }, random);
        game.AdoptRuntime(runtime);
        game.GameState = SuperMetroidGameState.MainGameplay;
        CartridgeAudioState audio = game.Session.audio;
        long hostSequence = 0;
        int messageFrame = -1, soundFrame = -1;
        for (int frame = 0; frame < 900 && soundFrame < 0; frame++)
        {
            audio.Reset();
            // A on alternate frames gives the message box a fresh press on each of its read frames.
            SnesButton input = runtime.MessageBox.IsActive && frame % 2 == 0 ? SnesButton.A : SnesButton.None;
            game.StepCaptured(input, ++hostSequence, 1);
            if (messageFrame < 0 && runtime.MessageBox.IsActive)
                messageFrame = frame;
            for (int index = 0; index < audio.SoundWritePositions[0]; index++)
            {
                if (audio.SoundQueues[0, index] == SoundEffectLibrary1Sounds.Saving.Value)
                    soundFrame = frame;
            }
        }
        AssertTrue(messageFrame >= 0, "landing on the save station opens the save message");
        AssertTrue(soundFrame >= 0, "accepting the save publishes library-one sound $2E");
        Console.WriteLine($"Save station sound: the save message opens on frame {messageFrame} and the saving sound plays on frame {soundFrame}.");
    }

    /// <summary>Runs the focused save-station sound reproduction.</summary>
    [VerificationCommand("--save-station-sound")]
    internal static int SaveStationSoundCommand()
    {
        Suite(nameof(VerifySaveStationSound), () => VerifySaveStationSound());
        return 0;
    }
}
