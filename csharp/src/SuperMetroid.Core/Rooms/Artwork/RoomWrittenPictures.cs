using System.Collections.Immutable;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// What a room's written pictures are drawn from, bound to the room at its load (#1392, #1466, #2554): its tileset's written
/// picture sheet, over which Zebes's or Ceres's warning text is drawn, or none in a tileset nothing writes pictures over.
/// </summary>
public abstract class RoomWrittenPictures
{
    /// <summary>Only the two below exist.</summary>
    private RoomWrittenPictures()
    {
    }

    /// <summary>The pictures <paramref name="overlay"/> writes over the room, as the room's appearance shows them.</summary>
    /// <param name="overlay">The text written over the room.</param>
    /// <param name="appearance">The room's appearance.</param>
    internal abstract ImmutableArray<PlacedWrittenPicture> Resolve(RoomWarningText overlay, RoomAppearanceView appearance);

    /// <summary>The room's tileset's written picture sheet.</summary>
    /// <param name="pictures">The sheet's resolver.</param>
    public sealed class Sheet(WrittenPictureResolver pictures) : RoomWrittenPictures
    {
        /// <summary>The sheet's resolver.</summary>
        private readonly WrittenPictureResolver _pictures = Ensure.NotNull(pictures);

        /// <inheritdoc/>
        internal override ImmutableArray<PlacedWrittenPicture> Resolve(RoomWarningText overlay, RoomAppearanceView appearance) =>
            _pictures.Resolve(overlay, appearance);
    }

    /// <summary>A tileset nothing writes pictures over, which has no written picture sheet.</summary>
    /// <param name="room">The room's name, for the failure that names it.</param>
    public sealed class Absent(string room) : RoomWrittenPictures
    {
        /// <inheritdoc/>
        /// <exception cref="InvalidDataException">
        /// Always: text is written over a room whose tileset has no installed written picture sheet, which only a tileset
        /// with one is.
        /// </exception>
        internal override ImmutableArray<PlacedWrittenPicture> Resolve(RoomWarningText overlay, RoomAppearanceView appearance) =>
            throw new InvalidDataException($"{room} shows the warning {overlay} with no installed warning picture sheet.");
    }
}
