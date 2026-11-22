using SuperMetroid.Core.Progression;
using static SuperMetroid.Core.Samus.Projectiles.SamusProjectileSoundRoutingDefinitions;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Which sound each projectile event plays.</summary>
public static partial class SamusProjectileSoundRouting
{
    /// <summary>
    /// The Samus-library sound a shot of <paramref name="combination"/> fires with, or none. Each Spazer and Plasma
    /// combination fires with the sound <see cref="UnauthoredBeamSounds{TBeams, TSound}"/> gives it.
    /// </summary>
    public static SoundEffectId? Resolve(bool charged, SamusBeamCombination combination) => combination.CanBeEquipped
        ? (charged ? Charged : Uncharged)[combination]
        : UnauthoredSounds.Sound(combination, charged);
}
