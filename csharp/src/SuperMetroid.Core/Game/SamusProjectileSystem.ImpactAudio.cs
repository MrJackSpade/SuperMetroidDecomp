using SuperMetroid.Core.Audio;

namespace SuperMetroid.Core.Game;

public sealed partial class SamusProjectileSystem
{
    // Frame-publication data, not persistent cartridge state. Reinitialize before enemy
    // collision as well as projectile movement: either owner can cause an impact.
    /// <summary>Requests for projectile impacts recorded during the current owner frame.</summary>
    [NonSerialized] private List<SamusSoundRequest>? _impactSoundRequests;
    /// <summary>Whether cinematic mode suppresses missile impact requests in the current owner frame.</summary>
    [NonSerialized] private bool _cinematicImpactAudioSuppressed;

    /// <summary>Borrowed current-owner-frame missile impact requests for library two with queue limit six; reading the list does not consume or play them.</summary>
    /// <remarks>The backing list is cleared by <see cref="BeginImpactAudioFrame"/> and is not a snapshot. Cinematic suppression omits requests; an active Power Bomb is recorded in each request's sound-suppression flag for native queue handling.</remarks>
    public IReadOnlyList<SamusSoundRequest> ImpactSoundRequests =>
        _impactSoundRequests is { } requests ? requests : Array.Empty<SamusSoundRequest>();

    /// <summary>Starts an owner frame; cinematic_function suppresses missile impact SFX in $93:80CF.</summary>
    public void BeginImpactAudioFrame(bool cinematicActive)
    {
        _impactSoundRequests?.Clear();
        _cinematicImpactAudioSuppressed = cinematicActive;
    }

    /// <summary>Records the beam-impact sound request, marking it suppressed while a Power Bomb is active.</summary>
    /// <param name="powerBomb">Current Power Bomb state used to set the request's native sound-suppression flag.</param>
    private void RequestBeamImpactSound(SamusPowerBombExplosionState powerBomb) =>
        (_impactSoundRequests ??= []).Add(new(SoundEffectLibrary2Sounds.BeamImpact, 6,
            SoundSuppressed: powerBomb.IsActive));

    /// <summary>Records a missile-impact sound request unless the current frame is in cinematic mode.</summary>
    /// <param name="powerBomb">Current Power Bomb state used to set the request's native sound-suppression flag.</param>
    private void RequestMissileImpactSound(SamusPowerBombExplosionState powerBomb)
    {
        if (!_cinematicImpactAudioSuppressed)
            (_impactSoundRequests ??= []).Add(new(SoundEffectLibrary2Sounds.MissileImpact, 6,
                SoundSuppressed: powerBomb.IsActive));
    }
}
