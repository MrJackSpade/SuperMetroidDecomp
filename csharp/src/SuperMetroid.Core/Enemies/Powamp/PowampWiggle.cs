namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// One of the twelve sideways offsets of a rising Powamp's wiggle (#2375), in the order the body and balloon take them:
/// a three-pixel triangle wave, centred where the offset is zero; the last is followed by the first.
/// </summary>
public readonly record struct PowampWiggle
{
    /// <summary>The offset at <paramref name="step"/> of the cycle.</summary>
    private PowampWiggle(int step) => Step = step;

    /// <summary>The offset's place in the cycle, from zero.</summary>
    private int Step { get; }

    /// <summary>Every offset of the cycle, in the order the Powamp takes them.</summary>
    internal static IReadOnlyList<PowampWiggle> All { get; } =
        [.. Enumerable.Range(0, PowampMotionDefinitions.WiggleOffsets.Length).Select(step => new PowampWiggle(step))];

    /// <summary>The cycle's first offset, the centre, where a new body starts.</summary>
    public static PowampWiggle First => All[0];

    /// <summary>How far right of the balloon's spawn the body and balloon stand, in whole pixels; negative to the left.</summary>
    public int Offset => PowampMotionDefinitions.WiggleOffsets[Step];

    /// <summary>Whether the wiggle stands at its centre, where its offset is zero.</summary>
    public bool IsCentered => Offset == 0;

    /// <summary>The offset after this one; the last is followed by the first.</summary>
    public PowampWiggle Next => All[(Step + 1) % All.Count];
}
