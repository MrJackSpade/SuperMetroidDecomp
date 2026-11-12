using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Five installed, palette-indexed takeoff frames resolved at accepted NMI.</summary>
public sealed class GunshipLiftoffArtworkCatalog
{

    /// <summary>Owned five-frame character atlases indexed by the fixed takeoff transfer order.</summary>
    internal readonly RoomCharacterAtlas[] frames;

    /// <summary>Validates five complete native-size atlases and copies the caller's frame array.</summary>
    internal GunshipLiftoffArtworkCatalog(RoomCharacterAtlas[] frames) => this.frames = GunshipLiftoffArtwork.Validated(frames);

    /// <summary>Returns the current editable characters for a typed pending VRAM transfer.</summary>
    public ReadOnlyMemory<byte> Resolve(InstalledArtworkId asset) => GunshipLiftoffArtwork.Resolve(frames, asset);
}
