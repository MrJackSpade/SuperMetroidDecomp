using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Desktop;

internal static partial class Program
{
    private static void VerifySpeedBoosterEchoStop()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var renderer = new CartridgeAudioRenderer(RepositoryInstallation.Installation.LoadAudio());
        var queue = new CartridgeAudioState();
        renderer.RenderFrame(queue.AdvanceFrame(bus, renderer.ReadAcknowledgements()));
        renderer.RenderFrame([CartridgeAudioCommand.Upload(AudioUploadAddresses.GreenBrinstar)]);
        var libraries = (ManagedSpcSoundLibrary[])typeof(ManagedSpcPlayer)
            .GetField("soundLibraries", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(renderer.Player)!;
        var samus = new SamusState { Pose = SamusPoseIds.RanIntoWallRightPose, XPosition = 128, YPosition = 128 };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        var speed = samus.HorizontalSpeed;
        speed.HasRunningMomentum = true;
        speed.SpeedBoostCounter = 0x0301;
        ushort animation = 0;
        speed.TryAdvanceSpeedBoosterAnimationStage(bus, SamusMovementType.Running, (ushort)SnesButton.B,
            0, ref animation, out _, () => queue.QueueSoundAndGetAccumulator(SoundEffectLibrary3Sounds.SpeedBoosterEcho, 6));
        var words = new ushort[256];
        for (int index = 160; index < 176; index++) words[index] = 0x8000;
        var level = new RoomLevelData(16, 16, words, new byte[256], new ushort[256], new byte[8]);
        int starts = 0, stops = 0, stopRequests = 0, finalPeak = 0;
        for (int frame = 0; frame < 90; frame++)
        {
            samus.LiquidPhysics.BeginFrameSoundRequests();
            if (frame == 40)
                SamusGroundedMovement.StepRanIntoWall(bus, level, samus, 40);
            SamusPostDrawAudio.Step(bus, samus, SamusMovementType.RanIntoWall, 0);
            foreach (var request in samus.LiquidPhysics.SoundRequests)
            {
                if (request.SoundEffect.Library == SoundEffectLibrary.Library3 && request.SoundEffect.Value == 0x25)
                {
                    AssertEqual(40, frame, "echo stop occurs on the wall-cancellation frame");
                    AssertEqual(15, request.MaximumQueued, "echo stop uses native Max15");
                    stopRequests++;
                }
                queue.QueueSoundAndGetAccumulator(request.SoundEffect, request.MaximumQueued, request.SoundSuppressed);
            }
            var commands = queue.AdvanceFrame(bus, renderer.ReadAcknowledgements());
            starts += commands.Count(command => command.Kind == CartridgeAudioCommandKind.WritePort && command.Port == 3 && command.Value == 3);
            stops += commands.Count(command => command.Kind == CartridgeAudioCommandKind.WritePort && command.Port == 3 && command.Value == 0x25);
            var pcm = renderer.RenderFrame(commands);
            finalPeak = pcm.Max(sample => Math.Abs((int)sample));
            if (frame == 39)
            {
                AssertEqual((byte)3, libraries[2].CurrentSound, "boost echo remains active before wall contact");
                AssertTrue(finalPeak > 0, "boost echo is audible before wall contact");
                AssertEqual(0, stopRequests, "active boost does not cancel its echo");
            }
        }
        AssertEqual(1, starts, "one native boost-start command");
        AssertEqual(1, stopRequests, "one native post-draw stop request");
        AssertEqual(1, stops, "one stop reaches the SPC");
        AssertEqual((byte)0, libraries[2].CurrentSound, "SPC echo ends without footsteps or landing sounds");
        AssertEqual(0, finalPeak, "echo PCM reaches silence after wall cancellation");
        Suite(nameof(VerifyEchoFlagOwnership), () => VerifyEchoFlagOwnership(bus));
        Console.WriteLine("Speed Booster echo: one start, no premature stop, one wall-cancellation stop, then SPC/PCM silence.");
    }

