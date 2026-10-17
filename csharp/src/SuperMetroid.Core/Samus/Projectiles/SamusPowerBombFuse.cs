using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>$90:C157 fuse transition, independent of which projectile array half owns the slot.</summary>
public static class SamusPowerBombFuse
{
    /// <summary>
    /// Computes the native timer/list writes and requests the caller's existing explosion
    /// or deletion operation. Collision consumes the expiration sentinel after this call;
    /// it must not be folded into the fuse, because other callbacks can reach this routine.
    /// </summary>
    public static PowerBombFuseStep Step(NativeCountdown timer, ushort explosionFlag)
    {
        // LDA, BEQ: a stopped fuse is left alone. Otherwise DEC, BNE: reaching zero
        // expires the fuse, and the fast-list switch tests the decremented count.
        if (timer.IsZero)
            return new(timer, FastAnimation: false, SpawnExplosion: false, DeleteProjectile: explosionFlag == 0);
        NativeWordCounterStep fuse = timer.Decrement();
        if (fuse.IsZero)
            return new(NativeCountdown.FromRaw(SamusPowerBombFuseData.ExplosionStartedSentinel), FastAnimation: false,
                SpawnExplosion: true, DeleteProjectile: false);
        return new(fuse.Remaining, FastAnimation: fuse.Remaining.RawValue == SamusPowerBombFuseData.FastAnimationTimer,
            SpawnExplosion: false, DeleteProjectile: false);
    }

    /// <summary>
    /// Switches a list to its fast-animation counterpart $1C bytes on, as the fuse's add to the live instruction pointer
    /// does; the slow and fast lists share their layout, so the phase carries over.
    /// </summary>
    internal static AnimationProgramCursor<SamusProjectileProgram>? FastAnimation(AnimationProgramCursor<SamusProjectileProgram>? cursor) =>
        cursor is { } live ? SamusProjectileProgramAddresses.Offset(live, SamusPowerBombFuseData.FastAnimationOffset) : null;
}
