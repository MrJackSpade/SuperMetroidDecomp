namespace SuperMetroid.Core.Progression;

/// <summary>
/// Reserve-health mode (#1323): zero until the first reserve tank, then AUTO or MANUAL. Pickups and refills
/// write only one, the pause menu toggles between one and two, and AUTO is the only mode with bit zero set, so the
/// game's bit-zero tests are AUTO tests.
/// </summary>
public enum ReserveTankMode
{
    /// <summary>No reserve tank collected.</summary>
    None = 0,
    /// <summary>Reserves refill health automatically; the pause menu labels it AUTO.</summary>
    Auto = 1,
    /// <summary>Reserves are transferred from the pause menu, which stores two when leaving AUTO.</summary>
    Manual = 2,
}

