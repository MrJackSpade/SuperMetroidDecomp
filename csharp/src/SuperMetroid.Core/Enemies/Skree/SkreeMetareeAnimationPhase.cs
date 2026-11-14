namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>
/// The animation phases Skree and Metaree share: the AI moves through idling, preparing and diving.
/// </summary>
public enum SkreeMetareeAnimationPhase
{
    /// <summary>Loops four idle poses at ten animation updates each while the AI waits for Samus.</summary>
    Idling,
    /// <summary>A 16-update then eight-update wind-up that sets the attack-ready flag and sleeps until AI installs the dive animation.</summary>
    PreparingAttack,
    /// <summary>Enables off-screen processing and loops four poses at two updates each; movement and impact transitions remain AI-owned.</summary>
    Diving,
}
