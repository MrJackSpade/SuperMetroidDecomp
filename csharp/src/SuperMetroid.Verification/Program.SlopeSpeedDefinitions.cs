using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCompiledSlopeSpeeds(SuperMetroidAddressSpace rom)
    {
        var bus = new SlopeHeightNoReadBus();
        var multipliers = new ushort[32];
        for (int shape = 0; shape < multipliers.Length; shape++)
        {
            int address = SamusMovementRomData.Slopes.HorizontalMultipliers + shape * 4 + 2;
            multipliers[shape] = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
            AssertEqual(multipliers[shape], SlopeSpeedDefinitions.HorizontalMultiplier(shape), "All native horizontal slope multipliers");
        }

        // Model the native byte loads/XBA, 16-bit negation and unsigned multiply with
        // wide arithmetic. Do not call production math to obtain the expected value.
        static int NativeScale(int displacement, ushort multiplier)
        {
            uint bits = unchecked((uint)displacement);
            uint operand = ((bits >> 16) & 255) * 256 + ((bits >> 8) & 255);
            if ((bits & 0x80000000) != 0) operand = (65536 - operand) % 65536;
            ulong product = (ulong)operand * multiplier;
            return unchecked((int)((bits & 0x80000000) != 0
                ? (uint)((0x100000000UL - product) & 0xffffffffUL)
                : (uint)product));
        }

        int cases = 0;
        uint[] highBytes = [0, 0x7f000000, 0x80000000, 0xff000000];
        for (int shape = 0; shape < 32; shape++)
        for (int operand = 0; operand <= ushort.MaxValue; operand++)
        foreach (uint high in highBytes)
        {
            // Vary discarded low bits across every operand while exercising both signs
            // and discarded high-byte boundaries independently of the middle word.
            int displacement = unchecked((int)(high | (uint)(operand << 8) | (uint)(operand & 255)));
            AssertEqual(NativeScale(displacement, multipliers[shape]),
                SamusSlopePhysics.ScaleGroundedHorizontalDisplacement(bus, (byte)shape, displacement, 0),
                "Native byte truncation, sign and multiplier through real slope movement");
            cases++;
        }

        int[] boundaries = [int.MinValue, int.MinValue + 1, -0x10001, -0x10000, -257, -256, -255, -1,
            0, 1, 255, 256, 257, 0xffff, 0x10000, 0x10001, int.MaxValue];
        uint[] verticalSpeeds = [0, 1, 0xffff, 0x10000, 0x80000000, uint.MaxValue];
        for (int raw = 0; raw < 256; raw++)
        foreach (int displacement in boundaries)
        foreach (uint vertical in verticalSpeeds)
        {
            int expected = (raw & 128) != 0 || vertical != 0
                ? displacement : NativeScale(displacement, multipliers[raw & 31]);
            AssertEqual(expected, SamusSlopePhysics.ScaleGroundedHorizontalDisplacement(bus, (byte)raw, displacement, vertical),
                "Every BTS orientation and whole/fractional airborne early return");
        }
        AssertEqual(8388608, cases, "Exhaustive slope multiplier operand coverage");
        Console.WriteLine("Slope speeds: 32 native multipliers, 8388608 signed byte-packed operands and 26112 BTS/airborne boundary cases pass without ROM reads.");
    }
}
