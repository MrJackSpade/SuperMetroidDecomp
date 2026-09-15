using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Diagnostic replay of the player's Snes9x CWJ, seeded after the incoming door handoff.</summary>
internal static class MoatMovieProbe
{
    public static int Run(string rom, string directory)
    {
        byte[] memory = MoatMovieFixture.LoadCheckpoint(
            directory,
            "frame-270.wram",
            "2F0F8D3F4C8BB7BD73C728C1E3F81F76663B35DCB5FFCC7C52ED035D74279DFA");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var runtime = MoatMovieFixture.CreateRuntime(bus, memory);
        SamusState s = runtime.Samus!;
        ushort[] inputs = MoatMovieFixture.LoadInputs(directory);
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
