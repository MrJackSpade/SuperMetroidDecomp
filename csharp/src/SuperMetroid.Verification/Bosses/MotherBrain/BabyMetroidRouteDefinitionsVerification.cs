using SuperMetroid.Core;
using SuperMetroid.Core.Bosses;
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
        IReadOnlyList<BabyMetroidRouteLeg> legs = BabyMetroidRouteDefinitions.Legs;
        AssertEqual(ListStep<BabyMetroidRouteLeg>.At(legs, 0), BabyMetroidRoute.FirstLeg, "the route starts on its first leg");
        for (int index = 0; index < legs.Count; index++)
        {
            int address = BabyMetroidRouteDefinitionsConstants.SourceAddress +
                index * BabyMetroidRouteDefinitionsConstants.RecordByteLength;
            ListStep<BabyMetroidRouteLeg> leg = ListStep<BabyMetroidRouteLeg>.At(legs, index);
            BabyMetroidRouteLeg record = leg.Entry;
            BabyMetroidRouteLeg? next = leg.Following?.Entry;
            bool endsWithLatchOntoSamus = next is null;

            AssertEqual(ReadRomWord(rom, address), record.Target.X,
                $"Baby route {index} target X");
            AssertEqual(ReadRomWord(rom, address + 2), record.Target.Y,
                $"Baby route {index} target Y");
            // The cartridge stores the divisor as its index into a table counting sixteen down to one.
            AssertEqual(16 - ReadRomWord(rom, address + 4), BabyMetroidRouteDefinitions.AccelerationDivisor,
                $"Baby route {index} acceleration divisor");
            AssertEqual(MotherBrainNativeWords.BabyMetroidRouteWrongWaySpeeds[ReadRomWord(rom, address + 6)],
                record.WrongWayOffScreenXSpeed.InTwoHundredFiftySixthsOfPixel,
                $"Baby route {index}: the cartridge's movement routine reverses with the leg's wrong-way speed");
            // The cartridge reads one word past each record and ends the route when that word's
            // sign bit is set: it is the next waypoint's X after an ordinary leg, and the
            // latch-onto-Samus function's address after the final one.
            ushort following = ReadRomWord(rom, address + 8);
            AssertEqual(endsWithLatchOntoSamus, unchecked((short)following) < 0,
                $"Baby route {index} ends exactly where the following word's sign bit is set");
            if (next is { } nextLeg)
                AssertEqual(nextLeg.Target.X, following,
                    $"Baby route {index} is followed by the next waypoint's X");
            else
                AssertEqual(BabyMetroidRouteDefinitionsConstants.LatchOntoSamusFunction, following,
                    $"Baby route {index} is followed by the latch-onto-Samus function");
        }

        Console.WriteLine(
            "  Baby route definitions: 33 overlapping native words, the legs' wrong-way speeds and the route's end agree.");
    }

    /// <summary>Reads one native route word solely as the verification oracle.</summary>
    internal static ushort ReadRomWord(CartridgeImportAddressSpace bus, int address) =>
        bus.ReadCartridgeWord(address);

}
