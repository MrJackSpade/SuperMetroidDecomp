using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    private const ushort KiHunterAcidInitialLeftPreInstruction =
        EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_KiHunterAcid_Left;
    private const ushort KiHunterAcidInitialRightPreInstruction =
        EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_KiHunterAcid_Right;
    // Definition records contain $CFF7. The disassembly labels the first C instruction at
    // $CFF8 because $CFF7 is the native callable entry byte; preserve the stored pointer.
    private const ushort KiHunterAcidMovingPreInstruction =
        EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_KiHunterAcid_Moving;
    private const ushort KiHunterAcidFloorImpactInstruction = 0xcf56;
    private const ushort KiHunterAcidHorizontalSpeed = 0x0300;
    private const ushort KiHunterAcidGravity = 0x0010;
    private const ushort KiHunterAcidTerminalYSpeed = 0x0200;

    /// <summary>
    /// Ports <c>SpawnEprojWithGfx</c> plus definitions $86:CF18/$CF26 and initializers
    /// $86:CF90/$CFA6. The projectile retains the firing body's exact VRAM/palette words.
    /// </summary>
    private void SpawnKiHunterAcidSpit(RoomEnemySlot body, bool movingRight)
    {
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        RoomEnemyProjectileKind kind = movingRight
            ? RoomEnemyProjectileKind.KiHunterAcidSpitRight
            : RoomEnemyProjectileKind.KiHunterAcidSpitLeft;
        InitializeEnemyProjectileFromDefinition(
            projectile,
            kind,
            unchecked((ushort)(body.VramTilesIndex | body.PaletteIndex)));

        projectile.XVelocity = movingRight
            ? KiHunterAcidHorizontalSpeed
            : unchecked((ushort)-KiHunterAcidHorizontalSpeed);
        projectile.XPosition = unchecked((ushort)(body.XPosition + (movingRight ? 22 : -22)));
        projectile.YVelocity = 0;
        projectile.YPosition = unchecked((ushort)(body.YPosition - 16));
        projectile.XSubposition = 0;
        projectile.YSubposition = 0;
    }

    /// <summary>
    /// Ports one-shot pre-instructions $86:CFD5/$CFE6. The animation list installs one via
    /// common opcode $8161 after its opening map; the callback shifts nineteen pixels and
    /// then restores the header's $CFF7 mover.
    /// </summary>
    private static void StartKiHunterAcidMovement(
        RoomEnemyProjectileSlot projectile,
        bool movingRight)
    {
        projectile.PreInstruction = KiHunterAcidMovingPreInstruction;
        projectile.XPosition = unchecked((ushort)(
            projectile.XPosition + (movingRight ? 19 : -19)));
    }

    /// <summary>
    /// Ports $86:CFF8 exactly: move/test Y, then X, then add capped 8.8 gravity. A floor
    /// collision swaps to the cartridge splash list; a wall collision only zeros X speed.
    /// </summary>
    private void RunKiHunterAcidMovement(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (MoveProjectileAxis(projectile, level, horizontal: false))
        {
            projectile.InstructionPointer = KiHunterAcidFloorImpactInstruction;
            projectile.InstructionTimer = 1;
            return;
        }

        if (MoveProjectileAxis(projectile, level, horizontal: true))
        {
            projectile.XVelocity = 0;
            return;
        }

        ushort accelerated = unchecked((ushort)(projectile.YVelocity + KiHunterAcidGravity));
        projectile.YVelocity = unchecked((short)(accelerated - KiHunterAcidTerminalYSpeed)) >= 0
            ? KiHunterAcidTerminalYSpeed
            : accelerated;
    }
}
