namespace SuperMetroid.Core.Progression;

/// <summary>
/// Reserve-health mode (#1323): zero until the first reserve tank, then AUTO or MANUAL. Pickups and refills
/// write only AUTO, the pause menu toggles between AUTO and MANUAL, and AUTO is the only odd mode, so the game's
/// oddness tests are AUTO tests.
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

