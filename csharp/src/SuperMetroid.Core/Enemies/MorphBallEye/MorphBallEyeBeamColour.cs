using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// One of the sixteen colours the full Morph Ball eye beam cycles through, one an update (#2343); the last is followed
/// by the first.
/// </summary>
public readonly partial record struct MorphBallEyeBeamColour
{
    /// <summary>The colour at <paramref name="step"/> of the cycle.</summary>
    private MorphBallEyeBeamColour(int step) => Step = step;

    /// <summary>The colour's place in the cycle, from zero.</summary>
    private int Step { get; }

    /// <summary>Every colour of the cycle, in order.</summary>
    internal static IReadOnlyList<MorphBallEyeBeamColour> All { get; } =
        [.. Enumerable.Range(0, MorphBallEyeBeamDefinitions.CycleColors.Count).Select(step => new MorphBallEyeBeamColour(step))];

    /// <summary>The cycle's first colour, which a newly full beam shows first.</summary>
    public static MorphBallEyeBeamColour First => All[0];

    /// <summary>The colour the beam adds.</summary>
    public RgbColor Colour => MorphBallEyeBeamDefinitions.CycleColors[Step];

    /// <summary>The colour after this one; the last is followed by the first.</summary>
    public MorphBallEyeBeamColour Next => All[(Step + 1) % All.Count];
}
