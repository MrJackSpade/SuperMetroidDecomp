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
        VerifyStream3ChootControl(rom);
        VerifyStream3RipperMappings(rom);
        VerifyStream3UniformEnemyLoops(rom);
        VerifyStream3MotherBrainFades(rom);
        VerifyStream3BabyFade(rom);
        VerifyStream3DrainFades(rom);
        VerifyStream3ShitroidPulse(rom);
        VerifyStream3CorpseGeometry(rom);
        VerifyStream3EscapeGeometry(rom);
        VerifyStream3PainfulWalking(rom);
        VerifyStream3DeathSelectors(rom);
        VerifyMotherBrainContactHitboxes();
        foreach (MotherBrainContactPart part in Enum.GetValues<MotherBrainContactPart>())
        {
            var regions = MotherBrainContactHitboxDefinitions.Get(part);
            foreach (int invalid in new[] { -1, regions.Count, int.MaxValue })
                AssertThrows<IndexOutOfRangeException>(() => _ = regions[invalid], "stream 3 contact region bounds");
        }
        Console.WriteLine("Lookup stream 3: all implemented mapping conversions match their original values and accepted domains.");
    }

    private static void VerifyStream3DeathSelectors(ISnesAddressSpace rom)
    {
        ushort Read(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (ushort parameter = 0; parameter < 3; parameter++)
            AssertEqual(Read(0x86c929 + 2 * parameter), MotherBrainDeathExplosionDefinitions.InstructionList(parameter),
                "stream 3 native death explosion variant");
        foreach (ushort invalid in new ushort[] { 3, 4, ushort.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => MotherBrainDeathExplosionDefinitions.InstructionList(invalid),
                "stream 3 death explosion selector bounds");
        var sequence = new MotherBrainRainbowBeamAttackSequence();
        typeof(MotherBrainRainbowBeamAttackSequence).GetProperty("Phase")!.SetValue(sequence,
            MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceStartEscape);
        var bus = new TestAddressSpace();
        var samus = new SamusState { Health = 99 };
        var firstPage = sequence.Step(bus, samus, 0, 0);
        AssertEqual(0, firstPage.EscapePaletteFxRequests.Count, "stream 3 escape palette effects wait for door graphics");
        var handoff = sequence.Step(bus, samus, 0, 0);
        AssertEqual(4, handoff.EscapePaletteFxRequests.Count, "stream 3 four escape palette registrations");
        for (int effect = 0; effect < 4; effect++)
            AssertEqual(Read(0xa9b296 + 7 * effect), handoff.EscapePaletteFxRequests[effect],
                "stream 3 native escape palette registration order");
        var text = sequence.Step(bus, samus, 0, 0);
        AssertEqual(0, text.EscapePaletteFxRequests.Count, "stream 3 escape palette registrations occur once");
    }

    private static void VerifyStream3PainfulWalking(ISnesAddressSpace rom)
    {
        ushort Read(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (int stage = 0; stage < 8; stage++)
        {
            AssertEqual(Read(0xa9beee + 2 * stage), MotherBrainPainfulWalkingDefinitions.AnimationDelay(stage), "stream 3 native stagger animation delay");
            AssertEqual(Read(0xa9befe + 2 * stage), MotherBrainPainfulWalkingDefinitions.NeckAngleDelta(stage), "stream 3 native stagger neck angle delta");
            AssertEqual(Read(0xa9c049 + 2 * stage), MotherBrainPainfulWalkingDefinitions.FunctionTimer(stage), "stream 3 native stagger pause timer");
        }
        foreach (int invalid in new[] { -1, 8, int.MaxValue })
        {
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainPainfulWalkingDefinitions.AnimationDelay(invalid), "stream 3 stagger delay bounds");
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainPainfulWalkingDefinitions.NeckAngleDelta(invalid), "stream 3 stagger neck bounds");
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainPainfulWalkingDefinitions.FunctionTimer(invalid), "stream 3 stagger timer bounds");
        }
        var step = typeof(MotherBrainRainbowBeamAttackSequence).GetMethod("StepPainfulWalking",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        foreach (ushort stage in new ushort[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, ushort.MaxValue })
        {
            var sequence = new MotherBrainRainbowBeamAttackSequence();
            sequence.Body.XPosition = 0;
            typeof(MotherBrainRainbowBeamAttackSequence).GetProperty("PainfulWalkingStage")!.SetValue(sequence, stage);
            step.Invoke(sequence, null);
            AssertEqual(Read(0xa9c049 + 2 * Math.Min(stage, (ushort)7)), sequence.PainfulWalkingFunctionTimer,
                "stream 3 real stagger timer clamps terminal stages");
        }
    }

    private static void VerifyStream3EscapeGeometry(ISnesAddressSpace rom)
    {
        ushort Read(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var sequence = new MotherBrainRainbowBeamAttackSequence();
        const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var timer = typeof(MotherBrainRainbowBeamAttackSequence).GetMethod("CreateNextEscapeTimerTileTransfer", flags)!;
        var door = typeof(MotherBrainRainbowBeamAttackSequence).GetMethod("CreateNextExplodedDoorTileTransfer", flags)!;
        foreach (bool timerList in new[] { true, false })
        {
            int count = timerList ? 7 : 2;
            int start = timerList ? 0xa6c4cb : 0xa9902f;
            var method = timerList ? timer : door;
            for (int index = 0; index < count; index++)
            {
                int address = start + index * 7;
                var expected = new MotherBrainSpriteTileTransferRequest((ushort)index, Read(address),
                    (uint)(Read(address + 2) | rom.ReadByte(address + 4) << 16), Read(address + 5));
                var calculated = timerList ? MotherBrainEscapeTextArtworkDefinitions.Transfer(index)
                    : MotherBrainSpecialSpriteArtworkDefinitions.ExplodedDoor.Transfer(index);
                AssertEqual(expected, calculated, "stream 3 native escape graphics record");
                AssertEqual(expected, (MotherBrainSpriteTileTransferRequest)method.Invoke(sequence, null)!,
                    "stream 3 production escape transfer selection");
                AssertEqual((ushort)(index + 1), timerList ? sequence.EscapeTimerTileTransferIndex : sequence.ExplodedDoorTileTransferIndex,
                    "stream 3 production escape cursor advances once");
            }
            try
            {
                method.Invoke(sequence, null);
                throw new InvalidDataException("Completed escape transfer list unexpectedly produced another entry.");
            }
            catch (System.Reflection.TargetInvocationException error) when (error.InnerException is InvalidOperationException) { }
        }
        foreach (int invalid in new[] { -1, 5, int.MaxValue })
        {
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainEscapeTextArtworkDefinitions.PageSource(invalid), "stream 3 text page source bounds");
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainEscapeTextArtworkDefinitions.PageDestination(invalid), "stream 3 text page destination bounds");
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainEscapeTextArtworkDefinitions.PageByteCount(invalid), "stream 3 text page size bounds");
        }
        foreach (int invalid in new[] { -1, 7, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainEscapeTextArtworkDefinitions.Transfer(invalid), "stream 3 escape transfer bounds");
        foreach (int invalid in new[] { -1, 2, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainSpecialSpriteArtworkDefinitions.ExplodedDoor.Transfer(invalid), "stream 3 door transfer bounds");
        using var temporary = new MapCatalogTestDirectory();
        SuperMetroid.AssetExtraction.EnemyTileArtworkFiles.Extract(rom, temporary.Root, SuperMetroid.AssetExtraction.SupportedCartridge.Sha256);
        VerifyInstalledMotherBrainEscapeTextArtwork(temporary.Root,
            SuperMetroid.AssetExtraction.EnemyTileArtworkFiles.Load(temporary.Root, null));
    }

    private static void VerifyStream3CorpseGeometry(ISnesAddressSpace rom)
    {
        ushort Read(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (int row = 0; row < 8; row++)
            AssertEqual((int)Read(0xa9e262 + row * 2), MotherBrainCorpseArtworkDefinitions.TileRowOffset(row),
                "stream 3 corpse native tile row offset");
        var transfers = MotherBrainCorpseArtworkDefinitions.RotTransfers;
        AssertEqual(6, transfers.Count, "stream 3 corpse transfer count");
        var enumerated = transfers.ToArray();
        for (int row = 0; row < 6; row++)
        {
            int entry = 0xa9e1f4 + row * 8;
            var expected = new MotherBrainSpriteTileTransferRequest((ushort)row, Read(entry),
                (uint)(Read(entry + 4) | (Read(entry + 2) >> 8) << 16), Read(entry + 6));
            AssertEqual(expected, transfers[row], "stream 3 native corpse rot transfer");
            AssertEqual(expected, enumerated[row], "stream 3 corpse transfer enumeration order");
            int pageEntry = 0xa99003 + row * 7;
            uint source = (uint)(Read(pageEntry + 2) | rom.ReadByte(pageEntry + 4) << 16);
            AssertEqual(source, MotherBrainCorpseArtworkDefinitions.VramPageSource(row), "stream 3 corpse source page");
            AssertEqual(Read(pageEntry + 5), MotherBrainCorpseArtworkDefinitions.VramPageDestination(row), "stream 3 corpse destination page");
        }
        int[] minimumY = [16, 8, 0, 0, 0, 8, 32]; // CMP/BCC gates in native $A9:EA40/$EB0B.
        for (int column = 0; column < 7; column++)
            AssertEqual(minimumY[column], MotherBrainCorpseArtworkDefinitions.ColumnMinimumY(column), "stream 3 corpse native outline gate");
        var copy = typeof(MotherBrainCorpseRottingState).GetMethod("CopyOrMovePixelRow",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!;
        // Confirm the changed row/column geometry through the real pixel-copy path.
        for (ushort y = 0; y < 48; y++)
        foreach (bool move in new[] { false, true })
        {
            var actual = new TestAddressSpace();
            var expected = new TestAddressSpace();
            for (int offset = 0; offset < 0x600; offset++)
            {
                byte value = (byte)(offset * 37 + 11);
                actual.WriteByte(0x7e9000 + offset, value);
                expected.WriteByte(0x7e9000 + offset, value);
            }
            int source = Read(0xa9e262 + (y >> 3) * 2) + (y & 7) * 2;
            int destination = source + ((y & 7) < 6 ? 0 : 0xd4);
            for (int column = 0; column < 7; column++)
            {
                if (y < minimumY[column]) continue;
                int start = 0x7e9000 + column * 32;
                if (y < 46)
                {
                    // Read both plane words before copying or clearing either source.
                    byte lo = expected.ReadByte(start + source);
                    byte hi = expected.ReadByte(start + source + 1);
                    byte lo2 = expected.ReadByte(start + source + 16);
                    byte hi2 = expected.ReadByte(start + source + 17);
                    expected.WriteByte(start + destination + 2, lo);
                    expected.WriteByte(start + destination + 3, hi);
                    expected.WriteByte(start + destination + 18, lo2);
                    expected.WriteByte(start + destination + 19, hi2);
                }
                if (move)
                {
                    expected.WriteByte(start + source, 0);
                    expected.WriteByte(start + source + 1, 0);
                    expected.WriteByte(start + source + 16, 0);
                    expected.WriteByte(start + source + 17, 0);
                }
            }
            copy.Invoke(null, [actual, actual, y, move]);
            for (int offset = 0; offset < 0x600; offset++)
                AssertEqual(expected.ReadByte(0x7e9000 + offset), actual.ReadByte(0x7e9000 + offset), "stream 3 corpse row geometry");
        }
        foreach (int invalid in new[] { -1, 6, int.MaxValue })
        {
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainCorpseArtworkDefinitions.VramPageSource(invalid), "stream 3 corpse source bounds");
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainCorpseArtworkDefinitions.VramPageDestination(invalid), "stream 3 corpse destination bounds");
            AssertThrows<IndexOutOfRangeException>(() => _ = transfers[invalid], "stream 3 corpse transfer bounds");
        }
        VerifyMotherBrainCorpseStockArtwork();
    }

    private static void VerifyStream3ShitroidPulse(ISnesAddressSpace rom)
    {
        ushort Read(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        PaletteRgb5 Rgb(ushort word) => new() { Red = word & 31, Green = word >> 5 & 31, Blue = word >> 10 & 31 };
        ushort[] Words(int address, int count) => Enumerable.Range(0, count).Select(i => Read(address + 2 * i)).ToArray();
        var normal = Enumerable.Range(0, 8).Select(frame => Words(0xa9f6d1 + 8 * frame, 4)).ToArray();
        var sidehopper = Words(0xa9f8c6, 16);
        var shitroid = Words(0xa9f8e6, 16);
        var dead = Words(0xa9f8a6, 16);
        var rows = normal.Select(row => row.Select(Rgb).ToArray()).ToArray();
        var document = new ShitroidColorDocument
        {
            Version = 1, Normal = rows, Sidehopper = sidehopper.Select(Rgb).ToArray(),
            Shitroid = shitroid.Select(Rgb).ToArray(), DeadSidehopper = dead.Select(Rgb).ToArray(),
        };
        ShitroidColorCatalog Load() => ShitroidColorCatalog.Load(new MemoryStream(ShitroidColorCatalog.Write(document)));
        void Check(ShitroidColorCatalog colors)
        {
            for (int frame = 0; frame < 8; frame++)
            for (int color = 0; color < 4; color++)
                AssertEqual(normal[frame][color], colors.NormalColor(frame, color), "stream 3 Shitroid pulse RGB5");
            string identity = SelectedPresentationHash.Create("ShitroidColorCatalog-v1", content =>
            {
                content.AppendWords("sidehopper", sidehopper);
                content.AppendWords("shitroid", shitroid);
                content.AppendWords("deadSidehopper", dead);
                content.AppendWordFrames("normal", normal);
            });
            AssertEqual(identity, colors.ContentIdentity, "stream 3 Shitroid pulse original identity framing");
        }
        var stock = Load();
        Check(stock);
        var pulse = typeof(ShitroidColorCatalog).GetField("normal",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(stock)!;
        AssertTrue(pulse.GetType().GetField("supplied", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .GetValue(pulse) is null, "stream 3 Shitroid original pulse rows discarded");
        for (int frame = 0; frame < 8; frame++)
        for (int color = 0; color < 4; color++)
        for (int channel = 0; channel < 3; channel++)
        {
            ushort original = normal[frame][color];
            normal[frame][color] ^= (ushort)(1 << (5 * channel));
            rows[frame][color] = Rgb(normal[frame][color]);
            Check(Load());
            normal[frame][color] = original;
            rows[frame][color] = Rgb(original);
        }
        foreach (int invalid in new[] { -1, 8, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.NormalColor(invalid, 0), "stream 3 Shitroid pulse frame bounds");
        foreach (int invalid in new[] { -1, 4, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.NormalColor(0, invalid), "stream 3 Shitroid pulse color bounds");
    }

    private static void VerifyStream3DrainFades(ISnesAddressSpace rom)
    {
        ushort Read(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        PaletteRgb5 Color(ushort word) => new() { Red = word & 31, Green = word >> 5 & 31, Blue = word >> 10 & 31 };
        ushort Word(PaletteRgb5 color) => (ushort)(color.Red | color.Green << 5 | color.Blue << 10);
        PaletteRgb5[] Zeros(int count) => Enumerable.Range(0, count).Select(_ => Color(0)).ToArray();
        MotherBrainRainbowPaletteFrameDocument Empty(int bodies, int legs, bool trailing) => new()
        { Body = Zeros(bodies), BackLegs = Zeros(legs), TrailingColor = trailing ? Color(0) : null };
        MotherBrainRainbowPaletteFrameDocument ReadFull(int bodySource, int legSource) => new()
        {
            Body = Enumerable.Range(0, 15).Select(color => Color(Read(bodySource + 2 * color))).ToArray(),
            BackLegs = Enumerable.Range(0, 15).Select(color => Color(Read(legSource + 2 * color))).ToArray(),
        };
        var rainbow = Enumerable.Range(0, 10).Select(frame =>
        {
            int source = 0xad0000 | Read(0xade434 + 2 * frame);
            return ReadFull(source, source + 30);
        }).ToArray();
        var drain = new MotherBrainRainbowPaletteFrameDocument[8];
        var fake = new PaletteRgb5[8][];
        for (int frame = 0; frame < 8; frame++)
        {
            int source = 0xad0000 | Read(0xadef87 + 2 * frame);
            drain[frame] = new()
            {
                Body = Enumerable.Range(0, 15).Select(color => Color(Read(source + 2 * color))).ToArray(),
                BackLegs = Enumerable.Range(0, 5).Select(color => Color(Read(source + 30 + 2 * color))).ToArray(),
                TrailingColor = Color(Read(source + 40)),
            };
            int fakeSource = 0xad0000 | Read(0xaded8a + 2 * frame);
            fake[frame] = Enumerable.Range(0, 3).Select(color => Color(Read(fakeSource + 2 * color))).ToArray();
        }
        var document = new MotherBrainRainbowPaletteDocument
        {
            Version = 3, Rainbow = rainbow,
            ToGrey = drain, FromGrey = Enumerable.Range(0, 8).Select(_ => Empty(13, 5, true)).ToArray(),
            FakeDeathToGrey = fake, Normal = ReadFull(0xa99474, 0xa99494), BeamInitial = Color(0), BeamCycle = Zeros(38),
        };
        MotherBrainRainbowPalettePresentation Load(MotherBrainRainbowPaletteDocument value,
            MotherBrainRainbowPalettePresentation? stock = null) => MotherBrainRainbowPalettePresentation.Load(
                new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(value, MapPresentationFormat.JsonOptions)), stock);
        void Check(MotherBrainRainbowPalettePresentation palette)
        {
            var cgram = new SnesCgram();
            var bus = new TestAddressSpace();
            for (int frame = 0; frame < 8; frame++)
            {
                palette.ApplyToGrey(bus, cgram, frame);
                for (int color = 0; color < 15; color++)
                {
                    AssertEqual(Word(drain[frame].Body[color]), cgram.Colors[0x41 + color], "stream 3 drain body RGB5");
                    AssertEqual(Word(drain[frame].Body[color]), cgram.Colors[0x91 + color], "stream 3 drain brain RGB5");
                }
                for (int color = 0; color < 5; color++)
                    AssertEqual(Word(drain[frame].BackLegs[color]), cgram.Colors[180 + color], "stream 3 drain legs RGB5");
                AssertEqual(Word(drain[frame].TrailingColor!), (ushort)(bus.ReadByte(0x7e017c) | bus.ReadByte(0x7e017d) << 8),
                    "stream 3 drain trailing word");
                palette.ApplyFakeDeathToGrey(cgram, frame);
                for (int color = 0; color < 3; color++)
                    AssertEqual(Word(fake[frame][color]), cgram.Colors[0x91 + color], "stream 3 fake-death brain RGB5");
            }
        }
        var stock = Load(document);
        Check(stock);
        void CheckRainbow(MotherBrainRainbowPalettePresentation palette)
        {
            var cgram = new SnesCgram();
            for (int frame = 0; frame <= 10; frame++)
            {
                var expected = frame == 10 ? document.Normal : rainbow[frame];
                if (frame == 10) palette.ApplyNormal(cgram);
                else palette.ApplyRainbow(cgram, frame);
                for (int color = 0; color < 15; color++)
                {
                    AssertEqual(Word(expected.Body[color]), cgram.Colors[0x41 + color], "stream 3 rainbow body");
                    AssertEqual(Word(expected.Body[color]), cgram.Colors[0x91 + color], "stream 3 rainbow brain");
                    AssertEqual(Word(expected.BackLegs[color]), cgram.Colors[0xb1 + color], "stream 3 rainbow shadow");
                }
            }
        }
        CheckRainbow(stock);
        var storedRainbow = (Array)typeof(MotherBrainRainbowPalettePresentation).GetField("rainbow",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(stock)!;
        foreach (object frame in storedRainbow)
            AssertTrue(frame.GetType().GetField("backLegs", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .GetValue(frame) is null, "stream 3 rainbow shadow tables discarded");
        for (int frame = 0; frame <= 10; frame++)
        for (int color = 0; color < 30; color++)
        for (int channel = 0; channel < 3; channel++)
        {
            var selected = frame == 10 ? document.Normal : rainbow[frame];
            PaletteRgb5[] row = color < 15 ? selected.Body : selected.BackLegs;
            int index = color % 15;
            PaletteRgb5 original = row[index];
            row[index] = Color((ushort)(Word(original) ^ 1 << (5 * channel)));
            CheckRainbow(Load(document));
            row[index] = original;
        }
        foreach (int invalid in new[] { -1, 10, int.MaxValue })
            AssertThrows<InvalidDataException>(() => stock.ApplyRainbow(new SnesCgram(), invalid), "stream 3 rainbow bounds");
        foreach (string field in new[] { "toGrey", "fakeDeathToGrey" })
        {
            var fade = typeof(MotherBrainRainbowPalettePresentation).GetField(field,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(stock)!;
            AssertTrue(fade.GetType().GetField("supplied", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .GetValue(fade) is null, "stream 3 drain stock intermediate rows discarded");
        }
        Check(Load(document with { Version = 2, FakeDeathToGrey = null }, stock));
        // Independently edited channels must still be delivered exactly, including endpoints and WRAM.
        for (int frame = 0; frame < 8; frame++)
        for (int color = 0; color < 24; color++)
        for (int channel = 0; channel < 3; channel++)
        {
            var row = drain[frame];
            PaletteRgb5 original = color < 15 ? row.Body[color] : color < 20 ? row.BackLegs[color - 15]
                : color == 20 ? row.TrailingColor! : fake[frame][color - 21];
            void Set(PaletteRgb5 value)
            {
                if (color < 15) row.Body[color] = value;
                else if (color < 20) row.BackLegs[color - 15] = value;
                else if (color == 20) drain[frame] = row with { TrailingColor = value };
                else fake[frame][color - 21] = value;
            }
            Set(Color((ushort)(Word(original) ^ 1 << (5 * channel))));
            Check(Load(document));
            Set(original);
        }
        foreach (int invalid in new[] { -1, 8, int.MaxValue })
        {
            AssertThrows<InvalidDataException>(() => stock.ApplyToGrey(new TestAddressSpace(), new SnesCgram(), invalid), "stream 3 drain bounds");
            AssertThrows<InvalidDataException>(() => stock.ApplyFakeDeathToGrey(new SnesCgram(), invalid), "stream 3 fake-death bounds");
        }
    }

    private static void VerifyStream3BabyFade(ISnesAddressSpace rom)
    {
        ushort Read(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var initial = new ushort[15];
        var fade = new ushort[6][];
        for (int color = 0; color < initial.Length; color++)
            initial[color] = Read(0xa994d4 + 2 * color);
        for (int frame = 0; frame < fade.Length; frame++)
        {
            fade[frame] = new ushort[14];
            for (int color = 0; color < fade[frame].Length; color++)
                fade[frame][color] = Read(0xade90c + 28 * frame + 2 * color);
        }
        static PaletteRgb5 Rgb(ushort value) => new()
        {
            Red = value & 31, Green = (value >> 5) & 31, Blue = (value >> 10) & 31,
        };
        BabyMetroidCutsceneColorCatalog Load() => BabyMetroidCutsceneColorCatalog.Load(new MemoryStream(
            BabyMetroidCutsceneColorCatalog.Write(new BabyMetroidCutsceneColorDocument
            {
                Version = 1,
                Initial = initial.Select(Rgb).ToArray(),
                Fade = fade.Select(row => row.Select(Rgb).ToArray()).ToArray(),
            }), writable: false));
        void Check(BabyMetroidCutsceneColorCatalog catalog)
        {
            for (int color = 0; color < initial.Length; color++)
                AssertEqual(initial[color], catalog.InitialColor(color), "stream 3 Baby initial color");
            for (int frame = 0; frame < fade.Length; frame++)
            for (int color = 0; color < fade[frame].Length; color++)
                AssertEqual(fade[frame][color], catalog.FadeColor(frame + 1, color), "stream 3 Baby fade selected color");
            string identity = SelectedPresentationHash.Create("BabyMetroidCutsceneColorCatalog-v1", content =>
            {
                content.AppendWords("initial", initial);
                content.AppendWordFrames("fade", fade);
            });
            AssertEqual(identity, catalog.ContentIdentity, "stream 3 Baby fade selected identity");
        }
        var stock = Load();
        Check(stock);
        const System.Reflection.BindingFlags fields = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        object selectedFade = typeof(BabyMetroidCutsceneColorCatalog).GetField("fade", fields)!.GetValue(stock)!;
        AssertTrue(selectedFade.GetType().GetField("supplied", fields)!.GetValue(selectedFade) is null,
            "stream 3 original Baby fade discards its stored frame table");
        uint[] endpoints = (uint[])selectedFade.GetType().GetField("endpointColors", fields)!.GetValue(selectedFade)!;
        for (int color = 0; color < endpoints.Length; color++)
        {
            uint value = endpoints[color];
            ushort rgb5 = (ushort)(((value & 255) >> 3) | (((value >> 8 & 255) >> 3) << 5) | (((value >> 16 & 255) >> 3) << 10));
            AssertEqual(Read(0xade8f0 + 2 * color), rgb5,
                "stream 3 compatible RGB8 endpoint also matches original undisplayed RGB5 palette");
        }
        for (int frame = 0; frame < fade.Length; frame++)
        for (int color = 0; color < fade[frame].Length; color++)
        for (int channel = 0; channel < 3; channel++)
        {
            ushort original = fade[frame][color];
            fade[frame][color] ^= (ushort)(1 << (5 * channel));
            Check(Load());
            fade[frame][color] = original;
        }
        foreach (int invalid in new[] { -1, 0, 7, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.FadeColor(invalid, 0), "stream 3 Baby fade frame bounds");
        foreach (int invalid in new[] { -1, 14, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.FadeColor(1, invalid), "stream 3 Baby fade color bounds");
    }

    private static void VerifyStream3MotherBrainFades(ISnesAddressSpace rom)
    {
        ushort Read(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var body = new ushort[16][];
        var leg = new ushort[16][];
        var corpse = new ushort[8][];
        for (int frame = 0; frame < 16; frame++)
        {
            body[frame] = new ushort[14];
            leg[frame] = new ushort[14];
            for (int color = 0; color < 14; color++)
            {
                body[frame][color] = Read(0xadea0a + 56 * frame + 2 * color);
                leg[frame][color] = Read(0xadea26 + 56 * frame + 2 * color);
            }
        }
        for (int frame = 0; frame < 8; frame++)
        {
            corpse[frame] = new ushort[15];
            for (int color = 0; color < 15; color++)
                corpse[frame][color] = Read(0xadf119 + 30 * frame + 2 * color);
        }
        var door = new ushort[14];
        for (int color = 0; color < door.Length; color++)
            door[color] = Read(0xa99534 + 2 * color);

        static PaletteRgb5 Rgb(ushort value) => new()
        {
            Red = value & 31, Green = (value >> 5) & 31, Blue = (value >> 10) & 31,
        };
        MotherBrainDeathColorCatalog Load() => MotherBrainDeathColorCatalog.Load(new MemoryStream(
            MotherBrainDeathColorCatalog.Write(new MotherBrainDeathColorDocument
            {
                Version = 1,
                BodyFade = body.Select(row => row.Select(Rgb).ToArray()).ToArray(),
                LegFade = leg.Select(row => row.Select(Rgb).ToArray()).ToArray(),
                CorpseFade = corpse.Select(row => row.Select(Rgb).ToArray()).ToArray(),
                ExplodedDoor = door.Select(Rgb).ToArray(),
            }), writable: false));
        void Check(MotherBrainDeathColorCatalog catalog)
        {
            for (int frame = 0; frame < 16; frame++)
            for (int color = 0; color < 14; color++)
            {
                AssertEqual(body[frame][color], catalog.BodyColor(frame, color), "stream 3 selected body fade color");
                AssertEqual(leg[frame][color], catalog.LegColor(frame, color), "stream 3 selected leg fade color");
            }
            for (int frame = 0; frame < 8; frame++)
            for (int color = 0; color < 15; color++)
                AssertEqual(corpse[frame][color], catalog.CorpseColor(frame, color), "stream 3 selected corpse fade color");
            for (int color = 0; color < door.Length; color++)
                AssertEqual(door[color], catalog.ExplodedDoorColor(color), "stream 3 unchanged door color");
            string identity = SelectedPresentationHash.Create("MotherBrainDeathColorCatalog-v1", content =>
            {
                content.AppendWords("explodedDoor", door);
                content.AppendWordFrames("bodyFade", body);
                content.AppendWordFrames("legFade", leg);
                content.AppendWordFrames("corpseFade", corpse);
            });
            AssertEqual(identity, catalog.ContentIdentity, "stream 3 death fade identity keeps exact selected values");
        }
        var stock = Load();
        Check(stock);
        const System.Reflection.BindingFlags fields = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        foreach (string name in new[] { "bodyFade", "legFade", "corpseFade" })
        {
            object fade = typeof(MotherBrainDeathColorCatalog).GetField(name, fields)!.GetValue(stock)!;
            AssertTrue(fade.GetType().GetField("supplied", fields)!.GetValue(fade) is null,
                "stream 3 original death fade discards its stored frame table");
        }
        foreach (ushort[][] frames in new[] { body, leg, corpse })
        for (int frame = 0; frame < frames.Length; frame++)
        for (int color = 0; color < frames[frame].Length; color++)
        for (int channel = 0; channel < 3; channel++)
        {
            ushort original = frames[frame][color];
            frames[frame][color] ^= (ushort)(1 << (5 * channel));
            Check(Load());
            frames[frame][color] = original;
        }
        foreach (int invalid in new[] { -1, 16, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => stock.BodyColor(invalid, 0), "stream 3 body fade frame bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => stock.LegColor(invalid, 0), "stream 3 leg fade frame bounds");
        }
        foreach (int invalid in new[] { -1, 8, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.CorpseColor(invalid, 0), "stream 3 corpse fade frame bounds");
        foreach (int invalid in new[] { -1, 14, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => stock.BodyColor(0, invalid), "stream 3 body fade color bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => stock.LegColor(0, invalid), "stream 3 leg fade color bounds");
        }
        foreach (int invalid in new[] { -1, 15, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.CorpseColor(0, invalid), "stream 3 corpse fade color bounds");
    }

    private static void VerifyStream3UniformEnemyLoops(ISnesAddressSpace rom)
    {
        VerifyMochtroidInstructionProgramDefinitions();
        VerifyYellowPipeBugInstructionProgramDefinitions();
        Check(0xa30000, [0xa745, 0xa759],
            index =>
            {
                var word = MochtroidInstructionProgramDefinitions.MechanicsWord(index);
                return (word.Address, word.Value);
            }, MochtroidInstructionProgramDefinitions.PresentationWordAddress,
            MochtroidInstructionProgramDefinitions.ReadMechanicsWord,
            MochtroidInstructionProgramDefinitions.IsCompiledMechanicsByte);
        Check(0xb30000, [0x8efc, 0x8f10, 0x8f24, 0x8f38],
            index =>
            {
                var word = YellowPipeBugInstructionProgramDefinitions.MechanicsWord(index);
                return (word.Address, word.Value);
            }, YellowPipeBugInstructionProgramDefinitions.PresentationWordAddress,
            YellowPipeBugInstructionProgramDefinitions.ReadMechanicsWord,
            YellowPipeBugInstructionProgramDefinitions.IsCompiledMechanicsByte);

        void Check(int bank, ushort[] starts,
            Func<int, (ushort Address, ushort Value)> wordAt, Func<int, ushort> visualAt,
            Func<ushort, ushort> read, Func<int, bool> ownsByte)
        {
            var bytes = new HashSet<int>();
            int wordIndex = 0, visualIndex = 0;
            foreach (ushort start in starts)
            {
                foreach (int offset in new[] { 0, 4, 8, 12, 16, 18 })
                {
                    ushort address = (ushort)(start + offset);
                    ushort value = (ushort)(rom.ReadByte(bank | address) | rom.ReadByte(bank | (address + 1)) << 8);
                    AssertEqual((address, value), wordAt(wordIndex++), "stream 3 uniform loop native mechanic");
                    AssertEqual(value, read(address), "stream 3 uniform loop mechanic dispatch");
                    bytes.Add(address);
                    bytes.Add(address + 1);
                }
                for (int frame = 0; frame < 4; frame++)
                {
                    ushort address = (ushort)(start + 4 * frame + 2);
                    AssertEqual(address, visualAt(visualIndex++), "stream 3 uniform loop visual order");
                    AssertThrows<InvalidDataException>(() => read(address), "stream 3 uniform loop visual excluded");
                }
            }
            for (int address = 0; address <= ushort.MaxValue; address++)
                AssertEqual(bytes.Contains(address), ownsByte(bank | address), "stream 3 uniform loop byte ownership");
            foreach (int invalid in new[] { -1, starts.Length * 6, int.MaxValue })
                AssertThrows<IndexOutOfRangeException>(() => wordAt(invalid), "stream 3 uniform loop mechanic bounds");
            foreach (int invalid in new[] { -1, starts.Length * 4, int.MaxValue })
                AssertThrows<IndexOutOfRangeException>(() => visualAt(invalid), "stream 3 uniform loop visual bounds");
        }
    }

    private static void VerifyStream3RipperMappings(ISnesAddressSpace rom)
    {
        VerifyRipperInstructionProgramDefinitions();
        ushort[] programs = [0xe19b, 0xe1af, 0xe2e0, 0xe2f4, 0xe477, 0xe48b];
        var bytes = new HashSet<int>();
        int wordIndex = 0, visualIndex = 0;
        foreach (ushort start in programs)
        {
            foreach (int offset in new[] { 0, 4, 8, 12, 16, 18 })
            {
                ushort address = (ushort)(start + offset);
                ushort value = (ushort)(rom.ReadByte(0xa20000 | address) | rom.ReadByte(0xa20000 | (address + 1)) << 8);
                AssertEqual(new RipperInstructionMechanicsWord(address, value), RipperInstructionProgramDefinitions.MechanicsWord(wordIndex++),
                    "stream 3 Ripper mechanic order and value");
                bytes.Add(address);
                bytes.Add(address + 1);
            }
            for (int frame = 0; frame < 4; frame++)
            {
                ushort address = (ushort)(start + 4 * frame + 2);
                AssertEqual(address, RipperInstructionProgramDefinitions.PresentationWordAddress(visualIndex++),
                    "stream 3 Ripper visual operand order");
                if (start < 0xe477)
                {
                    ushort expected = (ushort)(rom.ReadByte(0xa20000 | address) | rom.ReadByte(0xa20000 | (address + 1)) << 8);
                    AssertEqual(expected, RipperVisualDefinitions.FrameAt(RoomEnemySystem.GRipperDefinition, address),
                        "stream 3 GRipper preserves shared operand domain");
                    AssertEqual(expected, RipperVisualDefinitions.FrameAt(RoomEnemySystem.Ripper2Definition, address),
                        "stream 3 Ripper II preserves shared operand domain");
                }
            }
        }
        for (int address = 0; address <= ushort.MaxValue; address++)
            AssertEqual(bytes.Contains(address), RipperInstructionProgramDefinitions.IsCompiledMechanicsByte(0xa20000 | address),
                "stream 3 Ripper byte ownership domain");
        foreach (int invalid in new[] { -1, 36, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => RipperInstructionProgramDefinitions.MechanicsWord(invalid),
                "stream 3 Ripper mechanic bounds");
        foreach (int invalid in new[] { -1, 24, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => RipperInstructionProgramDefinitions.PresentationWordAddress(invalid),
                "stream 3 Ripper visual bounds");
        foreach (ushort invalid in new ushort[] { 0xe19b, 0xe1ad, 0xe1bf, 0xe477, 0xffff })
            AssertThrows<InvalidDataException>(() => RipperVisualDefinitions.FrameAt(RoomEnemySystem.GRipperDefinition, invalid),
                "stream 3 Ripper rejects nonvisual and foreign-family operands");
        AssertThrows<InvalidDataException>(() => RipperVisualDefinitions.FrameAt(0, 0xe19d),
            "stream 3 Ripper rejects foreign enemy");
    }

    private static void VerifyStream3ChootControl(ISnesAddressSpace rom)
    {
        ushort Read(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (ushort pattern = 0; pattern < 5; pattern++)
        {
            ushort pointer = Read(0xa2df5e + 2 * pattern);
            ushort distancePointer = Read(0xa2df6a + 2 * pattern);
            AssertEqual(new ChootPatternDefinition(pointer, Read(0xa20000 | distancePointer)),
                ChootPatternDefinitions.ForIndex(pattern), "stream 3 Choot pattern identity and loop advance");
        }
        foreach (ushort invalid in new ushort[] { 5, 6, ushort.MaxValue })
            AssertThrows<InvalidDataException>(() => ChootPatternDefinitions.ForIndex(invalid),
                "stream 3 Choot rejects alias and invalid patterns");
        ushort[] mechanics = [0xd82c, 0xd82e, 0xd832, 0xd834, 0xd836, 0xd83a, 0xd83e, 0xd840, 0xd842, 0xd846, 0xd84a];
        ushort[] presentation = [0xd830, 0xd838, 0xd83c, 0xd844, 0xd848];
        AssertEqual(mechanics.Length, ChootInstructionProgramDefinitions.MechanicsWordCount, "stream 3 Choot mechanics count");
        AssertEqual(presentation.Length, ChootInstructionProgramDefinitions.PresentationWordCount, "stream 3 Choot visual count");
        for (int index = 0; index < mechanics.Length; index++)
        {
            ushort address = mechanics[index];
            ushort value = Read(0xa20000 | address);
            AssertEqual(new ChootInstructionMechanicsWord(address, value), ChootInstructionProgramDefinitions.MechanicsWord(index),
                "stream 3 Choot native control instruction");
            AssertEqual(value, ChootInstructionProgramDefinitions.ReadMechanicsWord(address), "stream 3 Choot control dispatch");
        }
        for (int index = 0; index < presentation.Length; index++)
        {
            ushort address = presentation[index];
            AssertEqual(address, ChootInstructionProgramDefinitions.PresentationWordAddress(index), "stream 3 Choot visual operand");
            AssertThrows<InvalidDataException>(() => ChootInstructionProgramDefinitions.ReadMechanicsWord(address),
                "stream 3 Choot visual operand remains excluded");
        }
        var bytes = mechanics.SelectMany(address => new[] { (int)address, address + 1 }).ToHashSet();
        var visualWords = presentation.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            AssertEqual(bytes.Contains(address), ChootInstructionProgramDefinitions.IsCompiledMechanicsByte(0xa20000 | address),
                "stream 3 Choot byte ownership");
            AssertEqual(visualWords.Contains((ushort)address), ChootInstructionProgramDefinitions.IsPresentationWord((ushort)address),
                "stream 3 Choot presentation ownership");
        }
        foreach (int invalid in new[] { -1, 11, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => ChootInstructionProgramDefinitions.MechanicsWord(invalid),
                "stream 3 Choot mechanics bounds");
        foreach (int invalid in new[] { -1, 5, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => ChootInstructionProgramDefinitions.PresentationWordAddress(invalid),
                "stream 3 Choot visual bounds");
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
        AssertTrue(typeof(WorkRobotPaletteCycle).GetField("frames",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(stock) is null,
            "stream 3 original Work Robot colors discard their stored frame table");
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
