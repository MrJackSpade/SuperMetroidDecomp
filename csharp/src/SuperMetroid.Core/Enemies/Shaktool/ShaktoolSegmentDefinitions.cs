namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Compiled mechanics and callback metadata for Shaktool's linked body records.</summary>
internal static class ShaktoolSegmentDefinitions
{
    /// <summary>
    /// The seven physical linked-segment definitions, primary saw first. Properties: the end saws have
    /// the animation-script and process-off-screen properties, the five internal pieces also intangibility. Owner distance: how many slots each segment sits after the
    /// chain's first segment, its owner: its own index. The joints integrate symmetric increments 1,2,3,4,3,2 into
    /// steps 0,1,3,6,10,13,15: the initial neighbor angle is -2048 times the step, wrapped to a 16-bit turn,
    /// and the initial curling angle delta is 32 times it, the same curvature at 1/64 of the angle's
    /// magnitude. Initial program: both saws their primary-piece list, the center segment the head-down
    /// list, the four arms the normal arm list. Layer: saws and head use layer 2, arms layer 4. Callback
    /// by part: fixed anchor, head, final saw and ordinary arms have distinct roles.
    /// </summary>
    internal static readonly ShaktoolSegmentDefinition[] Segments =
    [
        new(EnemyProperties.RunsAnimationScript | EnemyProperties.ProcessOffScreen, 0,
            RotationAngle.FromRaw(0), ShaktoolProgram.SawHandPrimaryPiece, EnemyDrawLayer.Of(2),
            ShaktoolFrameBehavior.Idle, 0),
        new(EnemyProperties.RunsAnimationScript | EnemyProperties.ProcessOffScreen | EnemyProperties.IgnoreSamusCollision, 1,
            RotationAngle.NormalizeRaw(-2048 * 1), ShaktoolProgram.ArmPieceNormal, EnemyDrawLayer.Of(4),
            ShaktoolFrameBehavior.OrbitPreviousSegment, 32),
        new(EnemyProperties.RunsAnimationScript | EnemyProperties.ProcessOffScreen | EnemyProperties.IgnoreSamusCollision, 2,
            RotationAngle.NormalizeRaw(-2048 * 3), ShaktoolProgram.ArmPieceNormal, EnemyDrawLayer.Of(4),
            ShaktoolFrameBehavior.OrbitPreviousSegment, 96),
        new(EnemyProperties.RunsAnimationScript | EnemyProperties.ProcessOffScreen | EnemyProperties.IgnoreSamusCollision, 3,
            RotationAngle.NormalizeRaw(-2048 * 6), ShaktoolProgram.HeadAimingDown, EnemyDrawLayer.Of(2),
            ShaktoolFrameBehavior.OrbitAndOrientCenter, 192),
        new(EnemyProperties.RunsAnimationScript | EnemyProperties.ProcessOffScreen | EnemyProperties.IgnoreSamusCollision, 4,
            RotationAngle.NormalizeRaw(-2048 * 10), ShaktoolProgram.ArmPieceNormal, EnemyDrawLayer.Of(4),
            ShaktoolFrameBehavior.OrbitPreviousSegment, 320),
        new(EnemyProperties.RunsAnimationScript | EnemyProperties.ProcessOffScreen | EnemyProperties.IgnoreSamusCollision, 5,
            RotationAngle.NormalizeRaw(-2048 * 13), ShaktoolProgram.ArmPieceNormal, EnemyDrawLayer.Of(4),
            ShaktoolFrameBehavior.OrbitPreviousSegment, 416),
        new(EnemyProperties.RunsAnimationScript | EnemyProperties.ProcessOffScreen, 6,
            RotationAngle.NormalizeRaw(-2048 * 15), ShaktoolProgram.SawHandPrimaryPiece, EnemyDrawLayer.Of(2),
            ShaktoolFrameBehavior.DriveTailAndReverseAtWalls, 480),
    ];
}
