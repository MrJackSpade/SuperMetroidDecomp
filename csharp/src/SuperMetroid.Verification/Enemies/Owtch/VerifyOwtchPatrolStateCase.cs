using SuperMetroid.Core.Enemies.Owtch;

namespace SuperMetroid.Verification.Enemies.Owtch;

/// <summary>The depth, expected depth and expected behavior of one verify owtch patrol state case.</summary>
/// <param name="Depth">The depth.</param>
/// <param name="ExpectedDepth">The expected depth.</param>
/// <param name="ExpectedBehavior">The expected behavior.</param>
internal readonly record struct VerifyOwtchPatrolStateCase(int Depth, int ExpectedDepth, OwtchBehaviorState ExpectedBehavior);
