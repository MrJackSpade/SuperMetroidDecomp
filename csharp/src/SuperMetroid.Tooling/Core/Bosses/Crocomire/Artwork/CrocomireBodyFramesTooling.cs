namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>Development-tool members of <see cref="CrocomireBodyFrames"/>; never linked by player hosts.</summary>
internal static class CrocomireBodyFramesTooling
{
    /// <summary>The fifty fight-body and melting frames in the order of their cartridge roots.</summary>
    internal static readonly CrocomireBodyFrame[] BodyOrder =
    [
        CrocomireBodyFrame.ChargingFirst,
        CrocomireBodyFrame.ChargingSecond,
        CrocomireBodyFrame.ChargingThird,
        CrocomireBodyFrame.ChargingFourth,
        CrocomireBodyFrame.ChargingFifth,
        CrocomireBodyFrame.ChargingSixth,
        CrocomireBodyFrame.ChargingSeventh,
        CrocomireBodyFrame.ChargingEighth,
        CrocomireBodyFrame.ChargingNinth,
        CrocomireBodyFrame.ChargingTenth,
        CrocomireBodyFrame.ChargingEleventh,
        CrocomireBodyFrame.ChargingTwelfth,
        CrocomireBodyFrame.Initial,
        CrocomireBodyFrame.WaitForFirstSecondDamageSixth,
        CrocomireBodyFrame.WaitForFirstSecondDamageFifth,
        CrocomireBodyFrame.WaitForFirstSecondDamageFourth,
        CrocomireBodyFrame.WaitForFirstSecondDamageThird,
        CrocomireBodyFrame.WaitForFirstSecondDamageSecond,
        CrocomireBodyFrame.WaitForFirstSecondDamageFirst,
        CrocomireBodyFrame.ProjectileAttackFirst,
        CrocomireBodyFrame.ProjectileAttackSecond,
        CrocomireBodyFrame.ProjectileAttackThird,
        CrocomireBodyFrame.ProjectileAttackFourth,
        CrocomireBodyFrame.ProjectileAttackFifth,
        CrocomireBodyFrame.ProjectileAttackSixth,
        CrocomireBodyFrame.ProjectileAttackSeventh,
        CrocomireBodyFrame.ProjectileAttackEighth,
        CrocomireBodyFrame.StepForwardAfterDelay,
        CrocomireBodyFrame.SteppingForwardFirst,
        CrocomireBodyFrame.SteppingForwardSecond,
        CrocomireBodyFrame.SteppingForwardThird,
        CrocomireBodyFrame.SteppingForwardFourth,
        CrocomireBodyFrame.SteppingForwardFifth,
        CrocomireBodyFrame.SteppingForwardSixth,
        CrocomireBodyFrame.SteppingForwardSeventh,
        CrocomireBodyFrame.SteppingForwardEighth,
        CrocomireBodyFrame.SteppingForwardNinth,
        CrocomireBodyFrame.ProjectileAttackLoopFirst,
        CrocomireBodyFrame.ProjectileAttackLoopSecond,
        CrocomireBodyFrame.ProjectileAttackLoopThird,
        CrocomireBodyFrame.ProjectileAttackLoopFourth,
        CrocomireBodyFrame.ProjectileAttackLoopFifth,
        CrocomireBodyFrame.MeltingOneTopRow,
        CrocomireBodyFrame.MeltingOneTopTwoRows,
        CrocomireBodyFrame.MeltingOneTopThreeRows,
        CrocomireBodyFrame.MeltingOneTopFourRows,
        CrocomireBodyFrame.MeltingTwoTopRow,
        CrocomireBodyFrame.MeltingTwoTopTwoRows,
        CrocomireBodyFrame.MeltingTwoTopThreeRows,
        CrocomireBodyFrame.MeltingTwoTopFourRows,
    ];

    /// <summary>The thirty-three skeleton frames in the order of their cartridge roots.</summary>
    internal static readonly CrocomireBodyFrame[] SkeletonOrder =
    [
        CrocomireBodyFrame.SkeletonFallingFirst,
        CrocomireBodyFrame.SkeletonFallingSecond,
        CrocomireBodyFrame.SkeletonFallingThird,
        CrocomireBodyFrame.SkeletonFallsApartFirst,
        CrocomireBodyFrame.SkeletonFallsApartSecond,
        CrocomireBodyFrame.SkeletonFallsApartThird,
        CrocomireBodyFrame.SkeletonFallsApartFourth,
        CrocomireBodyFrame.SkeletonFallsApartFifth,
        CrocomireBodyFrame.SkeletonFallsApartSixth,
        CrocomireBodyFrame.SkeletonFallsApartSeventh,
        CrocomireBodyFrame.SkeletonFallsApartEighth,
        CrocomireBodyFrame.SkeletonFallsApartNinth,
        CrocomireBodyFrame.SkeletonFallsApartTenth,
        CrocomireBodyFrame.SkeletonFallsApartEleventh,
        CrocomireBodyFrame.SkeletonFallsApartTwelfth,
        CrocomireBodyFrame.SkeletonFallsApartThirteenth,
        CrocomireBodyFrame.SkeletonFallsApartFourteenth,
        CrocomireBodyFrame.SkeletonFallsApartFifteenth,
        CrocomireBodyFrame.SkeletonFallsApartSixteenth,
        CrocomireBodyFrame.SkeletonFallsApartSeventeenth,
        CrocomireBodyFrame.SkeletonFallsApartEighteenth,
        CrocomireBodyFrame.SkeletonStable,
        CrocomireBodyFrame.Dead,
        CrocomireBodyFrame.SkeletonFlowingDownRiverFirst,
        CrocomireBodyFrame.SkeletonFlowingDownRiverSecond,
        CrocomireBodyFrame.SkeletonFlowingDownRiverThird,
        CrocomireBodyFrame.SkeletonFlowingDownRiverFourth,
        CrocomireBodyFrame.SkeletonFlowingDownRiverFifth,
        CrocomireBodyFrame.SkeletonFlowingDownRiverSixth,
        CrocomireBodyFrame.SkeletonFlowingDownRiverSeventh,
        CrocomireBodyFrame.SkeletonFlowingDownRiverEighth,
        CrocomireBodyFrame.SkeletonFlowingDownRiverNinth,
        CrocomireBodyFrame.SkeletonFlowingDownRiverTenth,
    ];

    /// <summary>Whether <paramref name="frame"/> is one of the skeleton's frames.</summary>
    internal static bool IsSkeleton(CrocomireBodyFrame frame) => Array.IndexOf(SkeletonOrder, frame) >= 0;

    /// <summary>The installed extended spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(CrocomireBodyFrame frame)
    {
        int body = Array.IndexOf(BodyOrder, frame);
        if (body >= 0)
            return CrocomireBodyVisualToolingDefinitions.FramePointer(body);
        int skeleton = Array.IndexOf(SkeletonOrder, frame);
        return skeleton >= 0
            ? CrocomireSkeletonVisualToolingDefinitions.FramePointer(skeleton)
            : throw new InvalidOperationException($"Undefined Crocomire body frame {frame}.");
    }
}
