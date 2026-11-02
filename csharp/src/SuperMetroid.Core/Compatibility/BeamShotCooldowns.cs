namespace SuperMetroid.Core.Compatibility;

/// <summary>The cooldowns one beam combination's shots set the shared shot timer to, in updates (#1506).</summary>
/// <param name="Uncharged">After an uncharged shot fired with Charge equipped or on a fresh press.</param>
/// <param name="Charged">After a charged shot.</param>
/// <param name="HeldFire">After an uncharged shot repeated by holding the button without Charge.</param>
[CompatibilityService("the cooldowns a Spazer and Plasma beam's shots wait, read past the cooldown rows")]
public sealed record BeamShotCooldowns(int Uncharged, int Charged, int HeldFire)
{
    /// <summary>Which of a beam combination's cooldowns a shot sets.</summary>
    public enum Shot
    {
        /// <summary>An uncharged shot fired with Charge equipped or on a fresh press.</summary>
        Uncharged,
        /// <summary>A charged shot.</summary>
        Charged,
        /// <summary>An uncharged shot repeated by holding the button without Charge.</summary>
        HeldFire,
    }

    /// <summary>The cooldown <paramref name="shot"/> sets.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The shot is undefined.</exception>
    public int For(Shot shot) => shot switch
    {
        Shot.Uncharged => Uncharged,
        Shot.Charged => Charged,
        Shot.HeldFire => HeldFire,
        _ => throw new ArgumentOutOfRangeException(nameof(shot), shot, "Undefined beam shot."),
    };
}
