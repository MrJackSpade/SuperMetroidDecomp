using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Fx.LookupAnimatedAreasVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Fx;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Checks compiled area animated-tile selections against retail lists and runtime owners.</summary>
internal static class AreaBlockAnimationDefinitionsVerification
{
    /// <summary>
    /// Compares all eight native list pointers and all 64 object selections to the pinned
    /// cartridge, then decodes one record in two areas and executes both production population
    /// owners on the named objects while rejecting every read from those immutable bank-$83 sources.
    /// </summary>
    internal static void VerifyAreaBlockAnimationDefinitions()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        AssertEqual(SuperMetroid.AssetExtraction.Cartridge.SupportedCartridge.Sha256.ToUpperInvariant(),
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

    /// <summary>
    /// Decodes the record's animated-tile bits in <paramref name="area"/> and checks the sand and treadmill population
    /// of the objects it names while guarding immutable bank-$83 sources.
    /// </summary>
    private static void VerifyCompiledAreaSelection(
        ISnesAddressSpace rom,
        AreaId area,
        byte animatedTileBits,
        int expectedSand,
        int expectedTreadmills)
    {
        // Original compiled default-door FX record with both sand/treadmill bits.
        const ushort fxRecord = 0x9e44;
        AssertEqual(animatedTileBits, rom.ReadCartridgeByte(0x839e52), "Native fixture animation bitset");
        byte[] native = [.. Enumerable.Range(0, RoomFxToolingRomData.Record.ByteCount)
            .Select(offset => rom.ReadCartridgeByte(0x839e44 + offset))];
        var fx = new RoomFxSelection($"$83:{fxRecord:X4} in {area}", [],
            RoomFxRecordCodec.Decode(area, native, $"room-FX record $83:{fxRecord:X4}"));
        var guarded = NewAreaAnimatedTileSelectionForbiddenBus(rom);
        var sand = new RoomSandBlockAnimationsState(fx, door: null);
        var treadmills = new RoomTreadmillBlockAnimationsState(fx, door: null);

        AssertEqual(expectedSand, sand.Count, $"{area} compiled sand selection");
        AssertEqual(expectedTreadmills, treadmills.Count,
            $"{area} compiled treadmill selection");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            $"{area} selection performs no immutable table reads");
    }

    /// <summary>
    /// Delegates allowed reads to the cartridge and rejects the compiled pointer/list bytes.
    /// </summary>
    private static CartridgeReadGuard NewAreaAnimatedTileSelectionForbiddenBus(ISnesAddressSpace rom) =>
        new CartridgeReadGuard(rom)
            .Forbid(IsCompiledAreaAnimatedTileSource,
                address => $"Production read compiled area animated-tile byte " +
                    $"{SnesAddress.FromBusAddress(address)}.")
            .RejectWrites(_ => "Read-only verification bus.");

    /// <summary>Recognizes bytes in the native area pointer table or any compiled per-area object list.</summary>
    private static bool IsCompiledAreaAnimatedTileSource(int address)
    {
        if (address >= AreaBlockAnimationDefinitions.NativeListPointerTable &&
            address < AreaBlockAnimationDefinitions.NativeListPointerTable +
                AreaBlockAnimationDefinitions.NativeAreaCount * sizeof(ushort))
        {
            return true;
        }

        SnesAddress source = SnesAddress.FromBusAddress(address);
        if (source.Bank != SnesLittleEndianConstants.BankByte(RoomFxRomDataBanksTooling.RoomDefinitions))
            return false;

        for (int areaIndex = 0;
             areaIndex < AreaBlockAnimationDefinitions.NativeAreaCount;
             areaIndex++)
        {
            ushort list = AreaBlockAnimationDefinitions.NativeListPointer(areaIndex);
            if (source.Offset >= list &&
                source.Offset < list +
                    AreaBlockAnimationDefinitions.ObjectsPerArea * sizeof(ushort))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Runs the area animated-tile definition and production-owner checks.</summary>
    [VerificationCommand("--area-animated-tile-definitions")]
    internal static int AreaAnimatedTileDefinitionsCommand()
    {
        Suite(nameof(VerifyAreaBlockAnimationDefinitions), () => VerifyAreaBlockAnimationDefinitions());
        return 0;
    }
}
