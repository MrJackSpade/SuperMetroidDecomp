using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.AssetExtraction;

internal static partial class Program
{
    /// <summary>
    /// Compares all eight native list pointers and all 64 object selections to the pinned
    /// cartridge, then executes both production population owners while rejecting every
    /// read from those immutable bank-$83 sources.
    /// </summary>
    private static void VerifyAreaAnimatedTileObjectDefinitions()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        AssertEqual(SuperMetroid.AssetExtraction.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Animated area oracle revision");
        Suite(nameof(VerifyAnimatedAreaListPointers), () => VerifyAnimatedAreaListPointers(rom));
        Suite(nameof(VerifyAnimatedAreaObjectSelection), () => VerifyAnimatedAreaObjectSelection(rom));
        Suite(nameof(VerifyCompiledAreaSelection), () => VerifyCompiledAreaSelection(rom, AreaId.Maridia, 0x0c, expectedSand: 2,
            expectedTreadmills: 0));
        Suite(nameof(VerifyCompiledAreaSelection), () => VerifyCompiledAreaSelection(rom, AreaId.WreckedShip, 0x0c, expectedSand: 0,
            expectedTreadmills: 2));

        Console.WriteLine(
            "  Area animated-tile definitions: 8 list pointers and 64 object " +
            "selectors are compiled; both production owners reject their ROM sources.");
    }

    /// <summary>Runs both production animated-tile selectors against a guarded ROM view and checks their area-specific counts.</summary>
    /// <param name="rom">The retail cartridge address space used for allowed noncompiled data reads.</param>
    /// <param name="area">The map area whose compiled animated-tile selection is being verified.</param>
    /// <param name="animatedTileBits">The native animation bitset expected in the fixture FX record.</param>
    /// <param name="expectedSand">The expected number of selected sand tiles for this area.</param>
    /// <param name="expectedTreadmills">The expected number of selected treadmill tiles for this area.</param>
    private static void VerifyCompiledAreaSelection(
        ISnesAddressSpace rom,
        AreaId area,
        byte animatedTileBits,
        int expectedSand,
        int expectedTreadmills)
    {
        // Original compiled default-door FX record with both sand/treadmill bits.
        const ushort fxRecord = 0x9e44;
        AssertEqual(animatedTileBits, rom.ReadByte(0x839e52), "Native fixture animation bitset");
        var guarded = new AreaAnimatedTileSelectionForbiddenBus(rom);
        var sand = new RoomSandAnimatedTilesState();
        var treadmills = new RoomTreadmillAnimatedTilesState();
        sand.LoadRoom(guarded, fxRecord, doorPointer: 0, area);
        treadmills.LoadRoom(guarded, fxRecord, doorPointer: 0, area);

        AssertEqual(expectedSand, sand.Count, $"{area} compiled sand selection");
        AssertEqual(expectedTreadmills, treadmills.Count,
            $"{area} compiled treadmill selection");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            $"{area} selection performs no immutable table reads");
    }

    /// <summary>
    /// Delegates allowed reads to the cartridge and rejects the compiled pointer/list bytes.
    /// </summary>
    /// <param name="rom">The underlying retail address space for reads outside the compiled table sources.</param>
    private sealed class AreaAnimatedTileSelectionForbiddenBus(
        ISnesAddressSpace rom) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Number of attempted reads from the compiled native pointer table or its object lists.</summary>
        public int ForbiddenReadAttempts { get; private set; }

        /// <summary>Reads a cartridge byte through the guarded address-space path.</summary>
        /// <param name="address">The bus address to read.</param>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads from compiled selection sources and delegates all other reads to the retail bus.</summary>
        /// <param name="address">The bus address to read.</param>
        /// <returns>The byte at <paramref name="address"/> when the address is not a guarded source.</returns>
        public byte ReadByte(int address)
        {
            if (IsCompiledSource(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled area animated-tile byte " +
                    $"{SnesAddress.FromBusAddress(address)}.");
            }

            return rom.ReadByte(address);
        }

        /// <summary>Rejects writes because this verification wrapper exposes the ROM as read-only.</summary>
        /// <param name="address">The bus address a caller attempted to modify.</param>
        /// <param name="value">The byte a caller attempted to store.</param>
        /// <exception cref="InvalidOperationException">Every write is rejected by this read-only verification bus.</exception>
        public void WriteByte(int address, byte value) => throw new InvalidOperationException("Read-only verification bus.");

        /// <summary>Identifies addresses belonging to the native pointer table or compiled animated-object lists.</summary>
        /// <param name="address">The absolute bus address to classify.</param>
        /// <returns><see langword="true"/> for a compiled source address; otherwise, <see langword="false"/>.</returns>
        private static bool IsCompiledSource(int address)
        {
            if (address >= AreaAnimatedTileObjectDefinitions.NativeListPointerTable &&
                address < AreaAnimatedTileObjectDefinitions.NativeListPointerTable +
                    AreaAnimatedTileObjectDefinitions.NativeAreaCount * sizeof(ushort))
            {
                return true;
            }

            SnesAddress source = SnesAddress.FromBusAddress(address);
            if (source.Bank != (byte)(RoomFxRomDataBanksTooling.RoomDefinitions >> 16))
                return false;

            for (int areaIndex = 0;
                 areaIndex < AreaAnimatedTileObjectDefinitions.NativeAreaCount;
                 areaIndex++)
            {
                ushort list = AreaAnimatedTileObjectDefinitions.NativeListPointer(areaIndex);
                if (source.Offset >= list &&
                    source.Offset < list +
                        AreaAnimatedTileObjectDefinitions.ObjectsPerArea * sizeof(ushort))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
