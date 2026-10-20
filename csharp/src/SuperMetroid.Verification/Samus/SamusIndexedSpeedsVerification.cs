using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Samus;
/// <summary>Checks Samus's indexed horizontal speed records.</summary>
internal static class SamusIndexedSpeedsVerification
{
    /// <summary>Compares authored speed words with ROM through every table and movement type, rejecting overreads past the tables.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Samus indexed speeds")]
    internal static void VerifySamusIndexedSpeeds(SuperMetroidAddressSpace rom)
    {
        SpeedTableEntry Read(int a)
        {
            SnesSignedSixteenSixteen Pair(int offset) => SnesSignedSixteenSixteen.FromParts(
                unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(a + offset)),
                CartridgeImportSource.Require(rom).ReadCartridgeWord(a + offset + 2));
            return new(Pair(0), Pair(4), Pair(8));
        }
        var speed = new SamusHorizontalSpeedState();
        // Unaligned/non-catalog spans can overlap known words, so classify each
        // caller's entry in the test rather than forbidding every byte in a range.
        bool forbidReads = false;
        var guard = new CartridgeReadGuard(rom)
            .Forbid(_ => forbidReads, _ => "Authored indexed speed unexpectedly reads the ROM.")
            .RejectWrites(_ => "Unexpected indexed-speed write.");
        int compiledCount = 0;
        foreach (SamusLiquidMedium medium in Enum.GetValues<SamusLiquidMedium>())
        {
            speed.SelectEnvironmentSpeedTable(medium);
            for (int movement = 0; movement <= byte.MaxValue; movement++)
            {
                int address = speed.ResolveEntryAddress((SamusMovementType)movement);
                SpeedTableEntry expected = Read(address);
                bool authored = address is >= 0x909f55 and < 0x90a32d;
                AssertEqual(authored, SamusHorizontalMotionDefinitions.TryResolveIndexed(address, out var entry), "Native authored/overread boundary");
                if (authored)
                {
                    AssertEqual(expected, entry, "All six indexed words match native data");
                    compiledCount++;
                    forbidReads = true;
                    AssertEqual(expected, speed.ReadEntry((SamusMovementType)movement), "Actual byte-indexed read preserves adjacent authored tables");
                }
                else
                {
                    AssertThrows<InvalidDataException>(
                        () => speed.ReadEntry((SamusMovementType)movement),
                        "Byte-indexed high-bank overread rejected");
                }
            }
        }
        AssertEqual(166, compiledCount, "Air 82 + water 56 + lava 28 authored address outcomes");
        Console.WriteLine("Indexed Samus speeds: all 492 authored words match native data through every table and movement type; compiled reads are forbidden, and overreads past the tables fail loudly.");
    }
}
