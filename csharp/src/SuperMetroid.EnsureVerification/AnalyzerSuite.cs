namespace SuperMetroid.EnsureVerification;

/// <summary>A named analyzer verification suite.</summary>
/// <param name="Name">The name.</param>
/// <param name="Run">The run.</param>
internal readonly record struct AnalyzerSuite(string Name, Action Run);
