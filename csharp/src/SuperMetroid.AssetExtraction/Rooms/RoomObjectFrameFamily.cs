using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// One room object whose frames the import pictures (#1557): its installed file, its name in diagnostics, each frame's
/// geometry and each frame's published name.
/// </summary>
/// <typeparam name="TFrame">The object's frame identity.</typeparam>
/// <param name="FileName">The installed file holding the frames' pictures.</param>
/// <param name="Name">The object's name in diagnostics.</param>
/// <param name="Layouts">Every frame's geometry, which its picture covers exactly.</param>
/// <param name="Ids">Every frame's published name in the installed file.</param>
internal sealed record RoomObjectFrameFamily<TFrame>(
    string FileName,
    string Name,
    IReadOnlyDictionary<TFrame, RoomObjectFrameLayout> Layouts,
    IReadOnlyDictionary<TFrame, string> Ids) where TFrame : struct, Enum
{
    /// <summary>The geometry of <paramref name="draw"/>, which must be one of this object's frames.</summary>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> is not one of this object's frames.</exception>
    internal RoomObjectFrameLayout LayoutOf(RoomObjectDraw draw) =>
        Layouts.TryGetValue(RoomObjectDraw.FrameOf<TFrame>(draw, Name), out RoomObjectFrameLayout layout)
            ? layout
            : throw new InvalidDataException($"{Name} has no frame {draw}.");
}
