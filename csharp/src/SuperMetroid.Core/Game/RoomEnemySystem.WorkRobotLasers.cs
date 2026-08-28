using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    private const ushort WorkRobotLaserPreInstruction = 0xd3bf;
    private const ushort WorkRobotLaserInstructionList = 0xd2ec;
    private const ushort WorkRobotLaserSound = 0x0067;
    private const ushort WorkRobotLaserInvincibilityFrames = 96;

    /// <summary>
    /// Allocates one of bank $86's five Work Robot laser definitions. Horizontal left/right
    /// share $D2B4; the sign in the owner's laser velocity supplies the sixth direction.
    /// </summary>
    private void SpawnWorkRobotLaser(
        RoomEnemySlot robot,
        WorkRobotEnemyState state,
        ushort definition,
        ushort cameraX,
        ushort cameraY)
    {
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        projectile.Kind = (RoomEnemyProjectileKind)definition;
        projectile.XVelocity = state.LaserXVelocity;
        projectile.YVelocity = definition switch
        {
            WorkRobotLaserUpLeft or WorkRobotLaserUpRight => unchecked((ushort)-0x0080),
            WorkRobotLaserDownLeft or WorkRobotLaserDownRight => 0x0080,
            WorkRobotLaserHorizontal => 0,
            _ => throw new InvalidDataException(
                $"Work Robot laser definition $86:{definition:X4} is not translated."),
        };

        // All five initializers place the muzzle four pixels in the signed facing direction
        // and sixteen pixels above the robot. The temporary graphics word comes from the
        // owner; pre-instruction $D3BF clears it before the first draw, matching the ROM's
        // use of palette/tile bits embedded directly in the bank-$8D laser spritemaps.
        projectile.XPosition = unchecked((ushort)(robot.XPosition +
            (unchecked((short)state.LaserXVelocity) < 0 ? -4 : 4)));
        projectile.YPosition = unchecked((ushort)(robot.YPosition - 16));
        projectile.XSubposition = 0;
        projectile.YSubposition = 0;
        projectile.InstructionPointer = WorkRobotLaserInstructionList;
        projectile.InstructionTimer = 1;
        projectile.PreInstruction = WorkRobotLaserPreInstruction;
        projectile.GraphicsIndex = unchecked((ushort)(robot.VramTilesIndex | robot.PaletteIndex));

        bool horizontal = definition == WorkRobotLaserHorizontal;
        projectile.XRadius = horizontal ? (ushort)15 : (ushort)12;
        projectile.YRadius = horizontal ? (ushort)2 : (ushort)12;
        projectile.Damage = horizontal ? (ushort)0x0014 : (ushort)0x0004;
        projectile.InvincibilityFrames = WorkRobotLaserInvincibilityFrames;
        projectile.CanDamageSamus = true;

        // Preserve the original $86:D35B viewport bug. Its final Y comparison omits
        // `CMP Layer1YPosition`, so lasers only request sound while their owner's absolute
        // top edge is above scanline 224, even when a lower room row is on screen.
        bool horizontallyVisible =
            unchecked((short)(robot.XPosition + robot.XRadius - cameraX)) >= 0 &&
            unchecked((short)(robot.XPosition - robot.XRadius - 0x0101 - cameraX)) < 0;
        bool verticallyVisible =
            unchecked((short)(robot.YPosition + robot.YRadius - cameraY)) >= 0 &&
            unchecked((short)(robot.YPosition - robot.YRadius - 0x00e0)) < 0;
        if (horizontallyVisible && verticallyVisible)
            LastWorkRobotSoundEffect = WorkRobotLaserSound;
    }

    /// <summary>Ports pre-instruction $86:D3BF: X collision, then Y collision, else move.</summary>
    private void RunWorkRobotLaserPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        projectile.GraphicsIndex = 0;
        if (MoveProjectileAxis(projectile, level, horizontal: true) ||
            MoveProjectileAxis(projectile, level, horizontal: false))
        {
            projectile.Clear();
        }
    }
}
