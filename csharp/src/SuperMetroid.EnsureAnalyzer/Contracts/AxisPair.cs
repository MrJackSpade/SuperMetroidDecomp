namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>Two member names that are the components of one axis.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
/// <param name="Axis">The axis.</param>
public readonly record struct AxisPair(string First, string Second, string Axis);
