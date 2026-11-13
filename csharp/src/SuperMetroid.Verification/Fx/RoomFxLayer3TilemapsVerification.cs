using SuperMetroid.AssetExtraction.CartridgeTables;
using System.Text.Json;
using static SuperMetroid.Verification.Fx.LookupAtmosphereTilemapsVerification;
using static SuperMetroid.Verification.Fx.LookupFxTilemapDispatchVerification;
using static SuperMetroid.Verification.Fx.LookupFxTilemapSourcesVerification;
using static SuperMetroid.Verification.Fx.LookupLiquidTilemapsVerification;
using static SuperMetroid.Verification.Fx.LookupSporeTilemapVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Fx;
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
    internal static void VerifyRoomFxLayer3Tilemaps()
    {
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "FX tilemap oracle revision");
        Suite(nameof(VerifyFxTilemapTypeEnumeration), () => VerifyFxTilemapTypeEnumeration());
        Suite(nameof(VerifyFxTilemapSourceAddresses), () => VerifyFxTilemapSourceAddresses(rom));
        byte[] json = RoomFxLayer3TilemapExtractor.Extract(NewLiquidTilemapSourceGuard(rom));
        RoomFxLayer3Pages catalog = RoomFxLayer3Pages.Load(new MemoryStream(json));
        Suite(nameof(VerifyLiquidTilemaps), () => VerifyLiquidTilemaps(rom, catalog));
        Suite(nameof(VerifySporeTilemapAttributes), () => VerifySporeTilemapAttributes(rom, catalog));
        Suite(nameof(VerifyAtmosphereTilemapFields), () => VerifyAtmosphereTilemapFields(rom, catalog));
        Suite(nameof(VerifyFxTilemapPageDispatch), () => VerifyFxTilemapPageDispatch(rom, catalog));
        foreach (RoomFxType type in RoomFxLayer3TilemapFormat.Types)
        {
            int source = RoomFxLayer3TilemapSources.Page(type).Value;
            byte[] native = RomDataReader.ReadFixedBank(rom, source,
                RoomFxLayer3TilemapFormat.PageByteCount);
            if (type == RoomFxType.Spores) continue;
            ushort[] plane = LoadConstructedRoomFxTilemap(type, catalog, out CartridgeReadGuard guarded);
            int first = FxOverlayPlaneConstants.EffectPageFirstRow * FxOverlayPlaneConstants.Columns;
            for (int index = 0; index < native.Length / sizeof(ushort); index++)
                AssertEqual(LittleEndianBytes.Word(native[index * 2], native[index * 2 + 1]), plane[first + index],
                    $"room-FX {type} installed page word {index} is shown on the BG3 plane");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"room-FX {type} installed load reads no tilemap ROM or pointer table");
        }
        AssertThrows<InvalidDataException>(() => RoomFxLayer3Pages.Load(
            new MemoryStream([1, 2, 3])), "corrupt room-FX BG3 tilemap JSON fails loudly");
        Console.WriteLine("  Room-FX BG3 tilemaps: six complete native pages and five guarded installed loads pass.");
    }

    /// <summary>Checks that the room-FX BG3 tilemap codec rejects malformed documents.</summary>
    internal static void VerifyRoomFxLayer3TilemapsRejectMalformedDocuments(string extracted)
    {
        RoomFxLayer3TilemapDocument document = JsonSerializer.Deserialize<RoomFxLayer3TilemapDocument>(
            File.ReadAllBytes(Path.Combine(extracted, RoomFxLayer3TilemapFormat.FileName)), CamelCaseCaseInsensitiveJson) ?? throw new InvalidDataException("Stock room-FX BG3 document is null.");
        AssertThrows<InvalidDataException>(() =>
        {
            document.Pages[nameof(RoomFxType.Lava)][0] = document.Pages[nameof(RoomFxType.Lava)][0] with { TileColumn = 32 };
            using var invalid = new MemoryStream();
            RoomFxLayer3Pages.Write(invalid, document);
            _ = RoomFxLayer3Pages.Load(new MemoryStream(invalid.ToArray(), writable: false));
        }, "invalid room-FX BG3 tile index fails loudly on read");
    }

    /// <summary>
    /// Loads one compiled default FX record with native tilemap reads forbidden; returns the tile words <paramref name="catalog"/>
    /// spells for the BG3 plane it shows under a blank HUD (#1466): the plane names its page, the catalog gives its words.
    /// </summary>
    private static ushort[] LoadConstructedRoomFxTilemap(RoomFxType type,
        RoomFxLayer3Pages catalog, out CartridgeReadGuard guarded)
    {
        RoomFxRecordDefinition record = RoomFxDefinitions.AllRecords.FirstOrDefault(candidate =>
            candidate.DoorPointer == 0 && candidate.Type == type)
            ?? throw new InvalidDataException($"No compiled default room-FX record selects {type}.");
        var memory = new TestAddressSpace();
        guarded = NewForbiddenRoomFxTilemapBus(memory);
        var stateSession = new RoomFxSession{
        };
        RoomFxOverlayState state = RoomFxOverlayState.ForDefinition(stateSession, record, randomNumber: 0);
        AssertEqual(type, state.Type, "installed room-FX BG3 page preserves the selected FX type");
        AssertEqual<RoomFxType?>(type, state.EffectPlane.EffectPage, "the room's BG3 plane shows the selected FX type's page");
        return state.EffectPlane.WordsIn(catalog);
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
