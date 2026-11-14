using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>Reads the installed actor layouts unique to the destruction of Ceres and the following Zebes reveal.</summary>
public static class CeresDestructionArtworkFile
{
    /// <summary>Loads the destruction's actor layouts, validating each.</summary>
    /// <param name="revealActorsJson">The initial Zebes-reveal placements, one for each reveal role.</param>
    /// <param name="destructionActorsJson">The initial destruction-background placements, one for each destruction role.</param>
    /// <returns>The compiled placements; both input streams are consumed from their current positions and remain caller-owned.</returns>
    public static CeresDestructionArtwork Read(Stream revealActorsJson, Stream destructionActorsJson)
    {
        Ensure.NotNull(revealActorsJson);
        Ensure.NotNull(destructionActorsJson);
        return new CeresDestructionArtwork(CeresRevealActorLayoutFile.Read(revealActorsJson),
            CeresDestructionActorLayoutFile.Read(destructionActorsJson));
    }
}
