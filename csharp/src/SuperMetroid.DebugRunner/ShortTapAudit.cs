using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>
/// Separates accepted one-frame input from host polling for #314. Passing this test
/// does not establish that the desktop observes every physical controller press.
/// </summary>
internal static class ShortTapAudit
{
    public static int Run(string romPath)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        int cases = 0;
        foreach (bool water in new[] { false, true })
        foreach (bool left in new[] { false, true })
        for (int delay = 0; delay < 8; delay++)
        for (int duration = 1; duration <= 3; duration++)
        {
            var runtime = FlatFloorMovementFixture.Create(bus, water);
            var samus = runtime.Samus!;
            samus.Pose = left ? SamusPoseIds.FacingRightNormalPose : SamusPoseIds.FacingLeftNormalPose;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            byte initialPose = samus.Pose;
            int firstPoseChange = -1;
            for (int frame = 0; frame < delay + 60; frame++)
            {
                ushort input = frame >= delay && frame < delay + duration
                    ? (ushort)(left ? SnesButton.Left : SnesButton.Right) : (ushort)0;
                runtime.StepFrame(input);
                if (firstPoseChange < 0 && samus.Pose != initialPose)
                    firstPoseChange = frame;
                if (samus.LiquidPhysics.DetermineMovementMedium(samus) != (water ? 1 : 0))
                    throw new InvalidDataException("Short-tap fixture changed liquid medium.");
            }
            byte expectedPose = left ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose;
            if (firstPoseChange != delay || samus.Pose != expectedPose)
                throw new InvalidDataException(
                    $"Short tap water={water}, left={left}, delay={delay}, duration={duration}: first pose change={firstPoseChange}, final pose={samus.Pose:X2}, expected={expectedPose:X2}.");
            cases++;
        }
        Console.WriteLine($"PASS {cases} short-tap cases: pose changes on the input frame and settles facing the requested direction.");
        Console.WriteLine("Host sampling and exact retail animation timing are not asserted by this audit.");
        return 0;
    }
}
