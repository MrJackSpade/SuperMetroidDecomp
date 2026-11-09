namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed pictures of one room object's frames (#1557), keyed by the frame's identity. The import pictures every
/// frame the object draws; there is no compiled copy to fall back on.
/// </summary>
/// <typeparam name="TFrame">
/// The object's frame identity: its frame enum, or the <see cref="RoomObjectDraw"/> of a family whose frames are typed
/// records or span several frame types.
/// </typeparam>
internal sealed class RoomObjectFrameImages<TFrame> where TFrame : notnull
{

    /// <summary>Holds the pictures of the frames, which the import has checked cover every frame the object draws.</summary>
    /// <param name="images">The picture of each frame, owned by these pictures.</param>
    internal RoomObjectFrameImages(IReadOnlyDictionary<TFrame, RoomObjectFrameImage> images) =>
        All = Ensure.NotNull(images);

    /// <summary>Every frame's picture.</summary>
    internal IReadOnlyDictionary<TFrame, RoomObjectFrameImage> All { get; }

    /// <summary>The picture of <paramref name="frame"/>.</summary>
    /// <exception cref="InvalidDataException">No picture of <paramref name="frame"/> is installed.</exception>
    internal RoomObjectFrameImage Image(TFrame frame) =>
        All.TryGetValue(frame, out RoomObjectFrameImage? image)
            ? image
            : throw new InvalidDataException($"No installed picture of {typeof(TFrame).Name} {frame}.");
}
