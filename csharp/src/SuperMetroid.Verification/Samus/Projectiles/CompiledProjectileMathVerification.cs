using static SuperMetroid.Verification.Hardware.WordSamplesVerification;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Flies;
using SuperMetroid.Core.Enemies.Rios;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Core.Enemies.EyeDoor;
using SuperMetroid.Core.Enemies.NoobTube;
using SuperMetroid.Core.Spatial;

/// <summary>Checks compiled projectile trigonometry across production consumers against cartridge samples.</summary>
internal static class CompiledProjectileMathVerification
{
    /// <summary>Checks signed sine products, and native vector and arc arithmetic.</summary>
    internal static void VerifyCompiledProjectileMath(CartridgeImportAddressSpace rom)
    {
        var native = new short[320];
        for (int i = 0; i < native.Length; i++)
        {
            int address = EnemyMathReferenceData.SignedNegativeCosine + i * 2;
            native[i] = unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(address));
        }
        var enemy = new Func<ushort, byte, int>((speed, angle) => EnemyMath.SineSpeed(new SignedEightEight(speed), new TrigonometryAngle(angle)).RawValue);
        var baby = new Func<ushort, byte, int>((babySpeed, babyAngle) => BabyMetroidCutsceneState.CalculateVelocityComponent(new SignedEightEight(babySpeed), new TrigonometryAngle(babyAngle)).RawValue);
        var rainbow = new Func<ushort, RotationAngle, int>((rainbowSpeed, rainbowAngle) => MotherBrainRainbowBeamSamusMovement.CalculateYVelocity(new SignedEightEight(rainbowSpeed), rainbowAngle).RawValue);
        var flyCircle = new Func<TrigonometryAngle, EightEightVector<RoomPixels>>(FliesBehavior.CircleStep);
        var flyDive = new Func<TrigonometryAngle, EightEightVector<RoomPixels>>(FliesBehavior.DiveVelocity);
        var eye = new Func<byte, int>(angle => EyeDoorBehavior.ReadEyeDoorAcceleration(new TrigonometryAngle(angle)).InTwoHundredFiftySixthsOfPixel);
        var rio = new Func<byte, int>(angle => RioBehavior.ReadRioSignedSineCosineSample(new TrigonometryAngle(angle)).RawValue);
        // The native view's index runs a quarter turn ahead of the angle it holds: its first 64 words are the negative cosine.
        var shaktool = new Func<int, int>(index => ShaktoolBehavior.ReadShaktoolCommonSineSample(TrigonometryAngle.Wrapped(index - 64)).RawValue);
        var phantoon = new Func<ushort, ushort, int>((angle, radius) => PhantoonCombat.ReadPhantoonFlameComponent(new TrigonometryAngle(unchecked((byte)angle)), radius));
        var arc = new Action<EnemyProjectile, int>(NoobTubeBehavior.MoveNoobTubeProjectileHorizontallyAlongArc);
        var crocomire = new Func<byte, EightEightVector<RoomPixels>>(bearing => CrocomireCombat.CalculateCrocomireProjectileVelocity(new Bearing(bearing)));
        var echo = new Func<RotationAngle, int, Offset16<RoomPixels>>(SamusShinesparkState.ProjectileSinLookup);

        // Every angle high byte (with zero, mid and saturated fractions, which the reader
        // ignores) and every radius byte. The reference uses the cartridge's positive half,
        // truncates, then restores each sign.
        foreach (int rawAngle in AngleHighByteSamples())
        for (int radius = 0; radius <= byte.MaxValue; radius++)
        {
            int xAngle = RotationAngle.FromRaw(unchecked((ushort)rawAngle)).ToTrigonometryAngle().RawValue;
            int yAngle = RotationAngle.FromRaw(unchecked((ushort)(rawAngle - 16384))).ToTrigonometryAngle().RawValue;
            ushort xMagnitude = (ushort)(native[(xAngle & 127) + 64] * radius / 256);
            ushort yMagnitude = (ushort)(native[(yAngle & 127) + 64] * radius / 256);
            ushort x = xAngle < 128 ? xMagnitude : unchecked((ushort)-xMagnitude);
            ushort y = yAngle < 128 ? yMagnitude : unchecked((ushort)-yMagnitude);
            if (echo(RotationAngle.FromRaw((ushort)rawAngle), (byte)radius) != new Offset16<RoomPixels>(unchecked((short)x), unchecked((short)y)))
                throw new InvalidDataException($"Shinespark vector differs: angle={rawAngle:X4}, radius={radius}.");
        }

