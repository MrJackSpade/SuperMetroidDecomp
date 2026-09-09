using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyGameplayAudioPublication(SuperMetroidAddressSpace audioBus)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(0, true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        var samus = runtime.Samus!;
        samus.LiquidPhysics.CinematicFunctionActive = false;
        samus.LiquidPhysics.BeginFrameSoundRequests();
        var audio = new CartridgeAudioState();
        audio.AdvanceFrame(audioBus, default);
        var publication = new GameplayAudioFramePublication(audio);

        // Use real native landing publishers, not direct injection into private lists.
        samus.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(bus, samus,
            SamusMovementType.Standing, SamusPoseIds.FacingRightNormalPose, 5, 0);
        AssertEqual(0x0103, publication.QueueEcho(runtime),
            "echo sees earlier landing sound in the same queue");
        samus.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(bus, samus,
            SamusMovementType.Standing, SamusPoseIds.FacingRightNormalPose, 1, 1);
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
                    if (command != CartridgeAudioCommand.WritePort(3, value)) continue;
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
