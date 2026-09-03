using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// Proves the named room-asset definitions remain inside the retail cartridge and that
    /// every graphics set selected by a retail room state resolves to three valid streams.
    /// </summary>
    static void VerifyRoomAssetRomData()
    {
        ValidateTilemapTransfer(RoomAssetRomData.LibraryBackground.ClearFx, "clear FX");
        ValidateTilemapTransfer(RoomAssetRomData.LibraryBackground.ClearBg2, "clear BG2");
        AssertEqual(
            RoomAssetRomData.GraphicsLayout.CreBlockDefinitionsByteCount,
            RoomAssetRomData.GraphicsLayout.CreBlockDefinitionCount *
                RoomAssetRomData.GraphicsLayout.BytesPerBlockDefinition,
            "CRE block-table geometry");

        string romPath = Path.GetFullPath("Super Metroid.smc");
        string symbolPath = Path.GetFullPath(
            Path.Combine("upstream-sm", "assets", "names.txt"));
        if (!File.Exists(romPath) || !File.Exists(symbolPath))
        {
            Console.WriteLine(
                "  Room assets: memory-layout checks pass; retail range audit skipped " +
                "(private inputs absent).");
            return;
        }

        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        RoomAssetRomData.BoundedCompressedAsset[] boundedAssets =
        [
            RoomAssetRomData.LandingSite.CreBlockDefinitions,
            RoomAssetRomData.LandingSite.AreaBlockDefinitions,
            RoomAssetRomData.LandingSite.LevelData,
            RoomAssetRomData.LandingSite.CreCharacters,
            RoomAssetRomData.LandingSite.AreaCharacters,
        ];
        foreach (RoomAssetRomData.BoundedCompressedAsset asset in boundedAssets)
        {
            byte[] decompressed = DecompressBoundedRoomAsset(bus, asset);
            AssertTrue(decompressed.Length > 0,
                $"bounded room asset ${asset.Address:X6} decompresses to nonempty data");
        }

        byte[] creBlocks = DecompressBoundedRoomAsset(
            bus,
            RoomAssetRomData.LandingSite.CreBlockDefinitions);
        AssertEqual(
            RoomAssetRomData.GraphicsLayout.CreBlockDefinitionsByteCount,
            creBlocks.Length,
            "retail CRE block-definition byte count");

        ushort[] roomPointers = File.ReadLines(symbolPath)
            .Select(TryParseRoomHeaderPointer)
            .Where(pointer => pointer.HasValue)
            .Select(pointer => pointer!.Value)
            .Distinct()
            .ToArray();
        var graphicsSets = new HashSet<byte>();
        foreach (ushort roomPointer in roomPointers)
        {
            foreach (RoomStateSelectionContext context in BuildRoomStateAuditContexts())
            {
                graphicsSets.Add(CartridgeRoomHeader.Load(bus, roomPointer, context).State.GraphicsSet);
            }
        }

        foreach (byte graphicsSet in graphicsSets)
        {
            int pointerAddress = RoomAssetRomData.Tilesets.PointerTableAddress +
                graphicsSet * sizeof(ushort);
            ushort definitionPointer = RomDataReader.ReadWordFixedBank(bus, pointerAddress);
            int definitionAddress = RoomAssetRomData.Tilesets.DefinitionBank | definitionPointer;
            int blockAddress = RomDataReader.ReadLongFixedBank(
                bus,
                definitionAddress + RoomAssetRomData.Tilesets.BlockDefinitionsAddressOffset);
            int characterAddress = RomDataReader.ReadLongFixedBank(
                bus,
                definitionAddress + RoomAssetRomData.Tilesets.CharacterAddressOffset);
            int paletteAddress = RomDataReader.ReadLongFixedBank(
                bus,
                definitionAddress + RoomAssetRomData.Tilesets.PaletteAddressOffset);

            AssertTrue(RomDataReader.Decompress(bus, blockAddress).Length > 0,
                $"graphics set ${graphicsSet:X2} block stream is in the retail ROM");
            AssertTrue(RomDataReader.Decompress(bus, characterAddress).Length > 0,
                $"graphics set ${graphicsSet:X2} character stream is in the retail ROM");
            AssertTrue(
                RomDataReader.Decompress(bus, paletteAddress).Length >=
                    RoomAssetRomData.GraphicsLayout.BackgroundPaletteByteCount,
                $"graphics set ${graphicsSet:X2} has the complete background palette range");
        }

        Console.WriteLine(
            $"  Room assets: {boundedAssets.Length} bounded streams and " +
            $"{graphicsSets.Count} retail graphics sets validated.");
    }

    private static byte[] DecompressBoundedRoomAsset(
        SuperMetroidAddressSpace bus,
        RoomAssetRomData.BoundedCompressedAsset asset)
    {
        AssertTrue(asset.Address >= 0x808000 && asset.Address <= 0xffffff,
            $"room asset ${asset.Address:X6} begins in mapped cartridge space");
        AssertTrue(asset.StoredByteCount > 0,
            $"room asset ${asset.Address:X6} has a positive stored extent");
        AssertTrue(asset.Address + asset.StoredByteCount - 1 <= 0xffffff,
            $"room asset ${asset.Address:X6} ends in 24-bit cartridge space");

        var stored = new byte[asset.StoredByteCount];
        for (int index = 0; index < stored.Length; index++)
            stored[index] = bus.ReadByte(asset.Address + index);
        return SmCompression.Decompress(stored);
    }

    private static void ValidateTilemapTransfer(
        RoomAssetRomData.TilemapTransfer transfer,
        string description)
    {
        AssertEqual(
            RoomAssetRomData.LibraryBackground.WorkRamBank | transfer.WorkRamDestination,
            transfer.WorkRamSourceAddress,
            $"{description} WRAM source corresponds to its fill destination");
        AssertTrue((transfer.ByteCount & 1) == 0,
            $"{description} transfer contains complete words");
        AssertTrue(transfer.WorkRamDestination + transfer.ByteCount <=
            RoomAssetRomData.LibraryBackground.BankByteCount,
            $"{description} transfer remains in bank $7E");
        AssertTrue(transfer.VramDestinationWord + transfer.ByteCount / 2 <= 0x8000,
            $"{description} transfer remains in 64-KiB VRAM");
    }
}
