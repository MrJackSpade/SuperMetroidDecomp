using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>
/// A finite ledge through the full runtime dispatcher, not a direct call to the
/// aerial speed calculator. This covers the missing falling-pose initialization
/// observed by the room-seeded cartridge CPU probe for #312.
/// </summary>
internal static class WalkOffMomentumAudit
{
    public static int Run(string romPath)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        foreach (bool water in new[] { false, true })
        foreach (bool left in new[] { false, true })
        {
            var runtime = FlatFloorMovementFixture.Create(bus, water);
            var samus = runtime.Samus!;
            var level = runtime.LevelData!;
            // Keep four support blocks and remove the remainder of the floor.
            // Mirrored positions keep both runs away from room-edge streaming.
            for (int column = 0; column < level.WidthInBlocks; column++)
                level.SetForegroundEntry(16 * level.WidthInBlocks + column,
                    column >= 6 && column <= 9 ? (ushort)0x8000 : (ushort)0);
            samus.Pose = left ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose;
            if (left) samus.ApplyStandingLeftToRunningLeft(bus);
            else samus.ApplyStandingRightToRunningRight(bus);
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.XPosition = left ? (ushort)102 : (ushort)153;
            samus.Kinematics.XSubposition = 0;
            samus.HorizontalSpeed.BaseSpeed = 2;
            samus.HorizontalSpeed.BaseSubspeed = 0xc000;
            uint? fallingX = null;
            int fallingFrames = 0;
            for (int frame = 0; frame < 30; frame++)
            {
                runtime.StepFrame(0);
                if (samus.ReadMovementType(bus) != SamusMovementType.Falling) continue;
                fallingX ??= samus.Kinematics.XFixed;
                if (samus.HorizontalSpeed.AccelerationMode != 0 || samus.Kinematics.XFixed != fallingX)
                    throw new InvalidDataException(
                        $"Walk-off water={water} left={left} frame={frame}: mode={samus.HorizontalSpeed.AccelerationMode}, " +
                        $"X={samus.Kinematics.XFixed:X8}, expected zero mode and stationary X={fallingX:X8} after no-input ledge departure.");
                fallingFrames++;
            }
            if (fallingFrames < 10)
                throw new InvalidDataException("The fixture did not sustain ten falling frames.");
            Console.WriteLine($"PASS ledge water={water} left={left}: {fallingFrames} falling frames without unintended horizontal travel.");
        }
        return 0;
    }
}
