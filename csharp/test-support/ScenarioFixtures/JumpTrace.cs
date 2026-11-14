namespace SuperMetroid.TestSupport.ScenarioFixtures;

/// <summary>The rows a jump traced and its apex.</summary>
/// <param name="Rows">The rows.</param>
/// <param name="Apex">The highest position the jump reached: the least Y.</param>
internal readonly record struct JumpTrace(List<string> Rows, FixedCoordinate Apex);
