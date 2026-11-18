namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>A pose the Ceres elevator arrival's pad or platform shows, each its own picture (#2425).</summary>
internal enum CeresElevatorArrivalPose
{
    /// <summary>The moving pad with its lights out.</summary>
    PadUnlit,
    /// <summary>The moving pad with its lights lit.</summary>
    PadLit,
    /// <summary>The stationary platform.</summary>
    Platform,
}
