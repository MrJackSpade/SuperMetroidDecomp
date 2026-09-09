namespace SuperMetroid.Core.Game;

/// <summary>$90:C157 fuse transition, independent of which projectile array half owns the slot.</summary>
public static class SamusPowerBombFuse
{
    /// <summary>
    /// Computes the native timer/list writes and requests the caller's existing explosion
    /// or deletion operation. Collision consumes the expiration sentinel after this call;
    /// it must not be folded into the fuse, because other callbacks can reach this routine.
    /// </summary>
    public static PowerBombFuseStep Step(ushort timer, ushort instructionPointer, ushort explosionFlag)
    {
        if (timer == 0)
            return new(timer, instructionPointer, SpawnExplosion: false, DeleteProjectile: explosionFlag == 0);

        timer = unchecked((ushort)(timer - 1));
        if (timer == 0)
            return new(SamusPowerBombFuseData.ExplosionStartedSentinel, instructionPointer,
                SpawnExplosion: true, DeleteProjectile: false);
        if (timer == SamusPowerBombFuseData.FastAnimationTimer)
            instructionPointer = unchecked((ushort)(instructionPointer + SamusPowerBombFuseData.FastAnimationOffset));
        return new(timer, instructionPointer, SpawnExplosion: false, DeleteProjectile: false);
    }
}

/// <summary>Writes and caller-owned side effects of one native Power Bomb fuse invocation.</summary>
public readonly record struct PowerBombFuseStep(
    ushort Timer, ushort InstructionPointer, bool SpawnExplosion, bool DeleteProjectile);
