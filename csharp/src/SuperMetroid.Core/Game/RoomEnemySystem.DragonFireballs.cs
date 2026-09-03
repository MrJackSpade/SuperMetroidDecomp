namespace SuperMetroid.Core.Game;

/// <summary>Bank-$86 half of Dragon's arcing fireball projectile.</summary>
public sealed partial class RoomEnemySystem
{
    private const ushort DragonFireballRisingLeftInstruction = 0xb4bf;
    private const ushort DragonFireballRisingRightInstruction = 0xb4cb;
    private const ushort DragonFireballFallingLeftInstruction = 0xb4d7;
    private const ushort DragonFireballFallingRightInstruction = 0xb4e3;
    private const ushort DragonFireballPreInstruction =
        EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_DragonFireball;
    private const ushort DragonFireballInitialYVelocity = 0xfc3f;
    private const ushort DragonFireballLeftXVelocity = 0xfd40;
    private const ushort DragonFireballRightXVelocity = 0x02c0;
    private const ushort DragonFireballGravity = 0x0020;
    private const ushort DragonFireballBottomCull = 0x0120;

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
            projectile.InstructionPointer = DragonFireballRisingLeftInstruction;
        }
        else
        {
            projectile.XPosition = unchecked((ushort)(body.XPosition + 12));
            projectile.XVelocity = DragonFireballRightXVelocity;
            projectile.InstructionPointer = DragonFireballRisingRightInstruction;
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
                ? DragonFireballFallingLeftInstruction
                : DragonFireballFallingRightInstruction;
            projectile.InstructionTimer = 1;
            return;
        }

        projectile.YVelocity = unchecked((ushort)(
            projectile.YVelocity + DragonFireballGravity));

        // $86:B5B9 intentionally retains fireballs above the viewport. Only a descending
        // origin at or below camera+288 is deleted; horizontal distance is never consulted.
        ushort screenY = unchecked((ushort)(projectile.YPosition - cameraY));
        if (unchecked((short)screenY) >= 0 && screenY >= DragonFireballBottomCull)
            projectile.Clear();
    }
}
