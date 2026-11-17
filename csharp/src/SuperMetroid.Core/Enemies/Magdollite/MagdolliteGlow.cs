using SuperMetroid.Core.Enemies.Magdollite.Artwork;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// One frame of the room's lava glow (#2343), which every Magdollite and the lava they throw show at once; the glow steps
/// through its frames in order and the last is followed by the first.
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

    /// <summary>The cycle's first frame, which a restarted glow shows.</summary>
    public static MagdolliteGlow First => All[0];

    /// <summary>The frame after this one; the last is followed by the first.</summary>
    public MagdolliteGlow Next => All[(Step + 1) % All.Count];
}
