using System.Globalization;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;

/// <summary>
/// Replays a reported input interval using installed content and recording-owned SRAM.
/// It never subscribes to disk-save events or changes the player's installation.
/// The outer DebugRunner boundary owns no-dialog policy and error reporting.
/// </summary>
internal static class InstalledInputReplay
{
    internal static int Run(string recordingPath, string installationRoot,
        int firstFrame, int lastFrame, string tracePath,
        Action<SuperMetroidGame, int>? confirmFrame = null)
    {
        ControllerInputRecording recording = ControllerInputRecording.Read(recordingPath);
        if (firstFrame < 0 || lastFrame < firstFrame || lastFrame >= recording.ControllerInputs.Length)
            throw new ArgumentOutOfRangeException(nameof(lastFrame));
        var installation = new GameInstallation(installationRoot);
        var bus = installation.OpenRuntimeAddressSpace();
        recording.InitialSaveRam.CopyTo(bus.SaveRam);
        var game = new SuperMetroidGame(bus, recording.GameOptions, renderGameplayFrames: false);
        Bind(game, installation);
        var audio = new CartridgeAudioRenderer(installation.LoadAudio());
        using var output = new StreamWriter(tracePath);
        output.WriteLine("frame,room,state,input,latched,new,pose,x,y,xFixed,yFixed,base,baseSub,extra,extraSub,accel,momentum,yDirection,ySpeed,knockback,locked,health,cameraY,cooldown,fired,projectiles,plms,shotBlock,animationFrame,animationTimer");
        (int X, int Y)? shotBlock = null;
        for (int frame = 0; frame <= lastFrame; frame++)
        {
            try
            {
                game.SetAudioAcknowledgements(audio.ReadAcknowledgements());
                var result = game.Step(recording.ControllerInputs[frame]);
                audio.RenderFrame(result.AudioCommands);
                confirmFrame?.Invoke(game, frame);
            }
            catch (Exception error)
            {
                var failedSamus = game.RuntimeForVerification?.Samus;
                throw new InvalidDataException($"Reported recording failed at input frame {frame}, room {game.GameplayActiveRoomPointer:X4}, pose {failedSamus?.Pose:X2}, animation {failedSamus?.AnimationFrame}/{failedSamus?.AnimationFrameTimer}.", error);
            }
            if (frame < firstFrame || game.RuntimeForVerification is not { } runtime) continue;
            if (runtime.Samus is not { } samus) continue;
            if (runtime.Projectiles.LastFiredProjectileSnapshot is { } shot)
                shotBlock = (shot.XPosition >> 4, shot.YPosition >> 4);
            string sampledBlock = shotBlock is { } block && runtime.LevelData is { } level &&
                (uint)block.X < level.WidthInBlocks && (uint)block.Y < level.HeightInBlocks
                ? $"{block.X}/{block.Y}:{level.GetCollisionBlock(block.X, block.Y).LevelWord:X4}"
                : "";
            output.WriteLine(string.Join(',', frame.ToString(CultureInfo.InvariantCulture),
                $"{runtime.ActiveRoom?.Pointer:X4}", game.GameState,
                $"{recording.ControllerInputs[frame]:X4}", $"{runtime.Controller1.Current:X4}",
                $"{runtime.Controller1.NewlyPressed:X4}", $"{samus.Pose:X2}",
                samus.XPosition, samus.YPosition, samus.Kinematics.XFixed, samus.Kinematics.YFixed,
                samus.HorizontalSpeed.BaseSpeed, samus.HorizontalSpeed.BaseSubspeed,
                samus.HorizontalSpeed.ExtraRunSpeed, samus.HorizontalSpeed.ExtraRunSubspeed,
                samus.HorizontalSpeed.AccelerationMode, samus.HorizontalSpeed.HasRunningMomentum,
                samus.Kinematics.YDirection, samus.Kinematics.YSpeed,
                samus.KnockbackActive, samus.InputLocked, samus.Health, runtime.Camera?.YPosition,
                runtime.BombProjectiles.CooldownTimer, runtime.Projectiles.LastFrameResult.FiredSlot,
                string.Join('|', runtime.Projectiles.Slots.Where(slot => slot.IsActive).Select(slot =>
                    $"{slot.SlotIndex}:{slot.Type:X4}@{slot.XPosition}/{slot.YPosition}:{slot.PreInstruction}")),
                runtime.Plms.ActiveCount, sampledBlock, samus.AnimationFrame, samus.AnimationFrameTimer));
        }
        Console.WriteLine($"Reported input interval {firstFrame}..{lastFrame} written to {Path.GetFullPath(tracePath)}; total inputs={recording.ControllerInputs.Length}.");
        return 0;
    }

    // Match the current host's presentation bindings without opening a UI, installing
    // assets, loading disk saves or writing a new input recording.
    internal static void Bind(SuperMetroidGame game, GameInstallation installation) =>
        InstalledGameBindings.Create(installation)(game);
}
