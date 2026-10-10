using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Development-tool members of <see cref="SamusArmCannonArtworkCatalog"/>; never linked by player hosts.</summary>
internal static class SamusArmCannonArtworkCatalogTooling
{
    /// <summary>Builds an arm-cannon artwork catalog by combining placement metadata with the PNG tile atlas used by its source pointers.</summary>
    /// <param name="json">Stream containing the serialized cannon placement record.</param>
    /// <param name="tilePng">PNG stream containing the room-character tiles required by the cannon artwork format.</param>
    /// <returns>The catalog pairing the validated placement with its decoded tile atlas.</returns>
    public static SamusArmCannonArtworkCatalog Load(Stream json, Stream tilePng)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentNullException.ThrowIfNull(tilePng);
        SamusArmCannonArtworkCatalog.Placement placement = SamusArmCannonArtworkCatalog.LoadPlacement(json);
        RoomCharacterAtlas tiles = RoomCharacterAtlas.Load(tilePng,
            SamusArmCannonArtworkFormat.TileSourcePointers.Length *
                SamusRenderingRomData.ArmCannon.TileUploadByteCount);
        return SamusArmCannonArtworkCatalog.FromPlacement(placement, tiles);
    }
}
