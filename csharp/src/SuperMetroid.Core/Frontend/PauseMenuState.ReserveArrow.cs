using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

internal sealed partial class PauseMenuState
{
    /// <summary>Runs $82:AD0A after the tank input handler and D-pad response.</summary>
    private void UpdateReserveArrow(byte nmiFrameCounter8)
    {
        bool inTanks = selectedCategory == PauseEquipmentCategories.Reserves;
        bool animated = inTanks && selectedItem == PauseReserveTransferRomData.ModeItem &&
            samus.ReserveTankMode == PauseReserveLabelRomData.AutoMode;
        bool enabled = animated || inTanks && selectedItem == PauseReserveTransferRomData.TransferItem;
        SetReserveArrow(enabled, animated, nmiFrameCounter8);
    }

    private void SetReserveArrow(bool enabled, bool animated = false, byte nmiFrameCounter8 = 0)
    {
        PauseReserveUiPresentation presentation = (mapPresentation ?? throw new InvalidOperationException(
            "Reserve arrow requires installed presentation assets.")).PauseReserveUi;
        presentation.ApplyArrowColors(cgram, animated, nmiFrameCounter8,
            PauseReserveArrowRomData.Color6Index, PauseReserveArrowRomData.Color11Index);
        presentation.ApplyArrowTilePalettes(equipmentTilemap, enabled);
    }
}
