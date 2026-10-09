using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Compares compiled projectile sound selectors with retail tables and checks the selector bounds.</summary>
    /// <param name="retail">Retail address space containing charged and uncharged beam sound words.</param>
    private static void VerifyProjectileSoundRoutingDefinitions(
        SuperMetroidAddressSpace retail)
    {
        int observations = 0;
        foreach (bool charged in new[] { false, true })
        {
            int table = charged
                ? SamusProjectileRomData.Beams.ChargedSounds
                : SamusProjectileRomData.Beams.UnchargedSounds;
            for (int selector = 0;
                 selector < SamusProjectileSoundRoutingDefinitions.SelectorCount;
                 selector++)
            {
                ushort expected = ReadProjectileWord(retail, table + selector * 2);
                AssertEqual(
                    expected,
                    SamusProjectileSoundRoutingDefinitions.Resolve(charged, selector),
                    $"{(charged ? "charged" : "uncharged")} beam sound selector {selector:X}");
                observations++;
            }
        }

        AssertEqual(32, observations,
            "projectile sound routing authored and adjacent observations");
        foreach (int invalid in new[] { -1, 16, 17, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(
                () => SamusProjectileSoundRoutingDefinitions.Resolve(false, invalid),
                $"projectile sound selector {invalid} fails outside four-bit domain");
        }

        Console.WriteLine(
            "  Projectile sound routing: 24 authored and 8 bounded adjacent-table observations agree.");
    }

    /// <summary>Guards compiled projectile sound-routing bytes while forwarding other memory access to the wrapped bus.</summary>
    /// <param name="source">Address space used for permitted reads and all writes.</param>
    private sealed class ProjectileSoundRoutingForbiddenBus(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource, ISnesMutableMemory
    {
        /// <summary>Mutable-memory view over the same source, used to forward WRAM and SRAM reads.</summary>
        private readonly ISnesMutableMemory memory = ReferenceMutableMemory.From(source);

        /// <summary>Reads a WRAM byte through the mutable-memory view without applying cartridge-range checks.</summary>
        /// <param name="address">WRAM address requested by the caller.</param>
        /// <returns>The byte stored at that address.</returns>
        public byte ReadWorkRamByte(int address) => memory.ReadWorkRamByte(address);

        /// <summary>Reads an SRAM byte through the mutable-memory view without applying cartridge-range checks.</summary>
        /// <param name="address">SRAM address requested by the caller.</param>
        /// <returns>The byte stored at that address.</returns>
        public byte ReadSaveRamByte(int address) => memory.ReadSaveRamByte(address);

        /// <summary>Routes imported cartridge reads through the forbidden-range guard.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The byte at a permitted address.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads of compiled sound-routing bytes and delegates all other reads to the source bus.</summary>
        /// <param name="address">Address to inspect and, if permitted, read.</param>
        /// <returns>The source byte when the address is outside the guarded table range.</returns>
        public byte ReadByte(int address)
        {
            if (address is >= 0x90c28f and <= 0x90c2c6)
            {
                throw new InvalidOperationException(
                    $"Production reread compiled projectile sound-routing byte ${address:X6}.");
            }

            return source.ReadByte(address);
        }

        /// <summary>Forwards writes to the source bus; this verification guard constrains reads only.</summary>
        /// <param name="address">Destination address.</param>
        /// <param name="value">Byte to write.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
