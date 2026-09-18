using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifySamusStandaloneSpeeds(SuperMetroidAddressSpace rom)
    {
        SpeedTableEntry Read(ISnesAddressSpace source, int a)
        {
            ushort Word(int offset) => (ushort)(source.ReadByte(a + offset) | source.ReadByte(a + offset + 1) << 8);
            return new(Word(0), Word(2), Word(4), Word(6), Word(8), Word(10));
        }
        var actual = new SamusHorizontalSpeedState();
        var reference = new SamusHorizontalSpeedState();
        var calculate = typeof(SamusHorizontalSpeedState).GetMethod("CalculateBaseSpeed",
            BindingFlags.Instance | BindingFlags.NonPublic, [typeof(SpeedTableEntry)])!
            .CreateDelegate<Func<SpeedTableEntry, uint>>(reference);
        var guard = new SlopeHeightNoReadBus();
        int cases = 0;
        foreach (int address in new[] { 0x909f25, 0x909f31, 0x909f3d, 0x909f49 })
        {
            SpeedTableEntry native = Read(rom, address);
            AssertTrue(SamusHorizontalMotionDefinitions.TryResolveStandalone(address, out var compiled), "Authored standalone record recognized");
            AssertEqual(native, compiled, "All six native standalone words match");
            foreach (ushort mode in new ushort[] { 0, 1, 2, ushort.MaxValue })
            foreach (byte multiplier in new byte[] { 0, 1, 255 })
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                actual.BaseSpeed = reference.BaseSpeed = (ushort)raw;
                actual.BaseSubspeed = reference.BaseSubspeed = (ushort)raw;
                actual.AccelerationMode = reference.AccelerationMode = mode;
                actual.DecelerationMultiplier = reference.DecelerationMultiplier = multiplier;
                uint expected = calculate(native);
                uint result = actual.CalculateBaseSpeedAtAddress(guard, address);
                AssertEqual(expected, result, "Compiled entry preserves existing native-word arithmetic");
                AssertEqual(reference.BaseSpeed, actual.BaseSpeed, "Whole base speed write");
                AssertEqual(reference.BaseSubspeed, actual.BaseSubspeed, "Fraction base speed write");
                AssertEqual(reference.AccelerationMode, actual.AccelerationMode, "Underflow resets acceleration mode");
                cases++;
            }
        }

        // Low-bank aliases remain live because they are mutable SNES memory, not immutable
        // cartridge definitions. Unknown/unaligned high-bank records fail loudly rather
        // than treating executable or presentation bytes as horizontal physics.
        var mutable = new TestAddressSpace();
        const int mutableAddress = 0x900100;
        AssertTrue(!SamusHorizontalMotionDefinitions.TryResolveStandalone(mutableAddress, out _), "Mutable address not silently clamped");
        foreach (ushort acceleration in new ushort[] { 0x1234, 0x4321 })
        {
            ushort[] words = [0, acceleration, 15, 0, 0, 1];
            for (int i = 0; i < words.Length; i++) WriteTestWord(mutable, mutableAddress + i * 2, words[i]);
            actual.BaseSpeed = reference.BaseSpeed = 0;
            actual.BaseSubspeed = reference.BaseSubspeed = 0;
            actual.AccelerationMode = reference.AccelerationMode = 0;
            AssertEqual(calculate(Read(mutable, mutableAddress)), actual.CalculateBaseSpeedAtAddress(mutable, mutableAddress), "Mutable low-bank record observes live changed data");
        }
        foreach (int invalidAddress in new[] { 0x909f26, 0x90a32d, 0x919f25 })
            AssertThrows<InvalidDataException>(
                () => actual.CalculateBaseSpeedAtAddress(mutable, invalidAddress),
                "Unknown high-bank horizontal record rejected");

        Console.WriteLine($"Standalone Samus speeds: {cases} actual bomb/Grapple calculations match ROM-fed arithmetic with reads forbidden; mutable low-bank aliases remain live and unknown ROM records fail loudly.");
    }
}
