namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>Whether Samus escaped before Ceres exploded.</summary>
public enum CeresDestructionOutcome
{
    /// <summary>Game state $22: the gunship flies to Zebes.</summary>
    SamusEscaped,

    /// <summary>Game state $25: the timer expired with Samus aboard, ending the game.</summary>
    SamusAboard,
}
