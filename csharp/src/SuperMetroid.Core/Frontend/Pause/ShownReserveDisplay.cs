using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// The reserve labels and supply digits the equipment page shows once Samus has reserve capacity (#1467, #1515): the mode
/// her reserve tanks are in and the energy they hold.
/// </summary>
/// <param name="Mode">The reserve mode the MODE label names; before a mode is chosen the label reads MANUAL.</param>
/// <param name="Supply">The reserve energy the three supply digits show.</param>
internal sealed partial record ShownReserveDisplay(ReserveTankMode Mode, int Supply)
{
    /// <summary>The supply's decimal digits, hundreds first, as the equipment screen's reserve display splits it.</summary>
    internal int[] Digits() =>
        [Supply / PauseReserveEnergyDefinitions.Hundred, Supply % PauseReserveEnergyDefinitions.Hundred / PauseReserveEnergyDefinitions.DecimalBase,
            Supply % PauseReserveEnergyDefinitions.DecimalBase];
}
