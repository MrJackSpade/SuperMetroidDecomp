using System.Buffers.Binary;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rendering;
using M = MoatMovieMemory;

/// <summary>Diagnostic replay of the player's Snes9x CWJ, seeded after the incoming door handoff.</summary>
internal static class MoatMovieProbe
{
    public static int Run(string rom, string directory)
    {
        if (Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
                File.ReadAllBytes(Path.Combine(directory,"cwj.smv")))) !=
            "90CDD95DC88845972FEA9CFDBF636D6443C6A825FA9FE0A52A9F2E8648681D39")
            throw new InvalidDataException("Use the player-supplied CWJ movie, not a recaptured or retimed input sequence.");
        var memory = File.ReadAllBytes(Path.Combine(directory,"frame-270.wram"));
        if (memory.Length != SuperMetroidAddressSpace.WorkRamByteCount ||
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(memory)) !=
            "2F0F8D3F4C8BB7BD73C728C1E3F81F76663B35DCB5FFCC7C52ED035D74279DFA")
            throw new InvalidDataException("Expected complete Snes9x WRAM capture.");
        ushort W(int address) => BinaryPrimitives.ReadUInt16LittleEndian(memory.AsSpan(address));
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.System.LoadCollectedItemBytes(memory.AsSpan(M.CollectedItemBits,Bank80SystemState.ItemBitByteCount));
        runtime.LoadCartridgeRoomForDebug(W(M.Room), W(M.CameraX), W(M.CameraY));
        // Use the captured collision population, including the already-open entrance.
        for (int i=0;i<runtime.LevelData!.WidthInBlocks*runtime.LevelData.HeightInBlocks;i++)
        {
            runtime.LevelData.SetForegroundEntry(i,W(M.Level+i*sizeof(ushort)));
            runtime.LevelData.SetBehavior(i,memory[M.Bts+i]);
        }
        var s=runtime.Samus!;
        s.InputLocked=false;
        s.EquippedItems=W(M.Items); s.EquippedBeams=W(M.Beams);
        s.Health=W(M.Health); s.MaxHealth=W(M.MaxHealth);
        s.Pose=(byte)W(M.Pose);
        s.XPosition=W(M.X); s.YPosition=W(M.Y);
        s.Kinematics.XSubposition=W(M.XFraction); s.Kinematics.YSubposition=W(M.YFraction);
        s.RefreshCollisionRadii(bus); s.InitializeAnimation(bus);
        s.SetAnimationFrameFromSpecialHandler(W(M.Animation),W(M.AnimationTimer));
        s.PoseHistory.PreviousPose=W(M.PreviousPose);
        s.PoseHistory.PreviousDirectionAndMovement=W(M.PreviousDirection);
        s.PoseHistory.LastDifferentPose=W(M.LastDifferentPose);
        s.PoseHistory.LastDifferentDirectionAndMovement=W(M.LastDifferentDirection);
        s.HorizontalSpeed.BaseSpeed=W(M.BaseSpeed); s.HorizontalSpeed.BaseSubspeed=W(M.BaseFraction);
        s.HorizontalSpeed.ExtraRunSpeed=W(M.ExtraSpeed); s.HorizontalSpeed.ExtraRunSubspeed=W(M.ExtraFraction);
        s.HorizontalSpeed.AccelerationMode=W(M.AccelerationMode);
        s.HorizontalSpeed.HasRunningMomentum=W(M.Momentum)!=0;
        s.HorizontalSpeed.SpeedBoostCounter=W(M.BoostCounter);
        s.Kinematics.YSpeed=W(M.VerticalSpeed); s.Kinematics.YSubspeed=W(M.VerticalFraction);
        s.Kinematics.YDirection=W(M.VerticalDirection);
        var inputs=File.ReadLines(Path.Combine(directory,"inputs.csv")).Skip(1)
            .Select(l=>ushort.Parse(l.Split(',')[1],System.Globalization.NumberStyles.HexNumber)).ToArray();
        runtime.Controller1.Latch(inputs[269]);
        var expected=File.ReadAllLines(Path.Combine(directory,"native.csv")).Skip(1).ToArray();
        if (expected.Length != 262) throw new InvalidDataException("Expected movie frames 270 through 531.");
        bool jumped=false;
        for(int frame=270;frame<=531;frame++)
        {
            string actual=$"{frame},{s.Kinematics.XFixed:X8},{s.Kinematics.YFixed:X8},{s.Pose:X2},{s.HorizontalSpeed.BaseFixed:X8},{s.HorizontalSpeed.ExtraRunSpeed:X4}{s.HorizontalSpeed.ExtraRunSubspeed:X4},{s.AnimationFrame:X4},{s.AnimationFrameTimer:X4},{s.Kinematics.YSpeed:X4}{s.Kinematics.YSubspeed:X4},{s.Kinematics.YDirection:X4},{s.HorizontalSpeed.AccelerationMode:X4}";
            if (actual!=expected[frame-270])
                throw new InvalidDataException($"Movie frame {frame} differs.\nNative: {expected[frame-270]}\nPort:   {actual}");
            if(frame==345)
            {
                jumped=s.ReadMovementType(bus)==SamusMovementType.WallJumping &&
                    s.HorizontalSpeed.ExtraRunSpeed==1 && s.HorizontalSpeed.ExtraRunSubspeed==0x3000;
                if(!jumped) throw new InvalidDataException("Recorded far-side walljump failed to retain its native run speed.");
            }
            // Snes9x's pre-frame freeze already contains the previous input sample.
            // Applying frame+1 here jumps a frame early and invalidates the comparison.
            if(frame<531) runtime.StepFrame(inputs[frame]);
        }
        if(!jumped || s.PoseId!=SamusPoseId.RanIntoWallRightPose ||
            s.Kinematics.YSpeed!=0 || s.Kinematics.YSubspeed!=0 || s.Kinematics.YDirection!=0)
            throw new InvalidDataException("Movie did not finish its CWJ and landing.");
        Console.WriteLine("PASS: player CWJ movie, 262 native checkpoints; exact position/subpixels, pose, speeds, animation and landing match.");
        return 0;
    }
}
