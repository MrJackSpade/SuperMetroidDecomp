namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Which paint draws a look a cap's sequence shows.</summary>
internal enum DoorCapShownPaint
{
    /// <summary>The cap's own paint.</summary>
    Own,
    /// <summary>The grey caps' paint, which draws the cleared opening every cap leaves.</summary>
    Grey,
    /// <summary>The blue caps' paint, which draws the cap a hit flashes.</summary>
    Blue,
}
