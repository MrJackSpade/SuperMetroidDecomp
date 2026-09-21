namespace SuperMetroid.Core.Game;

/// <summary>Named bank-$86 animation-list pointers assigned to translated enemy projectiles.</summary>
internal static class EnemyProjectileInstructionLists
{
    /// <summary>Bomb Torizo drool wall-impact list at $86:A48A.</summary>
    public const ushort BombTorizoDroolWallImpact = 0xa48a;
    /// <summary>Bomb Torizo drool floor-impact list at $86:A48E.</summary>
    public const ushort BombTorizoDroolFloorImpact = 0xa48e;
    /// <summary>Bomb Torizo Chozo-orb wall-impact list at $86:AB25.</summary>
    public const ushort BombTorizoOrbWallImpact = 0xab25;
    /// <summary>Shared Torizo Chozo-orb floor-impact list at $86:AB41.</summary>
    public const ushort TorizoOrbFloorImpact = 0xab41;
    /// <summary>Bomb Torizo sonic-boom list facing left at $86:ADBF.</summary>
    public const ushort BombTorizoSonicBoomLeft = 0xadbf;
    /// <summary>Bomb Torizo sonic-boom list facing right at $86:ADD2.</summary>
    public const ushort BombTorizoSonicBoomRight = 0xadd2;
    /// <summary>Bomb Torizo sonic-boom impact list at $86:ADE5.</summary>
    public const ushort BombTorizoSonicBoomImpact = 0xade5;
    /// <summary>Golden Torizo egg hatched list facing left at $86:B190.</summary>
    public const ushort GoldenTorizoEggHatchedLeft = 0xb190;
    /// <summary>Golden Torizo egg hatched list facing right at $86:B1A8.</summary>
    public const ushort GoldenTorizoEggHatchedRight = 0xb1a8;
    /// <summary>Golden Torizo egg hatch branch target facing left at $86:B14B.</summary>
    public const ushort GoldenTorizoEggHatchTargetLeft = 0xb14b;
    /// <summary>Golden Torizo egg hatch branch target facing right at $86:B166.</summary>
    public const ushort GoldenTorizoEggHatchTargetRight = 0xb166;
    /// <summary>Golden Torizo super-missile impact list at $86:B2EF.</summary>
    public const ushort GoldenTorizoSuperMissileImpact = 0xb2ef;
    /// <summary>Golden Torizo eye-beam wall-impact list at $86:B3CD.</summary>
    public const ushort GoldenTorizoEyeBeamWallImpact = 0xb3cd;
    /// <summary>Golden Torizo eye-beam floor-impact list at $86:B3E5.</summary>
    public const ushort GoldenTorizoEyeBeamFloorImpact = 0xb3e5;
    /// <summary>Mother Brain falling-drool list at $86:C8E1.</summary>
    public const ushort MotherBrainDroolFalling =
        EnemyProjectileInstructionMechanicsDefinitions.MotherBrainDroolFalling;
}
