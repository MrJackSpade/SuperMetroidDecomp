using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Audio;

/// <summary>Checks same-frame gameplay sound publication, ordering, and Power Bomb echo suppression.</summary>
internal static class GameplayAudioPublicationVerification
{
    /// <summary>Uses native landing publishers to check queued echo order, repeated flushes, and suppressed echo accumulation.</summary>
    internal static void VerifyGameplayAudioPublication(SuperMetroidAddressSpace audioBus)
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.NmiHandler.RunNmi(0, true);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        var samus = runtime.Player.IntroducedSamus;
        samus.LiquidPhysics.CinematicFunctionActive = false;
        samus.LiquidPhysics.BeginFrameSoundRequests();
        var audio = new CartridgeAudioState();
        audio.AdvanceFrame(audioBus, default);
        var publication = new GameplayAudioFramePublication(audio);

        // Use real native landing publishers, not direct injection into private lists.
        samus.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(bus, samus,
            SamusMovementType.Standing, SamusPoseId.FacingRightNormalPose, 5, 0);
        AssertEqual(0x0103, publication.QueueEcho(runtime),
            "echo sees earlier landing sound in the same queue");
        samus.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(bus, samus,
            SamusMovementType.Standing, SamusPoseId.FacingRightNormalPose, 1, 1);
        publication.PublishPrefix(runtime);
        publication.PublishPrefix(runtime);

        var played = new List<byte>();
        byte acknowledgement = 0;
        for (int frame = 0; frame < 128; frame++)
        {
            foreach (var command in audio.AdvanceFrame(audioBus,
                new CartridgeAudioAcknowledgements(0, 0, 0, acknowledgement)))
            {
                bool recognized = false;
                foreach (byte value in new byte[] { 0, 3, 4, 5 })
                {
                    if (command != CartridgeAudioCommand.WritePort(ApuPort.SoundLibrary3, value)) continue;
                    acknowledgement = value;
                    if (value != 0) played.Add(value);
                    recognized = true;
                    break;
                }
                AssertTrue(recognized, "publication emitted only expected library-three writes");
            }
        }
        AssertTrue(played.SequenceEqual(new byte[] { 4, 3, 5 }),
            "earlier landing, echo, later landing play once in order across repeated flushes");

        samus.LiquidPhysics.BeginFrameSoundRequests();
        runtime.PowerBombExplosionStatus = 0x8000;
        var suppressedAudio = new CartridgeAudioState();
        var suppressedPublication = new GameplayAudioFramePublication(suppressedAudio);
        AssertEqual(3, suppressedPublication.QueueEcho(runtime),
            "active Power Bomb preserves native accumulator on suppressed echo");
        AssertTrue(!suppressedAudio.HasQueuedSounds, "active Power Bomb does not queue echo");
    }
}
