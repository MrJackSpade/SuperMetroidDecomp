using F = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileFrameId;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The poses of Torizo's projectiles on one side (#2472): those of a sonic boom, a Chozo orb, an egg and its hatchling facing or
/// flying that way, a thrown Super Missile's spin starting that way, and the dust the Torizo's foot on that side raises on
/// landing. Each projectile animation is written once over a side, as the body's are over a <see cref="TorizoSide"/> (#2411).
/// </summary>
/// <param name="SonicBoomSmall">A sonic boom just thrown.</param>
/// <param name="SonicBoomMedium">A sonic boom growing.</param>
/// <param name="SonicBoomLarge">A sonic boom at its full size, as it flies.</param>
/// <param name="ChozoOrb">A Chozo orb in flight.</param>
/// <param name="DustPuff">The landing dust's first puff.</param>
/// <param name="DustPuffSmall">The landing dust shrinking.</param>
/// <param name="DustPuffThin">The landing dust thinning.</param>
/// <param name="DustPuffFaint">The landing dust's last, faint puff.</param>
/// <param name="EggWhole">An egg whole, as it bounces.</param>
/// <param name="EggCracked">An egg cracked.</param>
/// <param name="EggSplitting">An egg splitting open.</param>
/// <param name="EggHatching">An egg hatching.</param>
/// <param name="HatchlingClawsRaised">A hatchling with its claws raised.</param>
/// <param name="HatchlingClawsLowered">A hatchling with its claws lowered.</param>
/// <param name="HatchlingHunched">A hatchling hunched.</param>
/// <param name="HatchlingCrumbling">A shot hatchling crumbling.</param>
/// <param name="HatchlingCollapsing">A shot hatchling collapsing.</param>
/// <param name="HatchlingSlumped">A shot hatchling slumped.</param>
/// <param name="HatchlingRemains">A shot hatchling's remains.</param>
/// <param name="HatchlingRemainsFlat">A shot hatchling's remains, flattened.</param>
/// <param name="SuperMissileDownForward">A thrown Super Missile pointing down and this way.</param>
/// <param name="SuperMissileForward">A thrown Super Missile pointing this way.</param>
/// <param name="SuperMissileUpForward">A thrown Super Missile pointing up and this way.</param>
/// <param name="SuperMissileUpBack">A thrown Super Missile pointing up and the other way.</param>
/// <param name="SuperMissileBack">A thrown Super Missile pointing the other way.</param>
/// <param name="SuperMissileDownBack">A thrown Super Missile pointing down and the other way.</param>
internal sealed record TorizoProjectileSide(
    F SonicBoomSmall, F SonicBoomMedium, F SonicBoomLarge, F ChozoOrb,
    F DustPuff, F DustPuffSmall, F DustPuffThin, F DustPuffFaint,
    F EggWhole, F EggCracked, F EggSplitting, F EggHatching,
    F HatchlingClawsRaised, F HatchlingClawsLowered, F HatchlingHunched,
    F HatchlingCrumbling, F HatchlingCollapsing, F HatchlingSlumped, F HatchlingRemains, F HatchlingRemainsFlat,
    F SuperMissileDownForward, F SuperMissileForward, F SuperMissileUpForward, F SuperMissileUpBack, F SuperMissileBack, F SuperMissileDownBack)
{
    /// <summary>The poses on the left: facing or flying left, and the dust of the Torizo's left foot.</summary>
    internal static TorizoProjectileSide Left { get; } = new(
        F.TorizoSonicBoomLeftSmall, F.TorizoSonicBoomLeftMedium, F.TorizoSonicBoomLeftLarge, F.TorizoChozoOrbFacingLeft,
        F.TorizoLandingDustLeftDustPuff, F.TorizoLandingDustLeftDustPuffSmall, F.TorizoLandingDustLeftDustPuffThin, F.TorizoLandingDustLeftDustPuffFaint,
        F.GoldenTorizoEggWholeLeft, F.GoldenTorizoEggCrackedLeft, F.GoldenTorizoEggSplittingLeft, F.GoldenTorizoEggHatchingLeft,
        F.GoldenTorizoHatchlingLeftClawsRaised, F.GoldenTorizoHatchlingLeftClawsLowered, F.GoldenTorizoHatchlingLeftHunched,
        F.GoldenTorizoHatchlingLeftCrumbling, F.GoldenTorizoHatchlingLeftCollapsing, F.GoldenTorizoHatchlingLeftSlumped,
        F.GoldenTorizoHatchlingLeftRemains, F.GoldenTorizoHatchlingLeftRemainsFlat,
        F.GoldenTorizoSuperMissilePointingDownLeft, F.GoldenTorizoSuperMissilePointingLeft, F.GoldenTorizoSuperMissilePointingUpLeft,
        F.GoldenTorizoSuperMissilePointingUpRight, F.GoldenTorizoSuperMissilePointingRight, F.GoldenTorizoSuperMissilePointingDownRight);

    /// <summary>The poses on the right: facing or flying right, and the dust of the Torizo's right foot.</summary>
    internal static TorizoProjectileSide Right { get; } = new(
        F.TorizoSonicBoomRightSmall, F.TorizoSonicBoomRightMedium, F.TorizoSonicBoomRightLarge, F.TorizoChozoOrbFacingRight,
        F.TorizoLandingDustRightDustPuff, F.TorizoLandingDustRightDustPuffSmall, F.TorizoLandingDustRightDustPuffThin, F.TorizoLandingDustRightDustPuffFaint,
        F.GoldenTorizoEggWholeRight, F.GoldenTorizoEggCrackedRight, F.GoldenTorizoEggSplittingRight, F.GoldenTorizoEggHatchingRight,
        F.GoldenTorizoHatchlingRightClawsRaised, F.GoldenTorizoHatchlingRightClawsLowered, F.GoldenTorizoHatchlingRightHunched,
        F.GoldenTorizoHatchlingRightCrumbling, F.GoldenTorizoHatchlingRightCollapsing, F.GoldenTorizoHatchlingRightSlumped,
        F.GoldenTorizoHatchlingRightRemains, F.GoldenTorizoHatchlingRightRemainsFlat,
        F.GoldenTorizoSuperMissilePointingDownRight, F.GoldenTorizoSuperMissilePointingRight, F.GoldenTorizoSuperMissilePointingUpRight,
        F.GoldenTorizoSuperMissilePointingUpLeft, F.GoldenTorizoSuperMissilePointingLeft, F.GoldenTorizoSuperMissilePointingDownLeft);
}
