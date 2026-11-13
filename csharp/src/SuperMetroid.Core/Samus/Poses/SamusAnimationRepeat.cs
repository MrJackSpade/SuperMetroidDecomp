namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Continues the animation from frame <paramref name="Frame"/> of the same animation: zero restarts it.</summary>
/// <param name="Frame">The frame the animation continues from, a delay of the same animation.</param>
internal sealed record SamusAnimationRepeat(int Frame) : SamusAnimationCue;
