using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Samus;
/// <summary>Checks Samus's indexed horizontal speed records.</summary>
internal static class SamusIndexedSpeedsVerification
{
    /// <summary>Compares authored speed words with ROM through every table and movement type, rejecting overreads past the tables.</summary>
    internal static void VerifySamusIndexedSpeeds(CartridgeImportAddressSpace rom)
    {
        SpeedTableEntry Read(int a)
        {
            SignedSixteenSixteen Pair(int offset) => SignedSixteenSixteen.FromParts(
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
        // The table entries lie in movement-byte order from the entry of the movement byte zero names.
        SamusMovementType byteZero = SamusMovementTypeNumbers.Of(0);
        foreach (SamusLiquidMedium medium in Enum.GetValues<SamusLiquidMedium>())
        {
            speed.SelectEnvironmentSpeedTable(medium);
            for (int movement = 0; movement <= byte.MaxValue; movement++)
            {
                // The cartridge indexes the selected table by the movement byte. A byte past the movement types names
                // none, so its row is the table's first row plus the byte, read through the row-indexed entry.
                int row = speed.ResolveEntryRow(byteZero) + movement;
                bool named = SamusMovementTypeNativeWords.Types.TryDecode(movement, out SamusMovementType type);
                if (named)
                    AssertEqual(row, speed.ResolveEntryRow(type), "Each movement type selects the row its movement byte indexes");
                Func<SpeedTableEntry> readEntry = named
                    ? () => speed.ReadEntry(type)
                    : () => SamusHorizontalSpeedState.ReadEntryAtRow(row);
                int address = speed.ResolveEntryAddress(byteZero) + SpeedTableEntryConstants.ByteCount * movement;
                SpeedTableEntry expected = Read(address);
                // The two records after knockback describe no movement type a pose selects, so the game holds no record for them.
                int position = row < SamusHorizontalMotionDefinitions.AirRecordCount
                    ? row
                    : (row - SamusHorizontalMotionDefinitions.AirRecordCount) % SamusHorizontalMotionDefinitions.LiquidRecordCount;
                bool authored = address is >= 0x909f55 and < 0x90a32d && !SamusMovementTypeNumbers.Unselected.Contains(position);
                AssertEqual(authored, SamusHorizontalMotionDefinitions.TryResolveIndexed(address, out var entry), "Native authored/overread boundary");
                if (authored)
                {
                    AssertEqual(expected, entry, "All six indexed words match native data");
                    compiledCount++;
                    forbidReads = true;
                    AssertEqual(expected, readEntry(), "Actual byte-indexed read preserves adjacent authored tables");
                }
                else
                {
                    AssertThrows<InvalidDataException>(
                        () => readEntry(),
                        "Byte-indexed high-bank overread rejected");
                }
            }
        }
        AssertEqual(154, compiledCount, "Air 76 + water 52 + lava 26 authored address outcomes, without the six records no movement type selects");
        Console.WriteLine("Indexed Samus speeds: all 456 authored words match native data through every table and movement type; compiled reads are forbidden, and overreads past the tables fail loudly.");
    }
}
