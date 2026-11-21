namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// One frame of a room object the import pictures, with the geometry its picture covers exactly (#2567) and the name its
/// picture is published under (#2565).
/// </summary>
/// <typeparam name="TFrame">The object's frame identity.</typeparam>
/// <param name="Frame">The frame.</param>
/// <param name="Layout">The frame's geometry as the cartridge writes it.</param>
/// <param name="Id">The frame's published name in the installed file.</param>
internal sealed record RoomObjectFamilyFrame<TFrame>(TFrame Frame, RoomObjectFrameLayout Layout, string Id) where TFrame : notnull;
