using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Checks Baby Metroid's compiled route records against pinned native words and pointer bounds.</summary>
internal static class BabyMetroidRouteDefinitionsVerification
{
    /// <summary>
    /// Compares every compiled Baby Metroid route word with the pinned cartridge. The
    /// integrated cutscene fixture separately runs the complete route through a bus that
    /// rejects reads of this source range.
    /// </summary>
    internal static void VerifyBabyMetroidRouteDefinitions()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        for (int index = 0; index < BabyMetroidRouteDefinitions.RecordCount; index++)
        {
            ushort pointer = unchecked((ushort)(
                BabyMetroidRouteDefinitions.FirstRecordPointer +
                index * BabyMetroidRouteDefinitions.RecordStride));
            int address = 0xa90000 | pointer;
            BabyMetroidRouteRecord record = BabyMetroidRouteDefinitions.GetRecord(pointer);

            AssertEqual(ReadRomWord(rom, address), record.Target.X,
                $"Baby route {index} target X");
            AssertEqual(ReadRomWord(rom, address + 2), record.Target.Y,
                $"Baby route {index} target Y");
            AssertEqual(ReadRomWord(rom, address + 4), record.AccelerationDivisorIndex,
                $"Baby route {index} acceleration-divisor index");
            AssertEqual(ReadRomWord(rom, address + 6), (ushort)record.MovementFunction,
                $"Baby route {index} movement callback");
            AssertEqual(ReadRomWord(rom, address + 8), record.FollowingWord,
                $"Baby route {index} overlapping following word");
        }

        AssertThrows<InvalidDataException>(
            () => BabyMetroidRouteDefinitions.GetRecord(0xca23),
            "Baby route rejects pointer before its definition range");
        AssertThrows<InvalidDataException>(
            () => BabyMetroidRouteDefinitions.GetRecord(0xca25),
            "Baby route rejects an unaligned definition pointer");
        AssertThrows<InvalidDataException>(
            () => BabyMetroidRouteDefinitions.GetRecord(0xca64),
            "Baby route rejects its terminal callback word as a record");
        AssertThrows<InvalidDataException>(
            () => BabyMetroidRouteDefinitions.GetWrongWayOffScreenXSpeed((BabyMetroidRouteMovementFunction)0),
            "Baby route rejects an unknown movement callback");

        Console.WriteLine(
            "  Baby route definitions: 33 overlapping native words and callback identities agree.");
    }

    /// <summary>Reads one native route word solely as the verification oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle words for cartridge definition comparisons")]
    internal static ushort ReadRomWord(SuperMetroidAddressSpace bus, int address) =>
        CartridgeImportSource.Require(bus).ReadCartridgeWord(address);

    /// <summary>Forbids production reads of the migrated Baby Metroid route source range.</summary>
    private static CartridgeReadGuard NewBabyRouteReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).ForbidRange(BabyMetroidRouteDefinitionsConstants.SourceAddress,
            BabyMetroidRouteDefinitionsConstants.SourceByteLength,
            address => $"Baby route still reads compiled ROM byte ${address:X6}.");
}
