using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.LevelData.SharedSlopeHeightsVerification;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Samus.Movement;
/// <summary>Checks Samus's standalone speed records and retained mutable low-bank aliases.</summary>
internal static class SamusStandaloneSpeedsVerification
{
    /// <summary>Compares four compiled records with native arithmetic across base-speed boundaries while rejecting unknown ROM addresses.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Samus standalone speeds")]
    internal static void VerifySamusStandaloneSpeeds(SuperMetroidAddressSpace rom)
    {
        static SpeedTableEntry Read(Func<int, byte> readByte, int a)
        {
            ushort Word(int offset) => SnesLittleEndian.Word(readByte(a + offset), readByte(a + offset + 1));
            SnesSignedSixteenSixteen Pair(int offset) => SnesSignedSixteenSixteen.FromParts(unchecked((short)Word(offset)), Word(offset + 2));
            return new(Pair(0), Pair(4), Pair(8));
        }
        var actual = new SamusHorizontalSpeedState();
        var reference = new SamusHorizontalSpeedState();
        var calculate = new Func<SpeedTableEntry, uint>(reference.CalculateBaseSpeed);
        var guard = NewSlopeHeightNoReadBus();
        int cases = 0;
        foreach (int address in new[] { 0x909f25, 0x909f31, 0x909f3d, 0x909f49 })
        {
            SpeedTableEntry native = Read(rom.ReadCartridgeByte, address);
            AssertTrue(SamusHorizontalMotionDefinitions.TryResolveStandalone(address, out var compiled), "Authored standalone record recognized");
            AssertEqual(native, compiled, "All six native standalone words match");
            foreach (ushort mode in new ushort[] { 0, 1, 2, ushort.MaxValue })
            foreach (byte multiplier in new byte[] { 0, 1, 255 })
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                actual.BaseSpeed = reference.BaseSpeed = new SnesUnsignedSixteenSixteen(raw, raw);
                actual.AccelerationMode = reference.AccelerationMode = mode;
                actual.DecelerationMultiplier = reference.DecelerationMultiplier = multiplier;
                uint expected = calculate(native);
                uint result = actual.CalculateBaseSpeedAtAddress(address);
                AssertEqual(expected, result, "Compiled entry preserves existing native-word arithmetic");
                AssertEqual(reference.BaseSpeed.Whole, actual.BaseSpeed.Whole, "Whole base speed write");
                AssertEqual(reference.BaseSpeed.Fraction, actual.BaseSpeed.Fraction, "Fraction base speed write");
                AssertEqual(reference.AccelerationMode, actual.AccelerationMode, "Underflow resets acceleration mode");
                cases++;
            }
        }

        // The cartridge only ever selects authored records, so any other address, a low-bank
        // WRAM alias or an unknown or unaligned high-bank record, is corrupt state and fails
        // loudly rather than treating executable or presentation bytes as horizontal physics.
        foreach (int invalidAddress in new[] { 0x900100, 0x909f26, 0x90a32d, 0x919f25 })
        {
            AssertTrue(!SamusHorizontalMotionDefinitions.TryResolveStandalone(invalidAddress, out _), "Unauthored address not silently resolved");
            AssertThrows<InvalidDataException>(
                () => actual.CalculateBaseSpeedAtAddress(invalidAddress),
                "Unauthored horizontal record rejected");
        }

        Console.WriteLine($"Standalone Samus speeds: {cases} actual bomb/Grapple calculations match ROM-fed arithmetic with reads forbidden; unauthored records fail loudly.");
    }
}
