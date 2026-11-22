using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The gunship takeoff's installed frames: their validation, the frame each transfer uploads, and their identity.</summary>
internal static partial class GunshipLiftoffArtwork
{

    /// <summary>A copy of five complete native-size atlases, each keyed by the takeoff transfer that uploads it.</summary>
    /// <exception cref="InvalidDataException">The frames are not one complete character frame for each takeoff transfer.</exception>
    internal static Dictionary<InstalledArtworkId, RoomCharacterAtlas> Validated(IReadOnlyDictionary<InstalledArtworkId, RoomCharacterAtlas> frames)
    {
        Ensure.NotNull(frames);
        if (frames.Count != GunshipLiftoffTransferDefinitions.Frames.Length)
            throw new InvalidDataException("Gunship takeoff requires five complete character frames.");
        var validated = new Dictionary<InstalledArtworkId, RoomCharacterAtlas>();
        foreach (var transfer in GunshipLiftoffTransferDefinitions.Frames)
        {
            if (!frames.TryGetValue(transfer.Asset, out RoomCharacterAtlas? frame) ||
                frame.Characters.Length != GunshipLiftoffCharacterDefinitions.ByteCount)
                throw new InvalidDataException($"Gunship takeoff requires a complete character frame for {transfer.Asset}.");
            validated.Add(transfer.Asset, frame);
        }
        return validated;
    }

    /// <summary>Returns the current editable characters for a typed pending VRAM transfer.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="asset"/> is not a takeoff transfer.</exception>
    internal static ReadOnlyMemory<byte> Resolve(IReadOnlyDictionary<InstalledArtworkId, RoomCharacterAtlas> frames, InstalledArtworkId asset) =>
        frames.TryGetValue(asset, out RoomCharacterAtlas? frame)
            ? frame.Characters
            : throw new ArgumentOutOfRangeException(nameof(asset), asset, "Not a gunship takeoff transfer.");
}
