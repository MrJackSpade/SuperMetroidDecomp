using SuperMetroid.Core.Progression;
using static SuperMetroid.Core.Samus.Projectiles.SamusProjectileCooldownDefinitions;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The cooldown each beam combination's shots set the shared shot timer to.</summary>
internal static partial class SamusProjectileCooldowns
{
    /// <summary>
    /// The cooldown a beam combination's <paramref name="shot"/> sets the shared shot timer to. Beam charge doubles
    /// the ordinary delay (<see cref="SamusProjectileCooldownDefinitions.UnchargedShot"/>); uncharged Plasma and Ice has
    /// its own cadence (<see cref="SamusProjectileCooldownDefinitions.PlasmaIceUnchargedShot"/>); held fire repeats on
    /// its own (<see cref="SamusProjectileCooldownDefinitions.HeldFireShot"/>). A Spazer and Plasma combination's cooldowns are read past the authored ones, which
    /// <see cref="SuperMetroid.Core.Compatibility.UnauthoredBeamFiring{TBeams}"/> reproduces (#1506).
    /// </summary>
    /// <exception cref="InvalidDataException">The combination pairs Spazer with Plasma, which the cooldowns do not author.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The shot is undefined.</exception>
    internal static int Read(SamusBeamCombination beams, BeamShotCooldowns.Shot shot)
    {
        if (!beams.CanBeEquipped)
            throw new InvalidDataException($"{beams} pairs Spazer with Plasma, whose cooldowns are not authored.");
        return shot switch
        {
            BeamShotCooldowns.Shot.Uncharged => UnchargedShots[beams],
            BeamShotCooldowns.Shot.Charged => ChargedShot,
            BeamShotCooldowns.Shot.HeldFire => HeldFireShot,
            _ => throw new ArgumentOutOfRangeException(nameof(shot), shot, "Undefined beam shot."),
        };
    }
}
