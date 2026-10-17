namespace SuperMetroid.Core.Audio;

/// <summary>Native stored-shine and shinespark queue identities.</summary>
public static class ShinesparkSounds
{
    /// <summary>$91:DAC7 Samus_SpeedBoosterShinePals queues library three $0C at timer 170.</summary>
    public static readonly SoundEffectId StoredWarning = new(SoundEffectLibrary.Library3, 0x0c); // magic-number-audit: allow(AudioId) - named cartridge shine warning identity
    /// <summary>$91:F80F directional shinespark setup queues library three $0F.</summary>
    public static readonly SoundEffectId Launch = new(SoundEffectLibrary.Library3, 0x0f); // magic-number-audit: allow(AudioId) - named cartridge spark launch identity
    /// <summary>$90:D2BA crash entry queues library one $35 before its echo sound.</summary>
    public static readonly SoundEffectId CrashImpact = new(SoundEffectLibrary.Library1, 0x35); // magic-number-audit: allow(AudioId) - named cartridge spark impact identity
    /// <summary>$90:D2BA crash entry queues library three $10 after its impact sound.</summary>
    public static readonly SoundEffectId CrashEcho = new(SoundEffectLibrary.Library3, 0x10); // magic-number-audit: allow(AudioId) - named cartridge spark crash echo identity
}