    private static void VerifyEchoFlagOwnership(ISnesAddressSpace bus)
    {
        var samus = new SamusState { Pose = SamusPoseIds.FacingRightNormalPose };
        var speed = samus.HorizontalSpeed;
        speed.HasRunningMomentum = true;
        speed.SpeedBoostCounter = 0x0301;
        ushort animation = 0;
        speed.TryAdvanceSpeedBoosterAnimationStage(bus, SamusMovementType.Running, (ushort)SnesButton.B,
            0, ref animation, out _);
        AssertTrue(speed.ConsumeEchoSoundRequest(), "deferred producer publishes its start request");
        AssertEqual((ushort)1, speed.EchoSoundFlag, "consuming the start event retains the persistent echo flag");
        speed.SpeedBoostCounter = 0;
        samus.ResumeChargingBeamSoundFlag = 0xffff;
        SamusPostDrawAudio.Step(bus, samus, SamusMovementType.Standing, 0);
        AssertEqual((ushort)1, speed.EchoSoundFlag, "negative charging latch skips echo cleanup in native order");
        AssertEqual(0, samus.LiquidPhysics.SoundRequests.Count, "negative charging latch sends no stop");
        SamusPostDrawAudio.Step(bus, samus, SamusMovementType.Standing, 0);
        AssertEqual((ushort)0, speed.EchoSoundFlag, "next post-draw pass consumes the echo flag");
        AssertEqual(SoundEffectLibrary3Sounds.StopSpeedBoosterEcho, samus.LiquidPhysics.SoundRequests.Single().SoundEffect,
            "deferred producer receives the same native stop");

        speed.EchoSoundFlag = 1;
        using var encoded = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(encoded, speed);
        encoded.Position = 0;
        var restored = DebuggerObjectGraphSerializer.Deserialize<SamusHorizontalSpeedState>(encoded);
        AssertEqual((ushort)1, restored.EchoSoundFlag, "serialized flag survives even after boost has stopped");
        var fields = (FieldInfo[])typeof(DebuggerObjectGraphSerializer).GetMethod("GetSerializableFields",
            BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, [typeof(SamusHorizontalSpeedState)])!;
        AssertTrue(LegacyLayout(typeof(SamusHorizontalSpeedState), fields, fields.Where(field => field.Name != "<EchoSoundFlag>k__BackingField"))
            .SequenceEqual(fields.Where(field => field.Name != "<EchoSoundFlag>k__BackingField")),
            "legacy speed layout omits only its unavailable echo flag");
        restored.SpeedBoostCounter = 0x0401;
        RestoreLegacy(restored, "<EchoSoundFlag>k__BackingField");
        AssertEqual((ushort)1, restored.EchoSoundFlag, "legacy active boost arms future cancellation");
        restored.SpeedBoostCounter = 0;
        RestoreLegacy(restored, "<EchoSoundFlag>k__BackingField");
        AssertEqual((ushort)0, restored.EchoSoundFlag, "legacy inactive boost does not invent a pending stop");

        samus.XPosition = samus.YPosition = 24;
        samus.Kinematics.YRadius = 8;
        speed.SpeedBoostCounter = 0x0401;
        speed.EchoSoundFlag = 1;
        speed.EchoSoundRequested = true;
        samus.LiquidPhysics.BeginFrameSoundRequests();
        var sand = new RoomLevelData(4, 4, Enumerable.Repeat((ushort)0x3000, 16).ToArray(),
            Enumerable.Repeat((byte)0x80, 16).ToArray(), new ushort[16], new byte[8]);
        SamusInsideBlockReactions.PrepareFrame(bus, sand, samus, AreaId.Maridia);
        AssertEqual((ushort)0, speed.EchoSoundFlag, "quicksand clears the native persistent flag");
        AssertTrue(speed.EchoSoundRequested, "quicksand does not retract a previously published start command");
        SamusPostDrawAudio.Step(bus, samus, SamusMovementType.Standing, 0);
        AssertEqual(0, samus.LiquidPhysics.SoundRequests.Count, "quicksand silently suppresses the post-draw stop");
    }
}
