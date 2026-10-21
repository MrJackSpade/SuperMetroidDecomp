namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>The axis and side a mirrored-method token names.</summary>
/// <param name="Axis">The axis.</param>
/// <param name="Side">The side.</param>
public readonly record struct MirrorClassification(char Axis, char Side);
