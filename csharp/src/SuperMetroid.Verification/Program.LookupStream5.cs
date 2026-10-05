using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyLookupStream5StatueRamps(SuperMetroidAddressSpace rom)
    {
        byte[] nativeJson = SuperMetroid.AssetExtraction.TourianStatueColorExtractor.Extract(rom);
        var document = System.Text.Json.JsonSerializer.Deserialize<TourianStatueColorDocument>(nativeJson,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        var stock = Check(document);
        CheckResidualCount("baseColors", 0);
        CheckResidualCount("greyColors", 1);
        foreach (bool grey in new[] { false, true })
        {
            PaletteRgb5[] source = grey ? document.Grey : document.Base;
            for (int color = 0; color < source.Length; color++)
            for (int channel = 0; channel < 3; channel++)
            {
                var changed = (PaletteRgb5[])source.Clone();
                changed[color] = channel switch
                {
                    0 => changed[color] with { Red = changed[color].Red ^ 1 },
                    1 => changed[color] with { Green = changed[color].Green ^ 1 },
                    _ => changed[color] with { Blue = changed[color].Blue ^ 1 },
                };
                Check(grey ? document with { Grey = changed } : document with { Base = changed });
            }
        }
        AssertThrows<IndexOutOfRangeException>(() => stock.ResolveBase(-1), "Base ramp lower bound");
        AssertThrows<IndexOutOfRangeException>(() => stock.ResolveGrey(8), "Grey ramp upper bound");
        Console.WriteLine("Tourian statue ramps: all56 native palette colors,72 independent channel edits, actual CGRAM and canonical identities pass; one grey residual color and independent endpoints/outside colors remain pending.");

        void CheckResidualCount(string field, int expected)
        {
            object ramp = typeof(TourianStatueColorCatalog).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(stock)!;
            var edits = (Dictionary<int, ushort>)ramp.GetType().GetField("edits", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(ramp)!;
            AssertEqual(expected, edits.Count, "Exact stock ramp residual count");
        }

        static ushort Pack(PaletteRgb5 rgb) => (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);

        static TourianStatueColorCatalog Check(TourianStatueColorDocument expected)
        {
            var actual = TourianStatueColorCatalog.Load(new MemoryStream(TourianStatueColorCatalog.Write(expected), writable: false));
            var cgram = new SnesCgram();
            actual.ApplyEntrance(cgram);
            for (int color = 0; color < expected.Base.Length; color++)
            {
                AssertEqual(Pack(expected.Base[color]), actual.ResolveBase(color), "Exact independent base ramp color");
                AssertEqual(Pack(expected.Base[color]), cgram.Colors[TourianStatuePaletteRomData.BaseCgramIndex + color], "Actual base CGRAM");
                AssertEqual(Pack(expected.Statue[color]), cgram.Colors[TourianStatuePaletteRomData.StatueCgramIndex + color], "Unchanged statue CGRAM");
            }
            actual.ApplyGrey(cgram, 0);
            for (int color = 0; color < expected.Grey.Length; color++)
            {
                AssertEqual(Pack(expected.Grey[color]), actual.ResolveGrey(color), "Exact independent grey ramp color");
                AssertEqual(Pack(expected.Grey[color]), cgram.Colors[color], "Actual grey CGRAM");
            }
            for (int row = 0; row < expected.Eye.Length; row++)
            {
                actual.ApplyEye(cgram, (ushort)(row * 2));
                for (int color = 0; color < expected.Eye[row].Length; color++)
                    AssertEqual(Pack(expected.Eye[row][color]), cgram.Colors[TourianStatuePaletteRomData.EyeCgramIndex + color], "Unchanged eye CGRAM");
            }
            string identity = SelectedPresentationHash.Create("TourianStatueColorCatalog-v1", hash =>
            {
                hash.AppendWords("baseColors", expected.Base.Select(Pack).ToArray());
                hash.AppendWords("statueColors", expected.Statue.Select(Pack).ToArray());
                hash.AppendWords("greyColors", expected.Grey.Select(Pack).ToArray());
                hash.AppendWordFrames("eyeColors", expected.Eye.Select(row => row.Select(Pack).ToArray()).ToArray());
            });
            AssertEqual(identity, actual.ContentIdentity, "Exact canonical selected palette identity");
            return actual;
        }
    }

    private static void VerifyLookupStream5PhantoonMarkers(SuperMetroidAddressSpace rom)
    {
        AssertEqual(SuperMetroid.AssetExtraction.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes("Super Metroid.smc"))), "Marker reference ROM revision");
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        var body = enemies.Slots[0];
        var state = new PhantoonEnemyState(body)
        {
            Eye = enemies.Slots[1], Tentacles = enemies.Slots[2], Mouth = enemies.Slots[3],
        };
        var shot = typeof(RoomEnemySystem).GetMethod("ResolvePhantoonShotReaction", flags)!
            .CreateDelegate<Action<RoomEnemySlot, PhantoonEnemyState, ushort, ushort>>(enemies);
        ushort random = 0;
        int calls = 0;
        typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(enemies,
            (Func<ushort>)(() => { calls++; return random; }));
        for (int bucket = 0; bucket < 8; bucket++)
        {
            random = (ushort)(0xfff8 | bucket);
            body.Health = 1000;
            body.AiHandlerBits = 2;
            body.VariableF = (ushort)PhantoonAiFunction.EyeTracksSamus;
            body.VariableE = 60;
            state.Tentacles.VariableA = state.Tentacles.VariableB = 0;
            byte expected = rom.ReadByte(0xa7cda5 + bucket);
            AssertEqual(expected, PhantoonPatternDefinitions.ShotEyeMarkers[bucket], "Exact retained native RNG bucket");
            shot(body, state, 0x100, 1);
            AssertEqual((ushort)expected, state.Eye.VariableB, "Actual exposed eye marker");
            AssertEqual((ushort)bucket, state.Mouth.Parameter2, "Actual selected RNG pattern");
            AssertEqual((ushort)16, body.VariableE, "Actual shot window");
            AssertEqual(bucket + 1, calls, "One preserved RNG call per shot");
        }
        Console.WriteLine("Phantoon retained random markers: eight SHA-verified native bytes and eight actual bus-free shot writes preserve RNG, pattern and timing.");
    }


    private static void VerifyLookupStream5MagdollitePulse(SuperMetroidAddressSpace rom)
    {
        byte[] json = SuperMetroid.AssetExtraction.MagdollitePaletteCycleExtractor.Extract(rom);
        var native = System.Text.Json.JsonSerializer.Deserialize<MagdollitePaletteCycleDocument>(json,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        MagdollitePaletteCycle stock = Check(native);
        var residuals = (Dictionary<int, ushort>)typeof(MagdollitePaletteCycle)
            .GetField("edits", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(stock)!;
        AssertEqual(0, residuals.Count, "Native Magdollite pulse requires no stored frame residuals");
        for (int frame = 0; frame < native.Frames.Length; frame++)
        for (int color = 0; color < native.Frames[frame].Length; color++)
        for (int channel = 0; channel < 3; channel++)
        {
            var rows = native.Frames.Select(row => (PaletteRgb5[])row.Clone()).ToArray();
            PaletteRgb5 before = rows[frame][color];
            rows[frame][color] = channel switch
            {
                0 => before with { Red = before.Red ^ 1 },
                1 => before with { Green = before.Green ^ 1 },
                _ => before with { Blue = before.Blue ^ 1 },
            };
            Check(native with { Frames = rows });
        }
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ApplyFrame(new SnesCgram(), -1, 0),
            "Magdollite pulse lower frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ApplyFrame(new SnesCgram(), 4, 0),
            "Magdollite pulse upper frame bound");
        Console.WriteLine("Magdollite pulse:16 native colors,48 independent RGB edits, actual CGRAM, canonical identity and frame bounds pass; four independent glow colors remain pending.");

        static MagdollitePaletteCycle Check(MagdollitePaletteCycleDocument expected)
        {
            var actual = MagdollitePaletteCycle.Load(new MemoryStream(MagdollitePaletteCycle.Write(expected), writable: false));
            var cgram = new SnesCgram();
            ushort[][] packed = expected.Frames.Select(row => row.Select(rgb =>
                (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10)).ToArray()).ToArray();
            for (int frame = 0; frame < packed.Length; frame++)
            {
                actual.ApplyFrame(cgram, frame, 144);
                for (int color = 0; color < packed[frame].Length; color++)
                    AssertEqual(packed[frame][color],
                        cgram.Colors[144 + color],
                        "Magdollite pulse exact selected CGRAM color");
            }
            string identity = SelectedPresentationHash.Create("MagdollitePaletteCycle-v1",
                content => content.AppendWordFrames("frames", packed));
            AssertEqual(identity, actual.ContentIdentity, "Magdollite pulse canonical identity");
            return actual;
        }
    }
    private static void VerifyLookupStream5ZebetitePulse(SuperMetroidAddressSpace rom)
    {
        byte[] json = SuperMetroid.AssetExtraction.ZebetiteColorExtractor.Extract(rom);
        var native = System.Text.Json.JsonSerializer.Deserialize<ZebetiteColorDocument>(json,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        ZebetiteColorCatalog stock = Check(native);
        var residuals = (Dictionary<int, ushort>)typeof(ZebetiteColorCatalog)
            .GetField("edits", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(stock)!;
        AssertEqual(0, residuals.Count, "Native Zebetite pulse requires no stored frame residuals");
        for (int frame = 0; frame < native.Frames.Length; frame++)
        for (int color = 0; color < native.Frames[frame].Length; color++)
        for (int channel = 0; channel < 3; channel++)
        {
            var rows = native.Frames.Select(row => (PaletteRgb5[])row.Clone()).ToArray();
            PaletteRgb5 before = rows[frame][color];
            rows[frame][color] = channel switch
            {
                0 => before with { Red = before.Red ^ 1 },
                1 => before with { Green = before.Green ^ 1 },
                _ => before with { Blue = before.Blue ^ 1 },
            };
            Check(native with { Frames = rows });
        }
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Apply(new SnesCgram(), -1, 0),
            "Zebetite pulse lower frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Apply(new SnesCgram(), 8, 0),
            "Zebetite pulse upper frame bound");
        Console.WriteLine("Zebetite pulse:16 native colors,48 independent RGB edits, actual CGRAM, canonical identity and frame bounds pass; endpoint color choices remain pending.");

        static ZebetiteColorCatalog Check(ZebetiteColorDocument expected)
        {
            var actual = ZebetiteColorCatalog.Load(new MemoryStream(ZebetiteColorCatalog.Write(expected), writable: false));
            var cgram = new SnesCgram();
            ushort[][] packed = expected.Frames.Select(row => row.Select(rgb =>
                (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10)).ToArray()).ToArray();
            for (int frame = 0; frame < packed.Length; frame++)
            {
                actual.Apply(cgram, frame, ZebetiteDefinitions.PaletteDestinationColor);
                for (int color = 0; color < packed[frame].Length; color++)
                    AssertEqual(packed[frame][color],
                        cgram.Colors[ZebetiteDefinitions.PaletteDestinationColor + color],
                        "Zebetite pulse exact selected CGRAM color");
            }
            string identity = SelectedPresentationHash.Create("ZebetiteColorCatalog-v1",
                content => content.AppendWordFrames("frames", packed));
            AssertEqual(identity, actual.ContentIdentity, "Zebetite pulse canonical identity");
            return actual;
        }
    }

    private static void VerifyLookupStream5YappingMawOffsets(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
        var begin = typeof(RoomEnemySystem).GetMethod("BeginYappingMawExtension", flags)!
            .CreateDelegate<Action<RoomEnemySlot,YappingMawEnemyState>>();
        var set = typeof(RoomEnemySystem).GetMethod("SetYappingMawHeldOffset", flags)!
            .CreateDelegate<Action<YappingMawEnemyState,int>>();
        var slot = new RoomEnemySystem().Slots[0];
        var state = new YappingMawEnemyState(slot) { DistanceToSamus = 32 };
        for (int direction = 0; direction < 8; direction++)
        {
            var offset = YappingMawRomData.HeldSamusOffset(direction);
            AssertEqual(Word(0xa8a0a7 + direction * 4), unchecked((ushort)offset.X), "Native held X");
            AssertEqual(Word(0xa8a0a9 + direction * 4), unchecked((ushort)offset.Y), "Native held Y");
            set(state, direction);
            Check(direction, "Actual held-offset callback");
        }
        for (ushort angle = 0; angle < 256; angle++)
        {
            state.AimAngle = angle;
            begin(slot, state);
            int direction = ((angle + 16) & 255) >> 5;
            AssertEqual((ushort)(direction * 2), state.DirectionTableByteOffset,
                "Actual angle quantization/wrap retained");
            Check(direction, "Actual begin-extension held offset");
        }
        AssertThrows<IndexOutOfRangeException>(() => YappingMawRomData.HeldSamusOffset(-1), "Held direction lower bound");
        AssertThrows<IndexOutOfRangeException>(() => YappingMawRomData.HeldSamusOffset(8), "Held direction upper bound");
        Console.WriteLine("Yapping Maw offsets:16 native words, eight actual held callbacks,256 angle-quantized begin-extension writes and direction bounds pass.");

        void Check(int direction, string context)
        {
            AssertEqual(Word(0xa8a0a7 + direction * 4), state.HeldSamusXOffset, context);
            AssertEqual(Word(0xa8a0a9 + direction * 4), state.HeldSamusYOffset, context);
        }
    }
    private static void VerifyLookupStream5EyeGeometry(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int direction = 0; direction < 4; direction++)
        {
            var expected = (unchecked((short)ReadVerificationWord(rom, 0xa890ca + direction * 2)),
                unchecked((short)ReadVerificationWord(rom, 0xa890d2 + direction * 2)),
                ReadVerificationWord(rom, 0xa890da + direction * 2));
            AssertEqual(expected, MorphBallEyeGeometryDefinitions.Mount(direction), "Native directional mount tuple");
            foreach (ushort origin in new ushort[] { 0, 65535 })
            {
                var enemies = new RoomEnemySystem();
                var slot = enemies.Slots[0];
                slot.EnemyDefinitionPointer = RoomEnemySystem.MorphBallEyeDefinition;
                slot.Parameter2 = (ushort)(0x8000 | direction);
                slot.XPosition = origin; slot.YPosition = origin;
                typeof(RoomEnemySystem).GetMethod("InitializeMorphBallEye", flags)!
                    .CreateDelegate<Action<RoomEnemySlot>>(enemies)(slot);
                AssertEqual(unchecked((ushort)(origin + expected.Item1)), slot.XPosition, "Actual mount X with native wrap");
                AssertEqual(unchecked((ushort)(origin + expected.Item2)), slot.YPosition, "Actual mount Y with native wrap");
                AssertEqual(expected.Item3, slot.CurrentInstruction, "Actual directional mount program");
                AssertEqual(MorphBallEyeAiFunction.MountNoOp, enemies.MorphBallEyeStates[0]!.Function, "Mount remains decorative");
            }
        }
        var owner = new RoomEnemySystem();
        var body = owner.Slots[1];
        body.EnemyDefinitionPointer = RoomEnemySystem.MorphBallEyeDefinition;
        typeof(RoomEnemySystem).GetMethod("InitializeMorphBallEye", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(owner)(body);
        owner.MorphBallEyeStates[1]!.ActivatedFlag = 1;
        owner.MorphBallEyeBeam.BodySlotIndex = 1;
        owner.MorphBallEyeBeam.Phase = MorphBallEyeBeamPhase.Full;
        owner.MorphBallEyeBeam.ColorIndex = 0xfff0;
        var step = typeof(RoomEnemySystem).GetMethod("StepFullMorphBallEyeBeam", flags)!
            .CreateDelegate<Action>(owner);
        for (int phase = 0; phase < 16; phase++)
        {
            var expected = (rom.ReadByte(0x88ea8b + phase * 4), rom.ReadByte(0x88ea8c + phase * 4));
            AssertEqual(expected, MorphBallEyeGeometryDefinitions.BeamColor(phase), "Native two-channel triangle");
            step();
            AssertEqual(expected.Item1, owner.MorphBallEyeBeam.Red, "Actual raw red COLDATA");
            AssertEqual(expected.Item2, owner.MorphBallEyeBeam.Green, "Actual raw green COLDATA");
            AssertEqual(rom.ReadByte(0x88ea8d + phase * 4), owner.MorphBallEyeBeam.Blue, "Actual zero blue intensity");
            AssertEqual((ushort)((phase + 1) & 15), owner.MorphBallEyeBeam.ColorIndex, "Actual masked cycling and wrap");
        }
        AssertThrows<InvalidDataException>(() => MorphBallEyeGeometryDefinitions.Mount(4), "Unsupported mount direction");
        AssertThrows<ArgumentOutOfRangeException>(() => MorphBallEyeGeometryDefinitions.BeamColor(16), "Beam phase bound");
        Console.WriteLine("Morph Ball eye: four native mount tuples, eight actual wrapped initializers,16 native beam colors and actual full-beam cycle pass.");
    }

    private static void VerifyLookupStream5MapHighlight(SuperMetroidAddressSpace rom)
    {
        var frames = Enumerable.Range(0, 14).Select(frame => new MapPaletteCycleFrame
        {
            DurationTicks = rom.ReadByte(MapAnimationRomData.PaletteTiming + frame * 3),
            Colors = Enumerable.Range(0, 16).Select(color =>
            {
                ushort word = ReadVerificationWord(rom, MapAnimationRomData.PaletteColors + (frame * 16 + color) * 2);
                return new PaletteRgb5 { Red = word & 31, Green = (word >> 5) & 31, Blue = (word >> 10) & 31 };
            }).ToArray(),
        }).ToArray();
        var original = new MapPaletteCycleDocument { Version = 1, Frames = frames };
        MapPaletteCycle stock = Check(original);
        AssertEqual(0, ((Dictionary<int, byte>)typeof(MapPaletteCycle).GetField("durationEdits", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(stock)!).Count, "No stock duration lookup retained");
        AssertEqual(0, ((Dictionary<int, ushort>)typeof(MapPaletteCycle).GetField("reverseColorEdits", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(stock)!).Count, "No stock reverse-frame lookup retained");
        AssertEqual(8, ((ushort[][])typeof(MapPaletteCycle).GetField("colors", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(stock)!).Length, "Only independent color phases remain");
        for (int frame = 0; frame < 14; frame++)
        {
            AssertEqual((byte)Math.Min(frame, 14 - frame), rom.ReadByte(MapAnimationRomData.PaletteTiming + frame * 3 + 1), "Native highlight phase traversal");
            var durationEdit = (MapPaletteCycleFrame[])frames.Clone();
            durationEdit[frame] = frames[frame] with { DurationTicks = 254 };
            Check(original with { Frames = durationEdit });
            for (int color = 0; color < 16; color++)
            {
                var changed = (MapPaletteCycleFrame[])frames.Clone();
                var changedColors = (PaletteRgb5[])frames[frame].Colors.Clone();
                changedColors[color] = changedColors[color] with { Red = changedColors[color].Red ^ 1 };
                changed[frame] = frames[frame] with { Colors = changedColors };
                Check(original with { Frames = changed });
            }
        }
        foreach (int count in new[] { 1, 15, 255 })
            Check(original with { Frames = Enumerable.Range(0, count).Select(i => frames[i % 14]).ToArray() });
        AssertThrows<IndexOutOfRangeException>(() => stock.Duration(-1), "Highlight lower frame bound");
        AssertThrows<IndexOutOfRangeException>(() => stock.Apply(new SnesCgram(), 14, 0), "Highlight upper frame bound");
        Console.WriteLine("Map highlight: 224 native colors,14 timing/phase records, all independent color/hold edits and custom frame counts pass actual CGRAM application; eight color seed rows remain pending.");

        static MapPaletteCycle Check(MapPaletteCycleDocument document)
        {
            byte[] json = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
            var cycle = MapPaletteCycle.Load(new MemoryStream(json, writable: false));
            AssertEqual(document.Frames.Length, cycle.FrameCount, "Custom highlight frame count");
            var cgram = new SnesCgram();
            for (int frame = 0; frame < cycle.FrameCount; frame++)
            {
                AssertEqual((byte)document.Frames[frame].DurationTicks, cycle.Duration(frame), "Independent highlight hold");
                cycle.Apply(cgram, frame, MapAnimationRomData.PaletteDestination);
                for (int color = 0; color < 16; color++)
                {
                    var rgb = document.Frames[frame].Colors[color];
                    AssertEqual((ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10),
                        cgram.Colors[MapAnimationRomData.PaletteDestination + color], "Independent highlight CGRAM color");
                }
            }
            return cycle;
        }
    }

    private static void VerifyLookupStream5SidehopperGeometry(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        ushort Word(int address) => ReadVerificationWord(rom, address);
        foreach (ushort variant in new ushort[] { 0, 2, ushort.MaxValue })
        {
            bool alternate = variant != 0;
            var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
            var enemies = new RoomEnemySystem { TileArtwork = LookupStream5CorpseFixtureArtwork(rom) };
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, memory);
            var initialize = typeof(RoomEnemySystem).GetMethod("InitializeDeadSidehopperGraphics", flags)!
                .CreateDelegate<Action<ushort>>(enemies);
            byte[] expected = new byte[0x800];
            for (int row = 0; row < 5; row++)
            {
                int native = (alternate ? 0xa9df08 : 0xa9dec1) + 14 * row;
                int source = Word(native + 2) - 0xc000;
                int destination = Word(native + 5) - 0x2000;
                int length = Word(native + 8) + 1;
                AssertEqual((source, destination, length),
                    DeadMonsterRottingDefinitions.SidehopperInitialCopy(variant, row), "Native Sidehopper MVN geometry");
                for (int i = 0; i < length; i++) expected[destination + i] = rom.ReadByte(0xb7c000 + source + i);
            }
            initialize(variant);
            for (int i = 0; i < expected.Length; i++)
                AssertEqual(expected[i], memory.ReadByte(0x7e2000 + i), "Actual installed Sidehopper graphics copy");

            // Native LDA abs,X operands and optional CMP #8 operands, independently read from each unrolled column.
            int[] sourceOperands = alternate
                ? [0xa9e56d, 0xa9e587, 0xa9e5a6, 0xa9e5c5, 0xa9e5e4]
                : [0xa9e476, 0xa9e495, 0xa9e4af, 0xa9e4c9, 0xa9e4e3];
            int[] minimumOperands = alternate ? [0, 0, 0xa9e59c, 0xa9e5bb, 0xa9e5da]
                : [0xa9e46c, 0xa9e48b, 0, 0, 0];
            for (int column = 0; column < 5; column++)
            {
                AssertEqual((Word(sourceOperands[column]) - 0x2000) / 2,
                    DeadMonsterRottingDefinitions.SidehopperColumnWordOffset(variant, column), "Native tile column word offset");
                AssertEqual(minimumOperands[column] == 0 ? 0 : (int)Word(minimumOperands[column]),
                    DeadMonsterRottingDefinitions.SidehopperColumnMinimumY(variant, column), "Native missing top-row clipping");
            }
            var state = new DeadSidehopperEnemyState(enemies.Slots[0], variant, 0, 0, 0, 0, 0, 0xe240, 0, 0, 0, 0, 148);
            var copy = typeof(RoomEnemySystem).GetMethod("CopyOrMoveDeadSidehopperPixelRow", flags)!
                .CreateDelegate<Action<DeadSidehopperEnemyState, ushort, bool>>(enemies);
            foreach (ushort y in new ushort[] { 0, 7, 8, 37, 38 })
            foreach (bool move in new[] { false, true })
            {
                for (int i = 0; i < expected.Length; i++)
                {
                    expected[i] = (byte)(i * 37 + i / 256);
                    memory.WriteByte(0x7e2000 + i, expected[i]);
                }
                int sourceRow = Word(0xa9e240 + (y >> 3) * 2) + (y & 7) * 2;
                int destinationRow = sourceRow + ((y & 7) >= 6 ? 148 : 0);
                for (int column = 0; column < 5; column++)
                {
                    int minimum = minimumOperands[column] == 0 ? 0 : Word(minimumOperands[column]);
                    if (y < minimum) continue;
                    int columnBase = Word(sourceOperands[column]) - 0x2000;
                    foreach (int plane in new[] { 0, 16 })
                    {
                        int source = sourceRow + columnBase + plane;
                        int destination = destinationRow + columnBase + plane + 2;
                        if (y < 38)
                        {
                            expected[destination] = expected[source];
                            expected[destination + 1] = expected[source + 1];
                        }
                        if (move) { expected[source] = 0; expected[source + 1] = 0; }
                    }
                }
                copy(state, y, move);
                for (int i = 0; i < expected.Length; i++)
                    AssertEqual(expected[i], memory.ReadByte(0x7e2000 + i), "Actual Sidehopper pixel copy/move including clipping, wrap and terminal row");
            }
        }
        AssertThrows<ArgumentOutOfRangeException>(() => DeadMonsterRottingDefinitions.SidehopperInitialCopy(0, -1), "Copy row lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => DeadMonsterRottingDefinitions.SidehopperInitialCopy(0, 5), "Copy row upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => DeadMonsterRottingDefinitions.SidehopperColumnWordOffset(0, 5), "Column upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => DeadMonsterRottingDefinitions.SidehopperColumnMinimumY(0, -1), "Column lower bound");
        Console.WriteLine("Sidehopper geometry: ten native MVN rows, ten column offsets/clips, installed artwork and actual copy/move boundary rows pass; nonzero variant semantics preserved.");
    }

    private static EnemyTileArtworkCatalog LookupStream5CorpseFixtureArtwork(SuperMetroidAddressSpace rom)
    {
        byte[] planar = Enumerable.Range(0,DeadTourianCorpseArtworkDefinitions.ByteCount)
            .Select(index => rom.ReadByte(DeadTourianCorpseArtworkDefinitions.SourceAddress+index)).ToArray();
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar,4,RoomCharacterAtlasFormat.TileColumns,out int width,out int height);
        using var png = new MemoryStream();
        IndexedPng.Write(png,width,height,pixels,SnesGraphics.DiagnosticPalette(16));
        png.Position=0;
        RoomCharacterAtlas atlas = RoomCharacterAtlas.Load(png,planar.Length);
        return EnemyTileArtworkCatalog.FromArtworkForVerification(
            new Dictionary<ushort,RoomCharacterAtlas> { [RoomEnemySystem.DeadSidehopperDefinition] = atlas },
            new Dictionary<ushort,EnemyPaletteSheet>
            {
                [RoomEnemySystem.DeadSidehopperDefinition] = EnemyPaletteSheet.Load(new MemoryStream(
                    EnemyPaletteSheet.Write(new EnemyPaletteSheetDocument
                    {
                        Version=1,
                        Colors=Enumerable.Range(0,16).Select(_ => new PaletteRgb5 { Red=0,Green=0,Blue=0 }).ToArray(),
                    }))),
            },
            dmaSources: new Dictionary<ushort,int> { [RoomEnemySystem.DeadSidehopperDefinition] = DeadTourianCorpseArtworkDefinitions.SourceAddress });
    }
    private static void VerifyLookupStream5PhantoonRain(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (int i = 0; i < 8; i++)
        {
            var placement = PhantoonPatternDefinitions.RainPlacement(i);
            AssertEqual(Word(0xa7cdad + i * 8), placement.Cursor, "Native rain figure-eight cursor");
            AssertEqual(Word(0xa7cdaf + i * 8), placement.X, "Native rain body X");
            AssertEqual(Word(0xa7cdb1 + i * 8), placement.Y, "Native rain body Y");
            AssertEqual((ushort)0, Word(0xa7cdb3 + i * 8), "Native unused rain record word");
            AssertEqual(rom.ReadByte(0xa7cfc2 + i), PhantoonPatternDefinitions.FirstRainColumns[i], "Native first rain column");
        }
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static |
            BindingFlags.NonPublic;
        var busField = typeof(RoomEnemySystem).GetField("_bus", flags)!;
        var randomField = typeof(RoomEnemySystem).GetField("_nextRandom", flags)!;
        var rainMethod = typeof(RoomEnemySystem).GetMethod("RunPhantoonHiddenFlameRain", flags)!;
        for (int high = 0; high < 1; high++)
        for (int pattern = 0; pattern < 8; pattern++)
        {
            var enemies = new RoomEnemySystem();
            busField.SetValue(enemies, new PhantoonPatternReadGuard(rom));
            ushort random = (ushort)((high << 8) | 0xf8 | pattern);
            int calls = 0;
            randomField.SetValue(enemies, (Func<ushort>)(() => { calls++; return random; }));
            var body = enemies.Slots[0];
            var eye = enemies.Slots[1];
            var state = new PhantoonEnemyState(body) { Eye = eye };
            body.VariableE = 1;
            eye.VariableC = 123;
            rainMethod.CreateDelegate<Action<RoomEnemySlot, PhantoonEnemyState>>(enemies)(body, state);
            AssertEqual(Word(0xa7cdad + pattern * 8), body.VariableA, "Real rain cursor handoff");
            AssertEqual(Word(0xa7cdaf + pattern * 8), body.XPosition, "Real rain body placement X");
            AssertEqual(Word(0xa7cdb1 + pattern * 8), body.YPosition, "Real rain body placement Y");
            AssertEqual((ushort)0, eye.VariableC, "Real rain direction reset");
            AssertEqual((ushort)PhantoonAiFunction.BecomeSolidAfterFlameRain, body.VariableF, "Real rain phase handoff");
            AssertEqual(1, calls, "Rain consumes one RNG word");
            var flames = enemies.EnemyProjectiles.Where(p => p.IsActive).OrderBy(p => p.XVelocity).ToArray();
            AssertEqual(8, flames.Length, "Actual rain population");
            AssertTrue(flames.All(flame => flame.XPosition != body.XPosition), "Calculated rain gap lies at the body X");
            for (int i = 0; i < 8; i++)
            {
                int column = (rom.ReadByte(0xa7cfc2 + pattern) + i) % 9;
                AssertEqual((ushort)rom.ReadByte(0x8698f7 + column), flames[i].XPosition, "Actual rain column order with wrap");
                AssertEqual((ushort)40, flames[i].YPosition, "Actual rain ceiling Y");
                AssertEqual((ushort)((i + 1) * 8), flames[i].XVelocity, "Actual staggered rain delay");
            }
        }

        (short X, short Y, ushort Direction)[] eyeTargets =
        [
            (0, -100, 0), (100, -100, 1), (100, 0, 2), (100, 100, 3),
            (0, 100, 4), (-100, 100, 6), (-100, 0, 7), (-100, -100, 8),
        ];
        for (ushort direction = 0; direction < 9; direction++)
        {
            AssertEqual(Word(0xa7d40d + direction * 2),
                PhantoonPatternDefinitions.EyeInstruction(direction),
                $"native Phantoon eye direction {direction}");
        }
        var eyeEnemies = new RoomEnemySystem();
        busField.SetValue(eyeEnemies, new PhantoonPatternReadGuard(rom));
        var pointEye = typeof(RoomEnemySystem).GetMethod("PointPhantoonEyeAtSamus", flags)!
            .CreateDelegate<Action<RoomEnemySlot, RoomEnemySlot, SamusState>>(eyeEnemies);
        RoomEnemySlot eyeBody = eyeEnemies.Slots[0];
        RoomEnemySlot trackingEye = eyeEnemies.Slots[1];
        eyeBody.XPosition = 0x4000;
        eyeBody.YPosition = 0x4000;
        foreach ((short x, short y, ushort direction) in eyeTargets)
        {
            pointEye(eyeBody, trackingEye, new SamusState
            {
                XPosition = unchecked((ushort)(eyeBody.XPosition + x)),
                YPosition = unchecked((ushort)(eyeBody.YPosition + y)),
            });
            AssertEqual(PhantoonPatternDefinitions.EyeInstruction(direction),
                trackingEye.CurrentInstruction,
                $"production Phantoon eye direction {direction}");
        }
        AssertThrows<InvalidDataException>(
            () => PhantoonPatternDefinitions.EyeInstruction(9),
            "Phantoon eye direction beyond authored table");
        AssertThrows<IndexOutOfRangeException>(() => _ = PhantoonPatternDefinitions.FirstRainColumns[-1], "rain pattern lower bound");
        AssertThrows<IndexOutOfRangeException>(() => _ = PhantoonPatternDefinitions.FirstRainColumns[8], "rain pattern upper bound");
        Console.WriteLine("Stream 5 Phantoon: eight native rain columns, eight actual rain populations/gaps and nine eye selectors/eight actual octants pass with native tables forbidden; shot markers have a separately reviewed random-bucket exception.");
    }

    private static void VerifyLookupStream5CeresDoorRamp(SuperMetroidAddressSpace rom)
    {
        var document = new CeresDoorVisualDocument
        {
            Version = 1,
            Normal = Colors(CeresDoorVisualRomData.NormalColors,15),
            Escape = Colors(CeresDoorVisualRomData.EscapeColors,15),
            Animation = Enumerable.Range(0,8).Select(row => Colors(CeresDoorVisualRomData.AnimationColors +16*row,6)).ToArray(),
            Mode7DoorFrames = Enumerable.Range(0,2).Select(frame => Enumerable.Range(0,4)
                .Select(index => (int)rom.ReadByte(CeresDoorVisualRomData.Mode7FirstFrameSource +4*frame+index)).ToArray()).ToArray(),
        };
        byte[] planar = Enumerable.Range(0,CeresDoorVisualRomData.TileByteCount)
            .Select(index => rom.ReadByte(CeresDoorVisualRomData.TileSource+index)).ToArray();
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar,4,RoomCharacterAtlasFormat.TileColumns,out int width,out int height);
        using var png = new MemoryStream();
        IndexedPng.Write(png,width,height,pixels,SnesGraphics.DiagnosticPalette(16));
        byte[] pngBytes = png.ToArray();
        CeresDoorVisualCatalog Load(CeresDoorVisualDocument value) => CeresDoorVisualCatalog.Load(
            new MemoryStream(pngBytes),new MemoryStream(CeresDoorVisualCatalog.Write(value)));
        var stock = Load(document);
        Check(stock,document);
        const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        AssertEqual(6,((ushort[])typeof(CeresDoorVisualCatalog).GetField("animationSeeds",flags)!.GetValue(stock)!).Length,"Ceres animation retains six unresolved seeds");
        AssertEqual(4,((Dictionary<int,ushort>)typeof(CeresDoorVisualCatalog).GetField("animationPhaseResiduals",flags)!.GetValue(stock)!).Count,"Ceres animation retains four unresolved phase colors");
        AssertEqual(0,((Dictionary<int,ushort>)typeof(CeresDoorVisualCatalog).GetField("animationRowEdits",flags)!.GetValue(stock)!).Count,"stock reverse phases require no row storage");
        for (int edited = 0; edited <48; edited++)
        {
            var rows = document.Animation.Select(row => row.ToArray()).ToArray();
            var original = rows[edited/6][edited%6];
            rows[edited/6][edited%6] = new PaletteRgb5 { Red = original.Red ^1, Green = original.Green, Blue = original.Blue };
            var changed = document with { Animation = rows };
            Check(Load(changed),changed);
        }
        AssertThrows<IndexOutOfRangeException>(() => stock.LoadAnimationColors(new SnesCgram(),-1),"Ceres animation lower bound");
        AssertThrows<IndexOutOfRangeException>(() => stock.LoadAnimationColors(new SnesCgram(),8),"Ceres animation upper bound");
        Console.WriteLine("Stream 5 Ceres door ramp:48 native colors,48 independently edited cells, actual CGRAM, untouched setup colors and canonical identities pass; six seeds/four phase residuals remain pending.");

        PaletteRgb5[] Colors(int source,int count) => Enumerable.Range(0,count).Select(index =>
        {
            ushort word = ReadVerificationWord(rom,source+2*index);
            return new PaletteRgb5 { Red = word &31, Green = (word>>5)&31, Blue = (word>>10)&31 };
        }).ToArray();
        static ushort Pack(PaletteRgb5 color) => (ushort)(color.Red | color.Green<<5 | color.Blue<<10);
        void Check(CeresDoorVisualCatalog actual,CeresDoorVisualDocument expected)
        {
            var cgram = new SnesCgram();
            for(int row =0;row<8;row++)
            {
                actual.LoadAnimationColors(cgram,row);
                for(int color=0;color<6;color++) AssertEqual(Pack(expected.Animation[row][color]),cgram.Colors[CeresDoorVisualRomData.AnimationTargetColor+color],"Ceres independent animation color");
            }
            actual.LoadNormalColors(cgram,0);
            for(int color=0;color<15;color++) AssertEqual(Pack(expected.Normal[color]),cgram.Colors[color],"Ceres normal palette unchanged");
            actual.LoadEscapeColors(cgram,0);
            for(int color=0;color<15;color++) AssertEqual(Pack(expected.Escape[color]),cgram.Colors[color],"Ceres escape palette unchanged");
            string identity = SelectedPresentationHash.Create("enemy-ceres-door-v1",content =>
            {
                content.Append("tiles",planar);
                content.AppendWords("normal",expected.Normal.Select(Pack).ToArray());
                content.AppendWords("escape",expected.Escape.Select(Pack).ToArray());
                content.AppendWordFrames("animation",expected.Animation.Select(row=>row.Select(Pack).ToArray()).ToArray());
                content.Append("mode7-frames",expected.Mode7DoorFrames.Length);
                foreach(var frame in expected.Mode7DoorFrames) content.Append("mode7-frame",frame.Select(value=>(byte)value).ToArray());
            });
            AssertEqual(identity,actual.ContentIdentity,"Ceres calculated colors preserve canonical resource identity");
        }
    }
    private static void VerifyLookupStream5CorpseViews()
    {
        var expected = new List<DeadMonsterVramTransferDefinition>();
        foreach (ushort table in new ushort[] { 0xe0e0, 0xe10a, 0xe134, 0xe146, 0xe158, 0xe16a, 0xe17c, 0xe18e, 0xe1b0, 0xe1d2 })
        {
            var rows = DeadMonsterRottingDefinitions.ForTransferTable(table);
            int index = 0;
            foreach (var row in rows)
            {
                AssertEqual(rows[index++], row, "corpse DMA enumeration retains indexed order");
                expected.Add(row);
            }
            AssertEqual(rows.Length, index, "corpse DMA enumeration count");
            AssertThrows<IndexOutOfRangeException>(() => { _ = rows[-1]; }, "corpse DMA negative index");
            AssertThrows<IndexOutOfRangeException>(() => { _ = rows[rows.Length]; }, "corpse DMA past end");
        }
        AssertTrue(expected.SequenceEqual(DeadMonsterRottingDefinitions.AllTransfers), "audit DMA order matches all native lists");
        AssertThrows<InvalidDataException>(() => DeadMonsterRottingDefinitions.ForTransferTable(0xe0e1), "unaligned DMA list rejected");
        AssertThrows<InvalidDataException>(() => DeadMonsterRottingDefinitions.RotationOffset(0xe227, 0), "unknown corpse row layout rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => DeadMonsterRottingDefinitions.RotationOffset(0xe226, 104), "corpse row past end rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => DeadMonsterRottingDefinitions.SandSource(16), "sand source past end rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => DeadMonsterRottingDefinitions.SandDestination(16), "sand destination past end rejected");
    }
    private static void VerifyLookupStream5ActorLayouts(ISnesAddressSpace rom)
    {
        var flight = Enumerable.Range(0, 5).Select(index =>
        {
            var source = CeresFlightActorDefinitions.RearViewPlacementSource(index);
            return new CeresFlightActorPlacement { Id = source.Id,
                X = ReadVerificationWord(rom, 0x8b0000 | source.XAddress),
                Y = ReadVerificationWord(rom, 0x8b0000 | source.YAddress) };
        }).ToArray();
        Check(flight, values =>
        {
            using var encoded = new MemoryStream();
            CeresFlightActorLayout.Write(encoded, new() { Version = 1, Actors = values });
            encoded.Position = 0;
            var layout = CeresFlightActorLayout.Load(encoded);
            return (layout, index => layout[index], layout.ContentIdentity);
        }, value => (value.Id, value.X, value.Y),
            (value, x) => x ? value with { X = (value.X + 1) & 65535 } : value with { Y = (value.Y + 1) & 65535 });
        var reveal = Enumerable.Range(0, 6).Select(index =>
        {
            var source = CeresDestructionActorDefinitions.ZebesPlacementSource(index);
            return new CeresRevealActorPlacement { Id = source.Id,
                X = ReadVerificationWord(rom, 0x8b0000 | source.XAddress),
                Y = ReadVerificationWord(rom, 0x8b0000 | source.YAddress) };
        }).ToArray();
        Check(reveal, values =>
        {
            using var encoded = new MemoryStream();
            CeresRevealActorLayout.Write(encoded, new() { Version = 1, Actors = values });
            encoded.Position = 0;
            var layout = CeresRevealActorLayout.Load(encoded);
            return (layout, index => layout[index], layout.ContentIdentity);
        }, value => (value.Id, value.X, value.Y),
            (value, x) => x ? value with { X = (value.X + 1) & 65535 } : value with { Y = (value.Y + 1) & 65535 });
        var destruction = Enumerable.Range(0, 3).Select(index =>
        {
            var inherited = flight[index == 0 ? 0 : index == 1 ? 2 : 3];
            return new CeresDestructionActorPlacement { Id = CeresDestructionActorDefinitions.InitialPlacementId(index),
                X = index == 2 ? ReadVerificationWord(rom, 0x8bbfa6) : inherited.X, Y = inherited.Y };
        }).ToArray();
        Check(destruction, values =>
        {
            using var encoded = new MemoryStream();
            CeresDestructionActorLayout.Write(encoded, new() { Version = 1, Actors = values });
            encoded.Position = 0;
            var layout = CeresDestructionActorLayout.Load(encoded);
            return (layout, index => layout[index], layout.ContentIdentity);
        }, value => (value.Id, value.X, value.Y),
            (value, x) => x ? value with { X = (value.X + 1) & 65535 } : value with { Y = (value.Y + 1) & 65535 });
        Console.WriteLine("Ceres layouts: all14 original actor placements/28 coordinate operands, no stored stock rows, all28 independent coordinate edits, complete identity preservation and bounds pass.");

        static void Check<T>(T[] original,
            Func<T[], (object Layout, Func<int, T> Read, string Identity)> load,
            Func<T, (string Id, int X, int Y)> fields, Func<T, bool, T> edit)
        {
            var stock = load(original);
            var storage = stock.Layout.GetType().GetField("placements",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
            AssertTrue(storage.GetValue(stock.Layout) is null, "Ceres stock placements select semantic actor dispatch");
            Verify(stock, original);
            for (int index = 0; index < original.Length; index++)
            foreach (bool horizontal in new[] { false, true })
            {
                T[] changed = original.ToArray();
                changed[index] = edit(changed[index], horizontal);
                var installed = load(changed);
                AssertTrue(storage.GetValue(installed.Layout) is not null, "Independent Ceres coordinate edit remains supplied");
                Verify(installed, changed);
            }
            AssertThrows<IndexOutOfRangeException>(() => stock.Read(-1), "Ceres layout lower bound");
            AssertThrows<IndexOutOfRangeException>(() => stock.Read(original.Length), "Ceres layout upper bound");

            void Verify((object Layout, Func<int, T> Read, string Identity) actual, T[] expected)
            {
                for (int index = 0; index < expected.Length; index++)
                    AssertEqual(fields(expected[index]), fields(actual.Read(index)), "Ceres complete selected placement");
                string identity = SelectedPresentationHash.Create(actual.Layout.GetType().Name, content =>
                {
                    content.Append("actors", expected.Length);
                    foreach (T placement in expected)
                    {
                        var value = fields(placement);
                        content.Append("id", System.Text.Encoding.UTF8.GetBytes(value.Id));
                        content.Append("x", value.X); content.Append("y", value.Y);
                    }
                });
                AssertEqual(identity, actual.Identity, "Ceres canonical selected content identity");
            }
        }
    }
    private static void VerifyLookupStream5Initialization(ISnesAddressSpace rom)
    {
        for (ushort variant = 0; variant < 7; variant++)
        {
            var actual = CeresDoorInitializationDefinitions.For(variant);
            AssertEqual(ReadVerificationWord(rom, 0xa6f52c + 2 * variant), actual.InstructionList, "Ceres door original instruction dispatch");
            AssertEqual(ReadVerificationWord(rom, 0xa6f72b + 2 * variant), actual.MainFunction, "Ceres door original function dispatch");
        }
        for (ushort variant = 0; variant < 6; variant++)
        {
            var actual = CeresSteamDefinitions.Initialization((CeresSteamVariant)variant);
            AssertEqual(ReadVerificationWord(rom, 0xa6eff5 + 2 * variant), actual.InstructionList, "Ceres steam original instruction dispatch");
            AssertEqual(ReadVerificationWord(rom, 0xa6f001 + 2 * variant), (ushort)actual.Function, "Ceres steam original function dispatch");
        }
        for (int index = 0; index < 9; index++)
        {
            var actual = MagdollitePhaseDefinitions.Phase(index);
            AssertEqual(ReadVerificationWord(rom, 0xa8af55 + 2 * index), actual.DistanceThreshold, "Magdollite original rise threshold");
            AssertEqual(ReadVerificationWord(rom, 0xa8af67 + 2 * index), actual.BodyInstructionList, "Magdollite original body program");
            AssertEqual(ReadVerificationWord(rom, 0xa8af79 + 2 * index), actual.OverlayYOffset, "Magdollite original overlay offset");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => CeresDoorInitializationDefinitions.For(7), "Ceres door upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => CeresDoorInitializationDefinitions.For(ushort.MaxValue), "Ceres door full-word rejection");
        AssertThrows<InvalidDataException>(() => CeresSteamDefinitions.Initialization((CeresSteamVariant)6), "Ceres steam upper bound");
        AssertThrows<InvalidDataException>(() => CeresSteamDefinitions.Initialization((CeresSteamVariant)ushort.MaxValue), "Ceres steam full-word rejection");
        AssertThrows<InvalidDataException>(() => MagdollitePhaseDefinitions.Phase(-1), "Magdollite lower bound");
        AssertThrows<InvalidDataException>(() => MagdollitePhaseDefinitions.Phase(9), "Magdollite upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => { _ = CeresEscapeVramTransferDefinitions.All[-1]; }, "Ceres DMA list lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => { _ = CeresEscapeVramTransferDefinitions.All[19]; }, "Ceres DMA list upper bound");
        Console.WriteLine("Stream 5 initialization: all 53 native selector/phase words and rejected domains pass.");
    }
    private static void VerifyLookupStream5PaletteEntries(ISnesAddressSpace rom)
    {
        CheckEntries(CeresCinematicLightPaletteFxProgramMechanicsDefinitions.All,
            entry => (entry.DefinitionPointer, entry.ProgramStart), 3);
        CheckEntries(CinematicGlowPaletteFxProgramMechanicsDefinitions.All,
            entry => (entry.DefinitionPointer, entry.ProgramStart), 2);
        CheckEntries(TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.All,
            entry => (entry.DefinitionPointer, entry.ProgramStart), 2);
        CheckEntries(TourianStatueGreyPaletteFxProgramMechanicsDefinitions.All,
            entry => (entry.DefinitionPointer, entry.ProgramStart), 4);
        int lightWords = 0, glowWords = 0, redWords = 0, greyWords = 0;
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            ushort pointer = (ushort)address;
            if (CeresCinematicLightPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out ushort light))
            {
                AssertEqual(ReadVerificationWord(rom, 0x8d0000 | pointer), light, "Ceres light original mechanics");
                lightWords++;
            }
            if (CinematicGlowPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out ushort glow))
            {
                AssertEqual(ReadVerificationWord(rom, 0x8d0000 | pointer), glow, "Cinematic glow original mechanics");
                glowWords++;
            }
            if (TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out ushort red))
            {
                AssertEqual(ReadVerificationWord(rom, 0x8d0000 | pointer), red, "Tourian red-flash original mechanics");
                redWords++;
            }
            if (TourianStatueGreyPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out ushort grey))
            {
                AssertEqual(ReadVerificationWord(rom, 0x8d0000 | pointer), grey, "Tourian statue original mechanics");
                greyWords++;
            }
        }
        AssertEqual(44, lightWords, "Ceres light mechanics coverage");
        AssertEqual(64, glowWords, "Cinematic glow mechanics coverage");
        AssertEqual(50, redWords, "Tourian shared red mechanics coverage");
        AssertEqual(31, greyWords, "Tourian statue mechanics coverage");
        Console.WriteLine("Stream 5 palette entries: all eleven entry identities, original controls and collection bounds pass.");

        void CheckEntries<T>(IReadOnlyList<T> entries, Func<T, (ushort Definition, ushort Program)> project, int expectedCount)
        {
            AssertEqual(expectedCount, entries.Count, "Palette entry count");
            int index = 0;
            foreach (T entry in entries)
            {
                var identity = project(entry);
                AssertEqual(identity, project(entries[index]), "Palette entry enumeration order");
                AssertEqual(ReadVerificationWord(rom, 0x8d0000 | identity.Definition + 2), identity.Program, "Original palette definition list pointer");
                index++;
            }
            AssertEqual(expectedCount, index, "All palette entries enumerated");
            AssertThrows<ArgumentOutOfRangeException>(() => { _ = entries[-1]; }, "Palette entries lower bound");
            AssertThrows<ArgumentOutOfRangeException>(() => { _ = entries[expectedCount]; }, "Palette entries upper bound");
        }
    }}
