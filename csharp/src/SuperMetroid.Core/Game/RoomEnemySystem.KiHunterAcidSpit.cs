using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    // Definition records hold the idle RTS at $86:CFF7; the start-moving callbacks install
    // the mover at $86:CFF8, so the spit does not move on the frames before that.
    private const ushort KiHunterAcidMovingPreInstruction =
        (ushort)EnemyProjectilePreInstruction.KiHunterAcid_Moving;
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

        projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(movingRight
            ? KiHunterAcidHorizontalSpeed
            : unchecked((ushort)-KiHunterAcidHorizontalSpeed)));
        projectile.Position = projectile.Position.WithWholeX(unchecked((ushort)(body.Position.X.Whole + (movingRight ? 22 : -22))));
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(0));
        projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole - 16))).WithFractionX(0).WithFractionY(0);
    }

    /// <summary>
    /// Ports one-shot pre-instructions $86:CFD5/$CFE6. The animation list installs one via
    /// common opcode $8161 after its opening map; the callback shifts nineteen pixels and
    /// then installs the $CFF8 mover.
    /// </summary>
    private static void StartKiHunterAcidMovement(
        RoomEnemyProjectileSlot projectile,
        bool movingRight)
    {
        projectile.PreInstruction = KiHunterAcidMovingPreInstruction;
        projectile.Position = projectile.Position.WithWholeX(unchecked((ushort)(
            projectile.Position.X.Whole + (movingRight ? 19 : -19))));
    }

    /// <summary>
    /// Ports $86:CFF8 exactly: move/test Y, then X, then add capped 8.8 gravity. A floor
    /// collision swaps to the cartridge splash list; a wall collision only zeros X speed.
    /// </summary>
    private static void RunKiHunterAcidMovement(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (MoveProjectileAxis(projectile, level, horizontal: false))
        {
            projectile.InstructionPointer = KiHunterAcidSpitInstructionProgramDefinitions.HitFloor;
            projectile.InstructionTimer = 1;
            return;
        }

        if (MoveProjectileAxis(projectile, level, horizontal: true))
        {
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(0));
            return;
        }

        ushort accelerated = unchecked((ushort)(projectile.Velocity.Y.RawValue + KiHunterAcidGravity));
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((short)(accelerated - KiHunterAcidTerminalYSpeed)) >= 0
            ? KiHunterAcidTerminalYSpeed
            : accelerated));
    }
}
