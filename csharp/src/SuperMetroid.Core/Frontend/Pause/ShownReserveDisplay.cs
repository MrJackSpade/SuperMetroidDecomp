using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// The reserve labels and supply digits the equipment page shows once Samus has reserve capacity (#1467): the mode the
/// labels were last written for and the supply the digits were last written for.
/// </summary>
/// <param name="Mode">The reserve mode the mode label names: none keeps MODE's initial MANUAL text.</param>
/// <param name="Supply">The reserve energy the three supply digits show.</param>
internal sealed record ShownReserveDisplay(ReserveTankMode Mode, int Supply)
{
    /// <summary>The supply's decimal digits, hundreds first, as <c>Load_EquipmentScreen_ReserveHealth_Tilemap</c> splits it.</summary>
    internal int[] Digits() =>
        [Supply / PauseReserveEnergyDefinitions.Hundred, Supply % PauseReserveEnergyDefinitions.Hundred / PauseReserveEnergyDefinitions.DecimalBase,
            Supply % PauseReserveEnergyDefinitions.DecimalBase];
}
