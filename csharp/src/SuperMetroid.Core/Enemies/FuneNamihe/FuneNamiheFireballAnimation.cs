using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.FuneNamihe.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>Fune/Namihe fireball's typed animation.</summary>
internal sealed class FuneNamiheFireballAnimation(FuneNamiheFireballProgram program)
    : EnemyProjectileProgramAnimation<FuneNamiheFireballProgram, FuneNamiheFireballFrame, EnemyProjectileCommonCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<FuneNamiheFireballProgram, FuneNamiheFireballFrame, EnemyProjectileCommonCue> Programs => FuneNamiheFireballAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(FuneNamiheFireballFrame frame) =>
        FuneNamiheFireballFrameDefinitions.ShownProjectileFrames[frame];
}
