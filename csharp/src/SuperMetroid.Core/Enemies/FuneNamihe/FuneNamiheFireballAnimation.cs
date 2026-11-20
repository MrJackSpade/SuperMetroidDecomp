using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>Fune/Namihe fireball's typed animation.</summary>
internal sealed class FuneNamiheFireballAnimation(FuneNamiheFireballSegment segment)
    : EnemyProjectileFrameAnimation<FuneNamiheFireballSegment, EnemyProjectileCommonCue>(FuneNamiheFireballAnimations.StartingWith(segment), EnemyProjectileCueRunners.Common<FuneNamiheFireballSegment>())
{
    /// <inheritdoc/>
    private protected override UnpausedAnimationSegments<FuneNamiheFireballSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => FuneNamiheFireballAnimations.Segments;

    /// <inheritdoc/>
    private protected override IEnumerable<KeyValuePair<FuneNamiheFireballSegment, Animation<FuneNamiheFireballSegment>>> StartingAt => FuneNamiheFireballAnimations.StartingAt;
}
