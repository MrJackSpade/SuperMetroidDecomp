using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Firing delays, independent of projectile presentation assets.</summary>
internal static partial class SamusProjectileCooldownDefinitions
{
    /// <summary>Cooldown that Samus's shot cooldown holds while time is frozen.</summary>
    internal const int FrozenTime = 32;

    /// <summary>
    /// The updates every beam combination but Plasma and Ice waits after an uncharged shot fired with Charge
    /// equipped or on a fresh press.
    /// </summary>
    internal const int UnchargedShot = 15;

    /// <summary>The updates the Plasma and Ice combination waits after such an uncharged shot: its own faster cadence.</summary>
    internal const int PlasmaIceUnchargedShot = 12;

    /// <summary>
    /// The updates each combination equipment makes waits after an uncharged shot: the ordinary delay, but Plasma and Ice's
    /// faster cadence.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SamusBeamCombination, int> UnchargedShots = new Dictionary<SamusBeamCombination, int>
    {
        [SamusBeamCombination.Power] = UnchargedShot,
        [SamusBeamCombination.Wave] = UnchargedShot,
        [SamusBeamCombination.Ice] = UnchargedShot,
        [SamusBeamCombination.IceWave] = UnchargedShot,
        [SamusBeamCombination.Spazer] = UnchargedShot,
        [SamusBeamCombination.SpazerWave] = UnchargedShot,
        [SamusBeamCombination.SpazerIce] = UnchargedShot,
        [SamusBeamCombination.SpazerIceWave] = UnchargedShot,
        [SamusBeamCombination.Plasma] = UnchargedShot,
        [SamusBeamCombination.PlasmaWave] = UnchargedShot,
        [SamusBeamCombination.PlasmaIce] = PlasmaIceUnchargedShot,
        [SamusBeamCombination.PlasmaIceWave] = UnchargedShot,
    };

    /// <summary>
    /// The updates every beam combination waits after a charged shot or a special attack, double the ordinary
    /// uncharged delay.
    /// </summary>
    internal const int ChargedShot = 30;

    /// <summary>The updates every beam combination waits after an uncharged shot repeated by holding the button without Charge.</summary>
    internal const int HeldFireShot = 25;

    /// <summary>
    /// The cooldown a beam, Hyper Beam or missile shot sets as it reserves its place, before its ammunition and a free
    /// place are checked: a refused shot still waits it out.
    /// </summary>
    internal const int FireAttempt = 1;

    /// <summary>The updates Samus waits after a Hyper Beam shot.</summary>
    internal const int HyperBeamShot = 21;

    /// <summary>The updates Samus waits after a missile.</summary>
    internal const int MissileShot = 10;

    /// <summary>The updates Samus waits after a Super Missile.</summary>
    internal const int SuperMissileShot = 20;

    /// <summary>The cooldown each update of a special attack's particles holds, so no shot fires while they fly.</summary>
    internal const int SpecialAttackHold = 2;

    /// <summary>The cooldown a shot striking a wall shortens a longer shared cooldown to; a shorter one is left as it is.</summary>
    internal const int WallImpactCooldownCap = 20;
}
