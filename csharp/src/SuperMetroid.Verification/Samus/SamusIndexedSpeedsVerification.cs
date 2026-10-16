using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Samus;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Samus's indexed horizontal speed records and restored table-address semantics.</summary>
internal static class SamusIndexedSpeedsVerification
{
    /// <summary>Compares authored speed words with ROM and rejects every non-catalog base, including bank $90's low half.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Samus indexed speeds")]
    internal static void VerifySamusIndexedSpeeds(SuperMetroidAddressSpace rom)
    {
        SpeedTableEntry Read(int a)
        {
            return new(CartridgeImportSource.Require(rom).ReadCartridgeWord(a + 0), CartridgeImportSource.Require(rom).ReadCartridgeWord(a + 2), CartridgeImportSource.Require(rom).ReadCartridgeWord(a + 4), CartridgeImportSource.Require(rom).ReadCartridgeWord(a + 6), CartridgeImportSource.Require(rom).ReadCartridgeWord(a + 8), CartridgeImportSource.Require(rom).ReadCartridgeWord(a + 10));
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
        // Every possible restored base word must preserve address wrapping and alignment.
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            speed.ActiveSpeedTableBaseAddress = (ushort)raw;
            int address = speed.ResolveEntryAddress(SamusMovementType.Running);
            AssertEqual(0x900000 | unchecked((ushort)(raw + 12)), address, "Restored base wraps as a native word");
            bool compiled = address >= 0x909f25 && address < 0x90a32d && (address - 0x909f25) % 12 == 0;
            if (compiled)
            {
                forbidReads = true;
                AssertEqual(Read(address), speed.ReadEntry(SamusMovementType.Running), "Restored authored ROM base respects exact record alignment");
            }
            else
            {
                AssertThrows<InvalidDataException>(
                    () => speed.ReadEntry(SamusMovementType.Running),
                    "Restored non-catalog ROM base rejected");
            }
        }
        Console.WriteLine("Indexed Samus speeds: all 492 authored words and every restored base address preserve authored data/alignment; compiled reads are forbidden, and low-half and ROM overreads fail loudly.");
    }
}
