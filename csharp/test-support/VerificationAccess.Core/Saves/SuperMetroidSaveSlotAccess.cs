using SuperMetroid.Diagnostics.Saves;

namespace SuperMetroid.Core.Saves;

/// <summary>Verification access to <see cref="SuperMetroidSaveSlot"/> members production does not use.</summary>
internal static class SuperMetroidSaveSlotAccess
{
    /// <summary>Copies the complete save-slot state into the snapshot accepted by the SRAM encoder.</summary>
    extension(SuperMetroidSaveSlot self)
    {
        /// <summary>Creates the complete translated snapshot accepted by the SRAM encoder, as the codec re-encodes it.</summary>
        internal SuperMetroidSaveSnapshot ToSnapshot() => SaveRamCodec.ToSnapshot(self);
    }
}
