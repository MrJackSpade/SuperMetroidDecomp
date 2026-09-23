namespace SuperMetroid.Core.Game;

/// <summary>
/// Named bank-$A4 code pointers consumed by translated Crocomire dispatchers.
/// The thirteen dust-projectile callbacks at $9A9B+5*i, i=0..12,
/// load signed X offsets -32, 0, -16, 16 for i=0..3 and
/// 8*(i-4) pixels for i=4..12. Pinned NTSC J/U v1.0 ROM uses
/// LDA-immediate then BRA-to-$9ADA stubs for i=0..11; the final
/// $9AD7 stub loads $0040 and falls through to the shared spawn
/// routine. The runtime switch accepts only these named opcodes;
/// the first four authored offsets make the switch worth retaining.
/// </summary>
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

    /// <summary><c>Instruction_Crocomire_SpawnBigDustCloudProjectile_Negative20</c>
    /// at $A4:9A9B. Its native signed immediate is -$20 (-32 pixels),
    /// the first authored exception in the thirteen-opcode family.</summary>
    public const ushort Instruction_Crocomire_SpawnBigDustCloudProjectile_Negative20 = 0x9a9b;

    /// <summary><c>Instruction_Crocomire_SpawnBigDustCloudProjectile_0</c> at $A4:9AA0.</summary>
    public const ushort Instruction_Crocomire_SpawnBigDustCloudProjectile_0 = 0x9aa0;

    /// <summary><c>Instruction_Crocomire_SpawnBigDustCloudProjectile_Negative10</c> at $A4:9AA5.</summary>
    public const ushort Instruction_Crocomire_SpawnBigDustCloudProjectile_Negative10 = 0x9aa5;

    /// <summary><c>Instruction_Crocomire_SpawnBigDustCloudProjectile_10</c> at $A4:9AAA.</summary>
    public const ushort Instruction_Crocomire_SpawnBigDustCloudProjectile_10 = 0x9aaa;

    /// <summary><c>Instruction_Crocomire_SpawnBigDustCloudProjectile_0_dup</c>
    /// at $A4:9AAF. This begins the bounded nine-opcode run
    /// $9AAF+5*j with native X offset 8*j, j=0..8.</summary>
    public const ushort Instruction_Crocomire_SpawnBigDustCloudProjectile_0_dup = 0x9aaf;

    /// <summary><c>Instruction_Crocomire_SpawnBigDustCloudProjectile_8</c> at $A4:9AB4.</summary>
    public const ushort Instruction_Crocomire_SpawnBigDustCloudProjectile_8 = 0x9ab4;

    /// <summary><c>Instruction_Crocomire_SpawnBigDustCloudProjectile_10_dup</c> at $A4:9AB9.</summary>
    public const ushort Instruction_Crocomire_SpawnBigDustCloudProjectile_10_dup = 0x9ab9;

    /// <summary><c>Instruction_Crocomire_SpawnBigDustCloudProjectile_18</c> at $A4:9ABE.</summary>
    public const ushort Instruction_Crocomire_SpawnBigDustCloudProjectile_18 = 0x9abe;

    /// <summary><c>Instruction_Crocomire_SpawnBigDustCloudProjectile_20</c> at $A4:9AC3.</summary>
    public const ushort Instruction_Crocomire_SpawnBigDustCloudProjectile_20 = 0x9ac3;

    /// <summary><c>Instruction_Crocomire_SpawnBigDustCloudProjectile_28</c> at $A4:9AC8.</summary>
    public const ushort Instruction_Crocomire_SpawnBigDustCloudProjectile_28 = 0x9ac8;

    /// <summary><c>Instruction_Crocomire_SpawnBigDustCloudProjectile_30</c> at $A4:9ACD.</summary>
    public const ushort Instruction_Crocomire_SpawnBigDustCloudProjectile_30 = 0x9acd;

    /// <summary><c>Instruction_Crocomire_SpawnBigDustCloudProjectile_38</c> at $A4:9AD2.</summary>
    public const ushort Instruction_Crocomire_SpawnBigDustCloudProjectile_38 = 0x9ad2;

    /// <summary><c>Instruction_Crocomire_SpawnBigDustCloudProjectile_40</c>
    /// at $A4:9AD7. Native X offset is +$40 (+64 pixels); this
    /// final stub falls through into shared routine $9ADA.</summary>
    public const ushort Instruction_Crocomire_SpawnBigDustCloudProjectile_40 = 0x9ad7;

}
