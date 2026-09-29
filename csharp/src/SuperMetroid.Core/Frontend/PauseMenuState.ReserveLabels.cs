using System.Buffers.Binary;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

internal sealed partial class PauseMenuState
{
    private void WriteReserveLabels()
    {
        // Absence of capacity leaves the original blank template intact. Restoring
        // the template before rebuilding also removes labels if inventory is changed.
        if (samus.MaxReserveEnergy == 0) return;
        PauseReserveUiPresentation presentation = (mapPresentation ?? throw new InvalidOperationException(
            "Reserve labels require installed presentation assets.")).PauseReserveUi;
        presentation.ApplyLabel(equipmentTilemap, "Mode");
        presentation.ApplyLabel(equipmentTilemap, "ReserveTank");
        // Native setup retains the initial MANUAL label for the zero/uninitialized
        // mode and patches its first four characters only after a nonzero mode exists.
        if (samus.ReserveTankMode != 0)
            presentation.ApplyLabel(equipmentTilemap,
                samus.ReserveTankMode == PauseReserveLabelRomData.AutoMode ? "Auto" : "Manual",
                preserveAttributes: true);
    }
}
