namespace SuperMetroid.Verification.Enemies.PipeBugs;

/// <summary>The before, after and departs of one verify pipe bug state quantities case.</summary>
/// <param name="Before">The before.</param>
/// <param name="After">The after.</param>
/// <param name="Departs">The departs.</param>
internal readonly record struct VerifyPipeBugStateQuantitiesCase(int Before, int After, bool Departs);
