using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.CartridgeTables;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Fx.LookupAtmosphereTilemapsVerification;
using static SuperMetroid.Verification.Fx.LookupFxTilemapDispatchVerification;
using static SuperMetroid.Verification.Fx.LookupFxTilemapSourcesVerification;
using static SuperMetroid.Verification.Fx.LookupLiquidTilemapsVerification;
using static SuperMetroid.Verification.Fx.LookupSporeTilemapVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Fx;

using SuperMetroid.Verification.Harness;

/// <summary>Checks six installed room-FX BG3 tilemap pages against native VRAM output under read guards.</summary>
internal static class RoomFxLayer3TilemapsVerification
{
    /// <summary>JSON options: Camel-case property names with case-insensitive matching.</summary>
    private static readonly JsonSerializerOptions CamelCaseCaseInsensitiveJson = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true };

    /// <summary>Checks all six extracted BG3 pages and guarded installed FX loads against native VRAM words.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for every room-FX BG3 tilemap page")]
    internal static void VerifyRoomFxLayer3Tilemaps()
    {
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "FX tilemap oracle revision");
        Suite(nameof(VerifyFxTilemapTypeEnumeration), () => VerifyFxTilemapTypeEnumeration());
        Suite(nameof(VerifyFxTilemapSourceAddresses), () => VerifyFxTilemapSourceAddresses(rom));
        byte[] json = RoomFxLayer3TilemapExtractor.Extract(NewLiquidTilemapSourceGuard(rom));
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
            Layer3Plane plane = LoadConstructedRoomFxTilemap(type, catalog, paletteColors,
                out CartridgeReadGuard guarded);
            int first = Layer3Plane.EffectPageFirstRow * Layer3Plane.Columns;
            for (int index = 0; index < native.Length / sizeof(ushort); index++)
                AssertEqual(SnesLittleEndian.Word(native[index * 2], native[index * 2 + 1]), plane.Words[first + index],
                    $"room-FX {type} installed page word {index} is shown on the BG3 plane");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"room-FX {type} installed load reads no tilemap ROM or pointer table");
        }
        AssertThrows<InvalidDataException>(() => RoomFxLayer3TilemapCatalog.Load(
            new MemoryStream([1, 2, 3])), "corrupt room-FX BG3 tilemap JSON fails loudly");
        Console.WriteLine("  Room-FX BG3 tilemaps: six complete native pages and five guarded installed loads pass.");
    }

    /// <summary>Checks a lava tile edit reaches production VRAM and removing the override restores stock art.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyRoomFxLayer3TilemapOverride(
        string stock, string overrides, AreaMapPresentationCatalog baseline)
    {
        string path = Path.Combine(overrides, RoomFxLayer3TilemapFormat.FileName);
        string stockPath = Path.Combine(stock, RoomFxLayer3TilemapFormat.FileName);
        RoomFxLayer3TilemapDocument document = JsonSerializer.Deserialize<RoomFxLayer3TilemapDocument>(
            File.ReadAllBytes(stockPath), CamelCaseCaseInsensitiveJson) ?? throw new InvalidDataException("Stock room-FX BG3 document is null.");
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
        Layer3Plane plane = LoadConstructedRoomFxTilemap(RoomFxType.Lava,
            edited.RoomFxLayer3Tilemaps, edited.RoomFxPaletteBlends,
            out CartridgeReadGuard guarded);
        ushort editedWord = BitConverter.ToUInt16(
            edited.RoomFxLayer3Tilemaps.Resolve(RoomFxType.Lava).Span[..sizeof(ushort)]);
        AssertEqual(editedWord, plane.Words[Layer3Plane.EffectPageFirstRow * Layer3Plane.Columns],
            "edited lava tile cell reaches the room's BG3 plane");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
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
        Console.WriteLine("Room-FX BG3 override: edited lava tile reaches the BG3 plane and removal restores stock.");
    }

    /// <summary>Loads one compiled default FX record with native tilemap reads forbidden; returns the BG3 plane it shows under a blank HUD.</summary>
    private static Layer3Plane LoadConstructedRoomFxTilemap(RoomFxType type,
        RoomFxLayer3TilemapCatalog catalog, RoomFxPaletteBlendCatalog paletteColors,
        out CartridgeReadGuard guarded)
    {
        RoomFxRecordDefinition record = RoomFxDefinitions.AllRecords.FirstOrDefault(candidate =>
            candidate.DoorPointer == 0 && candidate.Type == (byte)type)
            ?? throw new InvalidDataException($"No compiled default room-FX record selects {type}.");
        var memory = new TestAddressSpace();
        guarded = NewForbiddenRoomFxTilemapBus(memory);
        var stateSession = new RoomFxSession{
            Layer3Tilemaps = catalog,
            PaletteBlendColors = paletteColors,
        };
        RoomLayer3FxState state = RoomLayer3FxState.ForDefinition(stateSession, new GameplayColours(new SpriteColourSets()), record, randomNumber: 0);
        AssertEqual(type, state.Type, "installed room-FX BG3 page preserves the selected FX type");
        return Layer3Plane.Compose(new ushort[Layer3Plane.HudRows * Layer3Plane.Columns], state.EffectPageWords());
    }

    /// <summary>Forbids bank-$8A tilemap payloads and the FX tilemap pointer table.</summary>
    private static CartridgeReadGuard NewForbiddenRoomFxTilemapBus(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(
            address => (SnesAddressMath.BankNumber(address)) == 0x8a ||
                address >= (int)RoomFxLayer3SourceDefinitions.Layer3TilemapPointers &&
                address < (int)RoomFxLayer3SourceDefinitions.Layer3TilemapPointers + 14,
            address => $"Installed room-FX BG3 tilemap read ROM ${address:X6}.");

    /// <summary>Runs the FX tilemap source lookup suite.</summary>
    [VerificationCommand("--lookup-fx-tilemap-sources")]
    internal static int LookupFxTilemapSourcesCommand()
    {
        Suite(nameof(VerifyRoomFxLayer3Tilemaps), () => VerifyRoomFxLayer3Tilemaps());
        return 0;
    }

    /// <summary>Runs the installed room-FX BG3 tilemap suite.</summary>
    [VerificationCommand("--room-fx-layer3-tilemaps")]
    internal static int RoomFxLayer3TilemapsCommand()
    {
        Suite(nameof(VerifyRoomFxLayer3Tilemaps), () => VerifyRoomFxLayer3Tilemaps());
        return 0;
    }
}
