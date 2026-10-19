namespace SuperMetroid.Core.Hardware;

/// <summary>Verification writes into <see cref="SuperMetroidAddressSpace"/>'s WRAM image; production never writes through it.</summary>
[CpuBusBoundary("exact bus test: seeds the WRAM image the runtime reads")]
internal static class SuperMetroidAddressSpaceAccess
{
    /// <summary>Writes into the image.</summary>
    extension(SuperMetroidAddressSpace self)
    {
        /// <summary>
        /// Writes the supported mutable region: WRAM and its low-window mirrors.
        /// </summary>
        public void WriteByte(int address, byte value)
        {
            SuperMetroidAddressSpace.ValidateAddress(address);
            int bank = SnesAddressMath.BankNumber(address);
            int offset = SnesAddressMath.OffsetInBank(address);

            if (SnesBusLayout.IsWorkRamBank(bank))
            {
                self._workRam[(bank - SnesBusLayout.FirstWorkRamBank) * SnesBusLayout.BankBytes + offset] = value;
                return;
            }

            if (SnesBusLayout.IsSystemBank(bank) && offset < SnesBusLayout.WorkRamMirrorEnd)
            {
                self._workRam[offset] = value;
                return;
            }

            if (offset >= SnesBusLayout.RomWindowStart)
                throw new InvalidOperationException($"Cannot write cartridge ROM at ${bank:X2}:{offset:X4}.");

            throw new InvalidOperationException(
                $"CPU write ${bank:X2}:{offset:X4} is outside the runtime address map.");
        }
    }
}
