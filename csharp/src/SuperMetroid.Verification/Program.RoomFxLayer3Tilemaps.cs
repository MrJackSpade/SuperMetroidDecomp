using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    /// <summary>Verifies every compiled room-FX layer-three tilemap page and ensures production loading uses installed catalogs.</summary>
    private static void VerifyRoomFxLayer3Tilemaps()
    {
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "FX tilemap oracle revision");
        Suite(nameof(VerifyFxTilemapTypeEnumeration), () => VerifyFxTilemapTypeEnumeration());
        Suite(nameof(VerifyFxTilemapSourceAddresses), () => VerifyFxTilemapSourceAddresses(rom));
        byte[] json = RoomFxLayer3TilemapExtractor.Extract(new LiquidTilemapSourceGuard(rom));
        RoomFxLayer3TilemapCatalog catalog = RoomFxLayer3TilemapCatalog.Load(new MemoryStream(json));
        Suite(nameof(VerifyLiquidTilemaps), () => VerifyLiquidTilemaps(rom, catalog));
        Suite(nameof(VerifySporeTilemapAttributes), () => VerifySporeTilemapAttributes(rom, catalog));
        Suite(nameof(VerifyAtmosphereTilemapFields), () => VerifyAtmosphereTilemapFields(rom, catalog));
        Suite(nameof(VerifyFxTilemapPageDispatch), () => VerifyFxTilemapPageDispatch(rom, catalog));
        RoomFxPaletteBlendCatalog paletteColors = RoomFxPaletteBlendCatalog.Load(
            new MemoryStream(RoomFxPaletteBlendExtractor.Extract(rom)));
        foreach (RoomFxType type in RoomFxLayer3TilemapFormat.Types)
        {
            int source = RoomFxLayer3TilemapFormat.SourceAddress(type);
            byte[] native = RomDataReader.ReadFixedBank(rom, source,
                RoomFxLayer3TilemapFormat.PageByteCount);
            if (type == RoomFxType.Spores) continue;
            SnesVram vram = LoadConstructedRoomFxTilemap(type, catalog, paletteColors,
                out ForbiddenRoomFxTilemapBus guarded);
            int destination = RoomFxRomData.Layer3.TilemapDestinationWord * 2;
            for (int index = 0; index < native.Length; index++)
                AssertEqual(native[index], vram.ReadByte(destination + index),
                    $"room-FX {type} installed VRAM byte {index}");
            AssertEqual(0, guarded.ForbiddenReads,
                $"room-FX {type} installed load reads no tilemap ROM or pointer table");
        }
        AssertThrows<InvalidDataException>(() => RoomFxLayer3TilemapCatalog.Load(
            new MemoryStream([1, 2, 3])), "corrupt room-FX BG3 tilemap JSON fails loudly");
        Console.WriteLine("  Room-FX BG3 tilemaps: six complete native pages and five guarded installed loads pass.");
    }

    /// <summary>Checks that an edited stock tilemap reaches BG3 VRAM and removing the override restores stock identity.</summary>
    /// <param name="stock">Directory containing the validated stock room-FX tilemap document.</param>
    /// <param name="overrides">Directory in which the temporary override is written and then removed.</param>
    /// <param name="baseline">Presentation catalog whose identity and lava page represent the unmodified stock selection.</param>
    private static void VerifyRoomFxLayer3TilemapOverride(
        string stock, string overrides, AreaMapPresentationCatalog baseline)
    {
        string path = Path.Combine(overrides, RoomFxLayer3TilemapFormat.FileName);
        string stockPath = Path.Combine(stock, RoomFxLayer3TilemapFormat.FileName);
        RoomFxLayer3TilemapDocument document = JsonSerializer.Deserialize<RoomFxLayer3TilemapDocument>(
            File.ReadAllBytes(stockPath), new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
            }) ?? throw new InvalidDataException("Stock room-FX BG3 document is null.");
        RoomBackgroundTilemapCell originalCell = document.Pages[nameof(RoomFxType.Lava)][0];
        document.Pages[nameof(RoomFxType.Lava)][0] = originalCell with
        {
            TileColumn = (originalCell.TileColumn + 1) % RoomBackgroundTilemapFormat.TileColumns,
        };
        using (var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
            RoomFxLayer3TilemapCatalog.Write(output, document);
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != baseline.ContentIdentity,
            "room-FX BG3 edit changes installed content identity");
        AssertTrue(!edited.RoomFxLayer3Tilemaps.Resolve(RoomFxType.Lava).Span.SequenceEqual(
            baseline.RoomFxLayer3Tilemaps.Resolve(RoomFxType.Lava).Span),
            "room-FX BG3 edit changes the selected tilemap words");
        SnesVram vram = LoadConstructedRoomFxTilemap(RoomFxType.Lava,
            edited.RoomFxLayer3Tilemaps, edited.RoomFxPaletteBlends,
            out ForbiddenRoomFxTilemapBus guarded);
        ushort editedWord = BitConverter.ToUInt16(
            edited.RoomFxLayer3Tilemaps.Resolve(RoomFxType.Lava).Span[..sizeof(ushort)]);
        AssertEqual(editedWord, vram.ReadWord(RoomFxRomData.Layer3.TilemapDestinationWord),
            "edited lava tile cell reaches production BG3 VRAM");
        AssertEqual(0, guarded.ForbiddenReads,
            "edited lava page loads without native art or pointer reads");
        AssertThrows<InvalidDataException>(() =>
        {
            document.Pages[nameof(RoomFxType.Lava)][0] = originalCell with { TileColumn = 32 };
            using var invalid = new MemoryStream();
            RoomFxLayer3TilemapCatalog.Write(invalid, document);
        }, "invalid room-FX BG3 tile index fails loudly");
        File.Delete(path);
        AreaMapPresentationCatalog restored = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertEqual(baseline.ContentIdentity, restored.ContentIdentity,
            "removing room-FX BG3 override restores stock content identity");
        Console.WriteLine("Room-FX BG3 override: edited lava tile reaches VRAM and removal restores stock.");
    }

    /// <summary>Loads one compiled default room-FX record into a fresh VRAM instance through a bus that forbids tilemap ROM reads.</summary>
    /// <param name="type">Room-FX type whose default layer-three page is to be loaded.</param>
    /// <param name="catalog">Installed tilemap catalog supplying the page words.</param>
    /// <param name="paletteColors">Installed palette-blend catalog supplied to the room-FX state.</param>
    /// <param name="guarded">Receives the address-space wrapper used to detect forbidden native tilemap reads.</param>
    /// <returns>VRAM after the selected room-FX record has loaded its layer-three page.</returns>
    private static SnesVram LoadConstructedRoomFxTilemap(RoomFxType type,
        RoomFxLayer3TilemapCatalog catalog, RoomFxPaletteBlendCatalog paletteColors,
        out ForbiddenRoomFxTilemapBus guarded)
    {
        RoomFxRecordDefinition record = RoomFxRecordDefinitions.All.FirstOrDefault(candidate =>
            candidate.DoorPointer == 0 && candidate.Type == (byte)type)
            ?? throw new InvalidDataException($"No compiled default room-FX record selects {type}.");
        var memory = new TestAddressSpace();
        guarded = new ForbiddenRoomFxTilemapBus(memory);
        var state = new RoomLayer3FxState
        {
            Layer3Tilemaps = catalog,
            PaletteBlendColors = paletteColors,
        };
        var vram = new SnesVram();
        state.Load(guarded, vram, new SnesCgram(), record.Pointer, doorPointer: 0, randomNumber: 0);
        AssertEqual(type, state.Type, "installed room-FX BG3 page preserves the selected FX type");
        return vram;
    }

    /// <summary>Rejects reads from native layer-three tilemap storage and its pointer table during installed page loading.</summary>
    /// <param name="inner">Underlying address space for accesses outside the guarded source ranges.</param>
    private sealed class ForbiddenRoomFxTilemapBus(ISnesAddressSpace inner) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Number of attempted reads from a forbidden native tilemap page or pointer-table range.</summary>
        public int ForbiddenReads { get; private set; }

        /// <summary>Counts and rejects accesses to cartridge ranges that contain native layer-three pages or pointers.</summary>
        /// <param name="address">Address being checked before it reaches the underlying bus.</param>
        private void RejectTilemapSource(int address)
        {
            if ((address >> 16) == 0x8a ||
                address >= RoomFxRomData.Tables.Layer3TilemapPointers &&
                address < RoomFxRomData.Tables.Layer3TilemapPointers + 14)
            {
                ForbiddenReads++;
                throw new InvalidOperationException(
                    $"Installed room-FX BG3 tilemap read ROM ${address:X6}.");
            }
        }

        /// <summary>Checks the address guard before forwarding a general memory read.</summary>
        /// <param name="address">Address to read.</param>
        /// <returns>The underlying address-space byte when the address is permitted.</returns>
        public byte ReadByte(int address)
        {
            RejectTilemapSource(address);
            return inner.ReadByte(address);
        }

        /// <summary>Checks the same native-source guard before forwarding a cartridge-specific read.</summary>
        /// <param name="address">Cartridge address to read.</param>
        /// <returns>The underlying cartridge byte when the address is permitted.</returns>
        public byte ReadCartridgeByte(int address)
        {
            RejectTilemapSource(address);
            return CartridgeImportSource.Require(inner).ReadCartridgeByte(address);
        }

        /// <summary>Forwards writes to the wrapped address space.</summary>
        /// <param name="address">Address to update.</param>
        /// <param name="value">Byte to store at that address.</param>
        public void WriteByte(int address, byte value) => inner.WriteByte(address, value);
    }
}
