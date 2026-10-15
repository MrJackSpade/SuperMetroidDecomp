namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The Ceres elevator arrival projectiles' typed programs (#641), in place of their bank-$86 lists.</summary>
internal enum CeresElevatorArrivalProgram
{
    /// <summary>The moving pad's two-pose loop at $86:A28D.</summary>
    Pad,
    /// <summary>The stationary platform's one-pose loop at $86:A299.</summary>
    Platform,
    /// <summary>The shared delete at $86:A28B that landing redirects both projectiles to.</summary>
    Delete,
}
