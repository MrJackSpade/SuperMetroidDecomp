namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The Ceres elevator arrival projectiles' typed programs (#641).</summary>
internal enum CeresElevatorArrivalSegment
{
    /// <summary>The moving pad's two-pose loop.</summary>
    Pad,
    /// <summary>The stationary platform's one-pose loop.</summary>
    Platform,
    /// <summary>The shared deletion that landing redirects both projectiles to.</summary>
    Delete,
}
