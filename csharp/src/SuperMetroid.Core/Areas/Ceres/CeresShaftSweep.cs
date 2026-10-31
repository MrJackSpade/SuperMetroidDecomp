namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Which way the Ceres elevator shaft's turn is sweeping through its rotation steps.</summary>
public enum CeresShaftSweep
{
    /// <summary>Sweeping forward, from step 0 up to step 67.</summary>
    Forward,
    /// <summary>Sweeping backward, from step 68 down to step 1.</summary>
    Backward,
}
