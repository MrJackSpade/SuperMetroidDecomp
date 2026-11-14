using SuperMetroid.Core.Progression;
using static SuperMetroid.Core.Samus.Projectiles.SamusProjectileSoundRoutingDefinitions;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Which sound each projectile event plays.</summary>
public static class SamusProjectileSoundRouting
{
    /// <summary>
    /// The Samus-library sound a shot of <paramref name="combination"/> fires with, or none. A Spazer and Plasma
    /// combination fires with what follows the beam sound lists (<see cref="UnauthoredBeamSounds{TBeams, TSound}"/>).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The combination is not one Samus's beams can make.</exception>
    public static SoundEffectId? Resolve(bool charged, SamusBeamCombination combination) => combination.IsRetail
        ? (charged ? Charged : Uncharged)[combination]
        : UnauthoredSounds.Sound(combination, charged);
}
