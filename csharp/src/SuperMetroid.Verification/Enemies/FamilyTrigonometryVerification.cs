using static SuperMetroid.Verification.Enemies.Shaktool.ShaktoolOrbitAlgorithmVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Hardware.WordSamplesVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Bull;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.Core.Enemies.YappingMaws;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies;

/// <summary>Checks signed cartridge trigonometry across Bull, Yapping Maw, and Shaktool consumers.</summary>
internal static class FamilyTrigonometryVerification
{
    /// <summary>Compares all 256 signed sine samples and Bull's production lookup with retail words.</summary>
    internal static void VerifySignedSixteenBitSineDefinitions(SuperMetroidAddressSpace rom)
    {
        Func<byte, short> bullSample = angle => BullBehavior.ReadBullSignedSine(new ByteAngle(angle));
        for (int angle = 0; angle < 256; angle++)
        {
            int address = EnemyMathReferenceData.SignedSixteenBitSine + angle * 2;
            short original = unchecked((short)(LittleEndianBytes.Word(SignedSixteenBitSineDefinitionsRomByte(rom, address), SignedSixteenBitSineDefinitionsRomByte(rom, address + 1))));
            AssertEqual(original, Trigonometry.SignedSixteenBitSine(new ByteAngle((byte)angle)), "signed 16-bit sample");
            AssertEqual(original, bullSample((byte)angle), "Bull production sample");
        }
    }

    /// <summary>Reads a native sine-table byte for the signed-sample oracle.</summary>
    private static byte SignedSixteenBitSineDefinitionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks boundary-word Maw products, Bull movement, and linked Shaktool segment placement without runtime ROM reads.</summary>
    internal static void VerifyCompiledFamilyTrigonometry(SuperMetroidAddressSpace rom)
    {
        short Read(int address) => unchecked((short)(LittleEndianBytes.Word(CompiledFamilyTrigonometryRomByte(rom, address), CompiledFamilyTrigonometryRomByte(rom, address + 1))));
        short[] native = Enumerable.Range(0, 256)
            .Select(i => Read(EnemyMathReferenceData.SignedSixteenBitSine + i * 2)).ToArray();
        short[] orbit = Enumerable.Range(0, 320)
            .Select(i => Read(EnemyMathReferenceData.ShaktoolOrbit + i * 2)).ToArray();
        Suite(nameof(VerifySignedSixteenBitSineDefinitions), () => VerifySignedSixteenBitSineDefinitions(rom));
        var bullMove = new Action<RoomEnemySlot, BullEnemyState>(BullBehavior.MoveBull);
        var mawX = new Func<ushort, ushort, ushort>((angle, length) => YappingMawBehavior.CalculateYappingMawX(new ByteAngle(unchecked((byte)angle)), length));
        var mawY = new Func<ushort, ushort, ushort>((angle, length) => YappingMawBehavior.CalculateYappingMawY(new ByteAngle(unchecked((byte)angle)), length));
        Suite(nameof(VerifyShaktoolOrbitAlgorithm), () => VerifyShaktoolOrbitAlgorithm(rom));

        ushort MawReference(int angle, int length)
        {
            short sample = native[unchecked((byte)-angle)];
            int product = (Math.Abs((int)sample) / 256) * unchecked((byte)length);
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

        var bull = new RoomEnemySlot(0, EnemySlotFixtures.Registry);
        var bullState = new BullEnemyState(new BullExtensionWords());
        static uint BullReference(uint initial, short sample, int speed)
        {
            int product = (Math.Abs((int)sample) / 256) * speed;
            int displacement = product;
            if (sample < 0 && product != 0)
                displacement = -product - (SixteenSixteen.FractionOf(product) == 0 ? 1 : 0);
            return unchecked(initial + (uint)displacement);
        }
        foreach (uint initial in new uint[] { 0, 0xffffff37 })
        for (int angle = 0; angle < 256; angle++)
        foreach (int speed in WordBoundarySamples())
        {
            bull.Position = new(new(FixedCoordinate.WholeOf(unchecked((int)initial)), unchecked((ushort)initial)), new(FixedCoordinate.WholeOf(unchecked((int)initial)), unchecked((ushort)initial)));
            bullState.Angle = (ushort)(65280 | angle);
            bullState.Speed = (ushort)speed;
            bullMove(bull, bullState);
            uint x = bull.Position.X.RawValue;
            uint y = bull.Position.Y.RawValue;
            if (x != BullReference(initial, native[unchecked((byte)(angle + 64))], speed) ||
                y != BullReference(initial, native[angle], speed))
                throw new InvalidDataException($"Bull movement differs: angle={angle}, speed={speed}, origin={initial:X8}.");
        }

        // A real linked-slot placement with no loaded bus verifies the production
        // caller, low angular byte, preceding segment subpixels and signed carry/wrap.
        var enemies = new RoomEnemySystem(new EnemySession());
        var previous = enemies.SlotTable.Slots[0];
        var segment = enemies.SlotTable.Slots[1];
        previous.Bind(EnemyDefinitionId.Shaktool, previous.Definition);
        var segmentState = new ShaktoolSegmentState();
        Action<RoomEnemySlot, ShaktoolSegmentState> place = enemies.ShaktoolBehavior.PositionShaktoolAroundPreviousSegment;
        foreach (uint initial in new uint[] { 0, 0x0080ff37, 0xffffff37 })
        for (int angle = 0; angle <= ushort.MaxValue; angle++)
        {
            previous.Position = new(new(FixedCoordinate.WholeOf(unchecked((int)initial)), unchecked((ushort)initial)), new(FixedCoordinate.WholeOf(unchecked((int)initial)), unchecked((ushort)initial)));
            segmentState.OrbitAngle = RotationAngle.FromRaw((ushort)angle);
            place(segment, segmentState);
            uint expectedX = unchecked(initial + (uint)SixteenSixteen.FromEightEight(orbit[RotationAngle.FromRaw(unchecked((ushort)angle)).TableIndex + 64]));
            uint expectedY = unchecked(initial + (uint)SixteenSixteen.FromEightEight(orbit[RotationAngle.FromRaw(unchecked((ushort)angle)).TableIndex]));
            if ((segment.Position.X.RawValue) != expectedX ||
                (segment.Position.Y.RawValue) != expectedY ||
                segmentState.OrbitAngle.RawValue != angle)
                throw new InvalidDataException($"Shaktool placement differs: angle={angle:X4}, origin={initial:X8}.");
        }
        Console.WriteLine("Compiled family math: signed 16-bit samples, every angle with every speed-word boundary class for Bull moves, word/byte Yapping Maw products over every boundary class and 196,608 linked Shaktool placements match cartridge data without a production bus.");
    }

    /// <summary>Reads a retail trigonometry byte for the independent family-math reference arrays.</summary>
    private static byte CompiledFamilyTrigonometryRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
