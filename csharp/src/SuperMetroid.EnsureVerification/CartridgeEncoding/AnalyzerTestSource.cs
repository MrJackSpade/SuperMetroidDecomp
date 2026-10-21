namespace SuperMetroid.EnsureVerification.CartridgeEncoding;

/// <summary>A source file an analyzer test compiles.</summary>
/// <param name="Path">The path.</param>
/// <param name="Source">The source.</param>
internal readonly record struct AnalyzerTestSource(string Path, string Source);
