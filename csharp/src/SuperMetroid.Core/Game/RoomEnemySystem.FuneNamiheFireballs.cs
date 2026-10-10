namespace SuperMetroid.Core.Game;

/// <summary>Bank-$86 half of the shared Fune/Namihe fireball family.</summary>
public sealed partial class RoomEnemySystem
{
    private const ushort NamiFuneFireballPreInstruction =
        (ushort)EnemyProjectilePreInstruction.NamiFuneFireball;

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
            ? FuneNamiheFireballInstructionProgramDefinitions.Right
            : FuneNamiheFireballInstructionProgramDefinitions.Left;
        projectile.PreInstruction = NamiFuneFireballPreInstruction;
        projectile.Variable0 = movingRight
            ? (ushort)NamiFuneFireballFunction.MovingRight
            : (ushort)NamiFuneFireballFunction.MovingLeft;
        projectile.DirectionParameter = movingRight ? (ushort)1 : (ushort)0;

        projectile.XPosition = source.XPosition;
        projectile.XSubposition = source.XSubposition;
        projectile.YPosition = source.YPosition;
        projectile.YSubposition = source.YSubposition;

        // The initializer tests the actor parameter's species nibble, not the projectile
        // definition. Namihe's mouth is four pixels lower than Fune's in the shared art.
        if ((source.Parameter1 & 0x000f) != 0)
            projectile.YPosition = unchecked((ushort)(projectile.YPosition + 4));

        var velocities = EnemyFireballLaunchDefinitions.NamiFuneVelocities(source.Parameter2);
        projectile.YVelocity = velocities.Left;
        projectile.XVelocity = velocities.Right;
    }

    /// <summary>Ports pre-instruction $86:DF39 and both directional movers.</summary>
    private static void RunFuneNamiheFireballPreInstruction(
        RoomEnemyProjectileSlot projectile,
        ushort cameraX,
        ushort cameraY)
    {
        switch (ClosedNativeWords.Decode<NamiFuneFireballFunction>(projectile.Variable0, "Fune/Namihe fireball function"))
        {
            case NamiFuneFireballFunction.MovingLeft:
                // Yes, this is YVelocity. The retail initializer and mover agree on this
                // unconventional storage, so normalizing it would make debugger state lie.
                (projectile.XPosition, projectile.XSubposition) = AddEightBitVelocity(
                    projectile.XPosition,
                    projectile.XSubposition,
                    projectile.YVelocity);
                break;

            case NamiFuneFireballFunction.MovingRight:
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

/// <summary>Fune/Namihe fireball functions stored in the native Variable0 slot.</summary>
internal enum NamiFuneFireballFunction : ushort
{
    /// <summary>$86:DF40.</summary>
    MovingLeft = 0xdf40,

    /// <summary>$86:DF6A.</summary>
    MovingRight = 0xdf6a,
}
