namespace SuperMetroid.Verification.Enemies.PipeBugs;

/// <summary>One Norfair Pipe Bug departure case: the updates its leader has counted, and whether its next one departs.</summary>
/// <param name="Waited">The formation updates the leader has counted since its role started its wait.</param>
/// <param name="Departs">Whether the leader's next update departs.</param>
internal readonly record struct VerifyPipeBugStateQuantitiesCase(int Waited, bool Departs);
