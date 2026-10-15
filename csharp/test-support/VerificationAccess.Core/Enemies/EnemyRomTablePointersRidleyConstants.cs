
namespace SuperMetroid.Core.Enemies;

/// <summary>Cartridge values of <see cref="EnemyRomTablePointers.Ridley"/> that only verification reads.</summary>
internal static class EnemyRomTablePointersRidleyConstants
{
    /// <summary>Ascending pogo target-X words at $A6:B63B.</summary>
    public const int AscendingPogoTargetXWords = 0xa6b63b;
    /// <summary>Carry-anchor X-position words selected by facing at $A6:BBEB.</summary>
    public const int CarryAnchorXWords = 0xa6bbeb;
    /// <summary>Carry-release X-position words selected by facing at $A6:BC62.</summary>
    public const int CarryReleaseXWords = 0xa6bc62;
    /// <summary>Claw X-offset words selected by facing at $A6:B9D5.</summary>
    public const int ClawXOffsetWords = 0xa6b9d5;
    /// <summary>Claw Y-offset words selected by foot separation at $A6:B9DB.</summary>
    public const int ClawYOffsetWords = 0xa6b9db;
    /// <summary>Descending pogo target-X words at $A6:B60D.</summary>
    public const int DescendingPogoTargetXWords = 0xa6b60d;
    /// <summary>Ground-attack target-X words at $A6:B6C8.</summary>
    public const int GroundAttackTargetXWords = 0xa6b6c8;
    /// <summary>Health-stage movement-divisor indexes at $A6:BB4E.</summary>
    public const int HealthMovementDivisorIndexWords = 0xa6bb4e;
    /// <summary>Three fourteen-color health-palette records at $A6:E46A (84 bytes).</summary>
    public const int HealthPaletteWords = 0xa6e46a;
    /// <summary>Four hover/pogo health-stage acceleration-divisor indexes at $A6:B439.</summary>
    public const int HoverMovementDivisorIndexWords = 0xa6b439;
    /// <summary>Pogo downward acceleration words at $A6:B959.</summary>
    public const int PogoDownwardAccelerationWords = 0xa6b959;
    /// <summary>Pogo horizontal-path pointer words at $A6:B965.</summary>
    public const int PogoHorizontalPathPointers = 0xa6b965;
    /// <summary>Pogo upward acceleration words at $A6:B94D.</summary>
    public const int PogoUpwardAccelerationWords = 0xa6b94d;
    /// <summary>Pogo vertical-path pointer words at $A6:B96D.</summary>
    public const int PogoVerticalPathPointers = 0xa6b96d;
    /// <summary>Tail rotation divisor bytes at $A6:D61F.</summary>
    public const int TailRotationDivisorBytes = 0xa6d61f;

    /// <summary>Provides Ridley's native pogo, hover, carry, claw, health, and palette table pointers for behavior checks.</summary>
    extension(EnemyRomTablePointers.Ridley)
    {
        /// <inheritdoc cref="EnemyRomTablePointersRidleyConstants.AscendingPogoTargetXWords"/>
        internal static int AscendingPogoTargetXWords => EnemyRomTablePointersRidleyConstants.AscendingPogoTargetXWords;
        /// <inheritdoc cref="EnemyRomTablePointersRidleyConstants.CarryAnchorXWords"/>
        internal static int CarryAnchorXWords => EnemyRomTablePointersRidleyConstants.CarryAnchorXWords;
        /// <inheritdoc cref="EnemyRomTablePointersRidleyConstants.CarryReleaseXWords"/>
        internal static int CarryReleaseXWords => EnemyRomTablePointersRidleyConstants.CarryReleaseXWords;
        /// <inheritdoc cref="EnemyRomTablePointersRidleyConstants.ClawXOffsetWords"/>
        internal static int ClawXOffsetWords => EnemyRomTablePointersRidleyConstants.ClawXOffsetWords;
        /// <inheritdoc cref="EnemyRomTablePointersRidleyConstants.ClawYOffsetWords"/>
        internal static int ClawYOffsetWords => EnemyRomTablePointersRidleyConstants.ClawYOffsetWords;
        /// <inheritdoc cref="EnemyRomTablePointersRidleyConstants.DescendingPogoTargetXWords"/>
        internal static int DescendingPogoTargetXWords => EnemyRomTablePointersRidleyConstants.DescendingPogoTargetXWords;
        /// <inheritdoc cref="EnemyRomTablePointersRidleyConstants.GroundAttackTargetXWords"/>
        internal static int GroundAttackTargetXWords => EnemyRomTablePointersRidleyConstants.GroundAttackTargetXWords;
        /// <inheritdoc cref="EnemyRomTablePointersRidleyConstants.HealthMovementDivisorIndexWords"/>
        internal static int HealthMovementDivisorIndexWords => EnemyRomTablePointersRidleyConstants.HealthMovementDivisorIndexWords;
        /// <inheritdoc cref="EnemyRomTablePointersRidleyConstants.HealthPaletteWords"/>
        internal static int HealthPaletteWords => EnemyRomTablePointersRidleyConstants.HealthPaletteWords;
        /// <inheritdoc cref="EnemyRomTablePointersRidleyConstants.HoverMovementDivisorIndexWords"/>
        internal static int HoverMovementDivisorIndexWords => EnemyRomTablePointersRidleyConstants.HoverMovementDivisorIndexWords;
        /// <inheritdoc cref="EnemyRomTablePointersRidleyConstants.PogoDownwardAccelerationWords"/>
        internal static int PogoDownwardAccelerationWords => EnemyRomTablePointersRidleyConstants.PogoDownwardAccelerationWords;
        /// <inheritdoc cref="EnemyRomTablePointersRidleyConstants.PogoHorizontalPathPointers"/>
        internal static int PogoHorizontalPathPointers => EnemyRomTablePointersRidleyConstants.PogoHorizontalPathPointers;
        /// <inheritdoc cref="EnemyRomTablePointersRidleyConstants.PogoUpwardAccelerationWords"/>
        internal static int PogoUpwardAccelerationWords => EnemyRomTablePointersRidleyConstants.PogoUpwardAccelerationWords;
        /// <inheritdoc cref="EnemyRomTablePointersRidleyConstants.PogoVerticalPathPointers"/>
        internal static int PogoVerticalPathPointers => EnemyRomTablePointersRidleyConstants.PogoVerticalPathPointers;
        /// <inheritdoc cref="EnemyRomTablePointersRidleyConstants.TailRotationDivisorBytes"/>
        internal static int TailRotationDivisorBytes => EnemyRomTablePointersRidleyConstants.TailRotationDivisorBytes;
    }
}