        // An independent reference retains the unsigned multiplication and sign
        // restoration used by $86:C27A. Every speed word is legal to the helper.
        for (int angle = 0; angle < 256; angle++)
        foreach (int speed in WordBoundarySamples())
        {
            short sample = native[angle + 64];
            ushort magnitude = (ushort)((uint)Math.Abs(sample) * speed / 256);
            ushort expected = sample < 0 ? unchecked((ushort)-magnitude) : magnitude;
            // Subtract a quarter-turn so the rainbow wrapper's explicit cosine
            // offset should select the same sample. Poison the fractional byte.
            var cosineAngle = RotationAngle.FromSteps(unchecked((byte)(angle - 64))).AddRaw(255);
            if (Trigonometry.MultiplySignedSine(new SignedEightEight(speed), new TrigonometryAngle((byte)angle)).RawValue != expected ||
                enemy((ushort)speed, (byte)angle) != expected || baby((ushort)speed, (byte)angle) != expected ||
                rainbow((ushort)speed, cosineAngle) != expected)
                throw new InvalidDataException($"Signed projectile product differs: angle={angle}, speed={speed}.");
        }
        // The fly's circle step and dive read the native view at the angle a quarter turn ahead across and at the angle
        // itself down (the negative cosine), the dive doubling and quadrupling the samples; the eye shot takes a
        // sixteenth of the sample, shifted toward negative infinity.
        for (int angle = 0; angle < 256; angle++)
        {
            var tableAngle = new TrigonometryAngle(angle);
            AssertEqual(EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(native[angle + 64], native[angle]), flyCircle(tableAngle),
                "fly circle step: sine across, negative cosine down");
            AssertEqual(EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(native[angle + 64] * 2, native[angle] * 4), flyDive(tableAngle),
                "fly dive: doubled sine across, quadrupled negative cosine down");
            AssertEqual((short)(native[angle + 64] >> 4), eye((byte)angle), "eye arithmetic shift");
        }
        for (int index = short.MinValue; index <= ushort.MaxValue; index++)
        {
            AssertEqual(unchecked((ushort)native[unchecked((byte)(index - 64)) + 64]), shaktool(index), "Shaktool full-index wrapping");
        }
        for (int angle = 0; angle < 256; angle++)
        {
            AssertEqual(EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(unchecked((short)(native[angle + 64] << 2)),
                    unchecked((short)(native[angle] << 2))), crocomire((byte)angle),
                "Crocomire projectile signed X/Y shifts and negative-cosine origin");
            AssertEqual(unchecked((ushort)native[angle + 64]), rio((byte)angle), "Rio signed word bits");
            for (int radius = 0; radius < 256; radius++)
            {
                int magnitude = native[(angle & 127) + 64] * radius / 256;
                int expected = angle < 128 ? magnitude : -magnitude;
                AssertEqual(expected, phantoon((ushort)angle, (ushort)radius),
                    "Phantoon word peak and restored sign");
                // A flame's radius grows past 255 as a plain count; the product keeps its place within 256 (#1627).
                AssertEqual(expected, phantoon((ushort)angle, (ushort)(radius + ShortCircleProduct.RadiusSpan)),
                    "a Phantoon flame's radius of 256 or more places it as its remainder within 256 does");
            }
        }

        // Native $86:D843/$D89F OR bit seven into an even byte offset; this is not
        // equivalent to adding a quarter-turn. Check every sway phase, both step
        // sizes, carry/borrow, low subpixel preservation and the swing's wrap.
        var projectile = new EnemyProjectile(0, default);
        foreach (ushort step in new ushort[] { 2, 4 })
        foreach (uint initial in new uint[] { 0, 0x0080ff37, 0xfffffff1 })
        for (int phase = 0; phase < 512; phase++)
        {
            int sampleIndex = (((phase & 510) | 128) / 2) + 64;
            short velocity = (short)(native[sampleIndex] >> 2);
            uint expected = unchecked(initial + (uint)(velocity * 256));
            var debris = new NoobTubeDebrisState(column: 0) { X = SixteenSixteenWords.ToCoordinate(initial), SwayPhase = phase };
            projectile.State = debris;
            arc(projectile, step);
            if (debris.X.Whole != SixteenSixteenWords.WholeOf(expected) || debris.X.Fraction != SixteenSixteenWords.FractionOf(expected) ||
                debris.SwayPhase != (phase + step) % 512)
                throw new InvalidDataException($"N00b tube arc differs: phase={phase}, step={step}, initial={initial:X8}.");
        }
        Console.WriteLine("Compiled projectile math: every signed sample times every speed-word boundary class across five production readers, all sample/index variants, 65,536 Phantoon vectors, 256 Crocomire vectors, every Shinespark angle byte and radius, and 3,072 N00b-tube sway steps match without a production bus.");
    }
}
