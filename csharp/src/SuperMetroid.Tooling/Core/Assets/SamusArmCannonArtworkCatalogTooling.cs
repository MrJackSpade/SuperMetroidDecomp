using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Development-tool members of <see cref="SamusArmCannonArtworkCatalog"/>; never linked by player hosts.</summary>
internal static class SamusArmCannonArtworkCatalogTooling
{
    /// <summary>Loads.</summary>
    public static SamusArmCannonArtworkCatalog Load(Stream json, Stream tilePng)
    {
        Ensure.NotNull(json);
        Ensure.NotNull(tilePng);
        SamusArmCannonArtworkCatalog.Placement placement = SamusArmCannonArtworkCatalog.LoadPlacement(json);
        RoomCharacterAtlas tiles = RoomCharacterAtlas.Load(tilePng,
            SamusArmCannonArtworkFormat.TileSourcePointers.Length *
                SamusRenderingRomData.ArmCannon.TileUploadByteCount);
        return SamusArmCannonArtworkCatalog.FromPlacement(placement, tiles);
    }
}
