using SuperMetroid.Core.Audio;

namespace SuperMetroid.Core.Game;

public sealed partial class SamusProjectileSystem
{
    // Frame-publication data, not persistent cartridge state. Reinitialize before enemy
    // collision as well as projectile movement: either owner can cause an impact.
    [NonSerialized] private List<SamusSoundRequest>? _impactSoundRequests;
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

    private void RequestBeamImpactSound(SamusPowerBombExplosionState powerBomb) =>
        (_impactSoundRequests ??= []).Add(new(SoundEffectLibrary2Sounds.BeamImpact, 6,
            SoundSuppressed: powerBomb.IsActive));

    private void RequestMissileImpactSound(SamusPowerBombExplosionState powerBomb)
    {
        if (!_cinematicImpactAudioSuppressed)
            (_impactSoundRequests ??= []).Add(new(SoundEffectLibrary2Sounds.MissileImpact, 6,
                SoundSuppressed: powerBomb.IsActive));
    }
}
