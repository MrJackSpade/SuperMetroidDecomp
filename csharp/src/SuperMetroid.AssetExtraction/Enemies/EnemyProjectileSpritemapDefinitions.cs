using SuperMetroid.Core.Areas.Ceres;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Cartridge visual identities translated for bank-$8D projectile drawing.</summary>
public static partial class EnemyProjectileSpritemapDefinitions
{
    /// <summary>Installed editable JSON filename for projectile OAM compositions and presentation-only timed-program frames.</summary>
    public const string FileName = "enemy-projectile-compositions.json";
    /// <summary>The installed names of the Skree/Metaree debris compositions their bank-$86 programs select.</summary>
    internal static readonly string[] FrameNames =
    [
        "skree_debris",
        "metaree_debris",
    ];

    /// <summary>
    /// The installed name of the composition of each pose's picture of the Ceres elevator arrival (#2425), which the
    /// document holds beside the projectile compositions.
    /// </summary>
    internal static readonly IReadOnlyDictionary<CeresElevatorArrivalPose, string> ElevatorArrivalFrameNames =
        new Dictionary<CeresElevatorArrivalPose, string>
        {
            [CeresElevatorArrivalPose.PadUnlit] = "ceres_elevator_pad_unlit",
            [CeresElevatorArrivalPose.PadLit] = "ceres_elevator_pad_lit",
            [CeresElevatorArrivalPose.Platform] = "ceres_elevator_platform",
        };

    /// <summary>Every composition the document's frames hold: the projectile compositions, then the Ceres elevator arrival's.</summary>
    internal static IReadOnlyList<string> DocumentFrameNames =>
        [.. FrameNames, .. CeresElevatorArrivalPoseKeys.ListedOrder.Select(pose => ElevatorArrivalFrameNames[pose])];
}
