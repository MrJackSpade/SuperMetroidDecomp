namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Beam and missile impact sound requests, each queued as the impact happens.</summary>
internal sealed class ProjectileImpactAudio
{
    /// <summary>Frame-local gate that omits missile impact requests during a cinematic.</summary>
    private bool _cinematicImpactAudioSuppressed;

    /// <summary>Creates the impact sounds of a projectile system whose requests join <paramref name="requests"/>.</summary>
    internal ProjectileImpactAudio(GameplayAudioRequests requests) => Requests = Ensure.NotNull(requests);

    /// <summary>The update's audio requests the impacts' sounds join, the same as their projectile system's.</summary>
    internal GameplayAudioRequests Requests { get; private set; }

    /// <summary>Points the impacts' sounds at <paramref name="requests"/>, the update's audio requests of the runtime running them.</summary>
    internal void BindAudioRequests(GameplayAudioRequests requests) => Requests = Ensure.NotNull(requests);

    /// <summary>Marks whose frame the impacts belong to: a cinematic's suppresses missile impact sounds.</summary>
    public void BeginImpactAudioFrame(ImpactAudioContext context) =>
        _cinematicImpactAudioSuppressed = context == ImpactAudioContext.Cinematic;

    /// <summary>Queues enemy-library beam impact audio now.</summary>
    internal void RequestBeamImpactSound() => Requests.QueueSound(EnemySound.BeamImpact, SoundRequestPriority.Normal);

    /// <summary>Queues missile impact audio now unless the current cinematic frame omits it.</summary>
    internal void RequestMissileImpactSound()
    {
        if (!_cinematicImpactAudioSuppressed)
            Requests.QueueSound(EnemySound.MissileImpact, SoundRequestPriority.Normal);
    }
}
