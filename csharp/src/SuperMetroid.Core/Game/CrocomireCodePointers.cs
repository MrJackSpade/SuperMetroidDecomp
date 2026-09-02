namespace SuperMetroid.Core.Game;

/// <summary>Named bank-$A4 code pointers consumed by translated Crocomire dispatchers.</summary>
internal static class CrocomireCodePointers
{
    /// <summary><c>Instruction_Crocomire_FightAI</c> at $A4:86A6.</summary>
    public const ushort Instruction_Crocomire_FightAI = 0x86a6;

    /// <summary><c>Instruction_Crocomire_MaybeStartProjectileAttack</c> at $A4:8752.</summary>
    public const ushort Instruction_Crocomire_MaybeStartProjectileAttack = 0x8752;

    /// <summary><c>Instruction_Crocomire_QueueCrySFX</c> at $A4:8CFB.</summary>
    public const ushort Instruction_Crocomire_QueueCrySFX = 0x8cfb;

    /// <summary><c>Instruction_Crocomire_QueueBigExplosionSFX</c> at $A4:8D07.</summary>
    public const ushort Instruction_Crocomire_QueueBigExplosionSFX = 0x8d07;

    /// <summary><c>Instruction_Crocomire_QueueSkeletonCollapseSFX</c> at $A4:8D13.</summary>
    public const ushort Instruction_Crocomire_QueueSkeletonCollapseSFX = 0x8d13;

    /// <summary><c>Instruction_Crocomire_ShakeScreen</c> at $A4:8FC7.</summary>
    public const ushort Instruction_Crocomire_ShakeScreen = 0x8fc7;

    /// <summary><c>Instruction_Crocomire_MoveLeft4Pixels</c> at $A4:8FDF.</summary>
    public const ushort Instruction_Crocomire_MoveLeft4Pixels = 0x8fdf;

    /// <summary><c>Instruction_Crocomire_MoveLeft4Pixels_SpawnBigDustCloud</c> at $A4:8FFA.</summary>
    public const ushort Instruction_Crocomire_MoveLeft4Pixels_SpawnBigDustCloud = 0x8ffa;

    /// <summary><c>Instruction_Crocomire_MoveLeft4Pixels_SpawnBigDustCloud_dup</c> at $A4:8FFF.</summary>
    public const ushort Instruction_Crocomire_MoveLeft4Pixels_SpawnBigDustCloud_dup = 0x8fff;

    /// <summary><c>Instruction_Crocomire_MoveLeft_SpawnCloud_HandleSpikeWall</c> at $A4:901D.</summary>
    public const ushort Instruction_Crocomire_MoveLeft_SpawnCloud_HandleSpikeWall = 0x901d;

    /// <summary><c>Instruction_Crocomire_MoveRight4PixelsIfOnScreen</c> at $A4:905B.</summary>
    public const ushort Instruction_Crocomire_MoveRight4PixelsIfOnScreen = 0x905b;

    /// <summary><c>Instruction_Crocomire_MoveRight4Pixels</c> at $A4:907F.</summary>
    public const ushort Instruction_Crocomire_MoveRight4Pixels = 0x907f;

    /// <summary><c>Instruction_Crocomire_MoveRight4PixelsIfOnScreen_SpawnCloud</c> at $A4:908F.</summary>
    public const ushort Instruction_Crocomire_MoveRight4PixelsIfOnScreen_SpawnCloud = 0x908f;

    /// <summary><c>Instruction_Crocomire_MoveRight4Pixels_SpawnBigDustCloud</c> at $A4:9094.</summary>
    public const ushort Instruction_Crocomire_MoveRight4Pixels_SpawnBigDustCloud = 0x9094;

}
