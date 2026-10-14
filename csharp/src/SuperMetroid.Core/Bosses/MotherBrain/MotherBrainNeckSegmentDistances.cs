namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The distances of Mother Brain's five neck segments from the body (#633).</summary>
internal sealed record MotherBrainNeckSegmentDistances
{
    /// <summary>Segment0 distance.</summary>
    public ushort Segment0Distance { get; init; } = 2;

    /// <summary>Segment1 distance.</summary>
    public ushort Segment1Distance { get; init; } = 10;

    /// <summary>Segment2 distance.</summary>
    public ushort Segment2Distance { get; init; } = 20;

    /// <summary>Segment3 distance.</summary>
    public ushort Segment3Distance { get; init; } = 10;

    /// <summary>Segment4 distance.</summary>
    public ushort Segment4Distance { get; init; } = 20;
}
