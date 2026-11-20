using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The pictures each Zebetite form shows by its health (#2250), most health first: a barrier drains a step for each
/// band of health it loses below <see cref="ZebetiteDefinitions.MaximumHealth"/>, the last picture showing everything in its lowest band.
/// </summary>
internal static class ZebetiteHealthPictureDefinitions
{
    /// <summary>The least health of each band, most health first; each form shows one picture a band.</summary>
    private static readonly int[] BandMinimums = [800, 600, 400, 200, 0];

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

    /// <summary>A form's pictures: each of <paramref name="pictures"/> shown in its band of <see cref="BandMinimums"/>, in order.</summary>
    private static ZebetiteHealthPicture[] ByBand(params EnemyFrameId[] pictures) =>
        [.. BandMinimums.Zip(pictures, (minimum, picture) => new ZebetiteHealthPicture(minimum, picture))];
}
