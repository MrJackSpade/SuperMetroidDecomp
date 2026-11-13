namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>
/// Mutually exclusive animation phases shared by Skree and Metaree. The AI advances
/// through the first three values; the fourth is an unused authored animation.
/// </summary>
public enum SkreeMetareeAnimationPhase
{
    /// <summary>Loops four idle poses at ten animation updates each while the AI waits for Samus.</summary>
    Idling = 0,
    /// <summary>A 16-update then eight-update wind-up that sets the attack-ready flag and sleeps until AI installs the dive animation.</summary>
    PreparingAttack = 1,
    /// <summary>Enables off-screen processing and loops four poses at two updates each; movement and impact transitions remain AI-owned.</summary>
    Diving = 2,
    /// <summary>Unused authored animation: disables off-screen processing, displays one pose for one update, and sleeps; retail AI does not select it.</summary>
    StopAnimating = 3,
}
