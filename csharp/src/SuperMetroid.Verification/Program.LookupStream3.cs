using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyLookupStream3(ISnesAddressSpace rom)
    {
        // Confirm the replaced selector for its complete ushort input domain.
        for (int angle = 0; angle <= ushort.MaxValue; angle++)
        {
            int address = 0x9bc346 + 2 * (angle >> 10);
            int expected = 0x9a0000 | rom.ReadByte(address) | rom.ReadByte(address + 1) << 8;
            var asset = GrappleTileDefinitions.SegmentAssetFor((ushort)angle);
            AssertEqual(expected, GrappleTileDefinitions.TransferFor(asset).SourceAddress,
                "stream 3 native grapple angle sector");
        }

        GrappleTileTransfer[] expectedTransfers =
        [
            new(VramAssetId.GrapplePointFirstTiles, 0x9a8200, 0, 32),
            new(VramAssetId.GrapplePointSecondTiles, 0x9a8400, 32, 32),
            new(VramAssetId.GrapplePointThirdTiles, 0x9a8600, 64, 32),
            new(VramAssetId.GrapplePointFourthTiles, 0x9a8800, 96, 32),
            new(VramAssetId.GrappleHorizontalSegmentTiles, 0x9a8220, 128, 128),
            new(VramAssetId.GrappleDiagonalSegmentTiles, 0x9a8a20, 256, 128),
            new(VramAssetId.GrappleVerticalSegmentTiles, 0x9a9220, 384, 128),
        ];
        var actualTransfers = GrappleTileDefinitions.Transfers;
        AssertTrue(expectedTransfers.SequenceEqual(actualTransfers),
            "stream 3 grapple transfer enumeration and every field");
        for (int index = 0; index < expectedTransfers.Length; index++)
        {
            AssertEqual(expectedTransfers[index], actualTransfers[index],
                "stream 3 grapple transfer index");
            AssertEqual(expectedTransfers[index], GrappleTileDefinitions.TransferFor(expectedTransfers[index].Asset),
                "stream 3 grapple transfer asset dispatch");
        }
        foreach (int invalid in new[] { -1, 7, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => _ = actualTransfers[invalid],
                "stream 3 grapple transfer bounds");
        AssertThrows<InvalidDataException>(() => GrappleTileDefinitions.TransferFor((VramAssetId)(-1)),
            "stream 3 invalid grapple asset");

        for (int parameter = 0; parameter <= ushort.MaxValue; parameter++)
        {
            int offset = 2 * (parameter & 3);
            ushort list = (ushort)(rom.ReadByte(0xa8e682 + offset) | rom.ReadByte(0xa8e683 + offset) << 8);
            var function = (SparkEnemyFunction)(rom.ReadByte(0xa8e688 + offset) | rom.ReadByte(0xa8e689 + offset) << 8);
            AssertEqual(new SparkMovementDefinition(list, function),
                SparkMovementDefinitions.InitialState((ushort)parameter),
                "stream 3 native Spark selector including adjacent-word case");
        }
        for (int angle = 0; angle <= byte.MaxValue; angle++)
        {
            int offset = 2 * (angle >> 5);
            short x = (short)(rom.ReadByte(0x86bde3 + offset) | rom.ReadByte(0x86bde4 + offset) << 8);
            short y = (short)(rom.ReadByte(0x86bdf3 + offset) | rom.ReadByte(0x86bdf4 + offset) << 8);
            AssertEqual((x, y), ShaktoolProjectilePlacementDefinitions.Offset((byte)angle),
                "stream 3 native Shaktool circle offset");
        }
        for (int bucket = 0; bucket <= ushort.MaxValue; bucket++)
        {
            ushort direction = (ushort)bucket;
            if ((bucket & 31) != 0 || bucket > 224)
                AssertThrows<InvalidDataException>(() => ShaktoolInstructionDefinitions.ForOrientationBucket(direction),
                    "stream 3 invalid Shaktool orientation");
            else
            {
                int address = 0xaadd15 + 2 * (bucket >> 5);
                ushort expected = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
                AssertEqual(expected, ShaktoolInstructionDefinitions.ForOrientationBucket(direction),
                    "stream 3 native Shaktool orientation");
            }
        }
        for (int segment = 0; segment < 7; segment++)
        {
            int offset = segment * 2;
            ushort collision = (ushort)(rom.ReadByte(0xaadf13 + offset) | rom.ReadByte(0xaadf14 + offset) << 8);
            ushort attack = (ushort)(rom.ReadByte(0xaadf21 + offset) | rom.ReadByte(0xaadf22 + offset) << 8);
            AssertEqual(collision, ShaktoolInstructionDefinitions.CollisionForSegment(segment),
                "stream 3 native Shaktool collision program");
            AssertEqual(attack, ShaktoolInstructionDefinitions.AttackForSegment(segment),
                "stream 3 native Shaktool dormant attack program");
        }
        foreach (int invalid in new[] { -1, 7, int.MinValue, int.MaxValue })
        {
            AssertThrows<InvalidDataException>(() => ShaktoolInstructionDefinitions.CollisionForSegment(invalid),
                "stream 3 invalid Shaktool collision segment");
            AssertThrows<InvalidDataException>(() => ShaktoolInstructionDefinitions.AttackForSegment(invalid),
                "stream 3 invalid Shaktool attack segment");
        }
        VerifyStream3WorkRobotColors(rom);
        VerifyStream3PickupAndFirefleaPrograms(rom);
        Console.WriteLine("Lookup stream 3: grapple sectors/transfers, Spark initial states, Shaktool circle/selectors, and Work Robot colors match their originals.");
    }

    private static void VerifyStream3PickupAndFirefleaPrograms(ISnesAddressSpace rom)
    {
        ushort Read(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (int kind = 0; kind < 6; kind++)
            AssertEqual(new EnemyPickupAnimationDefinition((ushort)(2 * kind), Read(0x86ef04 + 2 * kind)),
                EnemyPickupDefinitions.Animation((EnemyPickupKind)kind), "stream 3 pickup kind dispatch");
        for (ushort animation = 0; animation < 5; animation++)
            AssertEqual(Read(0x86efd5 + 2 * animation), EnemyDeathExplosionDefinitions.InstructionPointer(animation),
                "stream 3 death variant dispatch");
        foreach (ushort invalid in new ushort[] { 6, 7, ushort.MaxValue })
            AssertThrows<InvalidDataException>(() => EnemyPickupDefinitions.Animation((EnemyPickupKind)invalid),
                "stream 3 invalid pickup kind");
        foreach (ushort invalid in new ushort[] { 5, 6, ushort.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => EnemyDeathExplosionDefinitions.InstructionPointer(invalid),
                "stream 3 invalid death variant");

        ushort[] pickupMechanics =
        [
            0xed8d, 0xed91, 0xed95, 0xed99, 0xed9d, 0xed9f, 0xeda1,
            0xeda3, 0xeda7, 0xedab, 0xedaf, 0xedb3, 0xedb5, 0xedb7,
            0xedb9, 0xedbd, 0xedc1, 0xedc3, 0xedc5,
            0xeddd, 0xede1, 0xede5, 0xede7, 0xede9,
            0xedeb, 0xedef, 0xedf3, 0xedf7, 0xedfb, 0xedfd,
        ];
        ushort[] pickupPresentation =
        [
            0xed8f, 0xed93, 0xed97, 0xed9b, 0xeda5, 0xeda9, 0xedad, 0xedb1,
            0xedbb, 0xedbf, 0xeddf, 0xede3, 0xeded, 0xedf1, 0xedf5, 0xedf9,
        ];
        AssertEqual(pickupMechanics.Length, EnemyPickupInstructionProgramDefinitions.MechanicsWordCount,
            "stream 3 pickup mechanic count");
        AssertEqual(pickupPresentation.Length, EnemyPickupInstructionProgramDefinitions.PresentationWordCount,
            "stream 3 pickup visual operand count");
        for (int index = 0; index < pickupMechanics.Length; index++)
        {
            ushort address = pickupMechanics[index];
            AssertEqual(new EnemyPickupInstructionMechanicsWord(address, Read(0x860000 | address)),
                EnemyPickupInstructionProgramDefinitions.MechanicsWord(index), "stream 3 native pickup mechanic");
            AssertEqual(Read(0x860000 | address), EnemyPickupInstructionProgramDefinitions.ReadMechanicsWord(address),
                "stream 3 pickup mechanic dispatch");
        }
        for (int index = 0; index < pickupPresentation.Length; index++)
        {
            ushort address = pickupPresentation[index];
            AssertEqual(address, EnemyPickupInstructionProgramDefinitions.PresentationWordAddress(index),
                "stream 3 pickup visual operand address");
            AssertThrows<InvalidDataException>(() => EnemyPickupInstructionProgramDefinitions.ReadMechanicsWord(address),
                "stream 3 pickup visual operand remains excluded");
        }
        var mechanicsSet = pickupMechanics.ToHashSet();
        var byteSet = pickupMechanics.SelectMany(address => new[] { (int)address, address + 1 }).ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool owned = mechanicsSet.Contains((ushort)address);
            AssertEqual(owned, EnemyPickupInstructionProgramDefinitions.Owns(RoomEnemyProjectileKind.EnemyDeathPickup, (ushort)address),
                "stream 3 pickup ownership domain");
            AssertEqual(owned, EnemyPickupInstructionProgramDefinitions.Owns(RoomEnemyProjectileKind.EnemyDeathExplosion, (ushort)address),
                "stream 3 explosion pickup ownership domain");
            AssertEqual(byteSet.Contains(address), EnemyPickupInstructionProgramDefinitions.IsCompiledMechanicsByte(0x860000 | address),
                "stream 3 pickup byte ownership domain");
        }
        AssertTrue(!EnemyPickupInstructionProgramDefinitions.Owns(RoomEnemyProjectileKind.ShaktoolAttackFrontCircle, pickupMechanics[0]),
            "stream 3 unrelated actor does not own pickup instructions");
        AssertTrue(!EnemyPickupInstructionProgramDefinitions.IsCompiledMechanicsByte(0xa3ed8d),
            "stream 3 pickup excludes other bank");

        AssertEqual(54, FirefleaInstructionProgramDefinitions.MechanicsWordCount, "stream 3 Fireflea mechanic count");
        AssertEqual(52, FirefleaInstructionProgramDefinitions.PresentationWordCount, "stream 3 Fireflea visual count");
        var fireBytes = new HashSet<int>();
        for (int index = 0; index < 54; index++)
        {
            ushort address = (ushort)(index < 52 ? 0x8c2f + index * 4 : 0x8cff + (index - 52) * 2);
            ushort value = Read(0xa30000 | address);
            AssertEqual(new FirefleaInstructionMechanicsWord(address, value), FirefleaInstructionProgramDefinitions.MechanicsWord(index),
                "stream 3 native Fireflea mechanic");
            AssertEqual(value, FirefleaInstructionProgramDefinitions.ReadMechanicsWord(address),
                "stream 3 Fireflea mechanic dispatch");
            fireBytes.Add(address);
            fireBytes.Add(address + 1);
            if (index < 52)
            {
                ushort visual = (ushort)(address + 2);
                AssertEqual(visual, FirefleaInstructionProgramDefinitions.PresentationWordAddress(index),
                    "stream 3 Fireflea visual operand");
                AssertThrows<InvalidDataException>(() => FirefleaInstructionProgramDefinitions.ReadMechanicsWord(visual),
                    "stream 3 Fireflea visual remains excluded");
            }
        }
        for (int address = 0; address <= ushort.MaxValue; address++)
            AssertEqual(fireBytes.Contains(address), FirefleaInstructionProgramDefinitions.IsCompiledMechanicsByte(0xa30000 | address),
                "stream 3 Fireflea byte ownership domain");
        foreach (int invalid in new[] { -1, 30, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => EnemyPickupInstructionProgramDefinitions.MechanicsWord(invalid),
                "stream 3 pickup mechanic bounds");
        foreach (int invalid in new[] { -1, 16, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => EnemyPickupInstructionProgramDefinitions.PresentationWordAddress(invalid),
                "stream 3 pickup visual bounds");
        foreach (int invalid in new[] { -1, 54, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => FirefleaInstructionProgramDefinitions.MechanicsWord(invalid),
                "stream 3 Fireflea mechanic bounds");
        foreach (int invalid in new[] { -1, 52, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => FirefleaInstructionProgramDefinitions.PresentationWordAddress(invalid),
                "stream 3 Fireflea visual bounds");
    }

    private static void VerifyStream3WorkRobotColors(ISnesAddressSpace rom)
    {
        var words = new ushort[6][];
        var colors = new PaletteRgb5[6][];
        for (int frame = 0; frame < 6; frame++)
        {
            words[frame] = new ushort[4];
            colors[frame] = new PaletteRgb5[4];
            for (int color = 0; color < 4; color++)
            {
                int address = 0xa8ccc1 + 10 * frame + 2 * color;
                ushort value = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
                words[frame][color] = value;
                colors[frame][color] = new PaletteRgb5
                {
                    Red = value & 31,
                    Green = (value >> 5) & 31,
                    Blue = (value >> 10) & 31,
                };
            }
        }
        var document = new WorkRobotPaletteCycleDocument { Version = 1, Frames = colors };
        WorkRobotPaletteCycle Load() => WorkRobotPaletteCycle.Load(
            new MemoryStream(WorkRobotPaletteCycle.Write(document), writable: false));
        void Check(WorkRobotPaletteCycle cycle)
        {
            var cgram = new SnesCgram();
            for (int frame = 0; frame < 6; frame++)
            {
                cycle.ApplyFrame(cgram, frame, 9);
                for (int color = 0; color < 4; color++)
                {
                    AssertEqual(words[frame][color], cycle.Resolve(frame, color),
                        "stream 3 Work Robot selected color");
                    AssertEqual(words[frame][color], cgram.Colors[9 + color],
                        "stream 3 Work Robot applied color");
                }
            }
            string expectedIdentity = SelectedPresentationHash.Create("WorkRobotPaletteCycle-v1",
                content => content.AppendWordFrames("frames", words));
            AssertEqual(expectedIdentity, cycle.ContentIdentity,
                "stream 3 Work Robot identity preserves original row framing");
        }
        var stock = Load();
        Check(stock);
        for (int frame = 0; frame < 6; frame++)
        for (int color = 0; color < 4; color++)
        for (int channel = 0; channel < 3; channel++)
        {
            PaletteRgb5 original = colors[frame][color];
            ushort originalWord = words[frame][color];
            colors[frame][color] = channel switch
            {
                0 => original with { Red = original.Red ^ 1 },
                1 => original with { Green = original.Green ^ 1 },
                _ => original with { Blue = original.Blue ^ 1 },
            };
            words[frame][color] ^= (ushort)(1 << (channel * 5));
            Check(Load());
            colors[frame][color] = original;
            words[frame][color] = originalWord;
        }
        foreach (int invalid in new[] { -1, 6, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(invalid, 0),
                "stream 3 Work Robot frame bounds");
        foreach (int invalid in new[] { -1, 4, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(0, invalid),
                "stream 3 Work Robot color bounds");
    }
}
