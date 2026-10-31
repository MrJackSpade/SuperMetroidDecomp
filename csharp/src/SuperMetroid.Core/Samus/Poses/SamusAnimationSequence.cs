namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// A run of frames in a Samus animation program: frames numbered from <paramref name="FirstFrame"/> are each shown for
/// their delay, and the frame after the last delay runs <paramref name="End"/>.
/// </summary>
/// <param name="FirstFrame">The number of the run's first frame in the pose's artwork.</param>
/// <param name="Delays">Each frame's delay, in ticks, in frame order.</param>
/// <param name="End">The instruction the frame after the delays runs.</param>
internal sealed record SamusAnimationSequence(int FirstFrame, IReadOnlyList<int> Delays, SamusAnimationInstruction End);
