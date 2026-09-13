using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend;

internal sealed partial class PauseMenuState
{
    // Native $0757 persists while the selector moves away. It is not a host
    // task or timer: only the selected reserve-transfer subdispatcher ticks it.
    private ushort reserveTransferSoundDelay;

    /// <summary>Runs the tank subdispatcher before its D-pad response, as at $82:AC70.</summary>
    private void HandleReserveInput(SnesButton pressed)
    {
        if (selectedItem == PauseReserveTransferRomData.ModeItem)
        {
            if ((pressed & SnesButton.A) == 0 || samus.MaxReserveEnergy == 0) return;
            audio?.QueueSound(SoundEffectLibrary1Sounds.MenuCursor, maximumQueued: 6);
            samus.ReserveTankMode = samus.ReserveTankMode == PauseReserveLabelRomData.AutoMode
                ? PauseReserveTransferRomData.ManualMode : PauseReserveLabelRomData.AutoMode;
            WriteReserveLabels();
            return;
        }
        if (selectedItem != PauseReserveTransferRomData.TransferItem)
            throw new InvalidDataException($"Unsupported native reserve subdispatcher {selectedItem}.");

        if (reserveTransferSoundDelay == 0)
        {
            if ((pressed & SnesButton.A) == 0) return;
            reserveTransferSoundDelay = unchecked((ushort)((samus.ReserveEnergy +
                PauseReserveTransferRomData.SoundCadenceMask) & ~PauseReserveTransferRomData.SoundCadenceMask));
        }
        reserveTransferSoundDelay = unchecked((ushort)(reserveTransferSoundDelay - 1));
        if ((reserveTransferSoundDelay & PauseReserveTransferRomData.SoundCadenceMask) ==
            PauseReserveTransferRomData.SoundCadenceMask)
            audio?.QueueSound(PauseReserveTransferRomData.RefillSound, PauseReserveTransferRomData.MaximumQueuedSounds);

        ushort amount = PauseEquipmentRules.ReserveEnergyPerFrame;
        samus.Health = unchecked((ushort)(samus.Health + amount));
        if (unchecked((short)(samus.Health - samus.MaxHealth)) >= 0)
            samus.Health = samus.MaxHealth;
        else
        {
            samus.ReserveEnergy = unchecked((ushort)(samus.ReserveEnergy - amount));
            if (unchecked((short)samus.ReserveEnergy) > 0) return;
            if (samus.ReserveEnergy != 0)
                samus.Health = unchecked((ushort)(samus.Health + samus.ReserveEnergy));
        }

        // Retail exhausts ALL reserves even when only a small top-up was needed.
        // Preserve that behavior and clear both bytes of the packed selector.
        samus.ReserveEnergy = 0;
        reserveTransferSoundDelay = 0;
        selectedCategory = PauseEquipmentCategories.Reserves;
        selectedItem = PauseReserveTransferRomData.ModeItem;
    }
}
