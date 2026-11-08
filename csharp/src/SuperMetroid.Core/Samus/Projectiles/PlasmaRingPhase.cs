namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The phase of one of the Plasma special beam attack's rings. Values are arbitrary.
/// </summary>
public enum PlasmaRingPhase
{
    /// <summary>The ring grows from its starting radius to its outer radius.</summary>
    Expanding,

    /// <summary>The ring shrinks from its outer radius toward Samus.</summary>
    Contracting,

    /// <summary>The ring grows outward again until it leaves the screen.</summary>
    FinalExpansion,
}
