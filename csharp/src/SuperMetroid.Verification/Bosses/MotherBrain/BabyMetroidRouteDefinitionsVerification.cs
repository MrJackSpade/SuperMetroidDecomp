using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.MotherBrain;

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
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        for (int index = 0; index < BabyMetroidRouteDefinitions.RecordCount; index++)
        {
            int address = BabyMetroidRouteDefinitionsConstants.SourceAddress +
                index * BabyMetroidRouteDefinitionsConstants.RecordByteLength;
            BabyMetroidRouteRecord record = BabyMetroidRouteDefinitions.GetRecord(index);

            AssertEqual(ReadRomWord(rom, address), record.Target.X,
                $"Baby route {index} target X");
            AssertEqual(ReadRomWord(rom, address + 2), record.Target.Y,
                $"Baby route {index} target Y");
            AssertEqual(ReadRomWord(rom, address + 4), record.AccelerationDivisorIndex,
                $"Baby route {index} acceleration-divisor index");
            AssertEqual(ReadRomWord(rom, address + 6), MotherBrainNativeWords.BabyMetroidRouteMovementFunctions.Word(record.MovementFunction),
                $"Baby route {index}: the cartridge's routine word names the record's movement callback");
            // The cartridge reads one word past each record and ends the route when that word's
            // sign bit is set: it is the next waypoint's X after an ordinary leg, and the
            // latch-onto-Samus function's address after the final one.
            ushort following = ReadRomWord(rom, address + 8);
            AssertEqual(record.EndsWithLatchOntoSamus, unchecked((short)following) < 0,
                $"Baby route {index} ends exactly where the following word's sign bit is set");
            if (record.EndsWithLatchOntoSamus)
                AssertEqual(BabyMetroidRouteDefinitionsConstants.LatchOntoSamusFunction, following,
                    $"Baby route {index} is followed by the latch-onto-Samus function");
            else
                AssertEqual(BabyMetroidRouteDefinitions.GetRecord(index + 1).Target.X, following,
                    $"Baby route {index} is followed by the next waypoint's X");
        }

        AssertThrows<InvalidDataException>(
            () => BabyMetroidRouteDefinitions.GetRecord(-1),
            "Baby route rejects a leg before the first");
        AssertThrows<InvalidDataException>(
            () => BabyMetroidRouteDefinitions.GetRecord(BabyMetroidRouteDefinitions.RecordCount),
            "Baby route rejects a leg past the final one");
        AssertThrows<InvalidDataException>(
            () => BabyMetroidRouteDefinitions.GetWrongWayOffScreenXSpeed(0),
            "Baby route rejects an unknown movement callback");

        Console.WriteLine(
            "  Baby route definitions: 33 overlapping native words and callback identities agree.");
    }

    /// <summary>Reads one native route word solely as the verification oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle words for cartridge definition comparisons")]
    internal static ushort ReadRomWord(SuperMetroidAddressSpace bus, int address) =>
        CartridgeImportSource.Require(bus).ReadCartridgeWord(address);

}
