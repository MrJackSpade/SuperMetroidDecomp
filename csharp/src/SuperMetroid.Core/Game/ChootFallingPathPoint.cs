namespace SuperMetroid.Core.Game;

/// <summary>One physical sample in an authored Choot falling path.</summary>
/// <param name="Offset">The signed room-pixel offset from the falling origin.</param>
internal readonly record struct ChootFallingPathPoint(Offset16<RoomPixels> Offset)
{
    /// <summary>The native <c>$8000</c> X sentinel that ends one path loop.</summary>
    internal bool IsTerminator => Offset.X == short.MinValue;
}
