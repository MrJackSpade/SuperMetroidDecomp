using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend;

internal static partial class Program
{
    private static void VerifyCompiledEnemyTrigonometry()
    {
        var rom = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var bytes = new byte[128];
        var words = new ushort[128];
        for (int i = 0; i < 128; i++)
        {
            bytes[i] = rom.ReadByte(EnemyMathReferenceData.ByteSine + i);
            int address = EnemyMathReferenceData.UnsignedSine + i * 2;
            words[i] = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        }
        AssertTrue(bytes.AsSpan().SequenceEqual(EnemyTrigonometryTables.EightBitHalfWave), "all compiled byte samples match ROM");
        AssertTrue(words.AsSpan().SequenceEqual(EnemyTrigonometryTables.UnsignedHalfWave), "all compiled unsigned samples match ROM");
        VerifyCompiledSignedTrigonometry(rom);
        VerifyCompiledGrappleMath(rom);
        VerifyCompiledProjectileMath(rom);
        VerifyCompiledFamilyTrigonometry(rom);
        VerifyPhantoonWaveMath(rom);
        VerifyCompiledLinearEnemySpeeds(rom);
        VerifyCompiledQuadraticEnemySpeeds(rom);
        VerifyCompiledBullMovement(rom);
        VerifyPowerBombCallbackDefinitions(rom);
        VerifyShotCallbackDefinitions(rom);
        VerifyCompiledPuyoHops(rom);
        VerifyCompiledBotwoonSpeeds(rom);
        VerifyCompiledCrawlerSpeeds(rom);
        VerifyCompiledPolypLaunchDefinitions(rom);
        VerifyCompiledShaktoolAngularVelocities(rom);
        VerifyCompiledCeresRidleyGetaway(rom);
        VerifyCompiledBoyonSpeeds(rom);
        VerifyCompiledSurfaceMotion(rom);
        VerifyCompiledEnemyFireballLaunches(rom);
        VerifyCompiledRioLaunches(rom);
        VerifyCompiledBoulderBounces(rom);
        VerifyCompiledZoaSpeeds(rom);
        VerifyCompiledGrowingShutters(rom);
        VerifyCompiledIntroEggMotion(rom);

        T Method<T>(string name) where T : Delegate => typeof(RoomEnemySystem)
            .GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic)!.CreateDelegate<T>();
        var pixel = Method<Func<ushort, ushort, int>>("ReadEightBitSineProduct");
        var fixedProduct = Method<Func<ushort, ushort, (short Whole, ushort Fraction)>>("ReadEightBitSineFixedProduct");
        var cosine = Method<Func<ushort, ushort, int>>("ReadEightBitCosineProduct");
        var negative = Method<Func<ushort, ushort, int>>("ReadEightBitNegativeSineProduct");
        var fixedCosine = Method<Func<ushort, ushort, (short Whole, ushort Fraction)>>("ReadEightBitCosineFixedProduct");
        var fixedNegative = Method<Func<ushort, ushort, (short Whole, ushort Fraction)>>("ReadEightBitNegativeSineFixedProduct");
        var sbugSigned = Method<Func<byte, byte, int, SbugVelocityWords>>("CalculateSignedSbugComponent");
        var sbugUnsigned = Method<Func<byte, byte, int, SbugVelocityWords>>("CalculateUnsignedSbugMagnitude");
        var unsigned = Method<Func<ushort, ushort, ushort, int>>("ReadUnsignedSineMagnitudeProduct");

