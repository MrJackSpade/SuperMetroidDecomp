using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Which shot a special attack's particle takes its damage and animation from.</summary>
internal abstract partial record BeamComboShotKind
{
    /// <summary>Allows only the shots below.</summary>
    private BeamComboShotKind()
    {
    }

    /// <summary>The charged beam's shot, whose animation and radius follow the shot's direction.</summary>
    internal static BeamComboShotKind Charged { get; } = new ChargedShot();

    /// <summary>The charged beam's shot.</summary>
    internal sealed partial record ChargedShot : BeamComboShotKind;

    /// <summary>An echo's shot, whose animation follows the shot's direction.</summary>
    /// <param name="Echo">The echo whose damage and animation the particle takes.</param>
    internal sealed partial record EchoShot(SamusBeamEcho Echo) : BeamComboShotKind;

    /// <summary>The special attack's own particle, whose animation ignores the shot's direction.</summary>
    /// <param name="Particles">The kind of the special attack's particles.</param>
    internal sealed partial record ComboShot(SamusProjectileKind Particles) : BeamComboShotKind;
}
