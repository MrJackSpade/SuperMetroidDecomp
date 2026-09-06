using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Input;

/// <summary>Constructed flat-floor regression for #313, through the complete frame dispatcher.</summary>
internal static class RunningReleaseAudit
{
    public static int Run(string romPath)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        foreach (bool water in new[] { false, true })
        foreach (bool left in new[] { true, false })
        foreach (bool keepDirection in new[] { false, true })
        {
            var runtime = new SuperMetroidRuntime(bus);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RunNmi(0, true);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            // Ordinary room loading clears the fresh-game elevator arrival coroutine;
            // leaving it live would teleport the test actor after sixty frames.
            runtime.LoadCartridgeRoomForDebug(runtime.ActiveRoom!.Pointer, 0, 0);
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
            uint travelled = 0;
            int zeroSpeedFrame = water ? 88 : 6;
            for (int frame = 0; frame < 100; frame++)
            {
                // Recenter without touching pose/speed, matching the native fixture.
                // This allows the full coast to finish without leaving a finite room.
                samus.XPosition = 200;
                samus.Kinematics.XSubposition = 0;
                uint expectedBase = keepDirection ? 0x2c000u : frame <= zeroSpeedFrame
                    ? (uint)Math.Max(0, 0x2c000 - frame * (water ? 0x800 : 0x8000))
                    : frame == zeroSpeedFrame + 1 ? (water ? 0x400u : 0x3000u) : 0;
                bool forwardProbe = !keepDirection && frame <= zeroSpeedFrame;
                uint distance = expectedBase + (forwardProbe ? 0x10000u : 0);
                uint expectedX = left ? (200u << 16) - distance : (200u << 16) + distance;
                runtime.StepFrame(keepDirection ? (ushort)(left ? SnesButton.Left : SnesButton.Right) : (ushort)0);
                if (samus.LiquidPhysics.DetermineMovementMedium(samus) != (water ? 1 : 0))
                    throw new InvalidDataException("Release fixture selected the wrong liquid medium.");
                if (samus.Kinematics.XFixed != expectedX)
                    throw new InvalidDataException(
                        $"Release water={water}, left={left}, frame={frame}: expected X={expectedX:X8}, actual={samus.Kinematics.XFixed:X8}; pose={samus.Pose:X2} y={samus.YPosition} fallback={runtime.ProspectiveSamusFallbackPose}, probe={runtime.LastRanIntoWallProbe}, movement={runtime.LastGroundedSamusMovement}.");
                travelled += left ? (200u << 16) - samus.Kinematics.XFixed : samus.Kinematics.XFixed - (200u << 16);
                if (samus.HorizontalSpeed.BaseFixed != expectedBase ||
                    runtime.LastRanIntoWallProbe?.AcceptedDisplacement !=
                        (forwardProbe ? (left ? -65536 : 65536) : (int?)null))
                    throw new InvalidDataException("Release must retain native base speed and exactly one forward probe pixel.");
            }
            if (travelled != (keepDirection ? 0x1130000u : water ? 0xd36400u : 0x103000u))
                throw new InvalidDataException("Full release distance disagrees with the cartridge CPU trace.");
            Console.WriteLine($"PASS release water={water}, left={left}, keep-direction={keepDirection}: 100 exact frame offsets; distance={travelled:X8}.");
        }
        return 0;
    }
}
