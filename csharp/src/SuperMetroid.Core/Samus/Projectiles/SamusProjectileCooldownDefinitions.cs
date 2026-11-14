namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Firing delays, independent of projectile presentation assets.</summary>
internal static class SamusProjectileCooldownDefinitions
{
    /// <summary>Cooldown that Samus's shot cooldown holds while time is frozen.</summary>
    internal const int FrozenTime = 32;

    /// <summary>The longest cooldown any shot, bomb or boss sets the shared shot cooldown to.</summary>
    internal const int LongestCooldown = 255;

    /// <summary>
    /// The updates every retail beam combination but Plasma and Ice waits after an uncharged shot fired with Charge
    /// equipped or on a fresh press.
    /// </summary>
    internal const int UnchargedShot = 15;

    /// <summary>The updates the Plasma and Ice combination waits after such an uncharged shot: its own faster cadence.</summary>
    internal const int PlasmaIceUnchargedShot = 12;

    /// <summary>
    /// The updates every retail beam combination waits after a charged shot or a special attack, double the ordinary
    /// uncharged delay.
    /// </summary>
    internal const int ChargedShot = 30;

    /// <summary>The updates every retail beam combination waits after an uncharged shot repeated by holding the button without Charge.</summary>
    internal const int HeldFireShot = 25;
}
