namespace SuperMetroid.Core.Progression;

/// <summary>
/// Reserve-health mode (#1323): none until the first reserve tank, then AUTO or MANUAL. Pickups and refills set only
/// AUTO, and the pause menu toggles between AUTO and MANUAL; the game's reserve-mode checks ask whether the mode is AUTO.
/// </summary>
public enum ReserveTankMode
{
    /// <summary>No reserve tank collected.</summary>
    None,
    /// <summary>Reserves refill health automatically; the pause menu labels it AUTO.</summary>
    Auto,
    /// <summary>Reserves are transferred by hand from the pause menu, which selects MANUAL when leaving AUTO.</summary>
    Manual,
}
