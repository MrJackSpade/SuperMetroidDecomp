namespace SuperMetroid.Core.Game;

/// <summary>Bank-$86 half of the shared Fune/Namihe fireball family.</summary>
public sealed partial class RoomEnemySystem
{
    private const ushort NamiFuneFireballLeftInstructionList = 0xde96;
    private const ushort NamiFuneFireballRightInstructionList = 0xdea6;
    private const ushort NamiFuneFireballPreInstruction =
        EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_NamiFuneFireball;
    private const ushort NamiFuneFireballMovingLeft = 0xdf40;
    private const ushort NamiFuneFireballMovingRight = 0xdf6a;
    private const int NamiFuneFireballVelocityTable = 0x86deb6;

    /// <summary>
    /// Ports the shared projectile initializer at $86:DED6. The source population's low
    /// parameter-two byte selects one four-byte {left,right} velocity pair. Left velocity
    /// is stored in the field normally named Y velocity—a cartridge quirk that the left
    /// movement function knowingly reads back as horizontal speed.
    /// </summary>
    private void SpawnFuneNamiheFireball(
        RoomEnemySlot source,
        bool movingRight,
        RoomEnemyProjectileKind kind)
    {
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            projectile,
            kind,
            unchecked((ushort)(source.PaletteIndex | source.VramTilesIndex)));
        projectile.InstructionPointer = movingRight
            ? NamiFuneFireballRightInstructionList
            : NamiFuneFireballLeftInstructionList;
        projectile.PreInstruction = NamiFuneFireballPreInstruction;
        projectile.Variable0 = movingRight
            ? NamiFuneFireballMovingRight
            : NamiFuneFireballMovingLeft;
        projectile.DirectionParameter = movingRight ? (ushort)1 : (ushort)0;

        projectile.XPosition = source.XPosition;
        projectile.XSubposition = source.XSubposition;
        projectile.YPosition = source.YPosition;
        projectile.YSubposition = source.YSubposition;

        // The initializer tests the actor parameter's species nibble, not the projectile
        // definition. Namihe's mouth is four pixels lower than Fune's in the shared art.
        if ((source.Parameter1 & 0x000f) != 0)
            projectile.YPosition = unchecked((ushort)(projectile.YPosition + 4));

        int velocityRecord = NamiFuneFireballVelocityTable +
            unchecked((byte)source.Parameter2) * 4;
        projectile.YVelocity = ReadWord(_bus!, velocityRecord);
        projectile.XVelocity = ReadWord(_bus!, velocityRecord + 2);
    }

    /// <summary>Ports pre-instruction $86:DF39 and both directional movers.</summary>
    private static void RunFuneNamiheFireballPreInstruction(
        RoomEnemyProjectileSlot projectile,
        ushort cameraX,
        ushort cameraY)
    {
        switch (projectile.Variable0)
        {
            case NamiFuneFireballMovingLeft:
                // Yes, this is YVelocity. The retail initializer and mover agree on this
                // unconventional storage, so normalizing it would make debugger state lie.
                (projectile.XPosition, projectile.XSubposition) = AddEightBitVelocity(
                    projectile.XPosition,
                    projectile.XSubposition,
                    projectile.YVelocity);
                break;

            case NamiFuneFireballMovingRight:
                (projectile.XPosition, projectile.XSubposition) = AddEightBitVelocity(
                    projectile.XPosition,
                    projectile.XSubposition,
                    projectile.XVelocity);
                break;

            default:
                throw new InvalidDataException(
                    $"Fune/Namihe fireball function $86:{projectile.Variable0:X4} is not translated.");
        }

        // The shared native cull treats both camera edges as inclusive: an origin exactly
        // at camera+256 survives this frame and is deleted only after crossing beyond it.
        DeleteEnemyProjectileIfOutsideInclusiveViewport(projectile, cameraX, cameraY);
    }
}
