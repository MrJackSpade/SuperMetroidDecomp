namespace SuperMetroid.Core.Assets;

/// <summary>Native facing-pair layout for Samus's landing and transition visual offsets.</summary>
internal static class SamusBodyPlacementDefinitions
{
    /// <summary>
    /// $91:B62D + pose*8: the stock visual origin shares the native real-pose byte
    /// with the already calculated physical projectile correction. Installed visual
    /// overrides remain independent. FD..FF adjacent instruction observations are excluded.
    /// </summary>
    internal static sbyte DefaultGraphicsYOffset(byte pose)
    {
        if (pose >= SamusBodyArtworkCatalog.PoseCount) throw new ArgumentOutOfRangeException(nameof(pose));
        return unchecked((sbyte)Game.SamusPoseProjectileOriginDefinitions.ReadYOffset(pose));
    }
    /// <summary>$90:8D28..8D37: normal/spin landing, each with right/left four-byte rows.</summary>
    internal const int LandingPoseBytes = 4;
    /// <summary>$90:8D28..8D37: sixteen true landing bytes; $90:8D38 PLB remains a separate REQUIRED observed byte.</summary>
    internal const int LandingDataBytes = 4 * LandingPoseBytes;
    /// <summary>$90:8D80..8D97: twelve transition poses with two bytes each, paired by facing.</summary>
    internal const int PosturePoseBytes = 2;
    /// <summary>$90:8D80..8D97: six right/left transition pairs, including two unused pairs.</summary>
    internal const int PostureDataBytes = 12 * PosturePoseBytes;

    /// <summary>Landing left-facing copies use their right-facing row; all first-row coordinates and the adjacent byte remain REQUIRED.</summary>
    internal static int LandingSourceIndex(int index)
    {
        if ((uint)index > LandingDataBytes) throw new ArgumentOutOfRangeException(nameof(index));
        return index == LandingDataBytes ? index : index / (2 * LandingPoseBytes) * (2 * LandingPoseBytes) + index % LandingPoseBytes;
    }

    /// <summary>Transition left-facing copies use their right-facing row; each first-facing coordinate remains REQUIRED.</summary>
    internal static int PostureSourceIndex(int index)
    {
        if ((uint)index >= PostureDataBytes) throw new ArgumentOutOfRangeException(nameof(index));
        return index / (2 * PosturePoseBytes) * (2 * PosturePoseBytes) + index % PosturePoseBytes;
    }
}