        int ExpectedPixel(int angle, int radius)
        {
            angle &= 255;
            int magnitude = bytes[angle & 127] * (radius & 255) >> 8;
            return angle < 128 ? magnitude : -magnitude;
        }
        for (int angle = 0; angle < 256; angle++)
        for (int radius = 0; radius < 256; radius++)
        {
            // Poison high bytes to prove native byte truncation remains in the
            // production helpers. The fractional negation deliberately has no carry.
            ushort a = (ushort)(0xff00 | angle), r = (ushort)(0xab00 | radius);
            int product = bytes[angle & 127] * radius;
            var expected = ((short)(angle < 128 ? product >> 8 : -(product >> 8)),
                unchecked((ushort)(angle < 128 ? product << 8 : -(product << 8))));
            if (pixel(a, r) != ExpectedPixel(angle, radius) || fixedProduct(a, r) != expected ||
                cosine(a, r) != ExpectedPixel(angle + 64, radius) || negative(a, r) != ExpectedPixel(angle + 128, radius))
                throw new InvalidDataException($"Compiled byte sine result differs at angle={angle}, radius={radius}.");
            foreach (int phase in new[] { 64, 128 })
            {
                int shiftedAngle = (angle + phase) & 255;
                int shiftedProduct = bytes[shiftedAngle & 127] * radius;
                short whole = (short)(shiftedAngle < 128 ? shiftedProduct >> 8 : -(shiftedProduct >> 8));
                ushort fraction = unchecked((ushort)(shiftedAngle < 128 ? shiftedProduct << 8 : -(shiftedProduct << 8)));
                var fixedActual = phase == 64 ? fixedCosine(a, r) : fixedNegative(a, r);
                var signedActual = sbugSigned((byte)angle, (byte)radius, phase);
                var unsignedActual = sbugUnsigned((byte)angle, (byte)radius, phase);
                uint magnitude = (uint)words[shiftedAngle & 127] * (uint)radius;
                if (fixedActual != (whole, fraction) || signedActual != new SbugVelocityWords(unchecked((ushort)whole), fraction) ||
                    unsignedActual.RawFixed != magnitude)
                    throw new InvalidDataException($"Compiled vector differs at angle={angle}, radius={radius}, phase={phase}.");
            }
        }
        for (int index = 0; index < 128; index++)
        for (int magnitude = 0; magnitude <= ushort.MaxValue; magnitude++)
        {
            int expected = unchecked((int)((uint)words[index] * magnitude));
            if (unsigned((ushort)(0xff80 | index), (ushort)magnitude, 128) != expected ||
                unsigned((ushort)(0xffc0 + index), (ushort)magnitude, 64) != expected)
                throw new InvalidDataException($"Compiled unsigned sine differs at index={index}, magnitude={magnitude}.");
        }
        Console.WriteLine("Compiled enemy sine: 256 exact samples, 65,536 byte inputs including both Sbug vector phases, and 8,388,608 unsigned products (two wrapped offsets) pass without any production bus dependency.");
    }

    private static void VerifyCompiledSignedTrigonometry(SuperMetroidAddressSpace rom)
    {
        short Reference(int index)
        {
            int address = EnemyMathReferenceData.SignedNegativeCosine + index * 2;
            return unchecked((short)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));
        }
        var cinematicReaders = new[] { typeof(CeresDestructionCinematicState), typeof(EndingCreditsState), typeof(IntroCeresFlightState) }
            .Select(type => type.GetMethod("ReadSine", BindingFlags.NonPublic | BindingFlags.Static)!
                .CreateDelegate<Func<byte, short>>()).ToArray();
        for (int index = 0; index < 320; index++)
        {
            short expected = Reference(index);
            AssertEqual(expected, EnemyTrigonometryTables.SignedNegativeCosineWord(index), "all 320 native prefix/full-wave words");
            if (index >= 64)
            {
                byte angle = (byte)(index - 64);
                AssertEqual(expected, EnemyTrigonometryTables.SignedSine(angle), "all signed sine quadrants match cartridge");
                foreach (var reader in cinematicReaders)
                    AssertEqual(expected, reader(angle), "cinematic matrix sample matches cartridge without bus");
            }
        }
        foreach (int invalid in new[] { -1, 320, int.MinValue, int.MaxValue })
        {
            bool rejected = false;
            try { _ = EnemyTrigonometryTables.SignedNegativeCosineWord(invalid); }
            catch (ArgumentOutOfRangeException) { rejected = true; }
            AssertTrue(rejected, "signed table does not silently wrap invalid prefix indexes");
        }
        Console.WriteLine("Compiled signed sine: all 320 native words and three cinematic readers match, including +/-256 peaks and prefix bounds.");
        var tide = new RoomLayer3FxState();
        var phaseField = typeof(RoomLayer3FxState).GetField("tidePhase", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var offsetField = typeof(RoomLayer3FxState).GetField("tideFixedOffset", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var options = typeof(RoomLayer3FxState).GetProperty(nameof(RoomLayer3FxState.LiquidOptions))!;
        var step = typeof(RoomLayer3FxState).GetMethod("StepLiquidTide", BindingFlags.Instance | BindingFlags.NonPublic)!
            .CreateDelegate<Action>(tide);
        foreach (ushort option in new ushort[] { 0, 0x40, 0x80, 0xc0 })
        {
            options.SetValue(tide, option);
            for (int phase = 0; phase <= ushort.MaxValue; phase++)
            {
                phaseField.SetValue(tide, (ushort)phase);
                offsetField.SetValue(tide, 12345);
                step();
                short sample = Reference(64 + (phase >> 8));
                bool small = (option & 0x80) != 0;
                int scale = option == 0 ? 0 : small ? 8 : 32;
                int delta = option == 0 ? 0 : small ? (sample >= 0 ? 288 : 192) : (sample >= 0 ? 224 : 128);
                if ((int)offsetField.GetValue(tide)! != (sample * scale << 8) ||
                    (ushort)phaseField.GetValue(tide)! != unchecked((ushort)(phase + delta)))
                    throw new InvalidDataException($"Native tide differs at options={option:X2}, phase={phase:X4}.");
            }
        }
        Console.WriteLine("Compiled tide: all 65,536 phases in four option combinations preserve exact offset, phase advance and small-tide precedence without a bus.");
    }

}

internal static class EnemyMathReferenceData
{
    /// <summary>Pinned $A0:B443 sine bytes, including the following $B643 PHB byte for an odd final read.</summary>
    public const int PhantoonSineByteRange = 0xa0b443;
    /// <summary>Pinned $A0:B1C3 signed 16-bit sine/cosine quadrants.</summary>
    public const int SignedSixteenBitSine = 0xa0b1c3;
    /// <summary>Pinned $AA:E03D Shaktool negative-cosine prefix and sine quadrants.</summary>
    public const int ShaktoolOrbit = 0xaae03d;
    /// <summary>Pinned $A0:B3C3 negative-cosine prefix followed by the full signed sine wave.</summary>
    public const int SignedNegativeCosine = 0xa0b3c3;
    /// <summary>Pinned $A0:B143 positive byte sine/cosine sample range.</summary>
    public const int ByteSine = 0xa0b143;
    /// <summary>Pinned $A0:B7EE UnsignedSineTable.</summary>
    public const int UnsignedSine = 0xa0b7ee;
}
