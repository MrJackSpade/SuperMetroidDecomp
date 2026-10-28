namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>
/// Mutually exclusive animation phases shared by Skree and Metaree. Native AI advances
/// through the first three values; the fourth selector is an unused authored list.
/// </summary>
public enum SkreeMetareeAnimationPhase
{
    /// <summary>Selector zero: Metaree $A3:8910 / Skree $A3:C65E, looping four idle poses at ten instruction updates each while the AI waits for Samus.</summary>
    Idling = 0,
    /// <summary>Selector one: Metaree $A3:8924 / Skree $A3:C672, a 16-update then eight-update wind-up that sets the attack-ready flag and sleeps until AI installs the dive list.</summary>
    PreparingAttack = 1,
    /// <summary>Selector two: Metaree $A3:8930 / Skree $A3:C67E, enables off-screen processing and loops four poses at two updates each; movement and impact transitions remain AI-owned.</summary>
    Diving = 2,
    /// <summary>Unused authored selector three: Metaree $A3:8946 / Skree $A3:C694, disables off-screen processing, displays one pose for one update, and sleeps; retail AI does not select it.</summary>
    StopAnimating = 3,
}
