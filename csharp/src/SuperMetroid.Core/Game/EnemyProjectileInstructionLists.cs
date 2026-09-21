namespace SuperMetroid.Core.Game;

/// <summary>Named bank-$86 animation-list pointers assigned to translated enemy projectiles.</summary>
internal static class EnemyProjectileInstructionLists
{
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
