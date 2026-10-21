namespace SuperMetroid.Verification.Enemies.PipeBugs;

/// <summary>The counter and rising of one pipe bug behavior expected.</summary>
/// <param name="Counter">The counter.</param>
/// <param name="Rising">The rising.</param>
internal readonly record struct PipeBugBehaviorExpected(int Counter, bool Rising);
