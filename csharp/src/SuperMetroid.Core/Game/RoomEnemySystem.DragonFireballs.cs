namespace SuperMetroid.Core.Game;

/// <summary>Bank-$86 half of Dragon's arcing fireball projectile.</summary>
public sealed partial class RoomEnemySystem
{
    private const ushort DragonFireballInitialYVelocity = 0xfc3f;
    private const ushort DragonFireballLeftXVelocity = 0xfd40;
    private const ushort DragonFireballRightXVelocity = 0x02c0;
    private const ushort DragonFireballGravity = 0x0020;

    /// <summary>Ports projectile initializer $86:B4EF.</summary>
    private void SpawnDragonFireball(RoomEnemySlot body, DragonEnemyState bodyState)
    {
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.DragonFireball,
            unchecked((ushort)(body.PaletteIndex | body.VramTilesIndex)));
        projectile.YPosition = unchecked((ushort)(body.YPosition - 28));
        projectile.YSubposition = 0;
        projectile.YVelocity = DragonFireballInitialYVelocity;

        if (unchecked((short)bodyState.DirectionWord) < 0)
        {
            projectile.XPosition = unchecked((ushort)(body.XPosition - 12));
            projectile.XVelocity = DragonFireballLeftXVelocity;
            projectile.InstructionPointer =
                DragonFireballInstructionProgramDefinitions.RisingLeft;
        }
        else
        {
            projectile.XPosition = unchecked((ushort)(body.XPosition + 12));
            projectile.XVelocity = DragonFireballRightXVelocity;
            projectile.InstructionPointer =
                DragonFireballInstructionProgramDefinitions.RisingRight;
        }
        projectile.XSubposition = 0;
    }

    /// <summary>Ports projectile pre-instruction $86:B535.</summary>
    private static void RunDragonFireballPreInstruction(
        RoomEnemyProjectileSlot projectile,
        ushort cameraY)
    {
        (projectile.XPosition, projectile.XSubposition) = AddEightBitVelocity(
            projectile.XPosition,
            projectile.XSubposition,
            projectile.XVelocity);
        (projectile.YPosition, projectile.YSubposition) = AddEightBitVelocity(
            projectile.YPosition,
            projectile.YSubposition,
            projectile.YVelocity);

        if (unchecked((short)projectile.YVelocity) < 0)
        {
            projectile.YVelocity = unchecked((ushort)(
                projectile.YVelocity + DragonFireballGravity));
            if (unchecked((short)projectile.YVelocity) < 0)
                return;

            // The exact zero crossing switches from the rising pair of two-frame loops to
            // the falling pair and forces an instruction tick on this same projectile pass.
            projectile.InstructionPointer = unchecked((short)projectile.XVelocity) < 0
                ? DragonFireballInstructionProgramDefinitions.FallingLeft
                : DragonFireballInstructionProgramDefinitions.FallingRight;
            projectile.InstructionTimer = 1;
            return;
        }

        projectile.YVelocity = unchecked((ushort)(
            projectile.YVelocity + DragonFireballGravity));

        // $86:B5B9 retains fireballs above the viewport; horizontal distance is never consulted.
        DeleteEnemyProjectileIfVerticallyOffScreen(projectile, cameraY);
    }
}
