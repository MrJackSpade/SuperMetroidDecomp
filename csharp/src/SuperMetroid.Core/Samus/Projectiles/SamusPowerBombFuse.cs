using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The fuse transition, independent of which projectile array half owns the slot.</summary>
public static class SamusPowerBombFuse
{
    /// <summary>
    /// Computes the timer/list writes and requests the caller's existing explosion
    /// or deletion operation. The step that runs the fuse out starts the explosion; the caller's
    /// boundary reactions wait for the update after it, because other callbacks can reach this step.
    /// </summary>
    public static PowerBombFuseStep Step(WrappingCountdown timer, bool explosionArmed)
    {
        // A stopped fuse is left alone. Otherwise the fuse counts down: reaching zero
        // runs the fuse out and starts the explosion, and the fast-list switch tests the decremented count.
        if (timer.IsZero)
            return new(timer, FastAnimation: false, SpawnExplosion: false, DeleteProjectile: !explosionArmed);
        CountdownStep fuse = timer.Decrement();
        if (fuse.IsZero)
            return new(fuse.Remaining, FastAnimation: false, SpawnExplosion: true, DeleteProjectile: false);
        return new(fuse.Remaining, FastAnimation: fuse.Remaining.IsAt(SamusPowerBombFuseData.FastAnimationTimer),
            SpawnExplosion: false, DeleteProjectile: false);
    }

    /// <summary>Switches a bomb or Power Bomb to the same frame of its fast animation.</summary>
    /// <exception cref="InvalidDataException">The playback is not at a slow bomb or Power Bomb place.</exception>
    internal static AnimationPlayback<SamusProjectileSegment>? FastAnimation(AnimationPlayback<SamusProjectileSegment>? playback) =>
        playback is { } live
            ? SamusPowerBombFuseData.FastCounterparts.TryGetValue(SamusProjectilePlayback.BeatOf(live), out SamusProjectileSegmentBeat fast)
                ? SamusProjectilePlayback.PlaybackAt(fast)
                : throw new InvalidDataException($"{SamusProjectilePlayback.BeatOf(live)} has no fast-animation counterpart.")
            : null;
}
