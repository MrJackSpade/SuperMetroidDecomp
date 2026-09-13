using SuperMetroid.Core.Audio;

namespace SuperMetroid.Core.Rooms;

/// <summary>Sound identities used by bank-$84 room actors.</summary>
internal static class RoomPlmSounds
{
    /// <summary>Library two $37: station access arm extension.</summary>
    public static readonly SoundEffectId StationExtension = new(SoundEffectLibrary.Library2, 0x37); // magic-number-audit: allow(AudioId) - named cartridge SFX identity
    /// <summary>Library two $38: station access arm retraction.</summary>
    public static readonly SoundEffectId StationRetraction = new(SoundEffectLibrary.Library2, 0x38); // magic-number-audit: allow(AudioId) - named cartridge SFX identity
    /// <summary>Library three $2E: Mother Brain glass-shard instruction sound.</summary>
    public static readonly SoundEffectId MotherBrainGlassShattering = new(SoundEffectLibrary.Library3, 0x2e); // magic-number-audit: allow(AudioId) - named cartridge SFX identity
}
