namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// One room object whose frames the import pictures (#1557): its installed file, its name in diagnostics, each frame's
/// geometry and each frame's published name. The frames are exactly <paramref name="Frames"/>, written to the installed
/// file in that listed order (#2567), never in a dictionary's enumeration order.
/// </summary>
/// <typeparam name="TFrame">
/// The object's frame identity: its frame enum, or the <see cref="Core.Rooms.RoomObjects.RoomObjectDraw"/> of a family
/// whose frames are typed records or span several frame types.
/// </typeparam>
/// <param name="FileName">The installed file holding the frames' pictures.</param>
/// <param name="Name">The object's name in diagnostics.</param>
/// <param name="Frames">Every frame with the geometry its picture covers exactly, in the installed file's order.</param>
/// <param name="Ids">Every frame's published name in the installed file.</param>
internal sealed record RoomObjectFrameFamily<TFrame>(
    string FileName,
    string Name,
    IReadOnlyList<RoomObjectFamilyFrame<TFrame>> Frames,
    IReadOnlyDictionary<TFrame, string> Ids) where TFrame : notnull
{
    /// <summary>Every frame's geometry, looked up by frame; a frame listed twice is rejected.</summary>
    public IReadOnlyDictionary<TFrame, RoomObjectFrameLayout> Layouts { get; } =
        Frames.ToDictionary(frame => frame.Frame, frame => frame.Layout);

    /// <summary>
    /// Every frame's geometry and published name as one entry, so a frame of the family always has both (#2554); a frame
    /// listed without its published name fails the family's construction.
    /// </summary>
    public IReadOnlyDictionary<TFrame, RoomObjectFrameEntry> Entries { get; } =
        Frames.ToDictionary(frame => frame.Frame, frame => new RoomObjectFrameEntry(frame.Layout, Ids[frame.Frame]));
}

/// <summary>A room-object frame's geometry and the name it is published under.</summary>
/// <param name="Layout">The frame's geometry.</param>
/// <param name="Id">The frame's published name.</param>
internal sealed record RoomObjectFrameEntry(RoomObjectFrameLayout Layout, string Id);
