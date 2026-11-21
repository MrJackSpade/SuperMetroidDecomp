namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// What a special attack particle does on its update (#1923): enemy collision only marks it spent, and its own update
/// then ends it. The sound the update makes joins the update's audio requests at once.
/// </summary>
/// <typeparam name="TState">The state the particle's kind keeps.</typeparam>
internal abstract class SpecialAttackParticleBehavior<TState> : SamusShotBehavior<TState>
    where TState : SpecialAttackParticleState
{
    /// <inheritdoc/>
    internal sealed override void Run(SamusShotUpdate update, TState state)
    {
        if (Step(update, state) is { } sound)
            update.Shots.AudioRequests.QueueSound(sound, SoundRequestPriority.Normal);
    }

    /// <summary>Runs the particle's update and returns the sound it makes, if any.</summary>
    /// <param name="update">What the shot update hands every behaviour.</param>
    /// <param name="state">The state the particle's kind keeps.</param>
    private protected abstract SoundEffectId? Step(SamusShotUpdate update, TState state);
}
