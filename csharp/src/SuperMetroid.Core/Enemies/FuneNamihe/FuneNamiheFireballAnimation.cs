using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>Fune/Namihe fireball's typed animation.</summary>
internal sealed class FuneNamiheFireballAnimation(FuneNamiheFireballSegment segment)
    : EnemyProjectileFrameAnimation<FuneNamiheFireballSegment, EnemyProjectileCommonCue>(FuneNamiheFireballAnimations.StartingWith(segment))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<FuneNamiheFireballSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => FuneNamiheFireballAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<FuneNamiheFireballSegment, Animation<FuneNamiheFireballSegment>> StartingAt => FuneNamiheFireballAnimations.StartingAt;
}
