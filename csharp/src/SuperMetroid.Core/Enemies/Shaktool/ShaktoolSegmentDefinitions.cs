namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Compiled mechanics and callback metadata for Shaktool's linked body records.</summary>
internal static class ShaktoolSegmentDefinitions
{
    /// <summary>
    /// The seven physical linked-segment definitions, primary saw first. Properties ($AA:DE95): the end saws have
    /// $2800, the five internal pieces $2C00. Owner distance ($AA:DEA3): how many slots each segment sits after the
    /// chain's first segment, its owner: its own index. The joints integrate symmetric increments 1,2,3,4,3,2 into
    /// steps 0,1,3,6,10,13,15: the initial neighbor angle ($AA:DEB1) is -2048 times the step, wrapped to a 16-bit turn,
    /// and the initial curling angle delta ($AA:DEE9) is 32 times it, the same curvature at 1/64 of the angle's
    /// magnitude. Initial program ($AA:DEBF): both saws their primary-piece list, the center segment the head-down
    /// list, the four arms the normal arm list. Layer ($AA:DECD): saws and head use layer 2, arms layer 4. Callback
    /// ($AA:DEDB): fixed anchor, head, final saw and ordinary arms have distinct roles.
    /// </summary>
    internal static readonly ShaktoolSegmentDefinition[] Segments =
    [
        new(EnemyProperties.ProcessInstructions | EnemyProperties.ProcessOffScreen, 0,
            RotationAngle.FromRaw(0), ShaktoolProgram.SawHandPrimaryPiece, 2,
            ShaktoolPreInstruction.IdleHead, 0),
        new(EnemyProperties.ProcessInstructions | EnemyProperties.ProcessOffScreen | EnemyProperties.IgnoreSamusCollision, 1,
            RotationAngle.NormalizeRaw(-2048 * 1), ShaktoolProgram.ArmPieceNormal, 4,
            ShaktoolPreInstruction.OrbitPreviousSegment, 32),
        new(EnemyProperties.ProcessInstructions | EnemyProperties.ProcessOffScreen | EnemyProperties.IgnoreSamusCollision, 2,
            RotationAngle.NormalizeRaw(-2048 * 3), ShaktoolProgram.ArmPieceNormal, 4,
            ShaktoolPreInstruction.OrbitPreviousSegment, 96),
        new(EnemyProperties.ProcessInstructions | EnemyProperties.ProcessOffScreen | EnemyProperties.IgnoreSamusCollision, 3,
            RotationAngle.NormalizeRaw(-2048 * 6), ShaktoolProgram.HeadAimingDown, 2,
            ShaktoolPreInstruction.OrbitAndOrientCenter, 192),
        new(EnemyProperties.ProcessInstructions | EnemyProperties.ProcessOffScreen | EnemyProperties.IgnoreSamusCollision, 4,
            RotationAngle.NormalizeRaw(-2048 * 10), ShaktoolProgram.ArmPieceNormal, 4,
            ShaktoolPreInstruction.OrbitPreviousSegment, 320),
        new(EnemyProperties.ProcessInstructions | EnemyProperties.ProcessOffScreen | EnemyProperties.IgnoreSamusCollision, 5,
            RotationAngle.NormalizeRaw(-2048 * 13), ShaktoolProgram.ArmPieceNormal, 4,
            ShaktoolPreInstruction.OrbitPreviousSegment, 416),
        new(EnemyProperties.ProcessInstructions | EnemyProperties.ProcessOffScreen, 6,
            RotationAngle.NormalizeRaw(-2048 * 15), ShaktoolProgram.SawHandPrimaryPiece, 2,
            ShaktoolPreInstruction.DriveTailAndReverseAtWalls, 480),
    ];
}
