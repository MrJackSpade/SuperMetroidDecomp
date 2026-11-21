namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// One room object whose frames the import pictures (#1557): its installed file, its name in diagnostics, and one list of
/// its frames, each with its geometry and published name (#2565). The frames are exactly <paramref name="Frames"/>,
/// written to the installed file in that listed order (#2567), never in a dictionary's enumeration order.
/// </summary>
/// <typeparam name="TFrame">
/// The object's frame identity: its frame enum, or the <see cref="Core.Rooms.RoomObjects.RoomObjectDraw"/> of a family
/// whose frames are typed records or span several frame types.
/// </typeparam>
/// <param name="FileName">The installed file holding the frames' pictures.</param>
/// <param name="Name">The object's name in diagnostics.</param>
/// <param name="Frames">Every frame with the geometry its picture covers exactly and its published name, in the installed file's order.</param>
internal sealed record RoomObjectFrameFamily<TFrame>(
    string FileName,
    string Name,
    IReadOnlyList<RoomObjectFamilyFrame<TFrame>> Frames) where TFrame : notnull
{
    /// <summary>Every frame's entry, looked up by frame; a frame listed twice is rejected.</summary>
    private readonly Dictionary<TFrame, RoomObjectFamilyFrame<TFrame>> byFrame = Frames.ToDictionary(frame => frame.Frame);

    /// <summary>Every frame's geometry, looked up by frame.</summary>
    public IReadOnlyDictionary<TFrame, RoomObjectFrameLayout> Layouts => byFrame.ToDictionary(entry => entry.Key, entry => entry.Value.Layout);

    /// <summary>Every frame's published name, looked up by frame.</summary>
    public IReadOnlyDictionary<TFrame, string> Ids => byFrame.ToDictionary(entry => entry.Key, entry => entry.Value.Id);
}
