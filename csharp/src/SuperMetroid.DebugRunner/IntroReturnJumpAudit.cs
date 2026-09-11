using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>Captures the scripted return jump after the real intro Rinka hit.</summary>
internal static class IntroReturnJumpAudit
{
    public static int Run(string romPath, string outputDirectory)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var intro = new IntroCinematicState(bus);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var samusField = typeof(IntroCinematicState).GetField("flashbackSamus", flags)!;
        var demoField = typeof(IntroCinematicState).GetField("flashbackDemoInput", flags)!;
        for (int frame = 0; frame < 8192 && intro.Phase != IntroCinematicPhase.PageOneAwaitingInput; frame++)
            intro.Step(0);
        if (intro.Phase != IntroCinematicPhase.PageOneAwaitingInput)
            throw new InvalidDataException("Intro first page was not reached.");
        intro.Step((ushort)SnesButton.A);
        Directory.CreateDirectory(outputDirectory);
        using var trace = new StreamWriter(Path.Combine(outputDirectory, "trajectory.csv"));
        trace.WriteLine("frame,phase,held,new,pose,x,y,animation,knockback");
        bool hit = false, jumpRequested = false, roseAfterJump = false;
        bool descended = false, landed = false, stoodAfterLanding = false;
        int minimumY = int.MaxValue;
        var spinFrames = new HashSet<ushort>();
        var landingFrames = new HashSet<ushort>();
        int jumpY = 0;
        for (int frame = 0; frame < 800; frame++)
        {
            intro.Step(0);
            // Drawing selects the next NMI's Samus tile transfers, so even uncaptured
            // frames must render to reproduce the host's animation/graphics pipeline.
            var pixels = intro.Render();
            var samus = (SamusState?)samusField.GetValue(intro);
            var demo = (DemoInputState?)demoField.GetValue(intro);
            if (samus is null || demo is null) continue;
            hit |= samus.KnockbackActive;
            if (hit && !jumpRequested && (demo.NewlyPressed & (ushort)SnesButton.A) != 0)
            {
                jumpRequested = true;
                jumpY = samus.YPosition;
                Console.WriteLine($"ROM return-jump edge frame {frame}: pose ${samus.Pose:X2}, ({samus.XPosition},{samus.YPosition}).");
            }
            if (jumpRequested && !samus.KnockbackActive && samus.YPosition < jumpY)
                roseAfterJump = true;
            if (jumpRequested && samus.Pose == SamusPoseIds.SpinJumpLeftPose)
            {
                minimumY = Math.Min(minimumY, samus.YPosition);
                descended |= roseAfterJump && samus.YPosition > minimumY;
                spinFrames.Add(samus.AnimationFrame);
            }
            if (jumpRequested && SamusState.IsLeftFacingLandingPose(samus.Pose))
            {
                landed = descended && samus.YPosition == jumpY;
                landingFrames.Add(samus.AnimationFrame);
            }
            stoodAfterLanding |= landed && samus.Pose == SamusPoseIds.FacingLeftNormalPose;
            trace.WriteLine($"{frame},{intro.Phase},{demo.Held:X4},{demo.NewlyPressed:X4},{samus.Pose:X2},{samus.XPosition},{samus.YPosition},{samus.AnimationFrame},{samus.KnockbackActive}");
            if (frame % 8 == 0 && hit)
                PngWriter.WriteRgba(Path.Combine(outputDirectory, $"frame-{frame:D4}.png"), 256, 224, pixels);
        }
        Console.WriteLine($"Intro return jump: hit={hit}, requested={jumpRequested}, rose={roseAfterJump}.");
        Console.WriteLine($"Return arc: descent={descended}, landing={landed}, standing={stoodAfterLanding}, spin frames={spinFrames.Count}, landing frames={landingFrames.Count}.");
        if (!hit || !jumpRequested || !roseAfterJump || !descended || !landed || !stoodAfterLanding ||
            spinFrames.Count < 2 || landingFrames.Count < 2)
            throw new InvalidDataException("The actual intro demo requested a return jump without its upward trajectory.");
        return 0;
    }
}
