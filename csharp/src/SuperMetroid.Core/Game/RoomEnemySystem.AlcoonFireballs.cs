using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$86 support for Alcoon projectile definition <c>$9E90</c>. Keeping the projectile
/// beside its producing enemy makes the three animation commands easy to follow while the
/// shared fixed-slot pool, instruction interpreter, collision, and renderer remain common.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const ushort AlcoonFireballInstructionList = 0x9e9e;
    private const ushort AlcoonFireballPreInstruction = 0x9eff;
    private const ushort AlcoonFireballRadius = 4;
    private const ushort AlcoonFireballDamage = 0x0014;
    private const ushort CommonEnemyProjectileInvincibilityFrames = 96;
    private const ushort AlcoonFireballGraphicsHorizontalSpeed = 0x0400;
    private const ushort AlcoonFireballTerminalHorizontalSpeed = 0x0200;
    private const ushort AlcoonFireballHorizontalDeceleration = 0x0040;
    private const int AlcoonFireballYVelocityTable = 0x869ef9;

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
        projectile.Kind = RoomEnemyProjectileKind.AlcoonFireball;
        projectile.XPosition = unchecked((ushort)(alcoon.XPosition + (movingLeft ? -16 : 16)));
        projectile.YPosition = unchecked((ushort)(alcoon.YPosition - 12));
        projectile.XSubposition = 0;
        projectile.YSubposition = 0;
        projectile.XVelocity = movingLeft
            ? unchecked((ushort)-AlcoonFireballGraphicsHorizontalSpeed)
            : AlcoonFireballGraphicsHorizontalSpeed;
        projectile.YVelocity = ReadWord(
            _bus!,
            AlcoonFireballYVelocityTable + yVelocityTableByteOffset);
        projectile.InstructionPointer = AlcoonFireballInstructionList;
        projectile.InstructionTimer = 1;
        projectile.PreInstruction = AlcoonFireballPreInstruction;
        projectile.GraphicsIndex = unchecked((ushort)(alcoon.VramTilesIndex | alcoon.PaletteIndex));
        projectile.XRadius = AlcoonFireballRadius;
        projectile.YRadius = AlcoonFireballRadius;
        projectile.Damage = AlcoonFireballDamage;
        projectile.InvincibilityFrames = CommonEnemyProjectileInvincibilityFrames;
        projectile.CanDamageSamus = true;
    }

    /// <summary>Ports Alcoon fireball pre-instruction <c>$86:9EFF-$9F40</c>.</summary>
    private void RunAlcoonFireballPreInstruction(
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

        short velocity = unchecked((short)projectile.XVelocity);
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
        projectile.XVelocity = unchecked((ushort)velocity);
    }
}
