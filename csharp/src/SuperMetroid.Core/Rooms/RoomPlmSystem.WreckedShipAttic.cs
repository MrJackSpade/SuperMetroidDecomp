namespace SuperMetroid.Core.Rooms;

/// <summary>Cartridge translation of Wrecked Ship attic room PLM $84:BB05.</summary>
public sealed partial class RoomPlmSystem
{
    /// <summary>
    /// Runs the callback installed by list $84:BAFF. The callback is intentionally inert
    /// in the cartridge, but validating its identity prevents an untranslated or corrupt
    /// callback from becoming a silent resident-actor failure.
    /// </summary>
    private static void RunWreckedShipAtticPreInstruction(PlmSlot slot)
    {
        if (slot.HeaderPointer != RoomPlmHeaders.WreckedShipAttic ||
            slot.PreInstruction == 0)
        {
            return;
        }

        if (slot.PreInstruction != WreckedShipAtticPlmRomData.NoOpCallback)
        {
            throw new InvalidDataException(
                $"Wrecked Ship attic PLM installed unknown pre-instruction " +
                $"$84:{slot.PreInstruction:X4}.");
        }

        // $84:BAFA toggles the accumulator width to 8-bit and immediately back to 16-bit,
        // then returns. Those processor-status writes have no persistent gameplay state.
    }
}
