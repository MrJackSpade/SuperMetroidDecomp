using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>A Kraid rock's (or the Kago bug's initial) typed animation.</summary>
/// <param name="program">The program to play.</param>
internal sealed class KraidRockAnimation(KraidRockProgram program)
    : EnemyProjectileProgramAnimation<KraidRockProgram, KraidRockFrame, KraidRockCue>(program)
{

    /// <inheritdoc/>
    private protected override AnimationProgramSet<KraidRockProgram, KraidRockFrame, KraidRockCue> Programs => KraidRockAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(KraidRockFrame frame) =>
        KraidRockVisualDefinitions.ShownProjectileFrames[frame];
}
