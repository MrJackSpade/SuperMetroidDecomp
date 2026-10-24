using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Diagnostics.SaveStates;
namespace SuperMetroid.Verification.Samus.Abilities;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Checks Speed Booster echo start, wall cancellation, and SPC silence.</summary>
internal static class SpeedBoosterEchoStopVerification
{
    /// <summary>Checks one audible start and one post-draw stop after running into a wall.</summary>
    internal static void VerifySpeedBoosterEchoStop()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var renderer = AudioSessionAccess.CreateSynthesizing(RepositoryInstallation.Installation.LoadAudio());
        var queue = new AudioQueues();
        renderer.RenderFrame(queue.AdvanceFrame(renderer.Acknowledgements));
        renderer.RenderFrame([new AudioRequest.ChangeMusicDataSet(MusicDataSet.GreenBrinstar)]);
        var libraries = renderer.PlayerForVerification.soundLibraries;
        var samus = new SamusState { Pose = SamusPoseId.RanIntoWallRightPose, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)) };
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        var speed = samus.HorizontalSpeed;
        speed.HasRunningMomentum = true;
        speed.SpeedBoostCounter = new SamusSpeedBoostCounter(0x0301);
        ushort animation = 0;
        speed.TryAdvanceSpeedBoosterAnimationStage(null, SamusMovementType.Running, SnesButton.B,
            0, ref animation, out _, () => queue.QueueSound(Library3Sound.SpeedBooster, 6));
        var words = new ushort[256];
        for (int index = 160; index < 176; index++) words[index] = 0x8000;
        var level = new RoomLevelData(new Extent16<RoomBlocks>(16, 16), new RoomLevelPlanes(words, new byte[256], new ushort[256]), new byte[8]);
        int starts = 0, stops = 0, stopRequests = 0, finalPeak = 0;
        // The queue rewrites a request every frame until the SPC echoes it ($82:8A55), so a request counts once,
        // when library three's port changes to its value.
        int library3Port = 0;
        for (int frame = 0; frame < 90; frame++)
        {
            samus.LiquidPhysics.BeginFrameSoundRequests();
            if (frame == 40)
                SamusGroundedMovement.StepRanIntoWall(level, samus, 40);
            SamusPostDrawAudio.Step(samus, SamusMovementType.RanIntoWall, 0);
            foreach (var request in samus.LiquidPhysics.SoundRequests)
            {
                if (request.SoundEffect.Library == SoundEffectLibrary.Library3 && request.SoundEffect.Value == 0x25)
                {
                    AssertEqual(40, frame, "echo stop occurs on the wall-cancellation frame");
                    AssertEqual(15, request.MaximumQueued.Requests, "echo stop uses native Max15");
                    stopRequests++;
                }
                queue.QueueSound(request);
            }
            var commands = queue.AdvanceFrame(renderer.Acknowledgements);
            foreach (AudioRequest command in commands)
            {
                int? requested = command switch
                {
                    AudioRequest.RequestSound { Sound.Library: SoundEffectLibrary.Library3 } sound => sound.Sound.Value,
                    AudioRequest.ClearSoundRequest { Library: SoundEffectLibrary.Library3 } => 0,
                    _ => null,
                };
                if (requested is not int value)
                    continue;
                if (value != library3Port && value == 3) starts++;
                if (value != library3Port && value == 0x25) stops++;
                library3Port = value;
            }
            var pcm = renderer.RenderFrame(commands);
            finalPeak = pcm.Max(sample => Math.Abs((int)sample));
            if (frame == 39)
            {
                AssertEqual(3, libraries[2].CurrentSound, "boost echo remains active before wall contact");
                AssertTrue(finalPeak > 0, "boost echo is audible before wall contact");
                AssertEqual(0, stopRequests, "active boost does not cancel its echo");
            }
        }
        AssertEqual(1, starts, "one native boost-start request");
        AssertEqual(1, stopRequests, "one native post-draw stop request");
        AssertEqual(1, stops, "one stop request reaches the SPC");
        AssertEqual(0, libraries[2].CurrentSound, "SPC echo ends without footsteps or landing sounds");
        AssertEqual(0, finalPeak, "echo PCM reaches silence after wall cancellation");
        Suite(nameof(VerifyEchoFlagOwnership), () => VerifyEchoFlagOwnership());
        Console.WriteLine("Speed Booster echo: one start, no premature stop, one wall-cancellation stop, then SPC/PCM silence.");
    }

    /// <summary>Checks deferred echo flags across charging, serialization, and quicksand suppression.</summary>
    private static void VerifyEchoFlagOwnership()
    {
        var samus = new SamusState { Pose = SamusPoseId.FacingRightNormalPose };
        var speed = samus.HorizontalSpeed;
        speed.HasRunningMomentum = true;
        speed.SpeedBoostCounter = new SamusSpeedBoostCounter(0x0301);
        ushort animation = 0;
        speed.TryAdvanceSpeedBoosterAnimationStage(null, SamusMovementType.Running, SnesButton.B,
            0, ref animation, out _);
        AssertTrue(speed.ConsumeEchoSoundRequest(), "deferred producer publishes its start request");
        AssertEqual(1, speed.EchoSoundFlag, "consuming the start event retains the persistent echo flag");
        speed.SpeedBoostCounter = new SamusSpeedBoostCounter(0);
        samus.ResumeChargingBeamSound = ResumeChargingBeamSoundLatch.SignedLatch;
        SamusPostDrawAudio.Step(samus, SamusMovementType.Standing, 0);
        AssertEqual(1, speed.EchoSoundFlag, "negative charging latch skips echo cleanup in native order");
        AssertEqual(0, samus.LiquidPhysics.SoundRequests.Count, "negative charging latch sends no stop");
        SamusPostDrawAudio.Step(samus, SamusMovementType.Standing, 0);
        AssertEqual(0, speed.EchoSoundFlag, "next post-draw pass consumes the echo flag");
        AssertEqual(Library3Sound.SpeedBoosterEnd, samus.LiquidPhysics.SoundRequests.Single().SoundEffect,
            "deferred producer receives the same native stop");

        speed.EchoSoundFlag = 1;
        using var encoded = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(encoded, speed);
        encoded.Position = 0;
        var restored = DebuggerObjectGraphSerializer.Deserialize<SamusHorizontalSpeedState>(encoded);
        AssertEqual(1, restored.EchoSoundFlag, "serialized flag survives even after boost has stopped");

        samus.Position = samus.Position.WithWhole(new(24, 24));
        samus.Kinematics.Radius = samus.Kinematics.Radius.WithY(8);
        speed.SpeedBoostCounter = new SamusSpeedBoostCounter(0x0401);
        speed.EchoSoundFlag = 1;
        speed.EchoSoundRequested = true;
        samus.LiquidPhysics.BeginFrameSoundRequests();
        var sand = new RoomLevelData(new Extent16<RoomBlocks>(4, 4), new RoomLevelPlanes(Enumerable.Repeat((ushort)0x3000, 16).ToArray(), Enumerable.Repeat((byte)0x80, 16).ToArray(), new ushort[16]), new byte[8]);
        SamusInsideBlockReactions.PrepareFrame(sand, samus, AreaId.Maridia);
        AssertEqual(0, speed.EchoSoundFlag, "quicksand clears the native persistent flag");
        AssertTrue(speed.EchoSoundRequested, "quicksand does not retract a previously published start command");
        SamusPostDrawAudio.Step(samus, SamusMovementType.Standing, 0);
        AssertEqual(0, samus.LiquidPhysics.SoundRequests.Count, "quicksand silently suppresses the post-draw stop");
    }

    /// <summary>Runs the Speed Booster echo lifecycle regression.</summary>
    [VerificationCommand("--speed-booster-echo-stop")]
    internal static int SpeedBoosterEchoStopCommand()
    {
        Suite(nameof(VerifySpeedBoosterEchoStop), () => VerifySpeedBoosterEchoStop());
        return 0;
    }
}
