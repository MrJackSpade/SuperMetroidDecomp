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
        projectile.Position = projectile.Position.WithY(new(unchecked((ushort)(body.Position.Y.Whole - 28)), 0));
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(DragonFireballInitialYVelocity));

        if (unchecked((short)bodyState.DirectionWord) < 0)
        {
            projectile.Position = projectile.Position.WithWholeX(unchecked((ushort)(body.Position.X.Whole - 12)));
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(DragonFireballLeftXVelocity));
            projectile.InstructionPointer =
                DragonFireballInstructionProgramDefinitions.RisingLeft;
        }
        else
        {
            projectile.Position = projectile.Position.WithWholeX(unchecked((ushort)(body.Position.X.Whole + 12)));
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(DragonFireballRightXVelocity));
            projectile.InstructionPointer =
                DragonFireballInstructionProgramDefinitions.RisingRight;
        }
        projectile.Position = projectile.Position.WithFractionX(0);
    }

    /// <summary>Ports projectile pre-instruction $86:B535.</summary>
    private static void RunDragonFireballPreInstruction(
        RoomEnemyProjectileSlot projectile,
        ushort cameraY)
    {
        projectile.Position = projectile.Position.Advance(projectile.Velocity.ToSixteenSixteen());

        if (unchecked((short)projectile.Velocity.Y.RawValue) < 0)
        {
            projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(
                projectile.Velocity.Y.RawValue + DragonFireballGravity))));
            if (unchecked((short)projectile.Velocity.Y.RawValue) < 0)
                return;

            // The exact zero crossing switches from the rising pair of two-frame loops to
            // the falling pair and forces an instruction tick on this same projectile pass.
            projectile.InstructionPointer = unchecked((short)projectile.Velocity.X.RawValue) < 0
                ? DragonFireballInstructionProgramDefinitions.FallingLeft
                : DragonFireballInstructionProgramDefinitions.FallingRight;
            projectile.InstructionTimer = 1;
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(
            projectile.Velocity.Y.RawValue + DragonFireballGravity))));

        // $86:B5B9 retains fireballs above the viewport; horizontal distance is never consulted.
        DeleteEnemyProjectileIfVerticallyOffScreen(projectile, cameraY);
    }
}
