using SuperMetroid.Core.Hardware;
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
    /// <summary>Checks Crocomire gradient overread, signed sine products, and native vector and arc arithmetic.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for compiled projectile math")]
    internal static void VerifyCompiledProjectileMath(SuperMetroidAddressSpace rom)
    {
        for (ushort parameter = 0; parameter < 20; parameter++)
        {
            int address = 0x869059 + (parameter >> 1) * 2;
            short expected = unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(address));
            AssertEqual(expected, CrocomireVolley.GradientForSpawnParameter(parameter),
                "Crocomire authored gradient and final instruction-byte overread");
        }
        AssertEqual(unchecked((short)0xb620), CrocomireVolley.GradientForSpawnParameter(18),
            "Crocomire ninth volley shot must preserve native overread");
        AssertThrows<InvalidDataException>(() => CrocomireVolley.GradientForSpawnParameter(20),
            "Crocomire unsupported gradient selector remains explicit");
        var native = new short[320];
        for (int i = 0; i < native.Length; i++)
        {
            int address = EnemyMathReferenceData.SignedNegativeCosine + i * 2;
            native[i] = unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(address));
        }
        var enemy = new Func<ushort, byte, ushort>((speed, angle) => EnemyMath.MultiplySineSample(speed, new SnesByteAngle(angle)));
        var baby = new Func<ushort, byte, ushort>(BabyMetroidCutsceneState.CalculateVelocityComponent);
        var rainbow = new Func<ushort, SnesAngle, ushort>(MotherBrainRainbowBeamSamusMovement.CalculateYVelocity);
        var fly = new Func<int, short>(FliesBehavior.ReadSignedSineCosine);
        var glass = new Func<int, ushort>(MotherBrainProjectiles.ReadSignedSineSample);
        var eye = new Func<int, short>(EyeDoorBehavior.ReadEyeDoorAcceleration);
        var rio = new Func<byte, ushort>(angle => RioBehavior.ReadRioSignedSineCosineSample(new SnesByteAngle(angle)));
        var shaktool = new Func<int, ushort>(ShaktoolBehavior.ReadShaktoolCommonSineSample);
        var phantoon = new Func<ushort, ushort, int>((angle, radius) => PhantoonCombat.ReadPhantoonFlameComponent(new SnesByteAngle(unchecked((byte)angle)), radius));
        var arc = new Action<RoomEnemyProjectileSlot, int>(NoobTubeBehavior.MoveNoobTubeProjectileHorizontallyAlongArc);
        var crocomire = new Func<byte, EightEightVector<RoomPixels>>(bearing => CrocomireCombat.CalculateCrocomireProjectileVelocity(new SnesBearing(bearing)));
        var echo = new Func<SnesAngle, byte, Offset16<RoomPixels>>(SamusShinesparkState.ProjectileSinLookup);

        // Every angle high byte (with zero, mid and saturated fractions, which the reader
        // ignores) and every radius byte. The reference uses the cartridge's positive half,
        // truncates, then restores each sign.
        foreach (int rawAngle in AngleHighByteSamples())
        for (int radius = 0; radius <= byte.MaxValue; radius++)
        {
            int xAngle = SnesAngle.FromRaw(unchecked((ushort)rawAngle)).TableIndex;
            int yAngle = SnesAngle.FromRaw(unchecked((ushort)(rawAngle - 16384))).TableIndex;
            ushort xMagnitude = (ushort)(native[(xAngle & 127) + 64] * radius / 256);
            ushort yMagnitude = (ushort)(native[(yAngle & 127) + 64] * radius / 256);
            ushort x = xAngle < 128 ? xMagnitude : unchecked((ushort)-xMagnitude);
            ushort y = yAngle < 128 ? yMagnitude : unchecked((ushort)-yMagnitude);
            if (echo(SnesAngle.FromRaw((ushort)rawAngle), (byte)radius) != new Offset16<RoomPixels>(unchecked((short)x), unchecked((short)y)))
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
            var cosineAngle = SnesAngle.FromTableIndex(unchecked((byte)(angle - 64))).AddRaw(255);
            if (EnemyTrigonometryTables.MultiplySignedSine((ushort)speed, new SnesByteAngle((byte)angle)) != expected ||
                enemy((ushort)speed, (byte)angle) != expected || baby((ushort)speed, (byte)angle) != expected ||
                rainbow((ushort)speed, cosineAngle) != expected)
                throw new InvalidDataException($"Signed projectile product differs: angle={angle}, speed={speed}.");
        }
        for (int index = 0; index < 320; index++)
        {
            AssertEqual(native[index], fly(index), "fly signed prefix sample");
            AssertEqual(unchecked((ushort)native[index]), glass(index), "glass signed prefix word bits");
        }
        for (int index = short.MinValue; index <= ushort.MaxValue; index++)
        {
            AssertEqual((short)(native[unchecked((byte)index) + 64] >> 4), eye(index), "eye arithmetic shift and angle wrapping");
            AssertEqual(unchecked((ushort)native[unchecked((byte)(index - 64)) + 64]), shaktool(index), "Shaktool full-index wrapping");
        }
        for (int angle = 0; angle < 256; angle++)
        {
            AssertEqual(new EightEightVector<RoomPixels>(unchecked((short)(native[angle + 64] << 2)),
                    unchecked((short)(native[angle] << 2))), crocomire((byte)angle),
                "Crocomire projectile signed X/Y shifts and negative-cosine origin");
            AssertEqual(unchecked((ushort)native[angle + 64]), rio((byte)angle), "Rio signed word bits");
            for (int radius = 0; radius < 256; radius++)
            {
                int magnitude = native[(angle & 127) + 64] * radius / 256;
                int expected = angle < 128 ? magnitude : -magnitude;
                AssertEqual(expected, phantoon((ushort)(0xff00 | angle), (ushort)(0xaa00 | radius)),
                    "Phantoon word peak, radius truncation and restored sign");
            }
        }

        // Native $86:D843/$D89F OR bit seven into an even byte offset; this is not
        // equivalent to adding a quarter-turn. Check every stored phase word,
        // both step sizes, carry/borrow, low subpixel preservation and word wrap.
        var projectile = new RoomEnemyProjectileSlot(0);
        foreach (ushort step in new ushort[] { 2, 4 })
        foreach (uint initial in new uint[] { 0, 0x0080ff37, 0xfffffff1 })
        for (int phase = 0; phase <= ushort.MaxValue; phase++)
        {
            int sampleIndex = (((phase & 510) | 128) / 2) + 64;
            short velocity = (short)(native[sampleIndex] >> 2);
            uint expected = unchecked(initial + (uint)(velocity * 256));
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight((ushort)phase));
            projectile.Variable1 = SnesFixedPosition.WholeOf(unchecked((int)initial));
            projectile.Variable0 = unchecked((ushort)initial);
            arc(projectile, step);
            if (projectile.Variable1 != SnesSixteenSixteen.WholeOf(expected) || projectile.Variable0 != SnesSixteenSixteen.FractionOf(expected) ||
                projectile.Velocity.X.RawValue != unchecked((ushort)(phase + step)))
                throw new InvalidDataException($"N00b tube arc differs: phase={phase}, step={step}, initial={initial:X8}.");
        }
        Console.WriteLine("Compiled projectile math: every signed sample times every speed-word boundary class across five production readers, all sample/index variants, 65,536 Phantoon vectors, 256 Crocomire vectors, every Shinespark angle byte and radius, and 393,216 N00b-tube arc steps match without a production bus.");
    }
}
