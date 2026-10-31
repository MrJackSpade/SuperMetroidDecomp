using SuperMetroid.Core.Samus.Poses;
using static SuperMetroid.Core.Samus.Artwork.SamusBodyPlacementDefinitions;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>How each stock pose frame places its body: graphics offsets, landing bytes, and the landing rows they share.</summary>
internal static class SamusBodyPlacements
{
    /// <summary>
    /// $91:B62D + pose*8: the stock visual origin shares the native real-pose byte
    /// with the already calculated physical projectile correction. Installed visual
    /// overrides remain independent. FD..FF adjacent instruction observations are excluded.
    /// </summary>
    internal static int DefaultGraphicsYOffset(SamusPoseId pose)
    {
        if (!SamusPoseNumbering.HasRecord(pose)) throw new ArgumentOutOfRangeException(nameof(pose));
        return WordWrap.SignedLowByte(SuperMetroid.Core.Samus.Poses.SamusPoseProjectileOrigins.ReadYOffset(pose));
    }

    /// <summary>Landing left-facing copies use their right-facing row; each source default follows the named landing/recovery phase.</summary>
    internal static int LandingSourceIndex(int index)
    {
        if ((uint)index > LandingDataBytes) throw new ArgumentOutOfRangeException(nameof(index));
        return index == LandingDataBytes ? index : index / (2 * LandingPoseBytes) * (2 * LandingPoseBytes) + index % LandingPoseBytes;
    }

    /// <summary>$90:8D28..8D38: named landing compression, standing recovery, inactive padding and adjacent instruction observation.</summary>
    internal static int DefaultLandingByte(int index)
    {
        int source = LandingSourceIndex(index);
        if (source == LandingDataBytes) return AdjacentLandingPlb;
        bool spin = source >= 2 * LandingPoseBytes;
        int frame = source % LandingPoseBytes;
        int recoveryFrame = spin ? 2 : 1;
        if (frame > recoveryFrame) return 0;
        SuperMetroid.Core.Samus.Poses.SamusPoseId pose = frame == recoveryFrame ? SuperMetroid.Core.Samus.Poses.SamusPoseId.FacingRightNormalPose
            : spin ? SuperMetroid.Core.Samus.Poses.SamusPoseId.SpinLandingRightPose : SuperMetroid.Core.Samus.Poses.SamusPoseId.NormalLandingRightPose;
        return WordWrap.LowByte(DefaultGraphicsYOffset(pose));
    }
}
