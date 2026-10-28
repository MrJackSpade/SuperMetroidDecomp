namespace SuperMetroid.Core.Progression;

/// <summary>
/// WRAM reserve-health mode (#1323): zero until the first reserve tank, then AUTO or MANUAL. Pickups and refills
/// write only one, the pause menu toggles between one and two, and AUTO is the only mode with bit zero set, so the
/// cartridge's bit-zero tests are AUTO tests.
/// </summary>
public enum ReserveTankMode
{
    /// <summary>No reserve tank collected.</summary>
    None = 0,
    /// <summary>Reserves refill health automatically ($82:AB47 labels it AUTO).</summary>
    Auto = 1,
    /// <summary>Reserves are transferred from the pause menu ($82:AEA9 stores two when leaving AUTO).</summary>
    Manual = 2,
}

