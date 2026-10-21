namespace SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.World;

/// <summary>Cartridge values of <see cref="RoomEnemySystem"/> that only verification reads.</summary>
internal static class RoomEnemySystemConstants
{
    /// <summary>Bank-$A8 palette table at $E7CC for Blue Brinstar's face-block actor.</summary>
    public const int BlueBrinstarFaceBlockPaletteTable = 0xa8e7cc;
    /// <summary>Bank-$AA instruction pointer at $B879 used on Bomb Torizo initialization.</summary>
    public const ushort BombTorizoInitialInstruction = 0xb879;
    /// <summary>Pre-instruction selected for the Cacatac spike enemy projectile.</summary>
    public const EnemyProjectilePreInstruction CacatacSpikePreInstruction = EnemyProjectilePreInstruction.CacatacSpike;
    /// <summary>Initial leftward Ki Hunter acid pre-instruction pointer.</summary>
    public const EnemyProjectilePreInstruction KiHunterAcidInitialLeftPreInstruction = EnemyProjectilePreInstruction.KiHunterAcid_Left;
    /// <summary>Initial rightward Ki Hunter acid pre-instruction pointer.</summary>
    public const EnemyProjectilePreInstruction KiHunterAcidInitialRightPreInstruction = EnemyProjectilePreInstruction.KiHunterAcid_Right;
    /// <summary>Normal Ninja Pirate OBJ palette slot value before its alternate coloring.</summary>
    public const ushort NinjaPiratePaletteNormal = 0x0200;
    /// <summary>Sound effect queued for the Ninja Pirate's claw kick or dive.</summary>
    public const ushort NinjaPirateSoundClawKickOrDive = 0x0066;
    /// <summary>Pre-instruction selected for Stoke's fireball projectile.</summary>
    public const EnemyProjectilePreInstruction StokeProjectilePreInstruction = EnemyProjectilePreInstruction.StokeFireball;

    /// <summary>Provides native room-enemy AI, instruction, palette, sound, and hitbox references for verification.</summary>
    extension(RoomEnemySystem)
    {
        /// <inheritdoc cref="RoomEnemySystemConstants.BlueBrinstarFaceBlockPaletteTable"/>
        internal static int BlueBrinstarFaceBlockPaletteTable => RoomEnemySystemConstants.BlueBrinstarFaceBlockPaletteTable;
        /// <inheritdoc cref="RoomEnemySystemConstants.BombTorizoInitialInstruction"/>
        internal static ushort BombTorizoInitialInstruction => RoomEnemySystemConstants.BombTorizoInitialInstruction;
        /// <inheritdoc cref="RoomEnemySystemConstants.CacatacSpikePreInstruction"/>
        internal static EnemyProjectilePreInstruction CacatacSpikePreInstruction => RoomEnemySystemConstants.CacatacSpikePreInstruction;
        /// <inheritdoc cref="RoomEnemySystemConstants.KiHunterAcidInitialLeftPreInstruction"/>
        internal static EnemyProjectilePreInstruction KiHunterAcidInitialLeftPreInstruction => RoomEnemySystemConstants.KiHunterAcidInitialLeftPreInstruction;
        /// <inheritdoc cref="RoomEnemySystemConstants.KiHunterAcidInitialRightPreInstruction"/>
        internal static EnemyProjectilePreInstruction KiHunterAcidInitialRightPreInstruction => RoomEnemySystemConstants.KiHunterAcidInitialRightPreInstruction;
        /// <inheritdoc cref="RoomEnemySystemConstants.NinjaPiratePaletteNormal"/>
        internal static ushort NinjaPiratePaletteNormal => RoomEnemySystemConstants.NinjaPiratePaletteNormal;
        /// <inheritdoc cref="RoomEnemySystemConstants.NinjaPirateSoundClawKickOrDive"/>
        internal static ushort NinjaPirateSoundClawKickOrDive => RoomEnemySystemConstants.NinjaPirateSoundClawKickOrDive;
        /// <inheritdoc cref="RoomEnemySystemConstants.StokeProjectilePreInstruction"/>
        internal static EnemyProjectilePreInstruction StokeProjectilePreInstruction => RoomEnemySystemConstants.StokeProjectilePreInstruction;
    }
}
