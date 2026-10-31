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

    /// <summary>The row of the shared linear speed table.</summary>
    internal int SpeedRow { get; set; }

    /// <summary>Which of the row's speeds is current, reversed when a vertical extent is crossed.</summary>
    internal EnemySpeedSign SpeedSign { get; set; }
}
