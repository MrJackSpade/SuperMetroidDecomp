using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

/// <summary>Constructed flat-floor regression for #313, through the complete frame dispatcher.</summary>
internal static class RunningReleaseAudit
{
    public static int Run(string romPath)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        foreach (bool water in new[] { false, true })
        foreach (bool left in new[] { true, false })
        {
            var runtime = new SuperMetroidRuntime(bus);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RunNmi(0, true);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            var level = runtime.LevelData!;
            // Replace the test area's geometry. The Ceres scaffold supplies the
            // runtime owners; these deliberately constructed liquid conditions match
            // the CPU fixture, not a claim about the retail room's water height.
            for (int y = 0; y <= 16; y++)
                for (int x = 0; x < level.WidthInBlocks; x++)
                    level.SetForegroundEntry(y * level.WidthInBlocks + x,
                        y == 16 ? (ushort)0x8000 : (ushort)0);
            runtime.InitializeDebugGroundedSamus(200, 166, 16);
            var samus = runtime.Samus!;
            if (left)
            {
                samus.Pose = SamusPoseIds.FacingLeftNormalPose;
                samus.ApplyStandingLeftToRunningLeft(bus);
            }
            else samus.ApplyStandingRightToRunningRight(bus);
            samus.InputLocked = false;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            if (water) samus.LiquidPhysics.ConfigureWater(8, 0x80);
            samus.HorizontalSpeed.BaseSpeed = 2;
            samus.HorizontalSpeed.BaseSubspeed = 0xc000;
            uint expectedX = 200u << 16;
            uint expectedBase = 0x2c000;
            for (int frame = 0; frame < 4; frame++)
            {
                if (frame != 0) expectedBase -= water ? 0x800u : 0x8000u;
                uint distance = expectedBase + 0x10000;
                expectedX = left ? expectedX - distance : expectedX + distance;
                runtime.StepFrame(0);
                if (samus.LiquidPhysics.DetermineMovementMedium(samus) != (water ? 1 : 0))
                    throw new InvalidDataException("Release fixture selected the wrong liquid medium.");
                if (samus.Kinematics.XFixed != expectedX)
                    throw new InvalidDataException(
                        $"Release water={water}, left={left}, frame={frame}: expected X={expectedX:X8}, actual={samus.Kinematics.XFixed:X8}; fallback={runtime.ProspectiveSamusFallbackPose}, probe={runtime.LastRanIntoWallProbe}.");
                if (samus.HorizontalSpeed.BaseFixed != expectedBase ||
                    runtime.LastRanIntoWallProbe?.AcceptedDisplacement != (left ? -65536 : 65536))
                    throw new InvalidDataException("Release must retain native base speed and exactly one forward probe pixel.");
            }
            Console.WriteLine($"PASS release water={water}, left={left}: four exact cartridge movement/pose offsets.");
        }
        return 0;
    }
}
