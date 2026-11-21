using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The pictures each Zebetite form shows by its health (#2250), most health first: a barrier drains a step for each
/// band of health it loses below <see cref="ZebetiteDefinitions.MaximumHealth"/>, the last picture showing everything in its lowest band.
/// </summary>
internal static class ZebetiteHealthPictureDefinitions
{
    /// <summary>The least health a barrier shows its intact picture at.</summary>
    private const int IntactMinimum = 800;

    /// <summary>The least health a barrier shows its drained picture at.</summary>
    private const int DrainedMinimum = 600;

    /// <summary>The least health a barrier shows its half-drained picture at.</summary>
    private const int HalfDrainedMinimum = 400;

    /// <summary>The least health a barrier shows its mostly drained picture at.</summary>
    private const int MostlyDrainedMinimum = 200;

    /// <summary>The least health a barrier shows its nearly drained picture at: any.</summary>
    private const int NearlyDrainedMinimum = 0;

    /// <summary>The pictures of each barrier form: the tall single barrier's, and the short ones of a split pair's halves.</summary>
    internal static IReadOnlyDictionary<ZebetiteBarrierForm, IReadOnlyList<ZebetiteHealthPicture>> ByForm { get; } =
        new Dictionary<ZebetiteBarrierForm, IReadOnlyList<ZebetiteHealthPicture>>
    {
        [ZebetiteBarrierForm.Single] = ByBand(
            EnemyFrameId.ZebetiteTallIntact, EnemyFrameId.ZebetiteTallDrained, EnemyFrameId.ZebetiteTallHalfDrained,
            EnemyFrameId.ZebetiteTallMostlyDrained, EnemyFrameId.ZebetiteTallNearlyDrained),
        [ZebetiteBarrierForm.SplitPair] = ByBand(
            EnemyFrameId.ZebetiteShortIntact, EnemyFrameId.ZebetiteShortDrained, EnemyFrameId.ZebetiteShortHalfDrained,
            EnemyFrameId.ZebetiteShortMostlyDrained, EnemyFrameId.ZebetiteShortNearlyDrained),
    };

    /// <summary>A form's pictures, most health first, each shown from the least health of its band.</summary>
    private static ZebetiteHealthPicture[] ByBand(EnemyFrameId intact, EnemyFrameId drained, EnemyFrameId halfDrained,
        EnemyFrameId mostlyDrained, EnemyFrameId nearlyDrained) =>
    [
        new(IntactMinimum, intact), new(DrainedMinimum, drained), new(HalfDrainedMinimum, halfDrained),
        new(MostlyDrainedMinimum, mostlyDrained), new(NearlyDrainedMinimum, nearlyDrained),
    ];
}
