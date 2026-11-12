using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction.Rooms.Artwork;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Development-tool loading of <see cref="SamusArmCannonSourceArtwork"/>; never linked by player hosts.</summary>
internal static class InstalledSamusArmCannonArtworkTooling
{
    /// <summary>The cover placement of validated placement JSON.</summary>
    public static SamusArmCannonSourceArtwork Load(Stream json)
    {
        Ensure.NotNull(json);
        return SamusArmCannonSourceArtwork.FromPlacement(SamusArmCannonSourceArtwork.LoadPlacement(json));
    }
}
