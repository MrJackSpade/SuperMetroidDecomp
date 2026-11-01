using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// A complete Samus loadout granted by <see cref="GameplayAssists.GrantedLoadout"/>: her equipment, beams, energy,
/// reserves and ammunition, each count filling both its current value and its maximum.
/// </summary>
public sealed record SamusLoadout
{
    /// <summary>The items Samus has collected and equipped.</summary>
    public required SamusEquipmentFlags Equipment { get; init; }

    /// <summary>The beam upgrades Samus has collected.</summary>
    public required SamusBeamFlags CollectedBeams { get; init; }

    /// <summary>The collected beams Samus has equipped; Spazer and Plasma cannot both be equipped.</summary>
    public required SamusBeamFlags EquippedBeams { get; init; }

    /// <summary>Samus's energy and maximum energy.</summary>
    public required int Energy { get; init => field = Ensure.AtLeastZero(value); }

    /// <summary>Samus's reserve energy and maximum reserve energy.</summary>
    public required int ReserveEnergy { get; init => field = Ensure.AtLeastZero(value); }

    /// <summary>How the reserve tanks refill Samus.</summary>
    public required ReserveTankMode ReserveTankMode { get; init; }

    /// <summary>Samus's missiles and maximum missiles.</summary>
    public required int Missiles { get; init => field = Ensure.AtLeastZero(value); }

    /// <summary>Samus's super missiles and maximum super missiles.</summary>
    public required int SuperMissiles { get; init => field = Ensure.AtLeastZero(value); }

    /// <summary>Samus's power bombs and maximum power bombs.</summary>
    public required int PowerBombs { get; init => field = Ensure.AtLeastZero(value); }

    /// <summary>Whether every item location in the game is marked collected, so no pickup remains to be found.</summary>
    public required bool CollectsEveryItemLocation { get; init; }
}
