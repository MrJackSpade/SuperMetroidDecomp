namespace SuperMetroid.Verification.Harness;

/// <summary>The argument and value of one bind case.</summary>
/// <param name="Argument">The argument.</param>
/// <param name="Value">The value.</param>
internal readonly record struct BindCase(string Argument, string Value);
