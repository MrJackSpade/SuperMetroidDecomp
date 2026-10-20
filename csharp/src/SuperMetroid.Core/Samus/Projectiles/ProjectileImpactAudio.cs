namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Beam and missile impact sound requests for the frame.</summary>
internal sealed class ProjectileImpactAudio
{
    /// <summary>Transient requests for the current published frame; enemy collision and projectile movement can both append impacts.</summary>
    [NonSerialized] private List<SoundRequest>? _impactSoundRequests;
    /// <summary>Frame-local gate that omits missile impact requests during a cinematic.</summary>
    [NonSerialized] private bool _cinematicImpactAudioSuppressed;

    /// <summary>Borrowed current-owner-frame missile impact requests for library two with queue limit six; reading the list does not consume or play them.</summary>
    /// <remarks>The backing list is cleared by <see cref="ProjectileImpactAudio.BeginImpactAudioFrame"/> and is not a snapshot. Cinematic suppression omits requests; an active Power Bomb is recorded in each request's sound-suppression flag for native queue handling.</remarks>
    public IReadOnlyList<SoundRequest> ImpactSoundRequests =>
        _impactSoundRequests is { } requests ? requests : Array.Empty<SoundRequest>();

    /// <summary>Starts an owner frame; cinematic_function suppresses missile impact SFX in $93:80CF.</summary>
    public void BeginImpactAudioFrame(bool cinematicActive)
    {
        _impactSoundRequests?.Clear();
        _cinematicImpactAudioSuppressed = cinematicActive;
    }

    /// <summary>Queues library-two beam impact audio, marking it suppressed when a Power Bomb is active.</summary>
    internal void RequestBeamImpactSound(SamusPowerBombExplosionState powerBomb) =>
        (_impactSoundRequests ??= []).Add(new(SoundEffectLibrary2Sounds.BeamImpact, 6,
            SoundSuppressed: powerBomb.IsActive));

    /// <summary>Queues missile impact audio unless the current cinematic frame suppresses it; an active Power Bomb marks the request suppressed.</summary>
    internal void RequestMissileImpactSound(SamusPowerBombExplosionState powerBomb)
    {
        if (!_cinematicImpactAudioSuppressed)
            (_impactSoundRequests ??= []).Add(new(SoundEffectLibrary2Sounds.MissileImpact, 6,
                SoundSuppressed: powerBomb.IsActive));
    }
}
