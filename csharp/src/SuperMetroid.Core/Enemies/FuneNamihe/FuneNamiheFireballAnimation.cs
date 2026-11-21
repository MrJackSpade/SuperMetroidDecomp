using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>Fune/Namihe fireball's typed animation.</summary>
internal sealed partial class FuneNamiheFireballAnimation
    : EnemyProjectileFrameAnimation<FuneNamiheFireballSegment, EnemyProjectileCommonCue>
{
    /// <summary>Creates a <see cref="FuneNamiheFireballAnimation"/>.</summary>
    /// <param name="segment">segment</param>
    public FuneNamiheFireballAnimation(FuneNamiheFireballSegment segment)
        : base(segment, FuneNamiheFireballAnimations.StartingAt[segment], EnemyProjectileCueRunners.Common<FuneNamiheFireballSegment>())
    {
    }

    /// <inheritdoc/>
    private protected override IUnpausedAnimationPlay<FuneNamiheFireballSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => FuneNamiheFireballAnimations.Segments;
}
