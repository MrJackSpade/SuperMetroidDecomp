using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$86 support for Alcoon projectile definition <c>$9E90</c>. Keeping the projectile
/// beside its producing enemy makes the three animation commands easy to follow while the
/// shared fixed-slot pool, instruction interpreter, collision, and renderer remain common.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const ushort AlcoonFireballGraphicsHorizontalSpeed = 0x0400;
    private const ushort AlcoonFireballTerminalHorizontalSpeed = 0x0200;
    private const ushort AlcoonFireballHorizontalDeceleration = 0x0040;

    /// <summary>
    /// Ports <c>InitAI_EnemyProjectile_AlcoonFireball</c> at <c>$86:9EB2</c>. The parameter
    /// is a byte offset (0, 2, or 4), exactly as passed in A by Alcoon's three instructions.
    /// </summary>
    private void SpawnAlcoonFireball(RoomEnemySlot alcoon, ushort yVelocityTableByteOffset)
    {
        if (yVelocityTableByteOffset is not (0 or 2 or 4))
        {
            throw new InvalidDataException(
                $"Alcoon fireball Y-velocity byte offset {yVelocityTableByteOffset} is invalid.");
        }

        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        AlcoonEnemyState state = RequireAlcoonState(alcoon);
        bool movingLeft = unchecked((short)state.XVelocity) < 0;

        // SpawnEnemyProjectileY_ParameterA_XGraphics copies the complete seven-word $9E90
        // definition before initializer $9EB2 touches position, subposition, and velocity.
        // Keeping that division exact makes radii, damage, persistence, shot blocking,
        // animation, and pre-instruction cartridge-owned instead of duplicating them here.
        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.AlcoonFireball,
            unchecked((ushort)(alcoon.VramTilesIndex | alcoon.PaletteIndex)));
        projectile.Position = FixedPosition<RoomPixels>.FromWhole(new(unchecked((ushort)(alcoon.Position.X.Whole + (movingLeft ? -16 : 16))), unchecked((ushort)(alcoon.Position.Y.Whole - 12))));
        projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(movingLeft
            ? unchecked((ushort)-AlcoonFireballGraphicsHorizontalSpeed)
            : AlcoonFireballGraphicsHorizontalSpeed));
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(EnemyFireballLaunchDefinitions.AlcoonYVelocity(yVelocityTableByteOffset)));
    }

    /// <summary>Ports Alcoon fireball pre-instruction <c>$86:9EFF-$9F40</c>.</summary>
    private static void RunAlcoonFireballPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        // Unlike Ridley's fireball, this definition checks vertical collision first. If
        // either axis collides, the native routine clears the projectile ID immediately and
        // never applies horizontal deceleration on that frame.
        if (MoveProjectileAxis(projectile, level, horizontal: false) ||
            MoveProjectileAxis(projectile, level, horizontal: true))
        {
            projectile.Clear();
            return;
        }

        short velocity = unchecked((short)projectile.Velocity.X.RawValue);
        if (velocity < 0)
        {
            velocity = unchecked((short)(velocity + AlcoonFireballHorizontalDeceleration));
            if (velocity >= unchecked((short)-AlcoonFireballTerminalHorizontalSpeed))
                velocity = unchecked((short)-AlcoonFireballTerminalHorizontalSpeed);
        }
        else
        {
            velocity = unchecked((short)(velocity - AlcoonFireballHorizontalDeceleration));
            if (velocity < AlcoonFireballTerminalHorizontalSpeed)
                velocity = unchecked((short)AlcoonFireballTerminalHorizontalSpeed);
        }
        projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)velocity)));
    }
}
