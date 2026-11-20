using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The pictures each Zebetite form shows by its health (#2250), most health first: a barrier drains a step for each
/// band of health it loses below <see cref="ZebetiteDefinitions.MaximumHealth"/>, the last picture showing everything in its lowest band.
/// </summary>
internal static class ZebetiteHealthPictureDefinitions
{
    /// <summary>The pictures of each barrier form: the tall single barrier's, and the short ones of a split pair's halves.</summary>
    internal static IReadOnlyDictionary<ZebetiteBarrierForm, IReadOnlyList<ZebetiteHealthPicture>> ByForm { get; } =
        new Dictionary<ZebetiteBarrierForm, IReadOnlyList<ZebetiteHealthPicture>>
    {
        [ZebetiteBarrierForm.Single] =
        [
            new(800, EnemyFrameId.ZebetiteTallIntact),
            new(600, EnemyFrameId.ZebetiteTallDrained),
            new(400, EnemyFrameId.ZebetiteTallHalfDrained),
            new(200, EnemyFrameId.ZebetiteTallMostlyDrained),
            new(0, EnemyFrameId.ZebetiteTallNearlyDrained),
        ],
        [ZebetiteBarrierForm.SplitPair] =
        [
            new(800, EnemyFrameId.ZebetiteShortIntact),
            new(600, EnemyFrameId.ZebetiteShortDrained),
            new(400, EnemyFrameId.ZebetiteShortHalfDrained),
            new(200, EnemyFrameId.ZebetiteShortMostlyDrained),
            new(0, EnemyFrameId.ZebetiteShortNearlyDrained),
        ],
    };
}
