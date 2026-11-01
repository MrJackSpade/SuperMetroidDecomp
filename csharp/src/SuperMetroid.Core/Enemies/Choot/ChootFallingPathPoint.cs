namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>One physical sample in an authored Choot falling path.</summary>
/// <param name="Offset">The signed room-pixel offset from the falling origin.</param>
internal readonly record struct ChootFallingPathPoint(Offset16<RoomPixels> Offset)
{
    /// <summary>The -32768 X sentinel that ends one path loop.</summary>
    internal bool IsTerminator => Offset.X == short.MinValue;
}
