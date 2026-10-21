namespace SuperMetroid.EnsureVerification.CartridgeEncoding;

/// <summary>A diagnostic id and the message it should carry.</summary>
/// <param name="Id">The id.</param>
/// <param name="Message">The message.</param>
internal readonly record struct ExpectedDiagnostic(string Id, string Message);
