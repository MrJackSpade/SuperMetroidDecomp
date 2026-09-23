using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// Exercise every cartridge-authored background list, including each door-gated
    /// transfer. Matching one representative list does not prove that an installed
    /// sky page or tilemap is selected by all the other native command paths.
    /// </summary>
    private static void VerifyLibraryBackgroundInstalledParity(
        string sourceRom, GameInstallation installed)
    {
        SuperMetroidAddressSpace inventoryBus = SuperMetroidAddressSpace.LoadRetailRom(sourceRom);
        VerifyCompiledLibraryBackgroundPrograms(inventoryBus);
        IReadOnlyList<LibraryBackgroundSource> sources =
            LibraryBackgroundSourceInventory.Scan(inventoryBus);
        RoomBackgroundTilemapCatalog backgrounds = installed.LoadRoomBackgroundTilemaps();
        RoomSkyTilemapCatalog skies = installed.LoadRoomSkyTilemaps();
        RoomCharacterAtlasCatalog characters = installed.LoadRoomCharacters();
        HudTileAtlas hud = installed.LoadMaps().HudTiles;
        int cases = 0;

        foreach (LibraryBackgroundProgram program in LibraryBackgroundProgramDefinitions.All)
        {
            LibraryBackgroundSource[] list = sources
                .Where(source => source.ListPointer == program.Pointer).ToArray();
            // Zero tests the ordinary path. Each authored door pointer additionally
            // selects its conditional DMA; a list can contain more than one.
            foreach (ushort door in list.Where(source => source.DoorPointer.HasValue)
                .Select(source => source.DoorPointer!.Value).Append((ushort)0).Distinct())
            {
                SuperMetroidAddressSpace nativeBus = SuperMetroidAddressSpace.LoadRetailRom(sourceRom);
                var nativeVram = new SnesVram();
                LibraryBackgroundExecutionResult native =
                    LibraryBackgroundLoader.ExecuteNativeForVerification(
                        nativeBus, nativeVram, program.Pointer, door);

                SuperMetroidAddressSpace selectedBus = SuperMetroidAddressSpace.LoadRetailRom(sourceRom);
                var guarded = new LibraryBackgroundVisualReadGuard(selectedBus, sources);
                var selectedVram = new SnesVram();
                LibraryBackgroundExecutionResult selected = LibraryBackgroundLoader.Execute(
                    guarded, selectedVram, program.Pointer, door,
                    backgrounds, skies, hud, characters);
                AssertEqual(native, selected,
                    $"library background $8F:{program.Pointer:X4}, door ${door:X4} command result");
                for (int index = 0; index < nativeVram.Bytes.Length; index++)
                {
                    if (nativeVram.ReadByte(index) == selectedVram.ReadByte(index)) continue;
                    throw new InvalidDataException(
                        $"Library background $8F:{program.Pointer:X4}, door ${door:X4} differs " +
                        $"at VRAM byte ${index:X4}: native ${nativeVram.ReadByte(index):X2}, " +
                        $"installed ${selectedVram.ReadByte(index):X2}; " +
                        $"sources {string.Join(", ", list.Select(source => $"${source.SourceAddress:X6}"))}.");
                }

                foreach (LibraryBackgroundSource source in list.Where(source =>
                    source.Command == LibraryBackgroundCommand.DecompressToWorkRam))
                {
                    int length = backgrounds.Get(source.SourceAddress).Transfer.Length;
                    int start = RoomAssetRomData.LibraryBackground.WorkRamBank |
                        source.WorkRamDestination!.Value;
                    for (int offset = 0; offset < length; offset++)
                        AssertEqual(nativeBus.ReadByte(start + offset),
                            selectedBus.ReadByte(start + offset),
                            $"library background $8F:{program.Pointer:X4} WRAM byte {offset}");
                }
                cases++;
            }
        }

        AssertTrue(cases > LibraryBackgroundProgramDefinitions.RetailProgramCount,
            "library background parity included door-selected variants");
        Console.WriteLine($"  Library backgrounds: {cases} ordinary and door-selected cases " +
            "match installed VRAM/WRAM without command-list or visual ROM-source reads.");
    }

    private sealed class LibraryBackgroundVisualReadGuard(
        ISnesAddressSpace source, IReadOnlyList<LibraryBackgroundSource> entries)
        : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            foreach (LibraryBackgroundProgram program in LibraryBackgroundProgramDefinitions.All)
            {
                int start = RoomAssetRomData.LibraryBackground.CommandBank | program.Pointer;
                if (address >= start && address - start < program.NativeByteCount)
                    throw new InvalidOperationException(
                        $"Installed background reread compiled command list $8F:{program.Pointer:X4}.");
            }
            foreach (LibraryBackgroundSource entry in entries)
            {
                if (entry.Command == LibraryBackgroundCommand.DecompressToWorkRam)
                {
                    // Only the first compressed byte is needed to detect an accidental
                    // fallback into RomDataReader.Decompress.
                    if (address == entry.SourceAddress)
                        throw new InvalidOperationException(
                            $"Installed background reread compressed source ${address:X6}.");
                }
                else if (entry.SourceAddress >=
                    RoomAssetRomData.LibraryBackground.RomSourceAddressFloor &&
                    address >= entry.SourceAddress &&
                    address - entry.SourceAddress < entry.TransferByteCount!.Value)
                    throw new InvalidOperationException(
                        $"Installed background reread direct visual source ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
