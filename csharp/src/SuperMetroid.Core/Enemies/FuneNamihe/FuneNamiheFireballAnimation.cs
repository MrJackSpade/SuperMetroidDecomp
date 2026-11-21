using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>Fune/Namihe fireball's typed animation.</summary>
internal sealed class FuneNamiheFireballAnimation(FuneNamiheFireballSegment segment)
    : EnemyProjectileFrameAnimation<FuneNamiheFireballSegment, EnemyProjectileCommonCue>(segment, FuneNamiheFireballAnimations.StartingAt[segment], EnemyProjectileCueRunners.Common<FuneNamiheFireballSegment>())
{
    /// <inheritdoc/>
    private protected override IUnpausedAnimationPlay<FuneNamiheFireballSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => FuneNamiheFireballAnimations.Segments;
}
