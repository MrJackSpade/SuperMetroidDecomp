using SuperMetroid.Core.Enemies.Magdollite.Artwork;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// One frame of the room's lava glow (#2343), which every Magdollite and the lava they throw show at once; the glow plays
/// its frames in order, the last followed by the first (<see cref="MagdolliteGlowAnimations"/>, #2439).
/// </summary>
public readonly record struct MagdolliteGlow
{
    /// <summary>The frame at <paramref name="step"/> of the cycle.</summary>
    private MagdolliteGlow(int step) => Step = step;

    /// <summary>The frame's place in the cycle, from zero: the step of its installed shade.</summary>
    public int Step { get; }

    /// <summary>Every frame of the cycle, in order.</summary>
    public static IReadOnlyList<MagdolliteGlow> All { get; } =
        [.. Enumerable.Range(0, MagdolliteShades.GlowFrames).Select(step => new MagdolliteGlow(step))];
}
