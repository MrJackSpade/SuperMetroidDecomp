using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Five installed, palette-indexed takeoff frames resolved at accepted NMI.</summary>
public sealed class GunshipLiftoffArtworkCatalog
{

    /// <summary>Owned five-frame character atlases, each keyed by the takeoff transfer that uploads it.</summary>
    internal readonly IReadOnlyDictionary<InstalledArtworkId, RoomCharacterAtlas> frames;

    /// <summary>Validates five complete native-size atlases, one per takeoff transfer, and copies them.</summary>
    internal GunshipLiftoffArtworkCatalog(IReadOnlyDictionary<InstalledArtworkId, RoomCharacterAtlas> frames) => this.frames = GunshipLiftoffArtwork.Validated(frames);

    /// <summary>Returns the current editable characters for a typed pending VRAM transfer.</summary>
    public ReadOnlyMemory<byte> Resolve(InstalledArtworkId asset) => GunshipLiftoffArtwork.Resolve(frames, asset);
}
