namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A projectile behaviour that carries out the cues of type <typeparamref name="TCue"/> its kinds' typed animations
/// publish, where a cue may hand the projectile's play over to another family's animation (#2554), as a death explosion
/// becoming its drop does: the play goes on in that animation from its start within the same pass, so the frame it shows
/// first is shown on the pass the cue ran in.
/// </summary>
/// <typeparam name="TCue">The cues of the animation family.</typeparam>
internal interface IEnemyProjectileHandingOverCues<in TCue>
{
    /// <summary>
    /// Carries out <paramref name="cue"/>, published on the context's projectile, returning the animation the projectile's
    /// play goes on in, or null to run on.
    /// </summary>
    /// <param name="context">The projectile, Samus and the camera of the pass running the animation.</param>
    /// <param name="cue">The cue the animation published.</param>
    EnemyProjectileAnimation? RunCue(EnemyProjectileCueContext context, TCue cue);
}
