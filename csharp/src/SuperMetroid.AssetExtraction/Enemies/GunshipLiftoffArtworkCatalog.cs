using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Five installed, palette-indexed takeoff frames resolved at accepted NMI.</summary>
public sealed class GunshipLiftoffArtworkCatalog
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => GunshipLiftoffArtwork.ContentIdentity(frames);

    /// <summary>Owned five-frame character atlases indexed by the fixed takeoff transfer order.</summary>
    private readonly RoomCharacterAtlas[] frames;

    /// <summary>Validates five complete native-size atlases and copies the caller's frame array.</summary>
    internal GunshipLiftoffArtworkCatalog(RoomCharacterAtlas[] frames) => this.frames = GunshipLiftoffArtwork.Validated(frames);

    /// <summary>Returns the current editable characters for a typed pending VRAM transfer.</summary>
    public ReadOnlyMemory<byte> Resolve(InstalledArtworkId asset) => GunshipLiftoffArtwork.Resolve(frames, asset);
}
