using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCompiledProjectileMath(SuperMetroidAddressSpace rom)
    {
        for (ushort parameter = 0; parameter < 20; parameter++)
        {
            int address = 0x869059 + (parameter >> 1) * 2;
            short expected = unchecked((short)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));
            AssertEqual(expected, CrocomireProjectileRomData.GradientForSpawnParameter(parameter),
                "Crocomire authored gradient and final instruction-byte overread");
        }
        AssertEqual(unchecked((short)0xb620), CrocomireProjectileRomData.GradientForSpawnParameter(18),
            "Crocomire ninth volley shot must preserve native overread");
        AssertThrows<InvalidDataException>(() => CrocomireProjectileRomData.GradientForSpawnParameter(20),
            "Crocomire unsupported gradient selector remains explicit");
        var native = new short[320];
        for (int i = 0; i < native.Length; i++)
        {
            int address = EnemyMathReferenceData.SignedNegativeCosine + i * 2;
            native[i] = unchecked((short)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));
        }
        T Reader<T>(Type type, string name) where T : Delegate => type
            .GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!.CreateDelegate<T>();
        var enemy = Reader<Func<ushort, byte, ushort>>(typeof(RoomEnemySystem), "MultiplyCartridgeSinCos");
        var baby = Reader<Func<ushort, byte, ushort>>(typeof(BabyMetroidCutsceneState), "CalculateVelocityComponent");
        var mother = Reader<Func<ushort, byte, ushort>>(typeof(MotherBrainEnemyProjectileSystem), "CalculateVelocityComponent");
        var rainbow = Reader<Func<ushort, SnesAngle, ushort>>(typeof(MotherBrainRainbowBeamSamusMovement), "CalculateYVelocity");
        var fly = Reader<Func<int, short>>(typeof(RoomEnemySystem), "ReadSignedSineCosine");
        var glass = Reader<Func<int, ushort>>(typeof(RoomEnemySystem), "ReadSignedSineSample");
        var eye = Reader<Func<int, short>>(typeof(RoomEnemySystem), "ReadEyeDoorAcceleration");
        var rio = Reader<Func<byte, ushort>>(typeof(RoomEnemySystem), "ReadRioSignedSineCosineSample");
        var shaktool = Reader<Func<int, ushort>>(typeof(RoomEnemySystem), "ReadShaktoolCommonSineSample");
        var phantoon = Reader<Func<ushort, ushort, int>>(typeof(RoomEnemySystem), "ReadPhantoonFlameComponent");
        var arc = Reader<Action<RoomEnemyProjectileSlot, ushort>>(typeof(RoomEnemySystem), "MoveNoobTubeProjectileHorizontallyAlongArc");
        var crocomire = Reader<Func<byte, (ushort X, ushort Y)>>(
            typeof(RoomEnemySystem), "CalculateCrocomireProjectileVelocity");
        var echo = Reader<Func<SnesAngle, byte, (ushort X, ushort Y)>>(
            typeof(SamusShinesparkState), "ProjectileSinLookup");

        // Exercise every fractional angle word and every radius byte. The reference
        // uses the cartridge's positive half, truncates, then restores each sign.
        for (int rawAngle = 0; rawAngle <= ushort.MaxValue; rawAngle++)
        for (int radius = 0; radius <= byte.MaxValue; radius++)
        {
            int xAngle = rawAngle >> 8;
            int yAngle = ((rawAngle - 16384) & 65535) >> 8;
            ushort xMagnitude = (ushort)(native[(xAngle & 127) + 64] * radius / 256);
            ushort yMagnitude = (ushort)(native[(yAngle & 127) + 64] * radius / 256);
            ushort x = xAngle < 128 ? xMagnitude : unchecked((ushort)-xMagnitude);
            ushort y = yAngle < 128 ? yMagnitude : unchecked((ushort)-yMagnitude);
            if (echo(SnesAngle.FromRaw((ushort)rawAngle), (byte)radius) != (x, y))
                throw new InvalidDataException($"Shinespark vector differs: angle={rawAngle:X4}, radius={radius}.");
        }

        // An independent reference retains the unsigned multiplication and sign
        // restoration used by $86:C27A. Every speed word is legal to the helper.
        for (int angle = 0; angle < 256; angle++)
        for (int speed = 0; speed <= ushort.MaxValue; speed++)
        {
            short sample = native[angle + 64];
            ushort magnitude = (ushort)((uint)Math.Abs(sample) * speed / 256);
            ushort expected = sample < 0 ? unchecked((ushort)-magnitude) : magnitude;
            // Subtract a quarter-turn so the rainbow wrapper's explicit cosine
            // offset should select the same sample. Poison the fractional byte.
            var cosineAngle = SnesAngle.FromRaw(unchecked((ushort)(((angle - 64) << 8) | 255)));
            if (EnemyTrigonometryTables.MultiplySignedSine((ushort)speed, (byte)angle) != expected ||
                enemy((ushort)speed, (byte)angle) != expected || baby((ushort)speed, (byte)angle) != expected ||
                mother((ushort)speed, (byte)angle) != expected || rainbow((ushort)speed, cosineAngle) != expected)
                throw new InvalidDataException($"Signed projectile product differs: angle={angle}, speed={speed}.");
        }
        for (int index = 0; index < 320; index++)
        {
            AssertEqual(native[index], fly(index), "fly signed prefix sample");
            AssertEqual(unchecked((ushort)native[index]), glass(index), "glass signed prefix word bits");
        }
        for (int index = short.MinValue; index <= ushort.MaxValue; index++)
        {
            AssertEqual((short)(native[(index & 255) + 64] >> 4), eye(index), "eye arithmetic shift and angle wrapping");
            AssertEqual(unchecked((ushort)native[((index - 64) & 255) + 64]), shaktool(index), "Shaktool full-index wrapping");
        }
        for (int angle = 0; angle < 256; angle++)
        {
            AssertEqual((unchecked((ushort)(native[angle + 64] << 2)),
                    unchecked((ushort)(native[angle] << 2))), crocomire((byte)angle),
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
            projectile.XVelocity = (ushort)phase;
            projectile.Variable1 = (ushort)(initial >> 16);
            projectile.Variable0 = unchecked((ushort)initial);
            arc(projectile, step);
            if (projectile.Variable1 != expected >> 16 || projectile.Variable0 != (expected & 65535) ||
                projectile.XVelocity != unchecked((ushort)(phase + step)))
                throw new InvalidDataException($"N00b tube arc differs: phase={phase}, step={step}, initial={initial:X8}.");
        }
        Console.WriteLine("Compiled projectile math: 16,777,216 products across four production readers, all sample/index variants, 65,536 Phantoon vectors, 256 Crocomire vectors, 16,777,216 Shinespark vectors and 393,216 N00b-tube arc steps match without a production bus.");
    }
}
