using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifySignedSixteenBitSineDefinitions(SuperMetroidAddressSpace rom)
    {
        var bullSample = typeof(RoomEnemySystem)
            .GetMethod("ReadBullSignedSine", BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Func<byte, short>>();
        for (int angle = 0; angle < 256; angle++)
        {
            int address = EnemyMathReferenceData.SignedSixteenBitSine + angle * 2;
            short original = unchecked((short)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));
            AssertEqual(original, EnemyTrigonometryTables.SignedSixteenBitSine((byte)angle), "signed 16-bit sample");
            AssertEqual(original, bullSample((byte)angle), "Bull production sample");
        }
    }

    private static void VerifyCompiledFamilyTrigonometry(SuperMetroidAddressSpace rom)
    {
        short Read(int address) => unchecked((short)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));
        short[] native = Enumerable.Range(0, 256)
            .Select(i => Read(EnemyMathReferenceData.SignedSixteenBitSine + i * 2)).ToArray();
        short[] orbit = Enumerable.Range(0, 320)
            .Select(i => Read(EnemyMathReferenceData.ShaktoolOrbit + i * 2)).ToArray();
        T Method<T>(string name) where T : Delegate => typeof(RoomEnemySystem)
            .GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic)!.CreateDelegate<T>();
        Suite(nameof(VerifySignedSixteenBitSineDefinitions), () => VerifySignedSixteenBitSineDefinitions(rom));
        var bullMove = Method<Action<RoomEnemySlot, BullEnemyState>>("MoveBull");
        var mawX = Method<Func<ushort, ushort, ushort>>("CalculateYappingMawX");
        var mawY = Method<Func<ushort, ushort, ushort>>("CalculateYappingMawY");
        Suite(nameof(VerifyShaktoolOrbitAlgorithm), () => VerifyShaktoolOrbitAlgorithm(rom));

        ushort MawReference(int angle, int length)
        {
            short sample = native[(-angle) & 255];
            int product = (Math.Abs((int)sample) / 256) * (length & 255);
            if (product == 0) return 0;
            if (sample >= 0) return (ushort)((product / 256) * 2);
            return unchecked((ushort)(((ushort)-product / 256 * 2) | 65280));
        }
        // Each boundary-class word in either parameter, with every low byte in the other.
        // This verifies angle negation/subtraction and length truncation independently.
        foreach (int word in WordBoundarySamples())
        for (int low = 0; low <= byte.MaxValue; low++)
        {
            ushort angle = (ushort)word, length = (ushort)(65280 | low);
            if (mawY(angle, length) != MawReference(angle, length) ||
                mawX(angle, length) != MawReference(angle - 64, length) ||
                mawY((ushort)low, (ushort)word) != MawReference(low, word) ||
                mawX((ushort)low, (ushort)word) != MawReference(low - 64, word))
                throw new InvalidDataException($"Yapping Maw product differs: word={word:X4}, low={low:X2}.");
        }

        var bull = new RoomEnemySlot(0);
        var bullState = new BullEnemyState(bull, new ushort[1], new ushort[1], new ushort[1],
            new ushort[1], new ushort[1], new ushort[1], new ushort[1], new ushort[1]);
        static uint BullReference(uint initial, short sample, int speed)
        {
            int product = (Math.Abs((int)sample) / 256) * speed;
            int displacement = product;
            if (sample < 0 && product != 0)
                displacement = -product - ((product & 65535) == 0 ? 1 : 0);
            return unchecked(initial + (uint)displacement);
        }
        foreach (uint initial in new uint[] { 0, 0xffffff37 })
        for (int angle = 0; angle < 256; angle++)
        foreach (int speed in WordBoundarySamples())
        {
            bull.XPosition = bull.YPosition = (ushort)(initial >> 16);
            bull.XSubposition = bull.YSubposition = unchecked((ushort)initial);
            bullState.Angle = (ushort)(65280 | angle);
            bullState.Speed = (ushort)speed;
            bullMove(bull, bullState);
            uint x = ((uint)bull.XPosition << 16) | bull.XSubposition;
            uint y = ((uint)bull.YPosition << 16) | bull.YSubposition;
            if (x != BullReference(initial, native[(angle + 64) & 255], speed) ||
                y != BullReference(initial, native[angle], speed))
                throw new InvalidDataException($"Bull movement differs: angle={angle}, speed={speed}, origin={initial:X8}.");
        }

        // A real linked-slot placement with no loaded bus verifies the production
        // caller, low angular byte, preceding segment subpixels and signed carry/wrap.
        var enemies = new RoomEnemySystem();
        var previous = enemies.Slots[0];
        var segment = enemies.Slots[1];
        previous.EnemyDefinitionPointer = EnemyDefinitionId.Shaktool;
        var segmentState = new ShaktoolSegmentState(segment);
        var place = typeof(RoomEnemySystem).GetMethod("PositionShaktoolAroundPreviousSegment",
            BindingFlags.Instance | BindingFlags.NonPublic)!
            .CreateDelegate<Action<RoomEnemySlot, ShaktoolSegmentState>>(enemies);
        foreach (uint initial in new uint[] { 0, 0x0080ff37, 0xffffff37 })
        for (int angle = 0; angle <= ushort.MaxValue; angle++)
        {
            previous.XPosition = previous.YPosition = (ushort)(initial >> 16);
            previous.XSubposition = previous.YSubposition = unchecked((ushort)initial);
            segmentState.OrbitAngle = (ushort)angle;
            place(segment, segmentState);
            uint expectedX = unchecked(initial + (uint)(orbit[(angle >> 8) + 64] << 8));
            uint expectedY = unchecked(initial + (uint)(orbit[angle >> 8] << 8));
            if ((((uint)segment.XPosition << 16) | segment.XSubposition) != expectedX ||
                (((uint)segment.YPosition << 16) | segment.YSubposition) != expectedY ||
                segmentState.OrbitAngle != angle)
                throw new InvalidDataException($"Shaktool placement differs: angle={angle:X4}, origin={initial:X8}.");
        }
        Console.WriteLine("Compiled family math: signed 16-bit samples, every angle with every speed-word boundary class for Bull moves, word/byte Yapping Maw products over every boundary class and 196,608 linked Shaktool placements match cartridge data without a production bus.");
    }
}
