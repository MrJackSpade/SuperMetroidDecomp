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
    internal const MotherBrainGlassDraw ShatteredImage = MotherBrainGlassDraw.RimGone;

    /// <summary>
    /// The first of the nine stages, each leading to the next; the image after the last is <see cref="ShatteredImage"/>.
    /// </summary>
    internal static MotherBrainGlassStage First { get; } =
        new MotherBrainGlassStage.Cracks(MotherBrainGlassDraw.Intact, 2,
        new MotherBrainGlassStage.Cracks(MotherBrainGlassDraw.PaneCracked, 4,
        new MotherBrainGlassStage.Bursts.Into(MotherBrainGlassDraw.PaneCrackedThrough, 6, new(
            [MotherBrainGlassShard.Right, MotherBrainGlassShard.Right, MotherBrainGlassShard.Right, MotherBrainGlassShard.Right],
            [MotherBrainGlassShard.Right, MotherBrainGlassShard.Right, MotherBrainGlassShard.Right, MotherBrainGlassShard.Right]),
        new MotherBrainGlassStage.Cracks(MotherBrainGlassDraw.PaneTransition, 8,
        new MotherBrainGlassStage.Cracks(MotherBrainGlassDraw.PaneShifted, 10,
        new MotherBrainGlassStage.Bursts.Into(MotherBrainGlassDraw.PaneShiftedCrumbling, 12, new(
            [MotherBrainGlassShard.Left, MotherBrainGlassShard.Left, MotherBrainGlassShard.Left, MotherBrainGlassShard.Left],
            [MotherBrainGlassShard.Left, MotherBrainGlassShard.Left, MotherBrainGlassShard.Left, MotherBrainGlassShard.Left]),
        new MotherBrainGlassStage.Bursts.Into(MotherBrainGlassDraw.PaneShiftedEmptying, 14, new(
            [MotherBrainGlassShard.Right, MotherBrainGlassShard.Right, MotherBrainGlassShard.Left, MotherBrainGlassShard.Left],
            [MotherBrainGlassShard.Centre, MotherBrainGlassShard.Centre, MotherBrainGlassShard.Centre, MotherBrainGlassShard.Centre]),
        new MotherBrainGlassStage.Bursts.Into(MotherBrainGlassDraw.RimBuckling, 16, new(
            [MotherBrainGlassShard.Left, MotherBrainGlassShard.Left, MotherBrainGlassShard.Centre, MotherBrainGlassShard.Centre],
            [MotherBrainGlassShard.Left, MotherBrainGlassShard.Left, MotherBrainGlassShard.Centre, MotherBrainGlassShard.Centre]),
        new MotherBrainGlassStage.Bursts.Last(MotherBrainGlassDraw.RimBursting, 18, new(
            [MotherBrainGlassShard.Left, MotherBrainGlassShard.Left, MotherBrainGlassShard.Centre, MotherBrainGlassShard.Centre],
            [MotherBrainGlassShard.Left, MotherBrainGlassShard.Left, MotherBrainGlassShard.Centre, MotherBrainGlassShard.Centre]))))))))));
}
