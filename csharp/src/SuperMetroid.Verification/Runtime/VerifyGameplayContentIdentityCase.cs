namespace SuperMetroid.Verification.Runtime;

/// <summary>The component and digest of one verify gameplay content identity case.</summary>
/// <param name="Component">The component.</param>
/// <param name="Digest">The digest.</param>
internal readonly record struct VerifyGameplayContentIdentityCase(string Component, string Digest);
