namespace SuperMetroid.Core.Game;

/// <summary>One rectangular Botwoon hole and its four-pixel inset movement target.</summary>
/// <param name="TopLeft">The $B3:949B/$B3:949F left and top boundaries of the hole hitbox.</param>
internal readonly record struct BotwoonHoleDefinition(Position16<RoomPixels> TopLeft)
{
    /// <summary>
    /// $B3:949D/$B3:94A1 exclusive right and bottom boundaries: all four hole hitboxes are
    /// eight pixels square.
    /// </summary>
    internal Position16<RoomPixels> BottomRight => TopLeft + new Offset16<RoomPixels>(8, 8);

    /// <summary>The hole's centre, four pixels inside its top-left corner.</summary>
    internal Position16<RoomPixels> Target => TopLeft + new Offset16<RoomPixels>(4, 4);
}
