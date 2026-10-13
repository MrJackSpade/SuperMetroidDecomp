using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static AssertionsVerification;
using static VerificationHelpersVerification;

internal static class SamusIndexedSpeedsVerification
{
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
                    AssertEqual(expected, speed.ReadEntry(guard, (SamusMovementType)movement), "Actual byte-indexed read preserves adjacent authored tables");
                }
                else
                {
                    AssertThrows<InvalidDataException>(
                        () => speed.ReadEntry(guard, (SamusMovementType)movement),
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
            if (address < 0x908000) continue; // Mutable aliases are separately exercised below.
            bool compiled = address >= 0x909f25 && address < 0x90a32d && (address - 0x909f25) % 12 == 0;
            if (compiled)
            {
                forbidReads = true;
                AssertEqual(Read(address), speed.ReadEntry(guard, SamusMovementType.Running), "Restored authored ROM base respects exact record alignment");
            }
            else
            {
                AssertThrows<InvalidDataException>(
                    () => speed.ReadEntry(guard, SamusMovementType.Running),
                    "Restored non-catalog ROM base rejected");
            }
        }
        var mutable = new TestAddressSpace();
        speed.ActiveSpeedTableBaseAddress = (ushort)0x0100;
        foreach (ushort changed in new ushort[] { 0x1234, 0x4321 })
        {
            WriteTestWord(mutable, 0x90010c, changed);
            AssertEqual(changed, speed.ReadEntry(mutable, SamusMovementType.Running).Acceleration, "Restored WRAM base remains live");
        }
        Console.WriteLine("Indexed Samus speeds: all 492 authored words and every restored base address preserve authored data/alignment; compiled reads are forbidden, mutable aliases stay live, and ROM overreads fail loudly.");
    }
}
