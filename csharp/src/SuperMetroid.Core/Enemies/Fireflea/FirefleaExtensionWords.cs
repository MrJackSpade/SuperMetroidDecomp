namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>
/// The Fireflea words kept per physical slot rather than per occupant, like the native per-slot RAM they
/// model. The port never resets them, so words an initializer does not write keep the previous occupant's values,
/// even from an earlier room.
/// </summary>
internal sealed class FirefleaExtensionWords
{
    /// <summary>Upper vertical travel extent; only its Y is meaningful.</summary>
    internal Position16<RoomPixels> MinimumYExtent { get; set; }

    /// <summary>Lower vertical travel extent; only its Y is meaningful.</summary>
    internal Position16<RoomPixels> MaximumYExtent { get; set; }

    /// <summary>Byte offset of the current signed speed-table entry, toggled when a vertical extent is crossed.</summary>
    internal int SpeedTableIndex { get; set; }
}
