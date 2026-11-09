namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Beam and missile impact sound requests for the frame.</summary>
internal sealed class ProjectileImpactAudio
{
    /// <summary>Transient requests for the current published frame; enemy collision and projectile movement can both append impacts.</summary>
    private List<SoundRequest>? _impactSoundRequests;
    /// <summary>Frame-local gate that omits missile impact requests during a cinematic.</summary>
    private bool _cinematicImpactAudioSuppressed;

    /// <summary>Borrowed current-owner-frame missile impact requests for the enemy library with queue limit six; reading the list does not consume or play them.</summary>
    /// <remarks>The backing list is cleared by <see cref="ProjectileImpactAudio.BeginImpactAudioFrame"/> and is not a snapshot. Cinematic suppression omits requests; an active Power Bomb is recorded in each request's sound-suppression flag for queue handling.</remarks>
    public IReadOnlyList<SoundRequest> ImpactSoundRequests =>
        _impactSoundRequests is { } requests ? requests : Array.Empty<SoundRequest>();

    /// <summary>Starts an owner frame; cinematic_function suppresses missile impact sounds.</summary>
    public void BeginImpactAudioFrame(ImpactAudioContext context)
    {
        _impactSoundRequests?.Clear();
        _cinematicImpactAudioSuppressed = context == ImpactAudioContext.Cinematic;
    }

    /// <summary>Queues enemy-library beam impact audio, marking it suppressed when a Power Bomb is active.</summary>
    internal void RequestBeamImpactSound(SamusPowerBombExplosionState powerBomb) =>
        (_impactSoundRequests ??= []).Add(new(EnemySound.BeamImpact, SoundRequestPriority.Normal,
            SoundSuppressed: powerBomb.IsActive));

    /// <summary>Queues missile impact audio unless the current cinematic frame suppresses it; an active Power Bomb marks the request suppressed.</summary>
    internal void RequestMissileImpactSound(SamusPowerBombExplosionState powerBomb)
    {
        if (!_cinematicImpactAudioSuppressed)
            (_impactSoundRequests ??= []).Add(new(EnemySound.MissileImpact, SoundRequestPriority.Normal,
                SoundSuppressed: powerBomb.IsActive));
    }
}
