using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Five installed, palette-indexed takeoff frames resolved at accepted NMI.</summary>
public sealed class GunshipLiftoffArtworkCatalog
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("enemy-gunship-liftoff-v1", content =>
        {
            content.Append("frames", frames.Length);
            foreach (RoomCharacterAtlas frame in frames)
                content.Append("tiles", frame.Characters.Span);
        });

    /// <summary>Owned five-frame character atlases indexed by the fixed takeoff transfer order.</summary>
    private readonly RoomCharacterAtlas[] frames;

    /// <summary>Validates five complete native-size atlases and copies the caller's frame array.</summary>
    internal GunshipLiftoffArtworkCatalog(RoomCharacterAtlas[] frames)
    {
        Ensure.NotNull(frames);
        if (frames.Length != GunshipLiftoffTransferDefinitions.Frames.Length ||
            frames.Any(frame => frame is null ||
                frame.Characters.Length != GunshipLiftoffTransferDefinitions.ByteCount))
            throw new InvalidDataException("Gunship takeoff requires five complete character frames.");
        this.frames = frames.ToArray();
    }

    /// <summary>Returns the current editable characters for a typed pending VRAM transfer.</summary>
    public ReadOnlyMemory<byte> Resolve(VramAssetId asset)
    {
        for (int index = 0; index < frames.Length; index++)
        {
            if (GunshipLiftoffTransferDefinitions.Frames[index].Asset == asset)
                return frames[index].Characters;
        }
        throw new ArgumentOutOfRangeException(nameof(asset), asset, "Not a gunship takeoff transfer.");
    }
}
