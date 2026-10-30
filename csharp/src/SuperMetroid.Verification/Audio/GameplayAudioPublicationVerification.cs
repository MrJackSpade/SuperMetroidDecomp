using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Runtime;
namespace SuperMetroid.Verification.Audio;

/// <summary>Checks same-frame gameplay sound publication, ordering, and Power Bomb echo suppression.</summary>
internal static class GameplayAudioPublicationVerification
{
    /// <summary>Uses native landing publishers to check queued echo order, repeated flushes, and suppressed echo accumulation.</summary>
    internal static void VerifyGameplayAudioPublication()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        var samus = runtime.Player.IntroducedSamus;
        samus.LiquidPhysics.CinematicFunctionActive = false;
        samus.LiquidPhysics.BeginFrameSoundRequests();
        var audio = new AudioQueues();
        audio.AdvanceFrame(default);
        var publication = new GameplayAudioFramePublication(audio);

        // Use real native landing publishers, not direct injection into private lists.
        samus.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(samus,
            SamusMovementType.Standing, SamusPoseId.FacingRightNormalPose, new UnsignedSixteenSixteen(5, 0));
        // The echo's queue call is admitted with the earlier landing already queued (formerly the accumulator $0103).
        AssertEqual(SoundQueueAdmission.Admitted(1), publication.QueueEcho(runtime),
            "echo sees earlier landing sound in the same queue");
        samus.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(samus,
            SamusMovementType.Standing, SamusPoseId.FacingRightNormalPose, new UnsignedSixteenSixteen(1, 1));
        publication.PublishPrefix(runtime);
        publication.PublishPrefix(runtime);

        var played = new List<SoundEffectId>();
        SoundEffectId? acknowledgement = null;
        for (int frame = 0; frame < 128; frame++)
        {
            foreach (var command in audio.AdvanceFrame(SoundAcknowledgements<SoundEffectId>.Of(null, null, acknowledgement)))
            {
                bool recognized = false;
                foreach (SoundEffectId? value in new SoundEffectId?[] { null, Library3Sound.SpeedBooster, Library3Sound.HardLanding, Library3Sound.Landing })
                {
                    if (command != AudioRequest.ForSound(SoundEffectLibrary.Library3, value)) continue;
                    acknowledgement = value;
                    if (value is SoundEffectId sound) played.Add(sound);
                    recognized = true;
                    break;
                }
                AssertTrue(recognized, "publication emitted only expected library-three writes");
            }
        }
        AssertTrue(played.SequenceEqual(new SoundEffectId[] { Library3Sound.HardLanding, Library3Sound.SpeedBooster, Library3Sound.Landing }),
            "earlier landing, echo, later landing play once in order across repeated flushes");

        samus.LiquidPhysics.BeginFrameSoundRequests();
        runtime.PowerBombExplosionActive = true;
        var suppressedAudio = new AudioQueues();
        var suppressedPublication = new GameplayAudioFramePublication(suppressedAudio);
        // A suppressed echo is still admitted at the threshold with an empty queue (formerly the accumulator $0003).
        AssertEqual(SoundQueueAdmission.Admitted(0), suppressedPublication.QueueEcho(runtime),
            "active Power Bomb preserves the native admission on suppressed echo");
        AssertTrue(!suppressedAudio.HasQueuedSounds, "active Power Bomb does not queue echo");
    }
}
