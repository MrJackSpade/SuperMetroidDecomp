using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>The gunship takeoff's installed frames: their validation, the frame each transfer uploads, and their identity.</summary>
internal static class GunshipLiftoffArtwork
{
    /// <summary>Hashes the frame count and each frame's characters in takeoff order.</summary>
    internal static string ContentIdentity(RoomCharacterAtlas[] frames) =>
        SelectedPresentationHash.Create("enemy-gunship-liftoff-v1", content =>
        {
            content.Append("frames", frames.Length);
            foreach (RoomCharacterAtlas frame in frames)
                content.Append("tiles", frame.Characters.Span);
        });

    /// <summary>A copy of five complete native-size atlases.</summary>
    /// <exception cref="InvalidDataException">The frames are not five complete character frames.</exception>
    internal static RoomCharacterAtlas[] Validated(RoomCharacterAtlas[] frames)
    {
        Ensure.NotNull(frames);
        if (frames.Length != GunshipLiftoffTransferDefinitions.Frames.Length ||
            frames.Any(frame => frame is null ||
                frame.Characters.Length != GunshipLiftoffTransferDefinitions.ByteCount))
            throw new InvalidDataException("Gunship takeoff requires five complete character frames.");
        return frames.ToArray();
    }

    /// <summary>Returns the current editable characters for a typed pending VRAM transfer.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="asset"/> is not a takeoff transfer.</exception>
    internal static ReadOnlyMemory<byte> Resolve(RoomCharacterAtlas[] frames, InstalledArtworkId asset)
    {
        for (int index = 0; index < frames.Length; index++)
        {
            if (GunshipLiftoffTransferDefinitions.Frames[index].Asset == asset)
                return frames[index].Characters;
        }
        throw new ArgumentOutOfRangeException(nameof(asset), asset, "Not a gunship takeoff transfer.");
    }
}
