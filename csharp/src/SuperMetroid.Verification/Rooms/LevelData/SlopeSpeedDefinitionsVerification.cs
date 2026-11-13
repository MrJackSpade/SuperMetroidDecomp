using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Rooms.LevelData;

/// <summary>Checks compiled grounded slope multipliers and packed displacement arithmetic against native values.</summary>
internal static class SlopeSpeedDefinitionsVerification
{
    /// <summary>Compares all slope shapes and BTS orientations, including signed byte packing and airborne early return.</summary>
    internal static void VerifyCompiledSlopeSpeeds(SuperMetroidAddressSpace rom)
    {
        var multipliers = new ushort[32];
        for (int shape = 0; shape < multipliers.Length; shape++)
        {
            int address = SamusMovementDefinitionsSlopes.HorizontalMultipliers + shape * 4 + 2;
            multipliers[shape] = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(multipliers[shape], SlopeSpeeds.HorizontalMultiplier(shape), "All native horizontal slope multipliers");
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

        // The cartridge's word arithmetic agrees with plain scaling wherever the truncated displacement fits a word,
        // under 256 pixels an update either way; beyond that the word wrap is not part of the slope rule (#1565).
        static long PlainScale(int displacement, ushort multiplier) => (long)(displacement >> 8) * multiplier;

        // The production rule takes a typed behaviour, displacement and speed; the cases are stated as raw words.
        static int Scale(int behavior, int displacement, uint verticalSpeed) =>
            SamusSlopePhysics.ScaleGroundedHorizontalDisplacement(RoomBlockBehaviorCodec.Decode(behavior),
                SixteenSixteenWords.ToDisplacement(displacement), UnsignedSixteenSixteen.FromRaw(verticalSpeed)).RawValue;

        int cases = 0;
        uint[] highBytes = [0, 0xff000000];
        for (int shape = 0; shape < 32; shape++)
        for (int operand = 0; operand <= ushort.MaxValue; operand++)
        foreach (uint high in highBytes)
        {
            // Vary discarded low bits across every operand while exercising both signs
            // and discarded high-byte boundaries independently of the middle word.
            int displacement = unchecked((int)(high | (uint)(operand << 8) | (uint)(operand & 255)));
            // The leftmost operand truncates to -65536, a magnitude no word holds; the boundaries below cover it.
            if (displacement >> 8 < -ushort.MaxValue)
                continue;
            int native = NativeScale(displacement, multipliers[shape]);
            AssertEqual(native, PlainScale(displacement, multipliers[shape]), "Native word arithmetic is plain scaling within a word");
            AssertEqual(native, Scale(shape, displacement, 0),
                "Native byte truncation, sign and multiplier through real slope movement");
            cases++;
        }

        int[] boundaries = [int.MinValue, int.MinValue + 1, -0x1000000, -0x10001, -0x10000, -257, -256, -255, -1,
            0, 1, 255, 256, 257, 0xffff, 0x10000, 0x10001, int.MaxValue];
        uint[] verticalSpeeds = [0, 1, 0xffff, 0x10000, 0x80000000, uint.MaxValue];
        for (int raw = 0; raw < 256; raw++)
        foreach (int displacement in boundaries)
        foreach (uint vertical in verticalSpeeds)
        {
            long expected = (raw & 128) != 0 || vertical != 0
                ? displacement : PlainScale(displacement, multipliers[raw & 31]);
            if (expected is < int.MinValue or > int.MaxValue)
            {
                AssertThrows<OverflowException>(() => { _ = Scale(raw, displacement, vertical); },
                    "A displacement whose scaled amount does not fit fails loudly");
                continue;
            }
            AssertEqual(expected, Scale(raw, displacement, vertical),
                "Every BTS orientation and whole/fractional airborne early return");
        }
        AssertEqual(4194272, cases, "Exhaustive slope multiplier operand coverage");
        Console.WriteLine("Slope speeds: 32 native multipliers, 4194272 signed byte-packed operands and 27648 BTS/airborne boundary cases pass without ROM reads.");
    }
}
