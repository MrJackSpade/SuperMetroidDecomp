using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>Fune/Namihe fireball's typed animation.</summary>
internal sealed class FuneNamiheFireballAnimation(FuneNamiheFireballProgram program)
    : EnemyProjectileFrameAnimation<FuneNamiheFireballProgram, EnemyProjectileCommonCue>(FuneNamiheFireballAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<FuneNamiheFireballProgram, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => FuneNamiheFireballAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<FuneNamiheFireballProgram, Animation<FuneNamiheFireballProgram>> StartingAt => FuneNamiheFireballAnimations.StartingAt;
}
