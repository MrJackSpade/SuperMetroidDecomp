namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Which way the Ceres elevator shaft's turn is sweeping through its rotation steps.</summary>
public enum CeresShaftSweep
{
    /// <summary>Sweeping clockwise, from the full anticlockwise tilt to one step short of the full clockwise tilt.</summary>
    Forward,
    /// <summary>Sweeping anticlockwise, from the full clockwise tilt to one step short of the full anticlockwise tilt.</summary>
    Backward,
}
