using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.World;

/// <summary>
/// The installed pictures of the room's room-object blocks (#2034): every family's pictures under the draws that show them,
/// composed from each family's catalog (#2571), and the blocks setups write of their own accord. They are built from the
/// installation's whole set of room-object pictures (#2290), which a rebinding replaces whole.
/// </summary>
/// <remarks>
/// The families live in Rooms, Bosses, Enemies and Areas, which World composes: each family's catalog lists its own
/// pictures by draw (<see cref="IRoomObjectFamilyPictures"/>), and World binds them here, so no family finds its catalog by
/// its type.
/// </remarks>
internal sealed class RoomObjectVisuals
{
    /// <summary>Every family's pictures, each under the draw that shows it.</summary>
    private RoomObjectFrameImages<RoomObjectDraw> _pictures;

    /// <summary>The pictures of the blocks setups and reactions write of their own accord.</summary>
    private InstalledRoomObjectSetupBlocks _setupBlocks;

    /// <summary>Binds the installation's whole set of room-object pictures.</summary>
    /// <param name="artwork">Every family's installed pictures.</param>
    /// <exception cref="ArgumentException">Two catalogs picture the same draw.</exception>
    internal RoomObjectVisuals(RoomObjectArtwork artwork)
    {
        _pictures = PicturesOf(artwork);
        _setupBlocks = artwork.RoomObjectSetupBlocks;
    }

    /// <summary>The installed picture <paramref name="draw"/> shows.</summary>
    /// <exception cref="InvalidDataException">No installed picture of the draw.</exception>
    internal RoomObjectFrameImage Image(RoomObjectDraw draw) => _pictures.Image(Ensure.NotNull(draw));

    /// <summary>What <paramref name="block"/>, a block a setup or a reaction writes of its own accord, shows.</summary>
    /// <exception cref="InvalidDataException">The installation lacks the block.</exception>
    internal RoomBlockAppearance SetupBlock(RoomObjectSetupBlock block) => _setupBlocks.Shown(block);

    /// <summary>Replaces the whole set with <paramref name="artwork"/>, as a host binding another installation's artwork does.</summary>
    /// <param name="artwork">Every family's installed pictures.</param>
    /// <exception cref="ArgumentException">Two catalogs picture the same draw.</exception>
    internal void Rebind(RoomObjectArtwork artwork)
    {
        _pictures = PicturesOf(artwork);
        _setupBlocks = artwork.RoomObjectSetupBlocks;
    }

    /// <summary>Every picture of <paramref name="artwork"/>'s families, each under the draw that shows it.</summary>
    /// <exception cref="ArgumentException">Two catalogs picture the same draw.</exception>
    private static RoomObjectFrameImages<RoomObjectDraw> PicturesOf(RoomObjectArtwork artwork) =>
        new(Ensure.NotNull(artwork).FamilyPictures.SelectMany(catalog => catalog.Pictures)
            .ToDictionary(picture => picture.Key, picture => picture.Value));
}
