namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// The Morph Ball eye's beam, stepped at the start of each update before the enemy pass selects its enemies (#1851): a
/// beam the eye's behaviour raises therefore initializes on the following update, and one it shuts down sees its body's
/// cleared activation on the following update.
/// </summary>
/// <param name="species">The Morph Ball eye species whose beam steps.</param>
internal sealed class MorphBallEyeBeamStep(MorphBallEyeBehavior species) : IStepBeforeEnemySelection
{
    /// <inheritdoc/>
    public void StepBeforeEnemySelection() => species.StepMorphBallEyeBeam();
}
