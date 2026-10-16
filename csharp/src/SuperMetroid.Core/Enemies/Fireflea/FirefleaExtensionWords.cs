namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>
/// The Fireflea words kept per physical slot rather than per occupant, like the native per-slot RAM they
/// model. The port never resets them, so words an initializer does not write keep the previous occupant's values,
/// even from an earlier room.
/// </summary>
internal sealed class FirefleaExtensionWords
{
    /// <summary>Lower vertical travel bound.</summary>
    internal ushort MinimumYPosition { get; set; }

    /// <summary>Upper vertical travel bound.</summary>
    internal ushort MaximumYPosition { get; set; }

    /// <summary>Current signed speed-table entries, toggled when a vertical bound is crossed.</summary>
    internal ushort SpeedTableIndex { get; set; }
}
