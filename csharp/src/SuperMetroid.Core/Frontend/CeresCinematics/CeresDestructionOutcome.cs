namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>Whether Samus escaped before Ceres exploded.</summary>
public enum CeresDestructionOutcome
{
    /// <summary>Ceres explodes after Samus escaped: the gunship flies to Zebes.</summary>
    SamusEscaped,

    /// <summary>The timer expired with Samus aboard, ending the game.</summary>
    SamusAboard,
}
