namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// One room object whose frames the import pictures (#1557): its installed file, its name in diagnostics, each frame's
/// geometry and each frame's published name. The frames are exactly the keys of <paramref name="Layouts"/>, written to
/// the installed file in that order.
/// </summary>
/// <typeparam name="TFrame">
/// The object's frame identity: its frame enum, or the <see cref="Core.Rooms.RoomObjects.RoomObjectDraw"/> of a family
/// whose frames are typed records or span several frame types.
/// </typeparam>
/// <param name="FileName">The installed file holding the frames' pictures.</param>
/// <param name="Name">The object's name in diagnostics.</param>
/// <param name="Layouts">Every frame's geometry, which its picture covers exactly.</param>
/// <param name="Ids">Every frame's published name in the installed file.</param>
internal sealed record RoomObjectFrameFamily<TFrame>(
    string FileName,
    string Name,
    IReadOnlyDictionary<TFrame, RoomObjectFrameLayout> Layouts,
    IReadOnlyDictionary<TFrame, string> Ids) where TFrame : notnull;
