using SuperMetroid.Core.Audio;

namespace SuperMetroid.Core.Game;

public sealed partial class SamusProjectileSystem
{
    // Frame-publication data, not persistent cartridge state. Reinitialize before enemy
    // collision as well as projectile movement: either owner can cause an impact.
    [NonSerialized] private List<SamusSoundRequest>? _impactSoundRequests;
    [NonSerialized] private bool _cinematicImpactAudioSuppressed;

    public IReadOnlyList<SamusSoundRequest> ImpactSoundRequests =>
        _impactSoundRequests is { } requests ? requests : Array.Empty<SamusSoundRequest>();

    /// <summary>Starts an owner frame; cinematic_function suppresses missile impact SFX in $93:80CF.</summary>
    public void BeginImpactAudioFrame(bool cinematicActive)
    {
        _impactSoundRequests?.Clear();
        _cinematicImpactAudioSuppressed = cinematicActive;
    }

    private void RequestMissileImpactSound()
    {
        if (!_cinematicImpactAudioSuppressed)
            (_impactSoundRequests ??= []).Add(new(SoundEffectLibrary2Sounds.MissileImpact, 6));
    }
}
