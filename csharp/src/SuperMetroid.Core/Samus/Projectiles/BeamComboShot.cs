using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Which shot a special attack's particle takes its damage and animation from (#2554).</summary>
internal abstract record BeamComboShot
{
    /// <summary>Only the shots below are choices.</summary>
    private BeamComboShot()
    {
    }

    /// <summary>The particle of the special attack <paramref name="Attack"/>, whose animation ignores the shot's direction.</summary>
    /// <param name="Attack">The special attack, one whose particles have their own kind: the Wave, Spazer or Plasma attack.</param>
    internal sealed record SpecialAttack(SamusComboKind Attack) : BeamComboShot;

    /// <summary>The charged beam's shot, whose animation and radius follow the shot's direction.</summary>
    internal sealed record Charged : BeamComboShot;

    /// <summary>The echo <paramref name="Of"/>, whose animation follows the shot's direction.</summary>
    /// <param name="Of">The echo the particle is.</param>
    internal sealed record Echo(SamusBeamEcho Of) : BeamComboShot;
}
