using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

internal sealed partial class PauseMenuState
{
    // The arrow state latched by its native owner: equipment setup lights it and tank
    // dispatches update it; other categories leave it latched. Content rebinds re-apply this
    // state with the new presentation rather than re-running the owner.
    /// <summary>Whether the reserve-arrow state has been set by its owning equipment or tank handler.</summary>
    private bool reserveArrowLatched;
    /// <summary>Whether the latched state displays the reserve arrow as enabled.</summary>
    private bool reserveArrowLatchedEnabled;
    /// <summary>Whether the latched enabled state uses the automatic-mode color animation.</summary>
    private bool reserveArrowLatchedAnimated;
    /// <summary>NMI frame counter captured with the latched color-animation state for presentation rebinding.</summary>
    private byte reserveArrowLatchedFrame;

    /// <summary>Runs $82:AD0A after the tank input handler and D-pad response.</summary>
    private void UpdateReserveArrow(byte nmiFrameCounter8)
    {
        bool inTanks = selectedCategory == PauseEquipmentCategories.Reserves;
        bool animated = inTanks && selectedItem == PauseReserveTransferRomData.ModeItem &&
            samus.ReserveTankMode == PauseReserveLabelRomData.AutoMode;
        bool enabled = animated || inTanks && selectedItem == PauseReserveTransferRomData.TransferItem;
        SetReserveArrow(enabled, animated, nmiFrameCounter8);
    }

    /// <summary>Applies reserve-arrow tile and color presentation, then records the state for future asset rebinding.</summary>
    /// <param name="enabled">Whether the reserve-arrow tiles should use their active palette.</param>
    /// <param name="animated">Whether the arrow colors follow the automatic reserve-mode animation.</param>
    /// <param name="nmiFrameCounter8">The current NMI frame counter used to select animated arrow colors.</param>
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
