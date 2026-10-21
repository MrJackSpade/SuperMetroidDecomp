namespace SuperMetroid.DebugRunner.Tools;

/// <summary>The offset that best aligns a capture with its target, and its score.</summary>
/// <param name="Offset">The offset.</param>
/// <param name="Score">The score.</param>
internal readonly record struct CaptureAlignment(int Offset, double Score);
