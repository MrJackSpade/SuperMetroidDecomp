using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's head entering a pose without showing it (#2399): his fight and death start each head animation on its opening
/// pose, and his growth resumes the roar on one of its poses, so. The head keeps the picture it shows while the entered pose
/// holds, and his mouth tests read the entered pose's shapes.
/// </summary>
/// <param name="Playback">The head animation, standing just past the entered pose.</param>
/// <param name="Delay">The entered pose's duration, in updates: the head holds the entered pose for it.</param>
internal readonly record struct KraidHeadEntry(AnimationPlayback<KraidHeadSegment> Playback, int Delay);
