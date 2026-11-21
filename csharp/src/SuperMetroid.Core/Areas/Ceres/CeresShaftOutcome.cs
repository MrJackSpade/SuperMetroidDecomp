namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>What one update of the Ceres elevator shaft's main did (#2560).</summary>
public enum CeresShaftOutcome
{
    /// <summary>Nothing the update follows.</summary>
    Nothing,

    /// <summary>The elevator took Samus, which begins the departure.</summary>
    ElevatorTookSamus,
}
