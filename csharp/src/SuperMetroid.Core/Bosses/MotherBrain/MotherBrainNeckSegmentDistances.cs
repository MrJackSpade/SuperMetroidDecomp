namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The distances of Mother Brain's five neck segments from the body (#633).</summary>
internal sealed record MotherBrainNeckSegmentDistances
{
    /// <summary>Lower-neck joint 0's pixel distance from the body anchor.</summary>
    public int Segment0Distance { get; init; } = 2;

    /// <summary>Lower-neck joint 1's pixel distance from the body anchor.</summary>
    public int Segment1Distance { get; init; } = 10;

    /// <summary>Lower-neck joint 2's pixel distance from the body anchor; joint 2 anchors the upper half.</summary>
    public int Segment2Distance { get; init; } = 20;

    /// <summary>Upper-neck joint 3's pixel distance from joint 2.</summary>
    public int Segment3Distance { get; init; } = 10;

    /// <summary>Upper-neck joint 4's pixel distance from joint 2; its position anchors the brain.</summary>
    public int Segment4Distance { get; init; } = 20;
}
