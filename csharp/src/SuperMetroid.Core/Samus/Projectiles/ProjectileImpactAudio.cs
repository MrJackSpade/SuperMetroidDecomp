namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Beam and missile impact sound requests, each queued as the impact happens.</summary>
internal sealed class ProjectileImpactAudio
{
    /// <summary>Whose frame the impacts belong to; a cinematic's omits missile impact sounds.</summary>
    private ImpactAudioContext _frame;

    /// <summary>Creates the impact sounds of a projectile system whose requests join <paramref name="requests"/>.</summary>
    internal ProjectileImpactAudio(GameplaySoundRequests requests) => Requests = Ensure.NotNull(requests);

    /// <summary>The update's audio requests the impacts' sounds join, the same as their projectile system's.</summary>
    internal GameplaySoundRequests Requests { get; }

    /// <summary>Marks whose frame the impacts belong to: a cinematic's suppresses missile impact sounds.</summary>
    public void BeginImpactAudioFrame(ImpactAudioContext context) => _frame = context;

    /// <summary>Queues enemy-library beam impact audio now.</summary>
    internal void RequestBeamImpactSound() => Requests.QueueSound(EnemySound.BeamImpact, SoundRequestPriority.Normal);

    /// <summary>Queues missile impact audio now unless the current cinematic frame omits it.</summary>
    internal void RequestMissileImpactSound()
    {
        if (_frame != ImpactAudioContext.Cinematic)
            Requests.QueueSound(EnemySound.MissileImpact, SoundRequestPriority.Normal);
    }
}
