namespace SuperMetroid.RenderVerification.Harness;

/// <summary>The time and allocation of one profiled simulation step.</summary>
/// <param name="Capture">The capture.</param>
/// <param name="Audio">The audio.</param>
/// <param name="Total">The total.</param>
/// <param name="Allocation">The allocation.</param>
internal readonly record struct SimulationStepCost(double Capture, double Audio, double Total, long Allocation);
