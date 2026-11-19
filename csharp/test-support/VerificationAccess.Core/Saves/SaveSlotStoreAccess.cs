using SuperMetroid.Diagnostics.Saves;
namespace SuperMetroid.Core.Saves;

/// <summary>Verification access to <see cref="SaveSlotStore"/> members production does not use.</summary>
internal static class SaveSlotStoreAccess
{
    /// <summary>Exposes <see cref="SaveSlotStore"/> state that only verification reads.</summary>
    extension(SaveSlotStore self)
    {
        /// <summary><c>LoadFromSRAM</c>'s outcome: whether <paramref name="slot"/> holds a save.</summary>
        public bool IsSlotValid(int slot)
        {
            return self.ReadSlot(SaveRamCodec.SlotAt(slot)) is not null;
        }
    }
}
