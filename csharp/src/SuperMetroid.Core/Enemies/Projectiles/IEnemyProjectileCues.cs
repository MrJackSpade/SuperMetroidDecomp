namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A projectile behaviour that carries out the cues of type <typeparamref name="TCue"/> its kinds' typed animations
/// publish (#1760).
/// </summary>
/// <typeparam name="TCue">The cues of the animation family.</typeparam>
internal interface IEnemyProjectileCues<in TCue>
{
    /// <summary>Carries out <paramref name="cue"/>, published on the context's projectile.</summary>
    /// <param name="context">The projectile, Samus and the camera of the pass running the animation.</param>
    /// <param name="cue">The cue the animation published.</param>
    void RunCue(EnemyProjectileCueContext context, TCue cue);
}
