using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

internal sealed partial class PauseMenuState
{
    // The arrow state latched by its native owner: equipment setup lights it and tank
    // dispatches update it; other categories leave it latched. Content rebinds re-apply this
    // state with the new presentation rather than re-running the owner.
    private bool reserveArrowLatched;
    private bool reserveArrowLatchedEnabled;
    private bool reserveArrowLatchedAnimated;
    private byte reserveArrowLatchedFrame;

    /// <summary>Runs $82:AD0A after the tank input handler and D-pad response.</summary>
    private void UpdateReserveArrow(byte nmiFrameCounter8)
    {
        bool inTanks = selectedCategory == PauseEquipmentCategory.Reserves;
        bool animated = inTanks && SelectedReserveItem == PauseReserveItem.Mode &&
            samus.ReserveTankMode == PauseReserveLabelRomData.AutoMode;
        bool enabled = animated || inTanks && SelectedReserveItem == PauseReserveItem.Transfer;
        SetReserveArrow(enabled, animated, nmiFrameCounter8);
    }

    private void SetReserveArrow(bool enabled, bool animated = false, byte nmiFrameCounter8 = 0)
    {
        PauseReserveUiPresentation presentation = (mapPresentation ?? throw new InvalidOperationException(
            "Reserve arrow requires installed presentation assets.")).PauseReserveUi;
        presentation.ApplyArrowColors(cgram, animated, nmiFrameCounter8,
            PauseReserveArrowRomData.Color6Index, PauseReserveArrowRomData.Color11Index);
        presentation.ApplyArrowTilePalettes(equipmentTilemap, enabled);
        reserveArrowLatched = true;
        reserveArrowLatchedEnabled = enabled;
        reserveArrowLatchedAnimated = animated;
        reserveArrowLatchedFrame = nmiFrameCounter8;
    }

    /// <summary>Re-applies the latched arrow tiles and colors with the currently bound presentation.</summary>
    private void ReapplyLatchedReserveArrow() =>
        SetReserveArrow(reserveArrowLatchedEnabled, reserveArrowLatchedAnimated, reserveArrowLatchedFrame);
}
