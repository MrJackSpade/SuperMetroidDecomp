namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>
/// The effects that change the Wrecked Ship ghost's colours (#1512): its darkening to black when it initializes, its
/// brightening from black, and its fades that materialize and vanish it.
/// </summary>
internal enum WreckedShipGhostColourEffect
{
    /// <summary>The ghost's colours darkened to black.</summary>
    Dark,

    /// <summary>The ghost's brightening from black.</summary>
    Brightening,

    /// <summary>The ghost's fade into its own colours.</summary>
    Materializing,

    /// <summary>The ghost's fade to white.</summary>
    Vanishing,
}
