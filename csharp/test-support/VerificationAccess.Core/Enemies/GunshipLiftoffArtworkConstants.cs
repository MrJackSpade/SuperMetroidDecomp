using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Cartridge values and helpers of <see cref="GunshipLiftoffArtwork"/> that only verification reads.</summary>
internal static class GunshipLiftoffArtworkConstants
{
    /// <summary>Hashes the frame count and each frame's characters in takeoff order.</summary>
    internal static string ContentIdentity(RoomCharacterAtlas[] frames) =>
        SelectedPresentationHash.Create("enemy-gunship-liftoff-v1", content =>
        {
            content.Append("frames", frames.Length);
            foreach (RoomCharacterAtlas frame in frames)
                content.Append("tiles", frame.Characters.Span);
        });
}
