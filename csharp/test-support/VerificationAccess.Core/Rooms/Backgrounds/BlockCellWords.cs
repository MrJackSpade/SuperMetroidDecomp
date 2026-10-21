namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>The four tilemap words of a block.</summary>
/// <param name="TopLeft">The top left.</param>
/// <param name="TopRight">The top right.</param>
/// <param name="BottomLeft">The bottom left.</param>
/// <param name="BottomRight">The bottom right.</param>
internal readonly record struct BlockCellWords(ushort TopLeft, ushort TopRight, ushort BottomLeft, ushort BottomRight);
