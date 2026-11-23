using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Gameplay;

/// <summary>The room's library background drawn from its image (#1392, #1649), where the background shows the picture it loaded.</summary>
public sealed partial record LibraryImageBackground : ShownRoomBackground
{
    /// <summary>Keeps the image, repeated across and down.</summary>
    /// <param name="image">The image, repeated across and down.</param>
    public LibraryImageBackground(RoomBackgroundImagePlane image) => Image = Ensure.NotNull(image);

    /// <summary>The image, repeated across and down.</summary>
    internal RoomBackgroundImagePlane Image { get; }

    /// <inheritdoc/>
    public override TResult Map<TResult>(IShownRoomBackgroundMapping<TResult> mapping) => Ensure.NotNull(mapping).LibraryImage(Image);
}
