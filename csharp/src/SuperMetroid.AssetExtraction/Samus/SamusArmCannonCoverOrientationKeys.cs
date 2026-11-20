using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The key of each <see cref="SamusArmCannonCoverOrientation"/> (#2556) the extracted arm-cannon document spells an aim's
/// cover orientation by, written out so renaming a member never changes it.
/// </summary>
internal static class SamusArmCannonCoverOrientationKeys
{
    /// <summary>Each SamusArmCannonCoverOrientation member's key.</summary>
    internal static InstalledKeys<SamusArmCannonCoverOrientation> Installed { get; } = new(new Dictionary<SamusArmCannonCoverOrientation, string>
    {
        [SamusArmCannonCoverOrientation.Vertical] = "Vertical",
        [SamusArmCannonCoverOrientation.Horizontal] = "Horizontal",
        [SamusArmCannonCoverOrientation.DownwardDiagonal] = "DownwardDiagonal",
        [SamusArmCannonCoverOrientation.UpwardDiagonal] = "UpwardDiagonal",
    });
}
