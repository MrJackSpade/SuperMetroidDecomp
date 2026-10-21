
namespace SuperMetroid.Core.Audio;

/// <summary>A tick accumulator after one step, and whether the step overflowed it.</summary>
/// <param name="Next">The next.</param>
/// <param name="Overflowed">The overflowed.</param>
public readonly record struct TickAdvance(TickAccumulator Next, bool Overflowed);
