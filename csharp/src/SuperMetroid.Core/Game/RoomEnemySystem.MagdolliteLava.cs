namespace SuperMetroid.Core.Game;

/// <summary>
/// Drop request emitted when a shot destroys Magdollite's thrown lava. The shared pickup
/// system still owns random drop selection; this record preserves the exact source enemy
/// definition and bank-$86 projectile slot used by instruction $DFEA.
/// </summary>
public readonly record struct MagdolliteLavaDropRequest(
    ushort X,
    ushort Y,
    ushort EnemyDefinitionPointer,
    ushort EnemyProjectileNativeIndex);

public sealed partial class RoomEnemySystem
{
    internal const ushort MagdolliteLavaPreInstruction = 0xe049;
    internal const ushort MagdolliteLavaDropInstruction = 0xdfea;

    private const ushort MagdolliteLavaLeftInstructionList = 0xdfd8;
    private const ushort MagdolliteLavaRightInstructionList = 0xdfde;
    private const ushort MagdolliteLavaHorizontalSpeed = 0x0300;
    private const ushort MagdolliteLavaUpwardSpeed = 0xfd00;

    /// <summary>Last drop request produced by a destroyed lava projectile this frame.</summary>
    public MagdolliteLavaDropRequest? LastMagdolliteLavaDropRequest { get; private set; }

    /// <summary>Ports <c>EprojInit_NorfairLavaMan</c> at $86:E000.</summary>
    private void SpawnMagdolliteLava(RoomEnemySlot source, ushort directionParameter)
    {
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.LavaThrownByMagdollite,
            unchecked((ushort)(source.VramTilesIndex | source.PaletteIndex)));

        // SpawnEprojInner has already cleared all generic scratch words. The initializer
        // copies both subpixels from the throwing overlay, starts two pixels below it, and
        // stores the one-bit facing parameter exactly as supplied by the enemy bytecode.
        projectile.DirectionParameter = directionParameter;
        projectile.XPosition = source.XPosition;
        projectile.XSubposition = source.XSubposition;
        projectile.YPosition = unchecked((ushort)(source.YPosition + 2));
        projectile.YSubposition = source.YSubposition;
        projectile.YVelocity = MagdolliteLavaUpwardSpeed;
        projectile.XVelocity = MagdolliteLavaHorizontalSpeed;
        projectile.PreInstruction = MagdolliteLavaPreInstruction;
        projectile.InstructionPointer = directionParameter == 0
            ? MagdolliteLavaLeftInstructionList
            : MagdolliteLavaRightInstructionList;
    }

    /// <summary>Ports $86:E049-$E09B, including the cartridge's asymmetric velocity use.</summary>
    private void RunMagdolliteLavaPreInstruction(
        RoomEnemyProjectileSlot projectile,
        ushort cameraX,
        ushort cameraY)
    {
        // The left routine at $E050 deliberately adds the Y-velocity word to X. The right
        // routine at $E07A uses X velocity. They begin as -$0300 and +$0300 respectively,
        // so expressing this oddity literally keeps debugger state aligned with the ROM.
        ushort horizontalVelocity = projectile.DirectionParameter == 0
            ? projectile.YVelocity
            : projectile.XVelocity;
        (projectile.XPosition, projectile.XSubposition) = AddEightBitVelocity(
            projectile.XPosition,
            projectile.XSubposition,
            horizontalVelocity);

        // $86:E049 deletes actors outside the inclusive 256x256 camera square after their
        // horizontal move. No vertical movement occurs despite the field's directional use.
        if (unchecked((ushort)(projectile.XPosition - cameraX)) >= 256 ||
            unchecked((ushort)(projectile.YPosition - cameraY)) >= 256)
        {
            projectile.Clear();
        }
    }

    /// <summary>Ports shot-list instruction $86:DFEA.</summary>
    private void RequestMagdolliteLavaDrop(RoomEnemyProjectileSlot projectile)
    {
        LastMagdolliteLavaDropRequest = new MagdolliteLavaDropRequest(
            projectile.XPosition,
            projectile.YPosition,
            MagdolliteDefinition,
            checked((ushort)(projectile.SlotIndex * 2)));
    }
}
