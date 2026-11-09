namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The authored course of Mother Brain's glass (#1554): the cleared glass once the boss is dead, the shattered glass once
/// it was destroyed, and otherwise nine stages, each holding until the glass has taken two more hits; later stages
/// throw shards as they break, and the last shows the shattered glass before the destroyed event.
/// </summary>
internal static class MotherBrainGlassStageDefinitions
{
    /// <summary>How long a burst's in-between image shows.</summary>
    internal const int BurstFrameDuration = 4;

    /// <summary>How long the shattered glass shows after the last burst, before the destroyed event.</summary>
    internal const int FinalShatterDuration = 48;

    /// <summary>What the glass shows for one update once Mother Brain is dead.</summary>
    internal const MotherBrainGlassDraw ClearedImage = MotherBrainGlassDraw.Cleared;

    /// <summary>What the glass shows for one update once it was destroyed on an earlier visit, and after its last stage.</summary>
    internal const MotherBrainGlassDraw ShatteredImage = MotherBrainGlassDraw.Shatter3;

    /// <summary>The nine stages in order; the image after the last is <see cref="ShatteredImage"/>.</summary>
    internal static IReadOnlyList<MotherBrainGlassStage> Stages { get; } =
    [
        new(MotherBrainGlassDraw.Initial, 2, null),
        new(MotherBrainGlassDraw.PaneDamage1, 4, null),
        new(MotherBrainGlassDraw.PaneDamage2, 6, new(
            [MotherBrainGlassShard.Right, MotherBrainGlassShard.Right, MotherBrainGlassShard.Right, MotherBrainGlassShard.Right],
            [MotherBrainGlassShard.Right, MotherBrainGlassShard.Right, MotherBrainGlassShard.Right, MotherBrainGlassShard.Right])),
        new(MotherBrainGlassDraw.PaneTransition, 8, null),
        new(MotherBrainGlassDraw.ShiftedPane1, 10, null),
        new(MotherBrainGlassDraw.ShiftedPane2, 12, new(
            [MotherBrainGlassShard.Left, MotherBrainGlassShard.Left, MotherBrainGlassShard.Left, MotherBrainGlassShard.Left],
            [MotherBrainGlassShard.Left, MotherBrainGlassShard.Left, MotherBrainGlassShard.Left, MotherBrainGlassShard.Left])),
        new(MotherBrainGlassDraw.ShiftedPane3, 14, new(
            [MotherBrainGlassShard.Right, MotherBrainGlassShard.Right, MotherBrainGlassShard.Left, MotherBrainGlassShard.Left],
            [MotherBrainGlassShard.Centre, MotherBrainGlassShard.Centre, MotherBrainGlassShard.Centre, MotherBrainGlassShard.Centre])),
        new(MotherBrainGlassDraw.Shatter1, 16, new(
            [MotherBrainGlassShard.Left, MotherBrainGlassShard.Left, MotherBrainGlassShard.Centre, MotherBrainGlassShard.Centre],
            [MotherBrainGlassShard.Left, MotherBrainGlassShard.Left, MotherBrainGlassShard.Centre, MotherBrainGlassShard.Centre])),
        new(MotherBrainGlassDraw.Shatter2, 18, new(
            [MotherBrainGlassShard.Left, MotherBrainGlassShard.Left, MotherBrainGlassShard.Centre, MotherBrainGlassShard.Centre],
            [MotherBrainGlassShard.Left, MotherBrainGlassShard.Left, MotherBrainGlassShard.Centre, MotherBrainGlassShard.Centre])),
    ];
}
