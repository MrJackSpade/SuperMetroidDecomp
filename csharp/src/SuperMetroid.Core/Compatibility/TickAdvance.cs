
namespace SuperMetroid.Core.Compatibility;

/// <summary>A tick accumulator after one step, and whether the step overflowed it.</summary>
/// <param name="Next">The accumulator after the step, holding the remainder.</param>
/// <param name="Overflowed">Whether the step reached a full period, which is when the paced work runs.</param>
[CompatibilityService("a wrapping phase after one step, and whether the step wrapped it")]
public readonly record struct TickAdvance(TickAccumulator Next, bool Overflowed);
