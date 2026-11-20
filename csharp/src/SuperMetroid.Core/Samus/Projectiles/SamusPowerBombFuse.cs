using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The fuse transition, the same whether a Power Bomb or a Chainsaw shot runs it.</summary>
public static class SamusPowerBombFuse
{
    /// <summary>
    /// Computes the fuse and animation changes and asks for the bomb's explosion or deletion. The step that burns the fuse
    /// out starts the explosion; the bomb's boundary reactions wait for the update after it, because other bombs' updates
    /// can reach this step.
    /// </summary>
    /// <param name="fuse">The fuse, counted once by this step while it burns and over once it has burned out.</param>
    /// <param name="explosionArmed">Whether a Power Bomb's blast is under way.</param>
    public static PowerBombFuseStep Step(UpdateCountdown fuse, bool explosionArmed)
    {
        // A burned-out fuse is left alone. Otherwise the fuse counts one update: its last burns it out and starts the
        // explosion, and the fast-animation switch is the named point of the updates it has left.
        Ensure.NotNull(fuse);
        if (fuse.IsOver)
            return new(0, FastAnimation: false, SpawnExplosion: false, DeleteProjectile: !explosionArmed);
        if (fuse.Count())
            return new(0, FastAnimation: false, SpawnExplosion: true, DeleteProjectile: false);
        return new(fuse.UpdatesLeft, FastAnimation: fuse.UpdatesLeft == SamusPowerBombFuseData.FastAnimationTimer,
            SpawnExplosion: false, DeleteProjectile: false);
    }

    /// <summary>Switches a bomb or Power Bomb's live animation onto the same frame of its fast animation.</summary>
    /// <exception cref="InvalidOperationException">The bomb has no animation.</exception>
    /// <exception cref="InvalidDataException">The animation is not at a slow bomb or Power Bomb place.</exception>
    internal static void SwitchToFastAnimation(AnimationPlayer<SamusProjectileSegment, SamusProjectileFrameId, NoAnimationCue>? animation)
    {
        AnimationPlayer<SamusProjectileSegment, SamusProjectileFrameId, NoAnimationCue> live = animation
            ?? throw new InvalidOperationException("A bomb with no animation has no fast animation to switch to.");
        SamusProjectileSegmentBeat at = SamusProjectilePlayback.BeatOf(live.Playback);
        live.GoOnFrom(SamusPowerBombFuseData.FastCounterparts.TryGetValue(at, out SamusProjectileSegmentBeat fast)
            ? SamusProjectilePlayback.PlaybackAt(fast)
            : throw new InvalidDataException($"{at} has no fast-animation counterpart."));
    }
}
