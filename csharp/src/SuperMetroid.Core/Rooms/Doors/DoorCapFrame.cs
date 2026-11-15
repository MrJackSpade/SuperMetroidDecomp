namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>What a coloured or grey cap's sequence shows (#1296, #1994); colour and facing come from the cap.</summary>
internal enum DoorCapFrame
{
    /// <summary>The cleared opening.</summary>
    Clear,
    /// <summary>The closed cap.</summary>
    Closed,
    /// <summary>The cap just starting to slide aside.</summary>
    Ajar,
    /// <summary>The cap half slid aside.</summary>
    HalfOpen,
    /// <summary>The cap nearly slid aside.</summary>
    NearlyOpen,
    /// <summary>The blue cap a hit flashes.</summary>
    BlueFlash,
}
