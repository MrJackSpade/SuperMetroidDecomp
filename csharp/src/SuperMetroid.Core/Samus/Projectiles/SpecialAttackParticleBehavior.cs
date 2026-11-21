namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// What a special attack particle does on its update (#1923): enemy collision only marks it spent, and its own update
/// then ends it. The sound the update makes joins the update's audio requests at once.
/// </summary>
internal abstract class SpecialAttackParticleBehavior : SamusShotBehavior
{
    /// <inheritdoc/>
    internal sealed override void Run(SamusShotUpdate update)
    {
        if (Step(update) is { } sound)
            update.Shots.AudioRequests.QueueSound(sound, SoundRequestPriority.Normal);
    }

    /// <summary>Runs the particle's update and returns the sound it makes, if any.</summary>
    /// <param name="update">What the shot update hands every behaviour.</param>
    private protected abstract SoundEffectId? Step(SamusShotUpdate update);

    /// <inheritdoc/>
    internal sealed override bool EndsItselfAfterEnemyHit => true;
}
