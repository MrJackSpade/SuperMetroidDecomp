using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyLookupStream1WorldBackground(ISnesAddressSpace rom)
    {
        byte[] Read(int address, int count) => Enumerable.Range(0, count).Select(i => rom.ReadByte(address + i)).ToArray();
        byte[] front = Read(WorldMapArtworkFormat.ForegroundSource, WorldMapArtworkFormat.ForegroundBytes);
        byte[] back = Read(WorldMapArtworkFormat.BackgroundSource, WorldMapArtworkFormat.BackgroundBytes);
        byte[] frontPixels = SnesGraphics.DecodePlanarTiles(front, 4, 16, out int width, out int frontHeight);
        byte[] pixels = SnesGraphics.DecodePlanarTiles(back, 2, 16, out _, out int backHeight);
        byte[] Png(byte[] selected, int height, int colors)
        {
            using var png = new MemoryStream(); IndexedPng.Write(png, width, height, selected, SnesGraphics.DiagnosticPalette(colors));
            return png.ToArray();
        }
        byte[] frontPng = Png(frontPixels, frontHeight, 16);
        WorldMapArtwork Create(byte[] selected) => WorldMapArtwork.Load(new MemoryStream(frontPng), new MemoryStream(Png(selected, backHeight, 4)));
        var field = typeof(WorldMapArtwork).GetField("background", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        var stock = Create(pixels);
        var stored = (Dictionary<int, byte>)field.GetValue(stock)!;
        var masks = (Dictionary<int, ulong>)typeof(WorldMapArtwork).GetField("digitFill",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(stock)!;
        AssertEqual(10, masks.Count, "Exactly ten selected digit footprints");
        ulong NativeMask(int tile)
        {
            ulong mask = 0;
            for (int y = 0; y < 8; y++) for (int x = 0; x < 8; x++)
                if (pixels[(tile / 16 * 8 + y) * width + tile % 16 * 8 + x] == 2) mask |= 1UL << (y * 8 + x);
            return mask;
        }
        foreach (var pair in masks) AssertEqual(NativeMask(pair.Key), pair.Value, "Exact native selected footprint bits");
        int calculated = 0;
        for (int index = 0; index < pixels.Length; index++)
        {
            int x = index % width, y = index / width;
            int tile = y / 8 * 16 + x / 8;
            bool available = tile is >= 0x3c and <= 0x45
                ? WorldMapTileDefinitions.TryOutlinedPixel(tile, x % 8, y % 8, NativeMask(tile), out byte value)
                : WorldMapTileDefinitions.TryBackgroundPixel(tile, x % 8, y % 8, out value);
            if (available) { AssertEqual(pixels[index], value, $"Native primitive tile{y / 8 * 16 + x / 8:X2} pixel{x % 8},{y % 8}"); calculated++; }
            AssertEqual(!available, stored.ContainsKey(index), "Exact font/icon basis, no stock primitive overrides");
        }
        AssertEqual(1725, stored.Count, "Exact residual contour pixels plus five selected font edges");
        void Check(WorldMapArtwork art, byte[] expected)
        {
            var vram = new SnesVram(); art.LoadTo(vram);
            AssertTrue(vram.Bytes.Slice(WorldMapArtworkFormat.ForegroundDestination, front.Length).SequenceEqual(front), "Foreground obligation remains exact and separate");
            AssertTrue(vram.Bytes.Slice(WorldMapArtworkFormat.BackgroundDestination, back.Length).SequenceEqual(
                SnesPlanarTileEncoder.Encode(expected, width, backHeight, 2)), "Actual complete BG3 upload preserves independent pixels");
        }
        Check(stock, pixels);
        for (int tile = 0; tile < 96; tile++)
        {
            byte[] edited = (byte[])pixels.Clone();
            int index = (tile / 16 * 8 + tile / 8 % 8) * width + tile % 16 * 8 + tile % 8;
            edited[index] ^= 3;
            Check(Create(edited), edited);
        }
        Check(Create(pixels.Select(p => (byte)(p ^ 3)).ToArray()), pixels.Select(p => (byte)(p ^ 3)).ToArray());
        byte[] invalid = (byte[])pixels.Clone(); invalid[0] = 4;
        AssertThrows<InvalidDataException>(() => WorldMapArtwork.Load(new MemoryStream(frontPng),
            new MemoryStream(Png(invalid, backHeight, 16))), "Invalid BG3 pen rejected during import, before upload");
        AssertThrows<ArgumentOutOfRangeException>(() => WorldMapTileDefinitions.TryBackgroundPixel(96, 0, 0, out _), "Background tile bound");
        Console.WriteLine($"World BG3:6144 native pixels,{calculated} geometry/mask defaults,{stored.Count} exact source pixels/10footprints,96 independent tile edits/all-pixel inversion and actual full uploads pass.");
    }

    private static void VerifyLookupStream1DeathPixels(ISnesAddressSpace rom)
    {
        var pages = SamusSpecialSequenceRomData.Death.TileSegments;
        int pageBytes = SamusSpecialSequenceRomData.Death.TileSegmentByteCount;
        byte[] native = Enumerable.Range(0, SamusDeathTileAtlasFormat.TotalByteCount)
            .Select(index => rom.ReadByte(pages[index / pageBytes].SourceAddress + index % pageBytes)).ToArray();
        SamusDeathTileAtlas Create(byte[] planar)
        {
            byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4, 8, out int width, out int height);
            using var png = new MemoryStream();
            IndexedPng.Write(png, width, height, pixels, SnesGraphics.DiagnosticPalette(16));
            png.Position = 0;
            return SamusDeathTileAtlas.Load(png);
        }
        var field = typeof(SamusDeathTileAtlas).GetField("sourceBytes",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        Dictionary<int, byte> Stored(SamusDeathTileAtlas art) => (Dictionary<int, byte>)field.GetValue(art)!;
        void Check(SamusDeathTileAtlas art, byte[] expected)
        {
            for (int page = 0; page < pages.Count; page++)
            {
                AssertTrue(art.TryResolve(pages[page].SourceAddress, pageBytes, out var bytes), "Native death page resolved");
                AssertTrue(bytes.Span.SequenceEqual(expected.AsSpan(page * pageBytes, pageBytes)), "Exact independent death page bytes");
            }
            AssertEqual(SelectedPresentationHash.Create(nameof(SamusDeathTileAtlas),
                content => content.Append("death characters", expected)), art.ContentIdentity, "Exact pre-conversion death hash");
        }
        var stock = Create(native);
        Check(stock, native);
        AssertEqual(151 * 32, Stored(stock).Count, "Exact151 source tiles, no stock padding/repetition overrides");
        var edits = new HashSet<int>();
        for (int index = 0; index < native.Length; index++)
        {
            int source = SamusDeathTileAtlasFormat.SourceByte(index);
            AssertEqual(native[index], source < 0 ? (byte)0 : native[source], "Direct native padding/patch relation");
            AssertEqual(source == index, Stored(stock).ContainsKey(index), "Exact retained source-byte membership");
            if (source != index) { edits.Add(index); if (source >= 0) edits.Add(source); }
        }
        foreach (int edit in edits)
        {
            byte[] changed = (byte[])native.Clone(); changed[edit] ^= 255;
            Check(Create(changed), changed);
        }
        for (int page = 0; page < pages.Count; page++)
        {
            var queue = new VramWriteQueue();
            queue.Enqueue((ushort)pageBytes, pages[page].SourceAddress, pages[page].EncodedVramDestination);
            var actual = new SnesVram(); var expected = new SnesVram();
            expected.ExecuteQueuedAssetWrite(native.AsSpan(page * pageBytes, pageBytes).ToArray(), pages[page].EncodedVramDestination);
            queue.DrainTo(actual, ReferenceMutableMemory.From(rom), new DeathTileAssetProvider(stock));
            AssertTrue(actual.Bytes.SequenceEqual(expected.Bytes), "Actual five-page NMI transport matches native");
        }
        AssertTrue(!stock.TryResolve(0, pageBytes, out _), "Unknown death source remains unresolved");
        AssertThrows<InvalidDataException>(() => stock.TryResolve(pages[0].SourceAddress, pageBytes - 1, out _), "Exact death page size required");
        Console.WriteLine($"Death atlas:5120 native bytes,151 exact source tiles,zero stock relation overrides,{edits.Count} independent source/derived-byte edits,canonical hashes and five actual uploads pass.");
    }

    private static void VerifyLookupStream1EscapeDachoraCadence(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        for (int index = 0; index < EscapeDachoraInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            var word = EscapeDachoraInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(Word(0xb30000 | word.Address), word.Value, "Every native escape Dachora control/value word");
        }
        foreach (int program in new[] { 0, 1, 2 })
        {
            var guard = new EscapeDachoraInstructionReadGuard(rom);
            var enemies = CreateEscapeDachoraProgramSystem(guard, flags);
            var slot = enemies.Slots[0];
            slot.CurrentInstruction = program == 0 ? (ushort)0xe964 : program == 1 ? (ushort)0xe9d0 : (ushort)0xea34;
            object?[] arguments = [slot, null, null, (ushort)0, (ushort)0, (ushort)0];
            int records = program == 2 ? 19 : 60, expectedX = slot.XPosition, calls = 0;
            for (int record = 0; record < records; record++)
            {
                int address;
                if (program == 2)
                {
                    address = record == 0 ? 0xea34 : 0xea38 + (record - 1) * 6;
                    if (record > 1) expectedX += Word(0xb3eadf);
                }
                else
                {
                    int direction = record / 30;
                    int start = program == 0 ? direction == 0 ? 0xe968 : 0xe99c : direction == 0 ? 0xe9d4 : 0xea04;
                    address = start + (record % 6) * 6;
                    if (record != 0) expectedX += (record <= 30 ? -1 : 1) * Word(0xb3eadf);
                }
                ushort duration = Word(0xb30000 | address), visual = Word(0xb30000 | (address + 2));
                AssertEqual(duration, EscapeDachoraInstructionProgramDefinitions.ReadMechanicsWord((ushort)address), "Native escape Dachora timing");
                for (int held = 0; held < duration; held++)
                {
                    process.Invoke(enemies, arguments); calls++;
                    AssertEqual((ushort)(duration - held), slot.InstructionTimer, "Every uninterrupted escape Dachora exposure tick");
                    AssertEqual(visual, slot.SpritemapPointer, "Native escape Dachora pose order");
                    AssertEqual((ushort)(address + 4), slot.CurrentInstruction, "Escape Dachora exact timed cursor");
                    AssertEqual(unchecked((ushort)expectedX), slot.XPosition, "Native callback displacement occurs only at pose completion");
                }
            }
            AssertEqual(program == 0 ? 180 : program == 1 ? 120 : 163, calls, "Exact full native pacing/departure duration");
            process.Invoke(enemies, arguments);
            expectedX += Word(0xb3eadf);
            AssertEqual(unchecked((ushort)expectedX), slot.XPosition, "Final movement callback is preserved");
            ushort next = program == 0 ? (ushort)0xe968 : program == 1 ? (ushort)0xe9d4 : (ushort)0xea80;
            AssertEqual((ushort)(next + 4), slot.CurrentInstruction, "Pacing reversal/maximum speed loop selects exact native record");
            AssertEqual(Word(0xb30000 | next), slot.InstructionTimer, "Loop reload keeps native cadence");
            AssertEqual(0, guard.ForbiddenReadAttempts, "Escape Dachora mechanics avoid cartridge reads");
        }
        Console.WriteLine("Escape Dachora cadence:139 native records,463 uninterrupted ticks,two complete pacing round trips and full accelerating departure pass.");
    }
    private static void VerifyLookupStream1PowampCadence(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        var align = typeof(RoomEnemySystem).GetMethod("AlignPowampBalloonY", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!;
        foreach (ushort root in new ushort[] { 0xc163, 0xc173, 0xc183, 0xc191 })
        {
            bool isBody = root < 0xc183;
            var guard = new PowampInstructionReadGuard(rom);
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            var initialize = typeof(RoomEnemySystem).GetMethod("InitializePowamp", flags)!.CreateDelegate<Action<RoomEnemySlot>>(enemies);
            var balloon = enemies.Slots[0];
            balloon.EnemyDefinitionPointer = EnemyDefinitionId.Powamp;
            balloon.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
            balloon.Parameter1 = 1;
            initialize(balloon);
            var body = enemies.Slots[1];
            body.EnemyDefinitionPointer = EnemyDefinitionId.Powamp;
            body.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
            initialize(body);
            body.YPosition = 100;
            var slot = isBody ? body : balloon;
            slot.CurrentInstruction = root;
            slot.InstructionTimer = 1;
            object?[] arguments = [slot, null, null, (ushort)0, (ushort)0, (ushort)0];
            int calls = 0;
            for (int pose = 0; pose < 3; pose++)
            {
                ushort address = (ushort)(root + pose * 4), duration = Word(0xa80000 | address);
                ushort sprite = Word(0xa80000 | (address + 2));
                AssertEqual(duration, PowampInstructionProgramDefinitions.ReadMechanicsWord(address), "Native Powamp duration");
                for (int held = 0; held < duration; held++)
                {
                    process.Invoke(enemies, arguments); calls++;
                    AssertEqual((ushort)(duration - held), slot.InstructionTimer, "Every uninterrupted Powamp hold tick");
                    AssertEqual(sprite, slot.SpritemapPointer, "Native Powamp held visual");
                    AssertEqual((ushort)(address + 4), slot.CurrentInstruction, "Powamp held cursor");
                    if (!isBody)
                    {
                        align.Invoke(null, [body, balloon]);
                        int nativeOffset = unchecked((short)Word(0xa80000 | ((root == 0xc183 ? 0xc277 : 0xc27d) + pose * 2)));
                        AssertEqual((ushort)(100 + nativeOffset), balloon.YPosition, "Native pose-driven balloon collision center");
                    }
                }
            }
            AssertEqual(root == 0xc163 ? 15 : root == 0xc173 ? 27 : 167, calls, "Native Powamp sequence duration");
            process.Invoke(enemies, arguments);
            if (isBody)
            {
                AssertEqual((ushort)(root + 4), slot.CurrentInstruction, "Actual Powamp Goto repeats first pose");
                AssertEqual(Word(0xa80000 | root), slot.InstructionTimer, "Powamp loop first hold reload");
            }
            else
            {
                AssertEqual((ushort)(root + 12), slot.CurrentInstruction, "Powamp Sleep parks at existing terminal cursor");
                AssertEqual((ushort)0, slot.InstructionTimer, "Powamp Sleep follows full terminal hold");
                ushort terminalY = balloon.YPosition;
                align.Invoke(null, [body, balloon]);
                AssertEqual(terminalY, balloon.YPosition, "Sleep preserves terminal balloon collision center");
                process.Invoke(enemies, arguments);
                AssertEqual(ushort.MaxValue, slot.InstructionTimer, "Sleeping Powamp retains native timer underflow");
            }
            AssertEqual(0, guard.ForbiddenReadAttempts, "Powamp timing uses no cartridge mechanics reads");
        }
        Console.WriteLine("Powamp cadence:12 native holds,376 uninterrupted exposure ticks,both Goto/Sleep boundaries and native balloon centers pass.");
    }
    private static void VerifyLookupStream1MetroidPulse(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        foreach (bool chasing in new[] { true, false })
        {
            ushort root = chasing ? (ushort)0xe9cf : (ushort)0xea25;
            int records = chasing ? 20 : 5, randomCalls = 0, calls = 0;
            var guard = new MetroidInstructionReadGuard(rom);
            var (enemies, slot) = NewMetroidInstructionSystem(guard, flags, root, () => { randomCalls++; return 5; });
            object?[] arguments = [slot, null, null, (ushort)0, (ushort)0, (ushort)0];
            for (int record = 0; record < records; record++)
            {
                ushort address = (ushort)(root + record * 4);
                ushort duration = Word(0xa30000 | address), visual = Word(0xa30000 | (address + 2));
                AssertEqual(duration, MetroidInstructionProgramDefinitions.ReadMechanicsWord(address), "Native Metroid pulse duration");
                process.Invoke(enemies, arguments); calls++;
                AssertEqual(duration, slot.InstructionTimer, "Actual Metroid duration reload");
                AssertEqual(visual, slot.SpritemapPointer, "Actual native inside-pose identity");
                AssertEqual((ushort)(address + 4), slot.CurrentInstruction, "Metroid selected record ordering");
                for (int held = 1; held < duration; held++)
                {
                    process.Invoke(enemies, arguments); calls++;
                    AssertEqual((ushort)(duration - held), slot.InstructionTimer, "Every actual Metroid exposure tick");
                    AssertEqual(visual, slot.SpritemapPointer, "Held Metroid inside pose stays selected");
                }
                AssertEqual(0, randomCalls, "No Metroid RNG call before complete pulse loop");
                AssertTrue(enemies.LastMetroidSoundEffectLibrary2 is null, "No Metroid callback before loop boundary");
            }
            AssertEqual(chasing ? 256 : 64, calls, "Exact native pulse-loop length");
            process.Invoke(enemies, arguments);
            AssertEqual(chasing ? 1 : 0, randomCalls, "Exact Metroid callback RNG ownership");
            ushort expectedSound = chasing ? Word(0xa3ead6 + 5 * 2) : Word(0xa3eaa8);
            AssertEqual((ushort?)expectedSound, enemies.LastMetroidSoundEffectLibrary2, "Native loop callback sound");
            AssertEqual((ushort)(root + 4), slot.CurrentInstruction, "Callback loops to first timed record");
            AssertEqual(Word(0xa30000 | root), slot.InstructionTimer, "Loop reload preserves first native hold");
            AssertEqual(0, guard.ForbiddenReadAttempts, "Metroid pulse mechanics use no cartridge reads");
        }
        Console.WriteLine("Metroid pulse:25 native records,320 actual exposure ticks,two callback boundaries and exact sound/RNG ownership pass.");
    }
    private static void VerifyLookupStream1AtmosphericCadence(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (byte type = 1; type <= 7; type++)
        {
            int pointer = 0x900000 | Word(0x908b93 + type * 2);
            byte count = (byte)Word(0x908bef + type * 2);
            AssertEqual(count, SamusAtmosphericAnimationDefinitions.FrameCount(type), "Named atmospheric frame domain matches native source");
            for (byte frame = 0; frame < count; frame++)
                AssertEqual(Word(pointer + frame * 2), SamusAtmosphericAnimationDefinitions.FrameTimer(type, frame), "Calculated atmospheric cadence preserves every native duration");
            AssertThrows<InvalidDataException>(() => SamusAtmosphericAnimationDefinitions.FrameTimer(type, count), "Atmospheric first out-of-range frame still rejects");
            AssertThrows<InvalidDataException>(() => SamusAtmosphericAnimationDefinitions.FrameTimer(type, byte.MaxValue), "Atmospheric byte-max frame still rejects");
        }
        foreach (byte invalid in new byte[] { 0, 8, byte.MaxValue })
        {
            AssertThrows<InvalidDataException>(() => SamusAtmosphericAnimationDefinitions.FrameCount(invalid), "Inactive/out-of-domain atmospheric type rejects");
            AssertThrows<InvalidDataException>(() => SamusAtmosphericAnimationDefinitions.FrameTimer(invalid, 0), "Invalid atmospheric type fails before frame selection");
        }
        Suite(nameof(VerifyProductionAtmosphericCadence), () => VerifyProductionAtmosphericCadence(new SamusAtmosphericAnimationReadGuard(rom)));
    }
    private static void VerifyLookupStream1TimerCadence(ISnesAddressSpace rom)
    {
        var failures = new List<string>();
        foreach (ushort frame in new ushort[] { 124, 125 })
        {
            var timer = new EscapeTimer();
            typeof(EscapeTimer).GetProperty(nameof(EscapeTimer.RawStatus))!.SetValue(timer, (ushort)(0x8000 | (ushort)EscapeTimerState.RunningInPlace));
            timer.SetTime(0, 1, 0x50);
            byte correction = rom.ReadByte(0x809eec + frame);
            byte expected = correction == 2 ? (byte)0x48 : (byte)0x49;
            bool expired = timer.Process(frame);
            if (timer.CentisecondsBcd != expected)
                failures.Add("NMI " + frame + ": native decrement" + correction + " expects$" + expected.ToString("X2") + ", actual$" + timer.CentisecondsBcd.ToString("X2"));
            AssertTrue(!expired && timer.SecondsBcd == 1 && timer.MinutesBcd == 0, "Exact cadence fixture preserves non-expiring whole time");
        }
        AssertTrue(failures.Count == 0, string.Join("; ", failures));
        int total = 0;
        for (int index = 0; index < 128; index++)
        {
            byte expected = rom.ReadByte(0x809eec + index);
            total += expected;
            AssertEqual(expected, EscapeTimerCadenceDefinitions.Centiseconds((ushort)index), "Every native rational cadence sample");
            AssertEqual(expected, EscapeTimerCadenceDefinitions.Centiseconds((ushort)(index + 128)), "Native128-frame wrap");
            AssertEqual(expected, EscapeTimerCadenceDefinitions.Centiseconds((ushort)(index + 0xff80)), "Native ignores high NMI bits");
            var timer = new EscapeTimer();
            typeof(EscapeTimer).GetProperty(nameof(EscapeTimer.RawStatus))!.SetValue(timer, (ushort)(0x8000 | (ushort)EscapeTimerState.RunningInPlace));
            timer.SetTime(0, 1, 0x50);
            AssertTrue(!timer.Process((ushort)index), "Calculated cadence stays non-expiring");
            AssertEqual(expected == 2 ? (byte)0x48 : (byte)0x49, timer.CentisecondsBcd, "Actual Process consumes each native correction");
        }
        AssertEqual(213, total, "Native full-period centisecond budget");
        foreach (ushort frame in new ushort[] { 124, 125 })
        {
            var timer = new EscapeTimer();
            typeof(EscapeTimer).GetProperty(nameof(EscapeTimer.RawStatus))!.SetValue(timer, (ushort)(0x8000 | (ushort)EscapeTimerState.RunningInPlace));
            timer.SetTime(0, 0, 2);
            bool expired = timer.Process(frame);
            AssertEqual(frame == 124, expired, "Corrected native expiration phase");
            AssertEqual(frame == 124 ? (byte)0 : (byte)1, timer.CentisecondsBcd, "Exact corrected expiration centisecond");
        }
        Console.WriteLine("Timer cadence:128 native corrections,384 phase observations,128 actual countdown updates and exact124/125 expiration pass.");
    }
    private static void VerifyLookupStream1TimerGlyphs(ISnesAddressSpace rom)
    {
        byte[] png = EscapeTimerTileAtlasExtractor.Extract(rom);
        var image = IndexedPng.Read(new MemoryStream(png), 200, 8);
        byte[] native = Enumerable.Range(0, 800).Select(index => rom.ReadByte(0xb0c000 + index)).ToArray();
        var stock = EscapeTimerTileAtlas.Load(new MemoryStream(png));
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        HashSet<int> Fill(EscapeTimerTileAtlas value) => (HashSet<int>)typeof(EscapeTimerTileAtlas).GetField("fillPixels", flags)!.GetValue(value)!;
        Dictionary<int, byte> Overrides(EscapeTimerTileAtlas value) => (Dictionary<int, byte>)typeof(EscapeTimerTileAtlas).GetField("pixelOverrides", flags)!.GetValue(value)!;
        Dictionary<int, bool> FillOverrides(EscapeTimerTileAtlas value) => (Dictionary<int, bool>)typeof(EscapeTimerTileAtlas).GetField("fillOverrides", flags)!.GetValue(value)!;
        bool DefaultFill(int pixel, out bool fill)
        {
            int tile = pixel % 200 / 8, x = pixel % 8, y = pixel / 200 + (tile >= 10 ? 8 : 0);
            if (tile >= 22)
            {
                int labelX = (tile - 22) * 8 + x, labelY = pixel / 200;
                fill = labelX is >= 0 and <= 5 && (labelY == 1 || labelY is >= 2 and <= 5 && labelX is 2 or 3) ||
                    labelX is 7 or 8 && labelY is >= 1 and <= 5 ||
                    labelX is >= 18 and <= 22 && (labelY is 1 or 5 ||
                        labelX is 18 or 19 && labelY is >= 2 and <= 4 || labelY == 3 && labelX <= 21);
                return labelX is <= 5 or 7 or 8 or >= 18 and <= 22;
            }
            fill = x is 3 or 4 && y is >= 1 and <= 13 || x == 2 && y is 2 or 3 || x is >= 2 and <= 5 && y is 12 or 13;
            if (tile is 0 or 10)
            {
                fill = x is >= 1 and <= 6 && y is >= 1 and <= 13 &&
                    !((x == 1 || x == 6) && (y == 1 || y == 13)) &&
                    !(x is >= 3 and <= 4 && y is >= 3 and <= 11);
            }
            if (tile == 8)
            {
                fill = x is >= 1 and <= 6 && y is >= 1 and <= 7 &&
                    !((x == 1 || x == 6) && (y == 1 || y == 7)) &&
                    !(x is >= 3 and <= 4 && y is >= 3 and <= 5);
            }
            if (tile is 2 or 12)
            {
                fill = y == 1 && x is >= 2 and <= 5 ||
                    y == 2 && x is >= 1 and <= 6 ||
                    y is 3 or 4 && x is 1 or 2 or 5 or 6 ||
                    y == 5 && x is 5 or 6 ||
                    y == 6 && x is >= 4 and <= 6 ||
                    y == 7 && x is >= 3 and <= 5 ||
                    y == 8 && x is 3 or 4 ||
                    y == 9 && x is >= 2 and <= 4 ||
                    y == 10 && x is 2 or 3 ||
                    y == 11 && x is >= 1 and <= 3 ||
                    y is 12 or 13 && x is >= 1 and <= 6;
            }
            if (tile is 7 or 17)
            {
                fill = y is 1 or 2 && x is >= 1 and <= 6 || y is >= 10 and <= 13 && x is 2 or 3;
                return y is < 3 or >= 10;
            }
            return tile is 0 or 1 or 2 or 8 or 10 or 11 or 12;
        }
        byte Ink(int pixel) => (byte)(pixel % 200 / 8 < 22 ? 1 : 2);
        int Source(int pixel)
        {
            int tile = pixel % 200 / 8, x = pixel % 8, y = pixel / 200;
            if (tile is 16 or 18) tile = 10;
            if (tile == 10 && y <= 6) { tile = 0; y = 6 - y; }
            if ((tile == 0 || tile == 8) && x >= 4) x = 7 - x;
            if (tile == 21) { tile = 20; if (x >= 4) x -= 3; }
            return y * 200 + tile * 8 + x;
        }
        byte Outline(byte[] pixels, int index)
        {
            int tile = index % 200 / 8, x = index % 8, y = index / 200;
            int gx = tile >= 22 ? (tile - 22) * 8 + x : x, gy = tile is >= 10 and < 20 ? y + 8 : y;
            for (int ny = gy - 1; ny <= gy + 1; ny++)
            for (int nx = gx - 1; nx <= gx + 1; nx++)
            {
                if (nx < 0 || nx >= (tile >= 22 ? 24 : 8) || ny < 0 || ny >= (tile < 20 ? 16 : 8)) continue;
                int selectedTile = tile < 20 ? tile % 10 + ny / 8 * 10 : tile >= 22 ? 22 + nx / 8 : tile;
                int neighbor = ny % 8 * 200 + selectedTile * 8 + nx % 8;
                if (pixels[neighbor] == Ink(neighbor)) return 14;
            }
            return 0;
        }
        byte[] Transfer(EscapeTimerTileAtlas value) => value.Resolve(VramAssetId.EscapeTimerFirstTiles).ToArray()
            .Concat(value.Resolve(VramAssetId.EscapeTimerSecondTiles).ToArray()).ToArray();
        AssertTrue(Transfer(stock).SequenceEqual(native), "Both reconstructed timer pages match all800 native planar bytes");
        AssertEqual(275, Fill(stock).Count, "Exact retained timer fill footprint after calculated digit and label shapes");
        AssertEqual(0, FillOverrides(stock).Count, "Zero stock calculated-footprint overrides");
        AssertTrue(Overrides(stock).Keys.ToHashSet().SetEquals(new[] { 1161, 1163, 189, 999 }), "Exact four still-retained native deviations");
        for (int index = 0; index < 1600; index++)
        {
            bool fill = image.Pixels[index] == Ink(index);
            bool calculated = DefaultFill(index, out bool expectedFill);
            AssertEqual(calculated, EscapeTimerGlyphDefinitions.TryDefaultFill(index, out bool productionFill), "Exact digit/T/I/E footprint domain");
            if (calculated) { AssertEqual(fill, expectedFill, "Direct native three-part digit/T/I/E footprint"); AssertEqual(fill, productionFill, "Direct calculated digit/T/I/E footprint"); }
            AssertEqual(Source(index) == index && !calculated && fill, Fill(stock).Contains(index), "Exact retained stock footprint membership");
            AssertEqual(Source(index), EscapeTimerGlyphDefinitions.SourcePixel(index), "Exact native shared lower-half pixel domain");
            if (Source(index) != index) AssertEqual(image.Pixels[index], image.Pixels[Source(index)], "Direct native repeated lower-half pixel");
            else if (!fill && !Overrides(stock).ContainsKey(index))
                AssertEqual(image.Pixels[index], EscapeTimerGlyphDefinitions.Outline(index, pixel => image.Pixels[pixel] == Ink(pixel)), "Direct native calculated outline/transparency");
            byte[] editedPixels = (byte[])image.Pixels.Clone(); editedPixels[index] ^= 15;
            using var stream = new MemoryStream();
            IndexedPng.Write(stream, 200, 8, editedPixels, image.Palette); stream.Position = 0;
            var edited = EscapeTimerTileAtlas.Load(stream);
            for (int other = 0; other < 1600; other++)
            {
                bool selectedFill = editedPixels[other] == Ink(other);
                bool calculatedFill = DefaultFill(other, out bool defaultFill);
                AssertEqual(Source(other) == other && !calculatedFill && selectedFill, Fill(edited).Contains(other), "Every independently supplied retained footprint edit preserved");
                AssertEqual(Source(other) == other && calculatedFill && selectedFill != defaultFill, FillOverrides(edited).ContainsKey(other), "Exact independently supplied calculated-footprint override membership");
                bool expectedOverride = Source(other) != other ? editedPixels[other] != editedPixels[Source(other)] : !selectedFill && editedPixels[other] != Outline(editedPixels, other);
                AssertEqual(expectedOverride, Overrides(edited).ContainsKey(other), "Exact independent outline/shared-pixel deviation membership");
            }
            byte[] expected = (byte[])native.Clone();
            int tile = index % 200 / 8, x = index % 8, y = index / 200;
            for (int plane = 0; plane < 4; plane++) expected[tile * 32 + plane / 2 * 16 + y * 2 + plane % 2] ^= (byte)(1 << (7 - x));
            AssertTrue(Transfer(edited).SequenceEqual(expected), "Every PNG pixel edit changes exactly its four native planar bits");
        }
        var queue = new VramWriteQueue(); stock.QueueTo(queue);
        AssertEqual(2, queue.Entries.Count, "Original two-page queue count");
        AssertEqual((ushort)512, queue.Entries[0].SizeInBytes, "First page size");
        AssertEqual((ushort)0x7e00, queue.Entries[0].EncodedVramDestination, "First page destination");
        AssertEqual(VramAssetId.EscapeTimerFirstTiles, queue.Entries[0].AssetId, "First typed page identity");
        AssertEqual((ushort)288, queue.Entries[1].SizeInBytes, "Second page size");
        AssertEqual((ushort)0x7f00, queue.Entries[1].EncodedVramDestination, "Second page destination");
        AssertEqual(VramAssetId.EscapeTimerSecondTiles, queue.Entries[1].AssetId, "Second typed page identity");
        var nativeQueue = new VramWriteQueue();
        AssertTrue(stock.TryQueueNativeTransfer(nativeQueue, 0xb0c000, 512, 0x7e00) && stock.TryQueueNativeTransfer(nativeQueue, 0xb0c200, 288, 0x7f00), "Native queue descriptors accepted");
        AssertTrue(nativeQueue.Entries.SequenceEqual(queue.Entries), "Native and typed queues preserve exact order and descriptors");
        var vram = new SnesVram();
        AssertTrue(stock.TryLoadNativeTransfer(vram, 0xb0c000, 512, 0x7e00) && stock.TryLoadNativeTransfer(vram, 0xb0c200, 288, 0x7f00), "Both synchronous native uploads accepted");
        AssertTrue(vram.Bytes.Slice(0xfc00, 800).SequenceEqual(native), "Actual two-page VRAM upload matches native bytes");
        AssertTrue(stock.TryResolve(0xb0c000, 512, out var first) && first.Span.SequenceEqual(native.AsSpan(0, 512)), "First restored native page resolution");
        AssertTrue(stock.TryResolve(0xb0c200, 288, out var second) && second.Span.SequenceEqual(native.AsSpan(512)), "Second restored native page resolution");
        AssertTrue(!stock.TryResolve(0xb0c000, 511, out _), "Mismatched native page rejected");
        AssertTrue(!stock.TryLoadNativeTransfer(vram, 0xb0c200, 288, 0x7e00), "Mismatched synchronous destination rejected");
        AssertTrue(Transfer(stock).SequenceEqual(native), "Stock timer remains immutable");
        Console.WriteLine("Timer font:836 direct outline pixels,312 shared/reflected pixels,275 retained fill positions/four deviations,173 calculated digit/T/I/E basis fill sites,1600 independent PNG edits,800 native planar bytes and exact two-page queue/VRAM uploads pass.");
    }
    private static void VerifyLookupStream1XrayBodyFrames(ISnesAddressSpace rom, bool basisOnly = false, bool finalOnly = false)
    {
        int count = SamusBodyArtworkCatalog.FrameCount * 4;
        byte[] native = Enumerable.Range(0, count).Select(index => rom.ReadByte(0x92db48 + index)).ToArray();
        ushort[] Words(int address, int length) => Enumerable.Range(0, length).Select(index =>
            (ushort)(rom.ReadByte(address + index * 2) | rom.ReadByte(address + index * 2 + 1) << 8)).ToArray();
        ushort[] top = Words(0x92d91e, 13), bottom = Words(0x92d938, 11), poses = Words(0x92d94e, 253);
        ushort[] starts = top.Concat(bottom).Order().ToArray();
        SamusBodyTileDefinition[][] Groups(ushort[] pointers) => pointers.Select(pointer =>
        {
            int next = Array.IndexOf(starts, pointer) + 1;
            int end = next == starts.Length ? 0xd7d3 : starts[next];
            return Enumerable.Range(0, (end - pointer) / 7).Select(_ => new SamusBodyTileDefinition(0x9a8000, 32, 0, new byte[32])).ToArray();
        }).ToArray();
        var upper = Groups(top); var lower = Groups(bottom);
        var template = CreateSamusIdentityFixture();
        SamusBodyFrameSelection[] Decode(byte[] data) => Enumerable.Range(0, count / 4).Select(index =>
            new SamusBodyFrameSelection(data[index * 4], data[index * 4 + 1], data[index * 4 + 2], data[index * 4 + 3])).ToArray();
        SamusBodyArtworkCatalog Create(byte[] data) => new(top, bottom, poses, template.GraphicsYOffsets.ToArray(), Decode(data), upper, lower,
            template.Spritemaps, template.Atmosphere, template.DeathPalettes, template.DeathTiles, template.ArmCannon,
            template.LandingYOffsets.ToArray(), template.PostureYOffsets.ToArray(), template.DrainedYOffsets.ToArray());
        int SourceStep(int index)
        {
            int address = 0xdb48 + index;
            if (address is >= 0xe888 and < 0xe890) return index - 8;
            if (address is >= 0xe298 and < 0xe2b8) return index - 36;
            if (address is >= 0xe2b8 and < 0xe374)
            {
                int offset = address - 0xe2b8, phase = offset / 4, component = offset % 4, basis = phase;
                if (component < 2 && phase is >= 13 and <= 20) basis = 13;
                if (phase >= 23)
                {
                    int relative = phase - 23;
                    basis = component >= 2 ? 23 + relative % 8 : relative / 3 % 2 == 1 ? 13 : 23 + relative / 3 * 3;
                }
                if (basis != phase) return index - (phase - basis) * 4;
            }
            if (address is >= 0xe050 and < 0xe260)
            {
                int start = address < 0xe158 ? 0xe050 : 0xe158, offset = address - start, phase = offset / 4;
                if (offset % 4 >= 2 && phase < 64)
                {
                    int within = phase % 16, basis = within switch { 4 => 3, 6 => 5, 8 or 9 => 7, 11 => 10, 13 => 12, _ => within };
                    return start - 0xdb48 + (phase - within + basis) * 4 + offset % 4;
                }
            }
            if (address >= 0xdf28 && address < 0xe018 && address % 4 < 2)
            {
                int offset = (address - 0xdf28) % 40, phase = offset / 4 % 5;
                int basis = phase is 0 or 1 ? 0 : phase == 4 ? 2 : phase;
                return address - offset - 0xdb48 + basis * 4 + offset % 4;
            }
            if (address is >= 0xdce8 and < 0xdd18 or >= 0xe450 and < 0xe4b0)
            {
                int range = address < 0xdd18 ? 0xdce8 : 0xe450, offset = (address - range) % 24;
                if (offset % 4 < 2) return address - offset - 0xdb48 + (offset / 4 % 3 == 0 ? 0 : 1) * 4 + offset % 4;
                if (range == 0xe450) return 0xdd00 - 0xdb48 + offset;
            }
            if (address is >= 0xe890 and < 0xe908)
            {
                int offset = (address - 0xe890) % 60, phase = offset / 4, component = offset % 4;
                int basis = phase switch { 5 or 10 or 11 => 4, 6 => 2, 9 => 7, 12 => 1, _ => phase };
                if (component >= 2 && (phase == 3 || phase is >= 6 and <= 9)) basis = 2;
                if (phase == 13) basis = component < 2 ? 1 : 0;
                if (basis != phase) return address - offset - 0xdb48 + basis * 4 + component;
            }
            if (address is >= 0xe938 and < 0xe9f4)
            {
                bool left = address >= 0xe974;
                int start = left ? 0xe974 : 0xe938, offset = address - start, phase = offset / 4, component = offset % 4;
                int basis = left ? phase switch
                {
                    >= 3 and <= 6 => 2, 11 => 9, 13 or 17 or 18 or 24 or 25 or 27 or 28 or 30 or 31 => 12,
                    >= 19 and <= 23 => 14 + Math.Min(phase - 19, 23 - phase), 26 or 29 => 8, _ => phase
                } : phase switch { >= 4 and <= 7 => 3, 11 => 9, 13 => 12, _ => phase };
                if (component >= 2 && phase is >= 9 and <= 11) basis = 8;
                if (basis != phase) return start - 0xdb48 + basis * 4 + component;
            }
            if (address >= 0xe9f4 && address < 0xea24 && (address - 0xe9f4) % 24 < 16)
            {
                int offset = (address - 0xe9f4) % 24, phase = offset / 4;
                return address - offset - 0xdb48 + (offset % 4 >= 2 ? 0 : phase == 3 ? 1 : phase) * 4 + offset % 4;
            }
            if (address is >= 0xe050 and < 0xe260)
            {
                int start = address < 0xe158 ? 0xe050 : 0xe158, offset = address - start, phase = offset / 4;
                if (offset % 4 < 2 && phase >= 32)
                    return start - 0xdb48 + (phase < 64 ? phase % 32 : 16) * 4 + offset % 4;
            }
            if (address is >= 0xe798 and < 0xe828)
            {
                int pair = (address - 0xe798) / 24, offset = (address - 0xe798) % 24;
                if (offset >= 12) return 0xe798 + pair * 24 + (2 - (offset - 12) / 4) * 4 + offset % 4 - 0xdb48;
                if (pair is 1 or 2 && offset % 4 >= 2) return 0xe798 - 0xdb48 + offset;
                if (pair >= 3 && offset % 4 < 2) return 0xe798 + (pair - 3) * 24 - 0xdb48 + offset;
                if (pair >= 4 && offset % 4 >= 2) return 0xe7e0 - 0xdb48 + offset;
            }
            if (address >= 0xea24)
            {
                bool suited = address >= 0xeba4;
                int start = suited ? 0xeba4 : 0xea24, offset = address - start, phase = offset / 4, component = offset % 4;
                if (suited && phase >= 1 && component < 2) return 0xea24 - 0xdb48 + offset;
                if (phase >= 2 && component >= 2) return start - 0xdb48 + 8 + component;
                if (suited && phase == 1) return 0xea24 - 0xdb48 + offset;
                if (!suited && phase >= 3 && component < 2)
                {
                    int basis = phase % 2 == 1 ? 3 : phase < 80 ? 2 + (phase - 2) % 6 : phase >= 90 ? phase - 6 : phase;
                    return 0xea24 - 0xdb48 + basis * 4 + component;
                }
            }
            if (address is >= 0xe37c and < 0xe430) return index - 188;
            if (address is >= 0xe530 and < 0xe5f8)
            {
                int list = (address - 0xe508) / 40, offset = (address - 0xe508) % 40;
                int phase = offset / 4;
                return 0xe508 - 0xdb48 + ((list % 2 == 1 && phase < 8) ? 7 - phase : phase) * 4 + offset % 4;
            }
            if (address is >= 0xe4c8 and < 0xe4d8)
            {
                int offset = (address - 0xe4c8) % 8;
                return 0xe4b8 - 0xdb48 + (address - 0xe4c8) / 8 * 8 + (1 - offset / 4) * 4 + offset % 4;
            }
            if (address is >= 0xe4f0 and < 0xe508)
            {
                int offset = (address - 0xe4f0) % 12;
                return 0xe4d8 - 0xdb48 + (address - 0xe4f0) / 12 * 12 + (2 - offset / 4) * 4 + offset % 4;
            }
            if (address is >= 0xe5f8 and < 0xe798)
            {
                int list = address < 0xe6b8 ? (address - 0xe5f8) / 48 : 4 + (address - 0xe6b8) / 112;
                int offset = address < 0xe6b8 ? (address - 0xe5f8) % 48 : (address - 0xe6b8) % 112;
                int phase = offset / 4, wall = list % 2 == 0 ? 0xe2b8 : 0xe374;
                int last = list < 4 ? 11 : 27;
                if (phase == last)
                {
                    if (list >= 2) return 0xe5f8 + list % 2 * 48 + 44 - 0xdb48 + offset % 4;
                }
                else
                {
                    int wallPhase = phase == 0 ? 1 : list < 2 ? phase + 2 : list < 4 ? phase + 12 : phase < 25 ? phase + 22 : phase - 4;
                    return wall - 0xdb48 + wallPhase * 4 + offset % 4;
                }
            }
            if (address is >= 0xdb48 and < 0xdb90)
            {
                int offset = (address - 0xdb48) % 36;
                int phase = offset / 4;
                int first = address - offset;
                int basis = phase switch { 3 or 6 or 8 => 1, 5 => 0, 7 when offset % 4 >= 2 => 2, _ => phase };
                return first - 0xdb48 + basis * 4 + offset % 4;
            }
            if (address >= 0xdba0 && address < 0xdbb8 && address % 4 < 2)
            {
                int side = (address - 0xdba0) / 12, phase = (address - 0xdba0) % 12 / 4;
                return 0xdb90 + side * 8 - 0xdb48 + (1 - phase % 2) * 4 + address % 4;
            }
            if (address >= 0xdbb8 && address < 0xdbf8 && address % 4 >= 2)
                return 0xdb90 + (address - 0xdbb8) / 8 % 2 * 8 - 0xdb48 + (address - 0xdbb8) % 8;
            if (address >= 0xdbf8 && address < 0xdc48 && address % 4 >= 2)
                return 0xdbf8 + (address - 0xdbf8) / 20 * 20 - 0xdb48 + address % 4;
            if (address >= 0xdc20 && address < 0xdc48 && (address - 0xdc20) % 4 < 2) return index - 40;
            if (address >= 0xdc70 && address < 0xdce8 && (address - 0xdc48) % 4 >= 2) return 0xdc48 - 0xdb48 + (address - 0xdc48) % 40;
            if (address >= 0xdf28 && address < 0xe018 && (address - 0xdf28) % 4 >= 2) return 0xdc48 - 0xdb48 + (address - 0xdf28) % 40;
            if (address >= 0xde18 && address < 0xde60 && (address - 0xde18) % 4 < 2) return address - 0xde18;
            if (address is >= 0xdd48 and < 0xdd58) return 0xdd28 - 0xdb48 + address - 0xdd48;
            if (address is >= 0xdeb0 and < 0xdec0) return 0xdd18 - 0xdb48 + address - 0xdeb0;
            return index;
        }
        int Source(int index)
        {
            int source = SourceStep(index);
            while (source != index) { index = source; source = SourceStep(index); }
            return source;
        }
        bool Direct(int index)
        {
            int address = 0xdb48 + index;
            int record = address & ~3;
            if (record is 0xdb58 or 0xdb7c or 0xde28 or 0xde4c or 0xe2c0 or 0xe2e4 or 0xe2e8 or 0xe30c or 0xe310 or
                0xe8a0 or 0xe8dc or 0xe968 or 0xe9a4 or 0xea04 or 0xea1c or 0xe528 or 0xea28 or 0xeba8) return true;            return address % 4 < 2 && (address >= 0xe050 && address < 0xe0d0 || address >= 0xe158 && address < 0xe1d8);
        }
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        Dictionary<int, byte> Stored(SamusBodyArtworkCatalog value) =>
            (Dictionary<int, byte>)typeof(SamusBodyArtworkCatalog).GetField("frames", flags)!.GetValue(value)!;
        var stock = Create(native);
        AssertEqual(Enumerable.Range(0, count).Count(index => Source(index) == index && !Direct(index)), Stored(stock).Count, "Exact native shared-component basis");
        for (int index = 0; index < count; index++)
        {
            AssertEqual(Source(index), SamusBodyFrameDefinitions.SourceComponent(index), "Exact native crouching/standing component relation domain");
            AssertEqual(native[index], native[Source(index)], "Direct native source relationship");
            AssertEqual(Direct(index), SamusBodyFrameDefinitions.TryComponent(index, out byte calculated), "Exact direct angular-allocation domain");
            if (Direct(index)) AssertEqual(native[index], calculated, "Direct native angular allocation set/position");
            AssertEqual(Source(index) == index && !Direct(index), Stored(stock).ContainsKey(index), "Exact required source-key membership without stock fallbacks");
        }
        AssertTrue(stock.Frames.SequenceEqual(Decode(native)), "All supplied frame components preserved");
        AssertEqual(CanonicalBodyHash(template, top, bottom, poses, Decode(native), upper, lower), stock.ContentIdentity, "Original canonical frame hash");
        Directory.CreateDirectory("csharp/test-temp");
        File.WriteAllLines("csharp/test-temp/samus-body-frame-required.csv", new[] { "Component,NativeAddress,Value" }
            .Concat(Stored(stock).OrderBy(pair => pair.Key).Select(pair => $"{pair.Key:X4},{0x92db48 + pair.Key:X6},{pair.Value:X2}")));
        if (basisOnly)
        {
            Console.WriteLine($"Body frame basis:{count} native components,{count - Stored(stock).Count} calculated/aliased components,{Stored(stock).Count} independent source components,zero stock alias fallbacks/canonical hash pass.");
            return;
        }
        IEnumerable<int> editAddresses = Enumerable.Range(0xdbf8, 240).Concat(Enumerable.Range(0xdf28, 240))
            .Concat(Enumerable.Range(0xdb48, 176)).Concat(Enumerable.Range(0xde18, 72))
            .Concat(Enumerable.Range(0xdd18, 32)).Concat(Enumerable.Range(0xdd48, 16)).Concat(Enumerable.Range(0xdeb0, 16))
            .Concat(new[] { 0xe050, 0xe0d0, 0xe150, 0xe158, 0xe1d8, 0xe258, 0xe2c0, 0xe37c, 0xe4c8, 0xe4d0, 0xe4f0, 0xe4fc,
                0xe508, 0xe530, 0xe558, 0xe580, 0xe5a8, 0xe5d0, 0xe5f8, 0xe628, 0xe658, 0xe688, 0xe6b8, 0xe728,
                0xe798, 0xe7a4, 0xe7b0, 0xe7bc, 0xe7c8, 0xe7d4, 0xe7e0, 0xe7ec, 0xe7f8, 0xe804, 0xe810, 0xe81c,
                0xe890, 0xe8cc, 0xe938, 0xe974, 0xe9f4, 0xea0c, 0xea24, 0xeba4, 0xea28, 0xe968 }.SelectMany(address => Enumerable.Range(address, 4)));
        if (finalOnly)
            editAddresses = new[] { 0xdb58, 0xdb54, 0xdba0, 0xdbbc, 0xdbfc, 0xe2ec, 0xe37c, 0xe600, 0xe65c,
                0xe6bc, 0xe534, 0xe29c, 0xe7a8, 0xe050, 0xe158, 0xe060, 0xe0d0, 0xea40, 0xeba8,
                0xe984, 0xe8a8, 0xea00, 0xdf40, 0xe458, 0xe888 }.SelectMany(address => Enumerable.Range(address, 4));
        int edits = 0;
        foreach (int address in editAddresses)
        {
            edits++;
            byte[] supplied = (byte[])native.Clone(); supplied[address - 0xdb48] ^= 1;
            var edited = Create(supplied);
            AssertTrue(edited.Frames.SequenceEqual(Decode(supplied)), "Independent source/derived/lower component edits preserved");
            for (int index = 0; index < count; index++)
                AssertEqual(Source(index) != index ? supplied[index] != supplied[Source(index)] : !Direct(index) || supplied[index] != native[index], Stored(edited).ContainsKey(index), "Exact required basis and independent exception membership");
            foreach (byte pose in new byte[] { 0xd5, 0xd6, 0xd9, 0xda })
            for (ushort frame = 0; frame < 5; frame++)
                AssertEqual(Decode(supplied)[(poses[pose] - 0xdb48) / 4 + frame], edited.Frame(pose, frame), "Actual four X-ray pose selections resolve independent supplied components");
            foreach (byte pose in new byte[] { 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12 })
            for (ushort frame = 0; frame < 10; frame++)
                AssertEqual(Decode(supplied)[(poses[pose] - 0xdb48) / 4 + frame], edited.Frame(pose, frame), "Actual ten moving poses resolve independent supplied lower gait and upper components");
            foreach (byte pose in new byte[] { 0x01, 0x02, 0x27, 0x28 })
            for (ushort frame = 0; frame < 9; frame++)
                AssertEqual(Decode(supplied)[(poses[pose] - 0xdb48) / 4 + frame], edited.Frame(pose, frame), "Actual standing/crouching normal upper selection");
            foreach (byte pose in new byte[] { 0x13, 0x14, 0x51, 0x52, 0x17, 0x18, 0x2d, 0x2e })
            for (ushort frame = 0; frame < 2; frame++)
                AssertEqual(Decode(supplied)[(poses[pose] - 0xdb48) / 4 + frame], edited.Frame(pose, frame), "Actual stationary/forward jump and downward-aim jump/fall selection");
            AssertEqual(CanonicalBodyHash(template, top, bottom, poses, Decode(supplied), upper, lower), edited.ContentIdentity, "Original canonical edited frame hash");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => SamusBodyFrameDefinitions.SourceComponent(-1), "Negative component rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => SamusBodyFrameDefinitions.SourceComponent(count), "Component end rejected");
        AssertThrows<InvalidDataException>(() => stock.Frame(0xfd, 0), "Original pose bound preserved");
        Console.WriteLine($"Body frames:{count - Stored(stock).Count} calculated/aliased components, exact remaining basis,{edits} independent component edits, actual pose selection and original canonical hashes pass.");
    }
    private static void VerifyLookupStream1BodyPosePointers(ISnesAddressSpace rom)
    {
        ushort[] native = Enumerable.Range(0, 253).Select(pose => (ushort)(rom.ReadByte(0x92d94e + pose * 2) | rom.ReadByte(0x92d94f + pose * 2) << 8)).ToArray();
        SamusBodyArtworkCatalog template = CreateSamusIdentityFixture();
        ushort[] top = template.TopSetPointers.ToArray(), bottom = template.BottomSetPointers.ToArray();
        var topGroups = Enumerable.Range(0, 13).Select(set => template.TopSet(set).ToArray()).ToArray();
        var bottomGroups = Enumerable.Range(0, 11).Select(set => template.BottomSet(set).ToArray()).ToArray();
        var frames = Enumerable.Range(0, SamusBodyArtworkCatalog.FrameCount)
            .Select(index => new SamusBodyFrameSelection((byte)(index % 13), 0, (byte)(index % 11), 0)).ToArray();
        SamusBodyArtworkCatalog Create(ushort[] selected) => new(top, bottom, selected,
            template.GraphicsYOffsets.ToArray(), frames, topGroups, bottomGroups,
            template.Spritemaps, template.Atmosphere, template.DeathPalettes, template.DeathTiles, template.ArmCannon,
            template.LandingYOffsets.ToArray(), template.PostureYOffsets.ToArray(), template.DrainedYOffsets.ToArray());
        SamusBodyArtworkCatalog stock = Create(native);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        int Count(SamusBodyArtworkCatalog value) => ((Dictionary<int, ushort>)typeof(SamusBodyArtworkCatalog).GetField("posePointers", flags)!.GetValue(value)!).Count;
        AssertEqual(0, Count(stock), "No stock pose-pointer override survives");
        AssertEqual(CanonicalBodyHash(template, top, bottom, native, frames, topGroups, bottomGroups), stock.ContentIdentity, "Exact old canonical stock pose hash");
        AssertTrue(stock.Frames.SequenceEqual(frames), "Independent frame payload remains intact");
        for (int pose = 0; pose < 253; pose++)
        {
            AssertEqual(native[pose], SamusBodyPoseDefinitions.DefaultFrameList((byte)pose), "Named semantic pose dispatch matches native list identity");
            int index = (native[pose] - 0xdb48) / 4;
            AssertEqual(frames[index], stock.Frame((byte)pose, 0), "Runtime selects supplied frame payload through native default");
            ushort[] selected = (ushort[])native.Clone();
            selected[pose] = native[pose] + 4 < 0xed24 ? (ushort)(native[pose] + 4) : (ushort)0xdb48;
            SamusBodyArtworkCatalog changed = Create(selected);
            AssertEqual(1, Count(changed), "Exactly one independent valid pose-pointer edit");
            AssertTrue(changed.PosePointers.SequenceEqual(selected), "All supplied pose-pointer identities preserved");
            AssertTrue(changed.Frames.SequenceEqual(frames), "Pointer edit never changes independent frame records");
            AssertEqual(frames[(selected[pose] - 0xdb48) / 4], changed.Frame((byte)pose, 0), "Runtime honors edited frame-list identity");
            AssertEqual(CanonicalBodyHash(template, top, bottom, selected, frames, topGroups, bottomGroups), changed.ContentIdentity, "Exact old canonical edited pose hash");
            AssertTrue(changed.ContentIdentity != stock.ContentIdentity, "Independent pose edit changes hash");
        }
        foreach (byte pose in new byte[] { 0xfd, 0xfe, 0xff })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => SamusBodyPoseDefinitions.DefaultFrameList(pose), "Default pose dispatch excludes adjacent data");
            AssertThrows<InvalidDataException>(() => stock.Frame(pose, 0), "Installed pose domain stays bounded");
        }
        ushort[] invalid = (ushort[])native.Clone(); invalid[0] = 0xdb49;
        AssertThrows<InvalidDataException>(() => Create(invalid), "Edited pose pointer retains alignment validation");
        AssertTrue(stock.PosePointers.SequenceEqual(native), "Stock pose identities immutable");
        Console.WriteLine("Body pose pointers:253 native semantic cases,156 list identities, zero stock overrides,253 independent edits, supplied frame resolution and exact canonical hashes pass.");
    }

    private static string CanonicalBodyHash(SamusBodyArtworkCatalog template, ushort[] upper, ushort[] lower, ushort[] posePointers, SamusBodyFrameSelection[] frames, SamusBodyTileDefinition[][] upperGroups, SamusBodyTileDefinition[][] lowerGroups) =>
        SelectedPresentationHash.Create(nameof(SamusBodyArtworkCatalog), content =>
        {
            content.AppendWords("top pointers", upper); content.AppendWords("bottom pointers", lower);
            content.AppendWords("pose pointers", posePointers);
            content.Append("graphics y offsets", template.GraphicsYOffsets.ToArray().Select(value => unchecked((byte)value)).ToArray());
            content.AppendWords("landing y offsets", template.LandingYOffsets);
            content.Append("posture y offsets", template.PostureYOffsets.ToArray().Select(value => unchecked((byte)value)).ToArray());
            content.Append("drained y offsets", template.DrainedYOffsets.ToArray().Select(value => unchecked((byte)value)).ToArray());
            foreach (SamusBodyFrameSelection frame in frames)
            {
                content.Append("top set", frame.TopSet); content.Append("top position", frame.TopPosition);
                content.Append("bottom set", frame.BottomSet); content.Append("bottom position", frame.BottomPosition);
            }
            var definitions = upper.SelectMany((pointer, set) => upperGroups[set].Select((definition, position) => (Address: 0x920000 | (pointer + position * 7), Definition: definition)))
                .Concat(lower.SelectMany((pointer, set) => lowerGroups[set].Select((definition, position) => (Address: 0x920000 | (pointer + position * 7), Definition: definition))));
            foreach (var item in definitions.OrderBy(item => item.Address))
            {
                content.Append("definition address", item.Address); content.Append("source address", item.Definition.SourceAddress);
                content.Append("first transfer size", item.Definition.FirstSize); content.Append("second transfer size", item.Definition.SecondSize);
                content.Append("characters", item.Definition.Planar.Span);
            }
            content.Append("spritemaps", Convert.FromHexString(template.Spritemaps.ContentIdentity));
            content.Append("atmosphere", Convert.FromHexString(template.Atmosphere.ContentIdentity));
            content.Append("death palettes", Convert.FromHexString(template.DeathPalettes.ContentIdentity));
            content.Append("death tiles", Convert.FromHexString(template.DeathTiles.ContentIdentity));
            content.Append("arm cannon", Convert.FromHexString(template.ArmCannon.ContentIdentity));
        });


    private static void VerifyLookupStream1BodySetPointers(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        ushort[] top = Enumerable.Range(0, 13).Select(set => Word(0x92d91e + set * 2)).ToArray();
        ushort[] bottom = Enumerable.Range(0, 11).Select(set => Word(0x92d938 + set * 2)).ToArray();
        var physical = top.Select((pointer, set) => (Upper: true, Set: set, Pointer: pointer))
            .Concat(bottom.Select((pointer, set) => (Upper: false, Set: set, Pointer: pointer))).OrderBy(value => value.Pointer).ToArray();
        SamusBodyArtworkCatalog template = CreateSamusIdentityFixture();
        SamusBodyTileDefinition sample = template.TopSet(0)[0];
        var topGroups = new SamusBodyTileDefinition[13][]; var bottomGroups = new SamusBodyTileDefinition[11][];
        for (int order = 0; order < physical.Length; order++)
        {
            var item = physical[order]; int end = order + 1 == physical.Length ? 0xd7d3 : physical[order + 1].Pointer;
            (item.Upper ? topGroups : bottomGroups)[item.Set] = Enumerable.Repeat(sample, (end - item.Pointer) / 7).ToArray();
        }
        SamusBodyArtworkCatalog Create(ushort[] upper, ushort[] lower, SamusBodyTileDefinition[][] upperGroups, SamusBodyTileDefinition[][] lowerGroups) => new(
            upper, lower, template.PosePointers.ToArray(), template.GraphicsYOffsets.ToArray(), template.Frames.ToArray(),
            upperGroups, lowerGroups, template.Spritemaps, template.Atmosphere, template.DeathPalettes, template.DeathTiles,
            template.ArmCannon, template.LandingYOffsets.ToArray(), template.PostureYOffsets.ToArray(), template.DrainedYOffsets.ToArray());
        // Independent oracle for the pre-conversion canonical serialization order (520c810c4).
        string Canonical(ushort[] upper, ushort[] lower, SamusBodyTileDefinition[][] upperGroups, SamusBodyTileDefinition[][] lowerGroups) =>
            CanonicalBodyHash(template, upper, lower, template.PosePointers.ToArray(), template.Frames.ToArray(), upperGroups, lowerGroups);
        SamusBodyArtworkCatalog stock = Create(top, bottom, topGroups, bottomGroups);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        int Count(SamusBodyArtworkCatalog value, string field) => ((Dictionary<int, ushort>)typeof(SamusBodyArtworkCatalog).GetField(field, flags)!.GetValue(value)!).Count;
        AssertEqual(0, Count(stock, "topPointers"), "Zero stock top pointer overrides");
        AssertEqual(0, Count(stock, "bottomPointers"), "Zero stock bottom pointer overrides");
        for (int set = 0; set < 13; set++) AssertEqual(top[set], SamusBodyDefinitionLayout.DefaultTopPointer(set), "Named top allocation matches original ROM");
        for (int set = 0; set < 11; set++) AssertEqual(bottom[set], SamusBodyDefinitionLayout.DefaultBottomPointer(set), "Named bottom allocation matches original ROM");
        AssertEqual(Canonical(top, bottom, topGroups, bottomGroups), stock.ContentIdentity, "Exact pre-conversion canonical stock hash");
        for (int edit = 0; edit < physical.Length; edit++)
        {
            var item = physical[edit]; ushort[] upper = (ushort[])top.Clone(), lower = (ushort[])bottom.Clone();
            var upperGroups = topGroups.Select(group => group.ToArray()).ToArray(); var lowerGroups = bottomGroups.Select(group => group.ToArray()).ToArray();
            (item.Upper ? upper : lower)[item.Set] += 7;
            (item.Upper ? upperGroups : lowerGroups)[item.Set] = (item.Upper ? upperGroups : lowerGroups)[item.Set][1..];
            if (edit > 0)
            {
                var previous = physical[edit - 1];
                var groups = previous.Upper ? upperGroups : lowerGroups;
                groups[previous.Set] = groups[previous.Set].Append(sample).ToArray();
            }
            SamusBodyArtworkCatalog changed = Create(upper, lower, upperGroups, lowerGroups);
            AssertEqual(1, Count(changed, "topPointers") + Count(changed, "bottomPointers"), "Exactly one valid independent pointer override");
            AssertTrue(changed.TopSetPointers.SequenceEqual(upper) && changed.BottomSetPointers.SequenceEqual(lower), "Independent selected allocation identities preserved");
            AssertEqual(0x920000 | (item.Pointer + 7), changed.DefinitionAddress(item.Upper, (byte)item.Set, 0), "Runtime definition selection honors arbitrary valid edited pointer");
            AssertEqual(Canonical(upper, lower, upperGroups, lowerGroups), changed.ContentIdentity, "Exact pre-conversion canonical edited hash");
            AssertTrue(changed.ContentIdentity != stock.ContentIdentity, "Each allocation edit changes identity");
        }
        AssertTrue(stock.TopSetPointers.SequenceEqual(top) && stock.BottomSetPointers.SequenceEqual(bottom), "Stock pointer snapshots remain immutable");
        AssertThrows<ArgumentOutOfRangeException>(() => SamusBodyDefinitionLayout.DefaultTopPointer(13), "Top allocation selector bound");
        AssertThrows<ArgumentOutOfRangeException>(() => SamusBodyDefinitionLayout.DefaultBottomPointer(11), "Bottom allocation selector bound");
        AssertThrows<InvalidDataException>(() => stock.GetDefinition(true, 13, 0), "Installed top selector retains rejection");
        AssertThrows<InvalidDataException>(() => stock.GetDefinition(false, 11, 0), "Installed bottom selector retains rejection");
        Console.WriteLine("Body allocation pointers:24 native semantic identities, zero stock overrides,24 valid independent allocation edits, runtime addresses and exact canonical hashes pass.");
    }

    private static void VerifyLookupStream1BodyGraphicsOrigins(ISnesAddressSpace rom)
    {
        sbyte[] native = Enumerable.Range(0, 253).Select(pose => unchecked((sbyte)rom.ReadByte(0x91b62d + pose * 8))).ToArray();
        SamusBodyArtworkCatalog template = CreateSamusIdentityFixture();
        SamusBodyArtworkCatalog Create(sbyte[] selected) => new(
            template.TopSetPointers.ToArray(), template.BottomSetPointers.ToArray(), template.PosePointers.ToArray(),
            selected, template.Frames.ToArray(),
            Enumerable.Range(0, SamusBodyArtworkCatalog.TopSetCount).Select(index => template.TopSet(index).ToArray()).ToArray(),
            Enumerable.Range(0, SamusBodyArtworkCatalog.BottomSetCount).Select(index => template.BottomSet(index).ToArray()).ToArray(),
            template.Spritemaps, template.Atmosphere, template.DeathPalettes, template.DeathTiles, template.ArmCannon,
            template.LandingYOffsets.ToArray(), template.PostureYOffsets.ToArray(), template.DrainedYOffsets.ToArray());
        SamusBodyArtworkCatalog stock = Create(native);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        int Count(SamusBodyArtworkCatalog value) => ((Dictionary<int, sbyte>)typeof(SamusBodyArtworkCatalog).GetField("graphicsYOffsets", flags)!.GetValue(value)!).Count;
        AssertEqual(0, Count(stock), "No stock graphics-origin override survives");
        for (int pose = 0; pose < 253; pose++)
        {
            AssertEqual(native[pose], SamusBodyPlacementDefinitions.DefaultGraphicsYOffset((byte)pose), "Direct calculated visual origin matches native real-pose byte");
            AssertEqual(native[pose], stock.GraphicsYOffset((byte)pose), "Installed default origin");
        }
        AssertTrue(stock.GraphicsYOffsets.SequenceEqual(native), "Visual origin snapshot ordering");
        string identity = stock.ContentIdentity;
        for (int edit = 0; edit < 253; edit++)
        {
            sbyte[] selected = (sbyte[])native.Clone(); selected[edit] ^= 1;
            SamusBodyArtworkCatalog changed = Create(selected);
            AssertEqual(1, Count(changed), "Exactly one independent graphics-origin override");
            AssertTrue(changed.GraphicsYOffsets.SequenceEqual(selected), "Every supplied visual origin remains independent");
            AssertTrue(changed.ContentIdentity != identity, "Every visual-origin edit changes content identity");
            for (int pose = 0; pose < 253; pose++)
            {
                AssertEqual(selected[pose], changed.GraphicsYOffset((byte)pose), "Edited runtime origin");
                AssertEqual(unchecked((byte)native[pose]), SamusPoseProjectileOriginDefinitions.ReadYOffset((byte)pose), "Visual edits never change physical projectile origins");
            }
        }
        AssertEqual(identity, stock.ContentIdentity, "Stock visual origin hash immutable");
        foreach (byte pose in new byte[] { 0xfd, 0xfe, 0xff })
        {
            AssertThrows<InvalidDataException>(() => stock.GraphicsYOffset(pose), "Installed graphics excludes adjacent instruction poses");
            AssertThrows<ArgumentOutOfRangeException>(() => SamusBodyPlacementDefinitions.DefaultGraphicsYOffset(pose), "Default graphics excludes adjacent instruction poses");
        }
        Console.WriteLine("Body graphics origins:253 direct native/default values, zero stock overrides,253 independent visual edits isolated from physics and hash/bounds checks pass.");
    }

    private static void VerifyLookupStream1DrainedGeometry(ISnesAddressSpace rom)
    {
        string directory = Path.GetFullPath("csharp/test-temp/drained-geometry-native");
        SamusBodyArtworkFiles.Extract(rom, directory, SupportedCartridge.Sha256);
        SamusBodyArtworkCatalog stock = SamusBodyArtworkFiles.Load(directory, null);
        sbyte[] native = Enumerable.Range(0, 32).Select(index => unchecked((sbyte)rom.ReadByte(0x908def + index))).ToArray();
        var field = typeof(SamusBodyArtworkCatalog).GetField("drainedYOffsets",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        int Stored(SamusBodyArtworkCatalog art) => ((Dictionary<int, sbyte>)field.GetValue(art)!).Count;
        SamusBodyArtworkCatalog Create(sbyte[] offsets, SamusSpritemapArtworkCatalog? maps = null,
            SamusBodyTileDefinition[][]? upper = null, SamusBodyTileDefinition[][]? lower = null, sbyte[]? graphics = null) => new(
            stock.TopSetPointers.ToArray(), stock.BottomSetPointers.ToArray(), stock.PosePointers.ToArray(),
            graphics ?? stock.GraphicsYOffsets.ToArray(), stock.Frames.ToArray(),
            upper ?? Enumerable.Range(0, SamusBodyArtworkCatalog.TopSetCount).Select(index => stock.TopSet(index).ToArray()).ToArray(),
            lower ?? Enumerable.Range(0, SamusBodyArtworkCatalog.BottomSetCount).Select(index => stock.BottomSet(index).ToArray()).ToArray(),
            maps ?? stock.Spritemaps, stock.Atmosphere, stock.DeathPalettes, stock.DeathTiles, stock.ArmCannon,
            stock.LandingYOffsets.ToArray(), stock.PostureYOffsets.ToArray(), offsets);
        AssertEqual(0, Stored(stock), "Native drained placement uses no stored offset fallback: " + string.Join(", ", ((Dictionary<int, sbyte>)field.GetValue(stock)!).Select(pair => pair.Key + " native=" + pair.Value + " derived=" + (SamusBodyPlacementDefinitions.TryDefaultDrainedByte(stock, pair.Key, out sbyte calculated) ? calculated.ToString() : "unavailable"))));
        for (int index = 0; index < 32; index++)
        {
            AssertTrue(SamusBodyPlacementDefinitions.TryDefaultDrainedByte(stock, index, out sbyte direct), "Every native drained phase has a derived default");
            AssertEqual(native[index], direct, "Direct native support-aligned offset");
            sbyte[] changed = (sbyte[])native.Clone(); changed[index] ^= 1;
            var edited = Create(changed);
            AssertTrue(edited.DrainedYOffsets.SequenceEqual(changed), "Every independent drained edit remains exact");
            AssertTrue(edited.ContentIdentity != stock.ContentIdentity, "Drained edit changes selected identity");
        }
        int draws = 0;
        for (ushort frame = 0; frame < 32; frame++)
        {
            AssertTrue(stock.TryDrainedYOffset(frame, out sbyte offset), "Entire native drained byte window admitted");
            AssertEqual(native[frame], offset, "Installed native byte including command slots");
            if (frame is 12 or 13 or 17 or 18 or 24 or 25 or 27 or 28 or 30 or 31) continue;
            var samus = new SamusState { Pose = 0xe9, AnimationFrame = frame, XPosition = 128, YPosition = 128 };
            samus.TileTransfers.BindArtwork(stock);
            samus.Draw(rom, new OamBuffer(), 0, 0);
            AssertEqual(unchecked((ushort)(128 + native[frame])), samus.SpritemapYPosition,
                "Actual drained drawing uses exact signed native correction");
            AssertEqual((ushort)128, samus.YPosition, "Drawing preserves physical center");
            draws++;
        }
        AssertEqual(22, draws, "Every nonempty source phase drawn");
        AssertTrue(!stock.TryDrainedYOffset(-1, out _) && !stock.TryDrainedYOffset(32, out _), "Exact drained bounds");
        var emptyMaps = new SamusSpritemapArtworkCatalog(stock.Spritemaps.TopBases.ToArray(), stock.Spritemaps.BottomBases.ToArray(),
            stock.Spritemaps.Pointers.ToArray(), stock.Spritemaps.Definitions.Select(map => new SamusSpritemapDefinition(map.Pointer, [])).ToArray());
        var empty = Create(native, emptyMaps);
        AssertTrue(empty.DrainedYOffsets.SequenceEqual(native), "Supplied empty art preserves all independent offsets");
        var zeroPointers = new SamusSpritemapArtworkCatalog(stock.Spritemaps.TopBases.ToArray(), stock.Spritemaps.BottomBases.ToArray(),
            new ushort[SamusSpritemapArtworkCatalog.PointerCount], []);
        AssertTrue(Create(native, zeroPointers).DrainedYOffsets.SequenceEqual(native), "Mutable-zero-pointer art preserves offset schema");
        SamusBodyTileDefinition[][] Blank(bool top) => Enumerable.Range(0, top ? SamusBodyArtworkCatalog.TopSetCount : SamusBodyArtworkCatalog.BottomSetCount)
            .Select(set => (top ? stock.TopSet(set) : stock.BottomSet(set)).ToArray().Select(definition =>
                new SamusBodyTileDefinition(definition.SourceAddress, definition.FirstSize, definition.SecondSize, new byte[definition.Planar.Length])).ToArray()).ToArray();
        var blankPixels = Create(native, upper: Blank(true), lower: Blank(false));
        AssertTrue(blankPixels.DrainedYOffsets.SequenceEqual(native), "Independently blank PNG pixels preserve original offsets");
        ushort changedPointer = stock.Spritemaps.Pointers[stock.Spritemaps.TopBase(0xe9)];
        var shiftedMaps = new SamusSpritemapArtworkCatalog(stock.Spritemaps.TopBases.ToArray(), stock.Spritemaps.BottomBases.ToArray(),
            stock.Spritemaps.Pointers.ToArray(), stock.Spritemaps.Definitions.Select(map => new SamusSpritemapDefinition(map.Pointer,
                map.Pointer == changedPointer ? map.Parts.Select(part => part with { Y = unchecked((byte)(part.Y + 1)) }).ToArray() : map.Parts)).ToArray());
        var shifted = Create(native, shiftedMaps);
        AssertTrue(shifted.DrainedYOffsets.SequenceEqual(native), "Changed source support does not couple independent offset edits");
        AssertTrue(Stored(shifted) > 0, "Edited source geometry stores explicit independent differences");
        sbyte[] editedGraphics = stock.GraphicsYOffsets.ToArray();
        editedGraphics[0xe9]++; editedGraphics[0x02]++;
        AssertTrue(Create(native, graphics: editedGraphics).DrainedYOffsets.SequenceEqual(native),
            "Independently changed crouching/standing origins preserve all supplied drained offsets");
        AssertThrows<ArgumentOutOfRangeException>(() => SamusBodyPlacementDefinitions.TryDefaultDrainedByte(stock, 32, out _), "Default drained bound");
        Console.WriteLine("Drained geometry:32 native defaults, zero stock fallbacks,32 independent edits,22 actual draws and independent empty/pointer/pixel/shifted-art checks pass.");
    }
    private static void VerifyLookupStream1PostureGeometry(ISnesAddressSpace rom)
    {
        string directory = Path.GetFullPath("csharp/test-temp/posture-geometry-native");
        SamusBodyArtworkFiles.Extract(rom, directory, SupportedCartridge.Sha256);
        SamusBodyArtworkCatalog stock = SamusBodyArtworkFiles.Load(directory, null);
        sbyte[] native = Enumerable.Range(0, 24).Select(index => unchecked((sbyte)rom.ReadByte(0x908d80 + index))).ToArray();
        var field = typeof(SamusBodyArtworkCatalog).GetField("postureYOffsets",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        int Stored(SamusBodyArtworkCatalog art) => ((Dictionary<int, sbyte>)field.GetValue(art)!).Count;
        SamusBodyArtworkCatalog Create(sbyte[] offsets, SamusSpritemapArtworkCatalog? maps = null,
            SamusBodyTileDefinition[][]? upper = null, SamusBodyTileDefinition[][]? lower = null) => new(
            stock.TopSetPointers.ToArray(), stock.BottomSetPointers.ToArray(), stock.PosePointers.ToArray(),
            stock.GraphicsYOffsets.ToArray(), stock.Frames.ToArray(),
            upper ?? Enumerable.Range(0, SamusBodyArtworkCatalog.TopSetCount).Select(index => stock.TopSet(index).ToArray()).ToArray(),
            lower ?? Enumerable.Range(0, SamusBodyArtworkCatalog.BottomSetCount).Select(index => stock.BottomSet(index).ToArray()).ToArray(),
            maps ?? stock.Spritemaps, stock.Atmosphere, stock.DeathPalettes, stock.DeathTiles, stock.ArmCannon,
            stock.LandingYOffsets.ToArray(), offsets, stock.DrainedYOffsets.ToArray());
        AssertEqual(0, Stored(stock), "Native posture uses no stored offset fallback: " + string.Join(", ", ((Dictionary<int, sbyte>)field.GetValue(stock)!).Select(pair => pair.Key + " native=" + pair.Value + " derived=" + (SamusBodyPlacementDefinitions.TryDefaultPostureByte(stock, pair.Key, out sbyte calculated) ? calculated.ToString() : "unavailable"))));
        for (int index = 0; index < 24; index++)
        {
            AssertTrue(SamusBodyPlacementDefinitions.TryDefaultPostureByte(stock, index, out sbyte direct), "Every native posture has a derived default");
            AssertEqual(native[index], direct, "Direct native support-aligned offset");
            sbyte[] changed = (sbyte[])native.Clone(); changed[index] ^= 1;
            var edited = Create(changed);
            AssertTrue(edited.PostureYOffsets.SequenceEqual(changed), "Every independent posture edit remains exact");
            AssertTrue(edited.ContentIdentity != stock.ContentIdentity, "Posture edit changes selected identity");
        }
        foreach (byte pose in new byte[] { 0x35, 0x36, 0x37, 0x38, 0x3b, 0x3c, 0x3d, 0x3e })
        {
            int count = pose is 0x35 or 0x36 or 0x3b or 0x3c ? 1 : 2;
            for (ushort frame = 0; frame < count; frame++)
            {
                var samus = new SamusState { Pose = pose, AnimationFrame = frame, XPosition = 128, YPosition = 128 };
                samus.TileTransfers.BindArtwork(stock);
                samus.Draw(rom, new OamBuffer(), 0, 0);
                AssertEqual(unchecked((ushort)(128 + native[(pose - 0x35) * 2 + frame])), samus.SpritemapYPosition,
                    "Actual transition rendering preserves native support correction");
                AssertEqual((ushort)128, samus.YPosition, "Posture art never mutates physical center");
            }
        }
        var emptyMaps = new SamusSpritemapArtworkCatalog(stock.Spritemaps.TopBases.ToArray(), stock.Spritemaps.BottomBases.ToArray(),
            stock.Spritemaps.Pointers.ToArray(), stock.Spritemaps.Definitions.Select(map => new SamusSpritemapDefinition(map.Pointer, [])).ToArray());
        var empty = Create(native, emptyMaps);
        AssertTrue(empty.PostureYOffsets.SequenceEqual(native), "Supplied empty art preserves all independent offsets");
        var zeroPointers = new SamusSpritemapArtworkCatalog(stock.Spritemaps.TopBases.ToArray(), stock.Spritemaps.BottomBases.ToArray(),
            new ushort[SamusSpritemapArtworkCatalog.PointerCount], []);
        AssertTrue(Create(native, zeroPointers).PostureYOffsets.SequenceEqual(native), "Mutable-zero-pointer art preserves offset schema");
        SamusBodyTileDefinition[][] Blank(bool top) => Enumerable.Range(0, top ? SamusBodyArtworkCatalog.TopSetCount : SamusBodyArtworkCatalog.BottomSetCount)
            .Select(set => (top ? stock.TopSet(set) : stock.BottomSet(set)).ToArray().Select(definition =>
                new SamusBodyTileDefinition(definition.SourceAddress, definition.FirstSize, definition.SecondSize, new byte[definition.Planar.Length])).ToArray()).ToArray();
        var blankPixels = Create(native, upper: Blank(true), lower: Blank(false));
        AssertTrue(blankPixels.PostureYOffsets.SequenceEqual(native), "Independently blank PNG pixels preserve original offsets");
        ushort changedPointer = stock.Spritemaps.Pointers[stock.Spritemaps.TopBase(0x37)];
        var shiftedMaps = new SamusSpritemapArtworkCatalog(stock.Spritemaps.TopBases.ToArray(), stock.Spritemaps.BottomBases.ToArray(),
            stock.Spritemaps.Pointers.ToArray(), stock.Spritemaps.Definitions.Select(map => new SamusSpritemapDefinition(map.Pointer,
                map.Pointer == changedPointer ? map.Parts.Select(part => part with { Y = unchecked((byte)(part.Y + 1)) }).ToArray() : map.Parts)).ToArray());
        var shifted = Create(native, shiftedMaps);
        AssertTrue(shifted.PostureYOffsets.SequenceEqual(native), "Changed source support does not couple independent offset edits");
        AssertTrue(Stored(shifted) > 0, "Edited source geometry stores explicit independent differences");
        Console.WriteLine("Posture geometry:24 direct native defaults, zero stock fallbacks,24 offset edits,12 actual draw cases and empty/pointer/pixel/shifted-art independence pass.");
    }
    private static void VerifyLookupStream1BodyFacingOffsets(ISnesAddressSpace rom)
    {
        ushort[] landing = Enumerable.Range(0, 17).Select(index => (ushort)rom.ReadByte(0x908d28 + index)).ToArray();
        sbyte[] posture = Enumerable.Range(0, 24).Select(index => unchecked((sbyte)rom.ReadByte(0x908d80 + index))).ToArray();
        SamusBodyArtworkCatalog template = CreateSamusIdentityFixture();
        SamusBodyArtworkCatalog Create(ushort[] selectedLanding, sbyte[] selectedPosture) => new(
            template.TopSetPointers.ToArray(), template.BottomSetPointers.ToArray(), template.PosePointers.ToArray(),
            template.GraphicsYOffsets.ToArray(), template.Frames.ToArray(),
            Enumerable.Range(0, SamusBodyArtworkCatalog.TopSetCount).Select(index => template.TopSet(index).ToArray()).ToArray(),
            Enumerable.Range(0, SamusBodyArtworkCatalog.BottomSetCount).Select(index => template.BottomSet(index).ToArray()).ToArray(),
            template.Spritemaps, template.Atmosphere, template.DeathPalettes, template.DeathTiles, template.ArmCannon,
            selectedLanding, selectedPosture, template.DrainedYOffsets.ToArray());
        SamusBodyArtworkCatalog stock = Create(landing, posture);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var landingBasis = (Dictionary<int, ushort>)typeof(SamusBodyArtworkCatalog).GetField("landingYOffsets", flags)!.GetValue(stock)!;

        AssertEqual(0, landingBasis.Count, "Landing stock stores no coordinate or instruction-byte overrides");
        // This synthetic fixture supplies independent artwork; direct native geometry is confirmed separately.
        for (int index = 0; index < landing.Length; index++)
        {
            int source = index is >= 4 and < 8 or >= 12 and < 16 ? index - 4 : index;
            AssertEqual(source, SamusBodyPlacementDefinitions.LandingSourceIndex(index), "Native normal/spin facing row alias");
            AssertEqual(landing[index], landing[source], "Direct native landing facing equality");
            AssertEqual(landing[index], (ushort)SamusBodyPlacementDefinitions.DefaultLandingByte(index), "Direct native semantic landing default including adjacent PLB");
        }
        for (int index = 0; index < posture.Length; index++)
        {
            int source = (index & 2) != 0 ? index - 2 : index;
            AssertEqual(source, SamusBodyPlacementDefinitions.PostureSourceIndex(index), "Native transition facing row alias");
            AssertEqual(posture[index], posture[source], "Direct native posture facing equality");

        }
        void Check(SamusBodyArtworkCatalog catalog, ushort[] expectedLanding, sbyte[] expectedPosture)
        {
            AssertTrue(catalog.LandingYOffsets.SequenceEqual(expectedLanding), "Landing snapshot preserves supplied bytes");
            AssertTrue(catalog.PostureYOffsets.SequenceEqual(expectedPosture), "Posture snapshot preserves supplied bytes");
            for (int index = 0; index < 16; index++)
            {
                AssertTrue(catalog.TryLandingYOffset(index, out ushort actual), "Landing word admission");
                AssertEqual((ushort)(expectedLanding[index] | expectedLanding[index + 1] << 8), actual, "Native unaligned landing word including adjacent byte");
            }
            for (int index = 0; index < 24; index++)
            {
                AssertTrue(catalog.TryPostureYOffset(index, out sbyte actual), "Posture admission");
                AssertEqual(expectedPosture[index], actual, "Signed posture offset");
            }
        }
        Check(stock, landing, posture);
        for (int index = 0; index < 16; index++)
        {
            var samus = new SamusState
            {
                Pose = (byte)(0xa4 + index / 4), AnimationFrame = (ushort)(index % 4),
                XPosition = 128, YPosition = 128,
            };
            samus.TileTransfers.BindArtwork(stock);
            samus.Draw(rom, new OamBuffer(), layer1X: 0, layer1Y: 0);
            ushort nativeWord = (ushort)(landing[index] | landing[index + 1] << 8);
            AssertEqual(unchecked((ushort)(128 - nativeWord)), samus.SpritemapYPosition, "Actual rendering preserves every bounded native landing word");
            AssertEqual((ushort)128, samus.YPosition, "Visual landing correction does not change physical position");
        }
        string identity = stock.ContentIdentity;
        for (int edit = 0; edit < 41; edit++)
        {
            ushort[] changedLanding = (ushort[])landing.Clone(); sbyte[] changedPosture = (sbyte[])posture.Clone();
            if (edit < 17) changedLanding[edit] ^= 1; else changedPosture[edit - 17] ^= 1;
            SamusBodyArtworkCatalog changed = Create(changedLanding, changedPosture);
            Check(changed, changedLanding, changedPosture);
            AssertTrue(changed.ContentIdentity != identity, "Every independent placement edit changes identity");
            Check(stock, landing, posture);
            AssertEqual(identity, stock.ContentIdentity, "Placement edits leave stock identity immutable");
        }
        foreach (int index in new[] { -1, 16, 17, int.MaxValue })
            AssertTrue(!stock.TryLandingYOffset(index, out _), "Landing reader original word bounds");
        foreach (int index in new[] { -1, 24, int.MaxValue })
            AssertTrue(!stock.TryPostureYOffset(index, out _), "Posture reader original bounds");
        AssertEqual((ushort)0xab, landing[16], "Adjacent native PLB byte is the exact retained instruction observation");
        Console.WriteLine("Body facing offsets:17 direct landing defaults with zero overrides,24 posture bytes,41 independent edits,16 unaligned windows and content identities pass.");
    }

    private static void VerifyLookupStream1BodyOamBases(ISnesAddressSpace rom)
    {
        ushort[] ReadWords(int address, int count) => Enumerable.Range(0, count)
            .Select(index => (ushort)(rom.ReadByte(address + index * 2) | rom.ReadByte(address + index * 2 + 1) << 8)).ToArray();
        ushort[] top = ReadWords(0x929263, 253), bottom = ReadWords(0x92945d, 253), pointers = ReadWords(0x92808d, 2096);
        // Marker parts confirm selected supplied payload identity, not native artwork.
        SamusSpritemapDefinition[] definitions = pointers.Where(pointer => pointer != 0).Distinct()
            .Select(pointer => new SamusSpritemapDefinition(pointer,
                [new SamusSpritePart(pointer, (byte)pointer, (ushort)(pointer ^ 0x5555))])).ToArray();
        SamusSpritemapArtworkCatalog Create(ushort[] upper, ushort[] lower) => new(upper, lower, pointers, definitions);
        var stock = Create(top, bottom);
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        int Count(SamusSpritemapArtworkCatalog value, string field) =>
            ((Dictionary<int, ushort>)typeof(SamusSpritemapArtworkCatalog).GetField(field, flags)!.GetValue(value)!).Count;
        string Canonical(ushort[] upper, ushort[] lower) => SelectedPresentationHash.Create(nameof(SamusSpritemapArtworkCatalog), content =>
        {
            content.AppendWords("top bases", upper);
            content.AppendWords("bottom bases", lower);
            content.AppendWords("pointers", pointers);
            foreach (SamusSpritemapDefinition definition in definitions.OrderBy(item => item.Pointer))
            {
                content.Append("pointer", definition.Pointer);
                content.Append("part count", definition.Parts.Length);
                foreach (SamusSpritePart part in definition.Parts)
                {
                    content.Append("x", part.X);
                    content.Append("y", part.Y);
                    content.Append("attributes", part.Attributes);
                }
            }
        });
        void CheckSelection(SamusSpritemapArtworkCatalog catalog, ushort selected)
        {
            bool found = catalog.TryGet(selected, out SamusSpritemapDefinition? definition);
            AssertEqual(pointers[selected] != 0, found, "Native zero pointer retains mutable-memory routing");
            if (found)
            {
                AssertEqual(pointers[selected], definition!.Pointer, "Actual selector resolves supplied pointer");
                AssertEqual(new SamusSpritePart(pointers[selected], (byte)pointers[selected], (ushort)(pointers[selected] ^ 0x5555)),
                    definition.Parts.Single(), "Actual selector resolves independent marker payload");
            }
        }
        AssertEqual(0, Count(stock, "topBases") + Count(stock, "bottomBases"), "Zero stock OAM-base overrides");
        AssertEqual(Canonical(top, bottom), stock.ContentIdentity, "Exact original canonical stock OAM hash");
        for (int half = 0; half < 2; half++)
        for (int pose = 0; pose < 253; pose++)
        {
            ushort expected = half == 0 ? top[pose] : bottom[pose];
            AssertEqual(expected, half == 0 ? SamusSpritemapPoseDefinitions.TopBase((byte)pose) : SamusSpritemapPoseDefinitions.BottomBase((byte)pose),
                "Direct named OAM-base case matches native ROM");
            AssertEqual(expected, half == 0 ? stock.TopBase((byte)pose) : stock.BottomBase((byte)pose), "Runtime native base selection");
            CheckSelection(stock, expected);
            ushort[] upper = (ushort[])top.Clone(), lower = (ushort[])bottom.Clone();
            ushort edited = (ushort)((expected + 1) % 2096);
            (half == 0 ? upper : lower)[pose] = edited;
            var changed = Create(upper, lower);
            AssertEqual(1, Count(changed, "topBases") + Count(changed, "bottomBases"), "Exactly one independent OAM-base override");
            AssertTrue(changed.TopBases.SequenceEqual(upper) && changed.BottomBases.SequenceEqual(lower), "All supplied independent bases preserved");
            AssertTrue(changed.Pointers.SequenceEqual(pointers), "OAM pointer payload unchanged");
            ushort selected = half == 0 ? changed.TopBase((byte)pose) : changed.BottomBase((byte)pose);
            AssertEqual(edited, selected, "Runtime honors independently edited base");
            CheckSelection(changed, selected);
            AssertEqual(Canonical(upper, lower), changed.ContentIdentity, "Exact original canonical edited OAM hash");
            AssertTrue(changed.ContentIdentity != stock.ContentIdentity, "Every independent base edit changes identity");
        }
        foreach (byte pose in new byte[] { 0xfd, 0xfe, 0xff })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => SamusSpritemapPoseDefinitions.TopBase(pose), "Top default excludes adjacent data");
            AssertThrows<ArgumentOutOfRangeException>(() => SamusSpritemapPoseDefinitions.BottomBase(pose), "Bottom default excludes adjacent data");
            AssertThrows<IndexOutOfRangeException>(() => stock.TopBase(pose), "Original installed top domain exception");
            AssertThrows<IndexOutOfRangeException>(() => stock.BottomBase(pose), "Original installed bottom domain exception");
        }
        ushort[] invalid = (ushort[])top.Clone(); invalid[0] = 2096;
        AssertThrows<InvalidDataException>(() => Create(invalid, bottom), "Edited base retains original range validation");
        AssertTrue(stock.TopBases.SequenceEqual(top) && stock.BottomBases.SequenceEqual(bottom), "Stock bases immutable after edits");
        Console.WriteLine("Samus OAM bases:506 native named cases, zero stock overrides,506 independent edits, actual supplied payload selection, exact canonical hashes and original bounds pass.");
    }
    private static void VerifyLookupStream1HurtBlend(ISnesAddressSpace rom)
    {
        ushort[] native = Enumerable.Range(0, 32).Select(index =>
            (ushort)(rom.ReadByte(0x9ba380 + index * 2) | rom.ReadByte(0x9ba381 + index * 2) << 8)).ToArray();
        PaletteRgb5 Color(ushort word) => new() { Red = word & 31, Green = (word >> 5) & 31, Blue = (word >> 10) & 31 };
        SamusHurtColorCatalog Create(ushort[] supplied) => SamusHurtColorCatalog.Load(new MemoryStream(SamusHurtColorCatalog.Write(
            new SamusHurtColorDocument { Version = 1, Hurt = supplied.Take(16).Select(Color).ToArray(), Intro = supplied.Skip(16).Select(Color).ToArray() })));
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        Dictionary<int, ushort> Overrides(SamusHurtColorCatalog catalog, string name) =>
            (Dictionary<int, ushort>)typeof(SamusHurtColorCatalog).GetField(name, flags)!.GetValue(catalog)!;
        Dictionary<int, byte> Levels(SamusHurtColorCatalog catalog) => (Dictionary<int, byte>)typeof(SamusHurtColorCatalog).GetField("introLevels", flags)!.GetValue(catalog)!;
        var stock = Create(native);
        AssertEqual(0, Overrides(stock, "introOverrides").Count + Overrides(stock, "hurtOverrides").Count, "Zero stock channel/blend fallbacks");
        AssertEqual(0, Levels(stock).Count, "Zero stock shade samples survive");
        AssertTrue(typeof(SamusHurtColorCatalog).GetField("hurtZero", flags)!.GetValue(stock) is null &&
            typeof(SamusHurtColorCatalog).GetField("introZero", flags)!.GetValue(stock) is null, "No stock transparent-word fallback");
        AssertEqual(native[0], SamusHurtColorDefinitions.HurtTransparentWord, "Native transparent black");
        AssertEqual(native[16], SamusHurtColorDefinitions.IntroTransparentWord, "Native reviewed transparent identity");
        for (int index = 1; index < 16; index++)
        {
            AssertEqual((byte)(native[index + 16] & 31), SamusHurtColorDefinitions.DefaultIntroLevel(index), "Direct calculated shade level equals native");
            AssertEqual(native[index + 16], SamusHurtColorDefinitions.IntroFromLevel((byte)(native[index + 16] & 31)), "Direct native intro channel relation");
            AssertEqual(native[index], SamusHurtColorDefinitions.HurtFromIntro(Bgr555.FromWord(checked((ushort)(native[index + 16])))), "Direct native hurt white blend");
        }
        for (int selected = 0; selected < 32; selected++)
        for (int channel = 0; channel < 3; channel++)
        {
            ushort[] supplied = (ushort[])native.Clone(); supplied[selected] ^= (ushort)(31 << (channel * 5));
            var edited = Create(supplied);
            for (int index = 0; index < 32; index++)
                AssertEqual(supplied[index], edited.Resolve(index < 16 ? SamusHurtColorVariant.Hurt : SamusHurtColorVariant.Intro, index % 16), "Every supplied RGB channel remains independent across palettes");
            for (int index = 1; index < 16; index++)
            {
                bool stored = (supplied[index + 16] & 31) != (native[index + 16] & 31);
                AssertEqual(stored, Levels(edited).ContainsKey(index), "Exact independent shade difference membership");
                if (stored) AssertEqual((byte)(supplied[index + 16] & 31), Levels(edited)[index], "Exact supplied shade override");
            }
            for (int index = 1; index < 16; index++)
            {
                int red = supplied[index + 16] & 31;
                ushort expectedIntro = (ushort)(red | red << 5 | Math.Max(4, red - 2) << 10);
                int Blend(int shift) => (2 * ((supplied[index + 16] >> shift) & 31) + 155) / 7;
                ushort expectedHurt = (ushort)(Blend(0) | Blend(5) << 5 | Blend(10) << 10);
                AssertEqual(supplied[index + 16] != expectedIntro, Overrides(edited, "introOverrides").ContainsKey(index), "Exact intro edit exception membership");
                AssertEqual(supplied[index] != expectedHurt, Overrides(edited, "hurtOverrides").ContainsKey(index), "Exact independent hurt exception membership");
            }
        }
        for (int index = 0; index < 32; index++)
            AssertEqual(native[index], stock.Resolve(index < 16 ? SamusHurtColorVariant.Hurt : SamusHurtColorVariant.Intro, index % 16), "Stock color remains immutable");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve((SamusHurtColorVariant)2, 0), "Variant rejection preserved");
        foreach (int index in new[] { -1, 16 })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(SamusHurtColorVariant.Intro, index), "Color bounds preserved");
        Console.WriteLine("Hurt/intro:32 native colors,30 direct relations,zero stock shade samples,zero stock overrides,96 independent RGB edits and exact exception membership pass.");
    }
    private static void VerifyLookupStream1NonBeamProgramLayout(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var expected = new Dictionary<int, ushort>();
        for (int cursor = 0x939ebb; cursor < 0x93a1a1;)
        {
            if (cursor == 0x93a117) { cursor += 2; continue; }
            ushort command = Word(cursor);
            expected.Add(cursor, command);
            if ((command & 0x8000) == 0)
            {
                expected.Add(cursor + 6, Word(cursor + 6)); cursor += 8;
            }
            else if (command == 0x822f) cursor += 2;
            else
            {
                AssertEqual((ushort)0x8239, command, "Native NonBeam terminal is Goto");
                expected.Add(cursor + 2, Word(cursor + 2)); cursor += 4;
            }
        }
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static;
        var stored = (IReadOnlyDictionary<int, ushort>)typeof(SamusProjectileInstructionDefinitions).GetField("Words", flags)?.GetValue(null)!;
        for (int address = 0x939eba; address <= 0x93a1a1; address++)
        {
            bool accepted = expected.TryGetValue(address, out ushort native);
            AssertEqual(accepted, SamusProjectileInstructionDefinitions.TryNonBeamWord(address, out ushort calculated), "NonBeam exact native mechanics domain");
            if (accepted)
            {
                AssertEqual(native, calculated, "NonBeam direct native duration, trail phase or control");
                AssertEqual(native, SamusProjectileInstructionDefinitions.ReadWord(address), "NonBeam production reader");
                AssertTrue(stored is null || !stored.ContainsKey(address), "No NonBeam stock fallback survives");
            }
            else if (address < 0x93a1a1)
                AssertThrows<InvalidDataException>(() => SamusProjectileInstructionDefinitions.ReadWord(address), "NonBeam nonmechanics retain rejection");
        }
        AssertEqual(214, expected.Count, "Native nonbeam and special program mechanics count");
        AssertThrows<InvalidDataException>(() => SamusProjectileInstructionDefinitions.ReadWord(0x93a1a1), "Following flare table remains outside mechanics");
        AssertTrue(typeof(SamusProjectileInstructionDefinitions).GetField("Words", flags) is null, "Original mechanics dictionary removed entirely; required scalar inputs remain explicit");
        foreach (int address in new[] { 0x928743, 0x948743, -1, int.MaxValue })
            AssertTrue(!SamusProjectileInstructionDefinitions.TryNonBeamWord(address, out _), "NonBeam bank and address bounds");
        Console.WriteLine("NonBeam projectile programs:214 direct native mechanics words, exact domain and zero stored fallbacks pass; selected holds and phase counts remain required.");
    }

    private static void VerifyLookupStream1ChargedProgramLayout(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var expected = new Dictionary<int, ushort>();
        for (int cursor = 0x938e77; cursor < 0x939ebb;)
        {
            ushort command = Word(cursor);
            expected.Add(cursor, command);
            if ((command & 0x8000) == 0)
            {
                expected.Add(cursor + 6, Word(cursor + 6)); cursor += 8;
            }
            else
            {
                AssertEqual((ushort)0x8239, command, "Native Charged terminal is Goto");
                expected.Add(cursor + 2, Word(cursor + 2)); cursor += 4;
            }
        }
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static;
        var stored = (IReadOnlyDictionary<int, ushort>)typeof(SamusProjectileInstructionDefinitions).GetField("Words", flags)?.GetValue(null)!;
        for (int address = 0x938e76; address <= 0x939ebb; address++)
        {
            bool accepted = expected.TryGetValue(address, out ushort native);
            AssertEqual(accepted, SamusProjectileInstructionDefinitions.TryChargedWord(address, out ushort calculated), "Charged exact native mechanics domain");
            if (accepted)
            {
                AssertEqual(native, calculated, "Charged direct native duration, trail phase or control");
                AssertEqual(native, SamusProjectileInstructionDefinitions.ReadWord(address), "Charged production reader");
                AssertTrue(stored is null || !stored.ContainsKey(address), "No Charged stock fallback survives");
            }
            else if (address < 0x939ebb)
                AssertThrows<InvalidDataException>(() => SamusProjectileInstructionDefinitions.ReadWord(address), "Charged nonmechanics retain rejection");
        }
        AssertEqual(1078, expected.Count, "Native charged beam program mechanics count");
        AssertEqual(Word(0x939ebb), SamusProjectileInstructionDefinitions.ReadWord(0x939ebb), "Following Missile required payload is unchanged");
        foreach (int address in new[] { 0x928743, 0x948743, -1, int.MaxValue })
            AssertTrue(!SamusProjectileInstructionDefinitions.TryChargedWord(address, out _), "Charged bank and address bounds");
        Console.WriteLine("Charged projectile programs:1078 direct native mechanics words, exact domain and zero stored fallbacks pass; holds, phase counts and loop-entry choices remain required.");
    }

    private static void VerifyLookupStream1PlasmaProgramLayout(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var expected = new Dictionary<int, ushort>();
        for (int cursor = 0x938cf7; cursor < 0x938e77;)
        {
            ushort command = Word(cursor);
            expected.Add(cursor, command);
            if ((command & 0x8000) == 0)
            {
                expected.Add(cursor + 6, Word(cursor + 6)); cursor += 8;
            }
            else
            {
                AssertEqual((ushort)0x8239, command, "Native Plasma terminal is Goto");
                expected.Add(cursor + 2, Word(cursor + 2)); cursor += 4;
            }
        }
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static;
        var stored = (IReadOnlyDictionary<int, ushort>)typeof(SamusProjectileInstructionDefinitions).GetField("Words", flags)?.GetValue(null)!;
        for (int address = 0x938cf6; address <= 0x938e77; address++)
        {
            bool accepted = expected.TryGetValue(address, out ushort native);
            AssertEqual(accepted, SamusProjectileInstructionDefinitions.TryPlasmaWord(address, out ushort calculated), "Plasma exact native mechanics domain");
            if (accepted)
            {
                AssertEqual(native, calculated, "Plasma direct native duration, trail phase or control");
                AssertEqual(native, SamusProjectileInstructionDefinitions.ReadWord(address), "Plasma production reader");
                AssertTrue(stored is null || !stored.ContainsKey(address), "No Plasma stock fallback survives");
            }
            else if (address < 0x938e77)
                AssertThrows<InvalidDataException>(() => SamusProjectileInstructionDefinitions.ReadWord(address), "Plasma nonmechanics retain rejection");
        }
        AssertEqual(104, expected.Count, "Native four persistent and four cyclic Plasma programs mechanics count");
        AssertEqual(Word(0x938e77), SamusProjectileInstructionDefinitions.ReadWord(0x938e77), "Following charged Power required payload is unchanged");
        foreach (int address in new[] { 0x928743, 0x948743, -1, int.MaxValue })
            AssertTrue(!SamusProjectileInstructionDefinitions.TryPlasmaWord(address, out _), "Plasma bank and address bounds");
        Console.WriteLine("Plasma projectile programs:104 direct native mechanics words, exact domain and zero stored fallbacks pass; holds1/15/2 and cyclic phase count8 remain required.");
    }

    private static void VerifyLookupStream1SpazerProgramLayout(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var expected = new Dictionary<int, ushort>();
        for (int cursor = 0x938977; cursor < 0x938cf7;)
        {
            ushort command = Word(cursor);
            expected.Add(cursor, command);
            if ((command & 0x8000) == 0)
            {
                expected.Add(cursor + 6, Word(cursor + 6)); cursor += 8;
            }
            else
            {
                AssertEqual((ushort)0x8239, command, "Native Spazer terminal is Goto");
                expected.Add(cursor + 2, Word(cursor + 2)); cursor += 4;
            }
        }
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static;
        var stored = (IReadOnlyDictionary<int, ushort>)typeof(SamusProjectileInstructionDefinitions).GetField("Words", flags)?.GetValue(null)!;
        for (int address = 0x938976; address <= 0x938cf7; address++)
        {
            bool accepted = expected.TryGetValue(address, out ushort native);
            AssertEqual(accepted, SamusProjectileInstructionDefinitions.TrySpazerWord(address, out ushort calculated), "Spazer exact native mechanics domain");
            if (accepted)
            {
                AssertEqual(native, calculated, "Spazer direct native duration, trail phase or control");
                AssertEqual(native, SamusProjectileInstructionDefinitions.ReadWord(address), "Spazer production reader");
                AssertTrue(stored is null || !stored.ContainsKey(address), "No Spazer stock fallback survives");
            }
            else if (address < 0x938cf7)
                AssertThrows<InvalidDataException>(() => SamusProjectileInstructionDefinitions.ReadWord(address), "Spazer nonmechanics retain rejection");
        }
        AssertEqual(240, expected.Count, "Native eight growth programs and eight spread cycles mechanics count");
        AssertEqual(Word(0x938cf7), SamusProjectileInstructionDefinitions.ReadWord(0x938cf7), "Following Plasma required payload is unchanged");
        foreach (int address in new[] { 0x928743, 0x948743, -1, int.MaxValue })
            AssertTrue(!SamusProjectileInstructionDefinitions.TrySpazerWord(address, out _), "Spazer bank and address bounds");
        Console.WriteLine("Spazer projectile programs:240 direct native mechanics words, exact domain and zero stored fallbacks pass; hold2 and phase counts3/10 remain required.");
    }

    private static void VerifyLookupStream1WaveIceProgramLayout(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var expected = new Dictionary<int, ushort>();
        for (int cursor = 0x93873b; cursor < 0x938977;)
        {
            ushort command = Word(cursor);
            expected.Add(cursor, command);
            if ((command & 0x8000) == 0)
            {
                expected.Add(cursor + 6, Word(cursor + 6)); cursor += 8;
            }
            else
            {
                AssertEqual((ushort)0x8239, command, "Native Wave/Ice terminal is Goto");
                expected.Add(cursor + 2, Word(cursor + 2)); cursor += 4;
            }
        }
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static;
        var stored = (IReadOnlyDictionary<int, ushort>)typeof(SamusProjectileInstructionDefinitions).GetField("Words", flags)?.GetValue(null)!;
        for (int address = 0x93873a; address <= 0x938977; address++)
        {
            bool accepted = expected.TryGetValue(address, out ushort native);
            AssertEqual(accepted, SamusProjectileInstructionDefinitions.TryWaveIceWord(address, out ushort calculated), "Wave/Ice exact native mechanics domain");
            if (accepted)
            {
                AssertEqual(native, calculated, "Wave/Ice direct native duration, trail phase or control");
                AssertEqual(native, SamusProjectileInstructionDefinitions.ReadWord(address), "Wave/Ice production reader");
                AssertTrue(stored is null || !stored.ContainsKey(address), "No Wave/Ice stock fallback survives");
            }
            else if (address < 0x938977)
                AssertThrows<InvalidDataException>(() => SamusProjectileInstructionDefinitions.ReadWord(address), "Wave/Ice nonmechanics retain rejection");
        }
        AssertEqual(148, expected.Count, "Native Wave prelude, four cycles and Ice mechanics count");
        AssertEqual(Word(0x938977), SamusProjectileInstructionDefinitions.ReadWord(0x938977), "Following Spazer required payload is unchanged");
        foreach (int address in new[] { 0x928743, 0x948743, -1, int.MaxValue })
            AssertTrue(!SamusProjectileInstructionDefinitions.TryWaveIceWord(address, out _), "Wave/Ice bank and address bounds");
        Console.WriteLine("Wave/Ice projectile programs:148 direct native mechanics words, exact domain and zero stored fallbacks pass; holds4/1 and phase counts16/4 remain required.");
    }

    private static void VerifyLookupStream1PowerProgramLayout(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static;
        var stored = (IReadOnlyDictionary<int, ushort>)typeof(SamusProjectileInstructionDefinitions).GetField("Words", flags)?.GetValue(null)!;
        int admitted = 0;
        for (int address = 0x9386da; address <= 0x93873b; address++)
        {
            int offset = address - 0x9386db;
            bool expected = offset >= 0 && offset < 96 && offset % 12 is 0 or 6 or 8 or 10;
            bool actual = SamusProjectileInstructionDefinitions.TryPowerWord(address, out ushort calculated);
            AssertEqual(expected, actual, "Power mechanics domain excludes sprites, radii and odd bytes");
            if (!expected) continue;
            admitted++;
            AssertEqual(Word(address), calculated, "Direct calculated Power mechanics matches native word");
            AssertEqual(calculated, SamusProjectileInstructionDefinitions.ReadWord(address), "Production mechanics reader uses calculated Power layout");
            AssertTrue(stored is null || !stored.ContainsKey(address), "No stock Power mechanics fallback survives");
        }
        AssertEqual(32, admitted, "Eight native Power programs have four mechanics words each");
        for (int direction = 0; direction < 8; direction++)
        {
            int start = 0x9386db + direction * 12;
            AssertEqual((ushort)start, SamusProjectileInstructionDefinitions.ReadWord(start + 10), "Power native Goto returns to its own compass phase");
            foreach (int field in new[] { 1, 2, 3, 4, 5, 7, 9, 11 })
                AssertThrows<InvalidDataException>(() => SamusProjectileInstructionDefinitions.ReadWord(start + field), "Power non-mechanics addresses retain rejection");
        }
        AssertEqual(Word(0x93873b), SamusProjectileInstructionDefinitions.ReadWord(0x93873b), "Following Wave required payload remains intact");
        foreach (int address in new[] { 0x9286db, 0x9486db, -1, int.MaxValue })
            AssertTrue(!SamusProjectileInstructionDefinitions.TryPowerWord(address, out _), "Power calculated bank/domain bounds");
        Console.WriteLine("Power projectile programs:32 direct native mechanics words, eight self-loop targets, exact domain and zero stored Power fallbacks pass; hold15 remains required.");
    }

    private static void VerifyLookupStream1CannonPoses(ISnesAddressSpace rom)
    {
        using var directory = new TestTempDirectory("map-catalog");
        SamusArmCannonArtworkFiles.Extract(rom, directory.Root, SupportedCartridge.Sha256);
        byte[] json = File.ReadAllBytes(Path.Combine(directory.Root, SamusArmCannonArtworkFormat.JsonFileName));
        byte[] png = File.ReadAllBytes(Path.Combine(directory.Root, SamusArmCannonArtworkFormat.TileFileName));
        var document = System.Text.Json.JsonSerializer.Deserialize<SamusArmCannonArtworkDocument>(json,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        var tiles = RoomCharacterAtlas.Load(new MemoryStream(png), SamusArmCannonArtworkFormat.TileSourcePointers.Length * 32);
        SamusArmCannonArtworkCatalog Load(SamusArmCannonArtworkDocument value) => SamusArmCannonArtworkCatalog.FromPlacement(
            SamusArmCannonArtworkCatalog.LoadPlacement(new MemoryStream(SamusArmCannonArtworkCatalog.Write(value))), tiles);
        var stock = Load(document);
        var field = typeof(SamusArmCannonArtworkCatalog).GetField("posePointers",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        Dictionary<int, ushort> Stored(SamusArmCannonArtworkCatalog value) => (Dictionary<int, ushort>)field.GetValue(value)!;
        ushort Native(int pose) => (ushort)(rom.ReadByte(0x90C7DF + pose * 2) | rom.ReadByte(0x90C7E0 + pose * 2) << 8);
        string Identity(SamusArmCannonArtworkDocument value) => SelectedPresentationHash.Create(nameof(SamusArmCannonArtworkCatalog), hash =>
        {
            hash.AppendWords("pose pointers", value.PosePointers.Select(pointer => (ushort)pointer).ToArray());
            hash.Append("drawing data", value.DrawingData.Select(item => (byte)item).ToArray());
            hash.AppendWords("attributes", value.SpriteAttributes.Select(item => (ushort)item).ToArray());
            foreach (int[] direction in value.TileSources) hash.AppendWords("tile sources", direction.Select(item => (ushort)item).ToArray());
            hash.Append("characters", tiles.Transfer.Span);
        });
        AssertEqual(253, SamusBodyArtworkCatalog.PoseCount, "cannon original 506-byte pointer extent");
        AssertEqual(0, Stored(stock).Count, "cannon pose stock stores no copied pointer overrides");
        AssertEqual(Identity(document), stock.ContentIdentity, "cannon canonical native identity unchanged");
        for (int pose = 0; pose < 253; pose++)
        {
            AssertEqual(Native(pose), SamusArmCannonArtworkFormat.StockPoseDrawingData(pose), "cannon direct native pose dispatch");
            AssertEqual(Native(pose), stock.PoseDrawingData(pose), "cannon installed native pose dispatch");
            int[] pointers = document.PosePointers.ToArray();
            // Preserve an arbitrary admitted byte address, including descriptor interiors.
            int replacement = SamusArmCannonArtworkFormat.DrawingDataStart + pose;
            if (replacement == pointers[pose]) replacement++;
            pointers[pose] = replacement;
            var editedDocument = document with { PosePointers = pointers };
            var edited = Load(editedDocument);
            AssertEqual(1, Stored(edited).Count, "cannon exactly one independent pointer override");
            AssertEqual((ushort)replacement, Stored(edited)[pose], "cannon stores arbitrary admitted pointer identity");
            for (int other = 0; other < 253; other++)
                AssertEqual(other == pose ? (ushort)replacement : Native(other), edited.PoseDrawingData(other), "cannon isolated pointer edit");
            AssertEqual(Identity(editedDocument), edited.ContentIdentity, "cannon edited pointer hash preserves canonical representation");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 253, 254, 255, 256, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => stock.PoseDrawingData(invalid), "cannon installed pose domain preserved including FD-FF");
            AssertThrows<ArgumentOutOfRangeException>(() => SamusArmCannonArtworkFormat.StockPoseDrawingData(invalid), "cannon calculated pose domain rejects aliases");
        }
        Console.WriteLine("Cannon poses:253 direct native defaults, zero stock overrides,253 independent arbitrary pointer edits/hashes and exact FD-FF rejection pass.");
    }

    private static void VerifyLookupStream1AnimationAliases(CartridgeImportAddressSpace bus)
    {
        for (int pose = 0xFD; pose <= 0xFF; pose++)
        {
            int source = SamusAnimationDelayDefinitions.DelayStreamsAddress + (pose - 0xFD) * 2;
            ushort expectedPointer = (ushort)(bus.ReadByte(source) | bus.ReadByte(source + 1) << 8);
            ushort pointer = SamusAnimationDelayDefinitions.PointerForPose((byte)pose);
            AssertEqual(expectedPointer, pointer, $"animation alias ${pose:X2} reads its running-delay source");
            for (int change = 0; change < 2; change++)
            {
                byte expected = (byte)(pose - 0xFD + change * 16);
                bus.WriteByte(0x7E0000 | pointer, expected);
                AssertEqual(expected, SamusAnimationDelayDefinitions.ReadAnimationByte(bus, pointer, 0),
                    $"animation alias ${pose:X2} observes WRAM mutation {change}");
            }
        }
        Console.WriteLine("Animation aliases: three original running-delay words and six live WRAM mutations match.");
    }

    private static void VerifyLookupStream1HudPosture(ISnesAddressSpace rom)
    {
        for (int pose = 0; pose < 256; pose++)
        {
            if (pose < 0xDB)
                AssertEqual(rom.ReadByte(0x90DDAA + pose - 0x35), SamusHudDefinitions.PostureObservation((byte)pose),
                    $"HUD exact bounded posture observation {pose:X2}");
            else
                AssertThrows<ArgumentOutOfRangeException>(() => SamusHudDefinitions.PostureObservation((byte)pose),
                    "HUD rejects prefiltered posture observation");
            foreach (bool grapple in new[] { false, true })
            {
                bool expected = pose >= 0xF1 || (pose < 0xDB && (rom.ReadByte(0x90DDAA + pose - 0x35) == 0 || grapple));
                AssertEqual(expected, SamusHudInput.PostureTransitionAdmitsWeapons((byte)pose, grapple),
                    $"HUD actual admission {pose:X2}, Grapple={grapple}");
            }
        }
        Console.WriteLine("HUD posture: twelve real flags plus207 bounded instruction bytes, all512 admission branches and rejection domain match native.");
    }

    private static void VerifyLookupStream1EscapeDachoraPrograms(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(0xb30000 | address) | rom.ReadByte(0xb30000 | address + 1) << 8);
        var controls = new List<ushort>(); var visuals = new List<ushort>();
        for (int cursor = 0xe964; cursor < 0xeaa8;)
        {
            ushort value = Word(cursor); controls.Add((ushort)cursor); cursor += 2;
            if ((value & 0x8000) == 0) { visuals.Add((ushort)cursor); cursor += 2; }
            else if (value is CommonEnemyInstructionCodes.Goto or CommonEnemyInstructionCodes.SetTimer or CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate
                or EscapeAnimalInstructionCodes.InstList_DachoraEscape_GotoY_IfAcidLessThanCE or EscapeAnimalInstructionCodes.InstList_DachoraEscape_GotoY_IfCrittersEscaped)
            { controls.Add((ushort)cursor); cursor += 2; }
            else AssertTrue(value is EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionMinus6
                or EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionPlus6, "Dachora native operand-free movement callback");
        }
        AssertEqual(controls.Count, EscapeDachoraInstructionProgramDefinitions.MechanicsWordCount, "Dachora native control count");
        AssertEqual(visuals.Count, EscapeDachoraInstructionProgramDefinitions.PresentationWordCount, "Dachora native visual count");
        for (int index = 0; index < controls.Count; index++)
        {
            var actual = EscapeDachoraInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(controls[index], actual.Address, "Dachora ordered native control identity");
            AssertEqual(Word(actual.Address), actual.Value, "Dachora native branch target/callback/required duration");
            AssertEqual(actual.Value, EscapeDachoraInstructionProgramDefinitions.ReadMechanicsWord(actual.Address), "Dachora calculated reader");
        }
        for (int index = 0; index < visuals.Count; index++)
        {
            AssertEqual(visuals[index], EscapeDachoraInstructionProgramDefinitions.PresentationWordAddress(index), "Dachora native visual operand location");
            AssertThrows<InvalidDataException>(() => EscapeDachoraInstructionProgramDefinitions.ReadMechanicsWord(visuals[index]), "Dachora visual operands are not controls");
        }
        for (int address = 0xe963; address <= 0xeaa9; address++)
        {
            AssertEqual(controls.Any(word => address == word || address == word + 1),
                EscapeDachoraInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0xb30000 | address), "Dachora exact control byte coverage");
            AssertTrue(!EscapeDachoraInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0xb20000 | address), "Dachora wrong bank rejects");
        }
        foreach (int invalid in new[] { int.MinValue, -1, controls.Count, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => EscapeDachoraInstructionProgramDefinitions.MechanicsWord(invalid), "Dachora control bounds");
        foreach (int invalid in new[] { int.MinValue, -1, visuals.Count, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => EscapeDachoraInstructionProgramDefinitions.PresentationWordAddress(invalid), "Dachora visual bounds");
        foreach (ushort invalid in new ushort[] { 0xe963, 0xe965, 0xeaa8, ushort.MaxValue })
            AssertThrows<InvalidDataException>(() => EscapeDachoraInstructionProgramDefinitions.ReadMechanicsWord(invalid), "Dachora odd/adjacent control pointers reject");
        Console.WriteLine("Escape Dachora:119 controls/43 visual operands match independent native traversal; exact branches/order/domains pass; selected holds and acceleration cadence remain required.");
    }
    private static void VerifyLookupStream1PowampPrograms(ISnesAddressSpace rom)
    {
        ushort Word(int pointer) => (ushort)(rom.ReadByte(0xa80000 | pointer) | rom.ReadByte(0xa80000 | pointer + 1) << 8);
        var expectedMechanics = new List<ushort>(); var expectedVisual = new List<ushort>();
        // Traverse original native words, using their instruction/frame encoding rather than the calculated program layout.
        for (int cursor = 0xc163; cursor < 0xc19f;)
        {
            ushort value = Word(cursor);
            expectedMechanics.Add((ushort)cursor);
            cursor += 2;
            if ((value & 0x8000) == 0) { expectedVisual.Add((ushort)cursor); cursor += 2; }
            else if (value == CommonEnemyInstructionCodes.Goto) { expectedMechanics.Add((ushort)cursor); cursor += 2; }
            else AssertEqual(CommonEnemyInstructionCodes.Sleep, value, "Powamp native terminal opcode");
        }
        AssertEqual(expectedMechanics.Count, PowampInstructionProgramDefinitions.MechanicsWordCount, "Powamp calculated control count");
        AssertEqual(expectedVisual.Count, PowampInstructionProgramDefinitions.PresentationWordCount, "Powamp calculated visual operand count");
        for (int index = 0; index < expectedMechanics.Count; index++)
        {
            var actual = PowampInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(expectedMechanics[index], actual.Address, "Powamp ordered native control identity");
            AssertEqual(Word(actual.Address), actual.Value, "Powamp every native control and still-required hold value");
            AssertEqual(actual.Value, PowampInstructionProgramDefinitions.ReadMechanicsWord(actual.Address), "Powamp control read resolves calculated record");
        }
        for (int index = 0; index < expectedVisual.Count; index++)
        {
            AssertEqual(expectedVisual[index], PowampInstructionProgramDefinitions.PresentationWordAddress(index), "Powamp calculated native visual operand location");
            AssertThrows<InvalidDataException>(() => PowampInstructionProgramDefinitions.ReadMechanicsWord(expectedVisual[index]), "Powamp visual operands remain outside control domain");
        }
        for (int address = 0xc162; address <= 0xc1a0; address++)
        {
            bool expected = expectedMechanics.Any(word => address == word || address == word + 1);
            AssertEqual(expected, PowampInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0xa80000 | address), "Powamp exact native control byte coverage");
            AssertTrue(!PowampInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0xa70000 | address), "Powamp wrong bank rejects");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 18, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => PowampInstructionProgramDefinitions.MechanicsWord(invalid), "Powamp control index rejection");
        foreach (int invalid in new[] { int.MinValue, -1, 12, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => PowampInstructionProgramDefinitions.PresentationWordAddress(invalid), "Powamp visual index rejection");
        foreach (ushort invalid in new ushort[] { 0xc162, 0xc164, 0xc19f, ushort.MaxValue })
            AssertThrows<InvalidDataException>(() => PowampInstructionProgramDefinitions.ReadMechanicsWord(invalid), "Powamp non-word/adjacent control pointer rejects");
        Console.WriteLine("Powamp program layout:18 native controls,12 visual operands, exact order/domains/byte classification pass; five chosen timing inputs remain required.");
    }
    private static void VerifyLookupStream1TimerLayout(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        byte[] json = EscapeTimerPresentationExtractor.Extract(rom);
        var stock = EscapeTimerPresentation.Load(new MemoryStream(json));
        var document = System.Text.Json.JsonSerializer.Deserialize<EscapeTimerPresentationDocument>(json, MapPresentationFormat.JsonOptions)!;
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        int Count(EscapeTimerPresentation value, string name) => ((System.Collections.IDictionary)typeof(EscapeTimerPresentation).GetField(name, flags)!.GetValue(value)!).Count;
        AssertEqual(0, Count(stock, "frames"), "Timer stock stores no calculated frame fallbacks");
        AssertEqual(0, Count(stock, "anchors"), "Timer stock stores no calculated anchor fallbacks");
        int parts = 0;
        foreach (string name in document.Frames.Keys)
        {
            int address = name == "Label" ? 0x80a060 : 0x800000 | Word(0x809fd4 + int.Parse(name.AsSpan(6)) * 2);
            var calculated = EscapeTimerPresentationDefinitions.DefaultParts(name);
            AssertEqual((int)Word(address), calculated.Count, "Timer part count matches native sprite record");
            for (int index = 0; index < calculated.Count; index++)
            {
                int native = address + 2 + index * 5;
                var part = calculated[index];
                AssertEqual(Word(native), part.X.Raw, "Timer calculated X/size native word");
                AssertEqual(rom.ReadByte(native + 2), part.Y, "Timer calculated native Y");
                AssertEqual((ushort)(Word(native + 3) & ~0x0e00), part.Attributes.Raw, "Timer calculated native tile/priority/flips with inherited palette removed");
                AssertTrue(part.InheritPalette, "Timer part inherits independently supplied timer palette");
                parts++;
            }
            AssertThrows<ArgumentOutOfRangeException>(() => _ = calculated[-1], "Timer part negative index rejects");
            AssertThrows<ArgumentOutOfRangeException>(() => _ = calculated[calculated.Count], "Timer part upper index rejects");
        }
        AssertEqual(25, parts, "All twenty-five native timer parts are calculated");
        foreach ((string name, int address) in new[] { ("Label", 0x809f73), ("Minutes", 0x809f7c), ("Seconds", 0x809f85), ("Centiseconds", 0x809f8e) })
            AssertEqual(new MapLabelPoint(unchecked((short)Word(address)), 0), EscapeTimerPresentationDefinitions.DefaultAnchor(name), "Timer centered anchor matches native renderer immediate");
        var timer = new EscapeTimer(); timer.Clear();
        for (int digit = 0; digit < 10; digit++)
        {
            timer.SetTime((byte)(digit * 17), 0, (byte)(digit * 17));
            var native = new OamBuffer(); var actual = new OamBuffer();
            DrawImportedEscapeTimer(rom, timer, native);
            stock.Draw(timer, actual);
            EqualOam(native, actual, "Every calculated decimal glyph draws exact native OAM");
        }
        foreach (string name in document.Frames.Keys)
        {
            var edited = Read();
            edited.Frames[name][0] = edited.Frames[name][0] with { OffsetX = edited.Frames[name][0].OffsetX + 3,
                OffsetY = edited.Frames[name][0].OffsetY + 2, TileNumber = edited.Frames[name][0].TileNumber ^ 1,
                Size = 16, Priority = 1, Palette = 2, FlipX = true, FlipY = true };
            var compiled = Load(edited);
            AssertEqual(1, Count(compiled, "frames"), "One timer frame edit stores exactly one independent frame");
            AssertEqual(0, Count(compiled, "anchors"), "Frame edits do not store unchanged timer anchors");
            int digit = name == "Label" ? 0 : int.Parse(name.AsSpan(6));
            timer.SetTime((byte)(digit * 17), 0, (byte)(digit * 17));
            var actual = new OamBuffer(); compiled.Draw(timer, actual);
            EqualOam(DrawDocument(edited), actual, "Edited timer geometry, size, tile, priority, palette and flips remain exact");
        }
        foreach (string anchor in document.Anchors.Keys)
        {
            var edited = Read(); edited.Anchors[anchor] = edited.Anchors[anchor] with { X = edited.Anchors[anchor].X + 2, Y = 3 };
            edited = edited with { DigitSpacing = 10, Palette = 1 };
            var compiled = Load(edited);
            AssertEqual(1, Count(compiled, "anchors"), "One timer anchor edit stores exactly one independent anchor");
            AssertEqual(0, Count(compiled, "frames"), "Anchor, spacing and palette edits do not store native frames");
            timer.SetTime(0x12, 0x34, 0x56);
            var actual = new OamBuffer(); compiled.Draw(timer, actual);
            EqualOam(DrawDocument(edited), actual, "Independent anchor, digit spacing and palette edits preserve selected document");
        }
        Console.WriteLine("Escape timer layout:25 native parts,4 native anchors, zero stock dictionaries,10 decimal OAM fixtures and independent frame/anchor/spacing/palette edits pass.");

        EscapeTimerPresentationDocument Read() => System.Text.Json.JsonSerializer.Deserialize<EscapeTimerPresentationDocument>(json, MapPresentationFormat.JsonOptions)!;
        static EscapeTimerPresentation Load(EscapeTimerPresentationDocument value)
        {
            using var stream = new MemoryStream(); EscapeTimerPresentation.Write(stream, value); stream.Position = 0;
            return EscapeTimerPresentation.Load(stream);
        }
        static void EqualOam(OamBuffer expected, OamBuffer actual, string message)
        {
            AssertEqual(expected.NextByteOffset, actual.NextByteOffset, message + " count");
            AssertTrue(expected.LowTable.SequenceEqual(actual.LowTable) && expected.HighTable.SequenceEqual(actual.HighTable), message);
        }
        OamBuffer DrawDocument(EscapeTimerPresentationDocument selected)
        {
            var result = new OamBuffer();
            Draw("Label", selected.Anchors["Label"]);
            Pair(timer.MinutesBcd, "Minutes"); Pair(timer.SecondsBcd, "Seconds"); Pair(timer.CentisecondsBcd, "Centiseconds");
            return result;
            void Pair(byte bcd, string anchor)
            {
                Draw($"Digit.{bcd >> 4}", selected.Anchors[anchor]);
                Draw($"Digit.{bcd & 15}", selected.Anchors[anchor] with { X = selected.Anchors[anchor].X + selected.DigitSpacing });
            }
            void Draw(string frame, MapLabelPoint anchor)
            {
                foreach (var part in selected.Frames[frame])
                    result.AddOnScreenSpritePart(SnesSpritemapXWord.Create(part.OffsetX, part.Size == 16), unchecked((byte)(sbyte)part.OffsetY),
                        SnesObjAttributeWord.Create(part.TileNumber, part.Palette ?? selected.Palette, part.Priority,
                            (part.FlipX ? SnesTileFlipFlags.Horizontal : 0) | (part.FlipY ? SnesTileFlipFlags.Vertical : 0)),
                        unchecked((ushort)(timer.XPixel + anchor.X)), unchecked((ushort)(timer.YPixel + anchor.Y)));
            }
        }
    }
    private static void VerifyLookupStream1FlarePlacement(ISnesAddressSpace rom)
    {
        short Word(int address) => unchecked((short)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));
        byte[] json = ChargeFlarePlacementExtractor.Extract(rom);
        var stock = ChargeFlarePlacementCatalog.Load(new MemoryStream(json));
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var field = typeof(ChargeFlarePlacementCatalog).GetField("offsets", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        AssertEqual(0, ((System.Collections.IDictionary)field.GetValue(stock)!).Count, "Beam flare stock has zero stored fallback origins");
        for (int mode = 0; mode < 2; mode++)
        for (int direction = 0; direction < 16; direction++)
        {
            var expected = new ChargeFlareOffset { X = Word((mode == 0 ? 0x90c1a8 : 0x90c1dc) + 2 * direction),
                Y = Word((mode == 0 ? 0x90c1c2 : 0x90c1f0) + 2 * direction) };
            AssertEqual(expected, ChargeFlarePlacementDefinitions.BeamOffset(mode != 0, direction), "Named flare default matches direct native coordinates and aliases");
            AssertEqual(expected, stock.Resolve(mode != 0, direction), "Installed flare default matches native");
            for (int axis = 0; axis < 2; axis++)
            {
                var document = System.Text.Json.JsonSerializer.Deserialize<ChargeFlarePlacementDocument>(json, options)!;
                string key = ChargeFlarePlacementDefinitions.Key(mode != 0, direction);
                var before = document.Offsets[key];
                document.Offsets[key] = axis == 0 ? before with { X = (short)(before.X + 7) } : before with { Y = (short)(before.Y + 7) };
                var edited = ChargeFlarePlacementCatalog.Load(new MemoryStream(ChargeFlarePlacementCatalog.Write(document)));
                AssertEqual(1, ((System.Collections.IDictionary)field.GetValue(edited)!).Count, "One independent origin edit stores exactly one override");
                for (int selectedMode = 0; selectedMode < 2; selectedMode++)
                for (int selectedDirection = 0; selectedDirection < 16; selectedDirection++)
                    AssertEqual(document.Offsets[ChargeFlarePlacementDefinitions.Key(selectedMode != 0, selectedDirection)],
                        edited.Resolve(selectedMode != 0, selectedDirection), "Editing a source origin does not edit an independently supplied adjacent alias");
            }
        }
        var grappleDocument = System.Text.Json.JsonSerializer.Deserialize<ChargeFlarePlacementDocument>(GrappleFlarePlacementExtractor.Extract(rom), options)!;
        var grapple = ChargeFlarePlacementCatalog.LoadGrapple(new MemoryStream(ChargeFlarePlacementCatalog.Write(grappleDocument)));
        AssertEqual(0, ((System.Collections.IDictionary)field.GetValue(grapple)!).Count, "Grapple stock has zero stored fallback origins");
        for (int mode = 0; mode < 2; mode++)
        for (int direction = 0; direction < 16; direction++)
        {
            var expected = new ChargeFlareOffset { X = Word((mode == 0 ? 0x9bc14a : 0x9bc19a) + direction * 2),
                Y = Word((mode == 0 ? 0x9bc15e : 0x9bc1ae) + direction * 2) };
            AssertEqual(expected, ChargeFlarePlacementDefinitions.GrappleOffset(mode != 0, direction), "Grapple direct defaults preserve all native words including packed adjacent bytes");
            AssertEqual(expected, grapple.Resolve(mode != 0, direction), "Installed Grapple defaults match native");
            for (int axis = 0; axis < 2; axis++)
            {
                var document = System.Text.Json.JsonSerializer.Deserialize<ChargeFlarePlacementDocument>(GrappleFlarePlacementExtractor.Extract(rom), options)!;
                string key = ChargeFlarePlacementDefinitions.Key(mode != 0, direction);
                var before = document.Offsets[key];
                document.Offsets[key] = axis == 0 ? before with { X = (short)(before.X + 7) } : before with { Y = (short)(before.Y + 7) };
                var edited = ChargeFlarePlacementCatalog.LoadGrapple(new MemoryStream(ChargeFlarePlacementCatalog.Write(document)));
                AssertEqual(1, ((System.Collections.IDictionary)field.GetValue(edited)!).Count, "Grapple independent edit stores exactly one override");
                for (int selectedMode = 0; selectedMode < 2; selectedMode++)
                for (int selectedDirection = 0; selectedDirection < 16; selectedDirection++)
                    AssertEqual(document.Offsets[ChargeFlarePlacementDefinitions.Key(selectedMode != 0, selectedDirection)], edited.Resolve(selectedMode != 0, selectedDirection),
                        "Grapple edits preserve every other independent origin and alias");
            }
        }
        foreach (int invalid in new[] { int.MinValue, -1, 16, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(false, invalid), "Flare placement catalog bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => grapple.Resolve(true, invalid), "Grapple catalog bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => ChargeFlarePlacementDefinitions.GrappleOffset(false, invalid), "Grapple calculated bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => ChargeFlarePlacementDefinitions.BeamOffset(true, invalid), "Flare calculated bounds");
        }
        var compositions = ChargeFlareSpriteCatalog.Load(new MemoryStream(ChargeFlareSpriteExtractor.Extract(rom)));
        var body = CreateSamusIdentityFixture();
        var system = new SamusProjectileSystem();
        var draw = typeof(SamusProjectileSystem).GetMethod("DrawFlareComponent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .CreateDelegate<Action<ISnesAddressSpace, OamBuffer, SamusState, ushort, ushort, int, SamusMode7Transform?, ChargeFlarePlacementCatalog?, ChargeFlareSpriteCatalog?>>(system);
        var shiftedDocument = System.Text.Json.JsonSerializer.Deserialize<ChargeFlarePlacementDocument>(json, options)!;
        foreach (string key in shiftedDocument.Offsets.Keys.ToArray())
            shiftedDocument.Offsets[key] = shiftedDocument.Offsets[key] with { X = (short)(shiftedDocument.Offsets[key].X + 7) };
        var shifted = ChargeFlarePlacementCatalog.Load(new MemoryStream(ChargeFlarePlacementCatalog.Write(shiftedDocument)));
        foreach (byte pose in new byte[] { 1, 2, 9, 10 })
        {
            var samus = new SamusState { Pose = pose, XPosition = 100, YPosition = 100 };
            samus.TileTransfers.BindArtwork(body);
            int direction = rom.ReadByte(0x91b629 + pose * 8 + 3);
            bool running = pose is 9 or 10;
            short x = Word((running ? 0x90c1dc : 0x90c1a8) + direction * 2);
            short y = Word((running ? 0x90c1f0 : 0x90c1c2) + direction * 2);
            var expected = new OamBuffer(); var actual = new OamBuffer(); var edited = new OamBuffer();
            DrawImportedFlareSpritemap((SuperMetroidAddressSpace)rom, expected, 0, (ushort)(100 + x),
                unchecked((ushort)(100 + y - unchecked((byte)body.GraphicsYOffset(pose)))));
            draw(new LookupFlareForbiddenBus(), actual, samus, 0, 0, 0, null, stock, compositions);
            draw(new LookupFlareForbiddenBus(), edited, samus, 0, 0, 0, null, shifted, compositions);
            AssertTrue(expected.LowTable.SequenceEqual(actual.LowTable) && expected.HighTable.SequenceEqual(actual.HighTable), "Actual flare OAM matches independently positioned native sprite");
            AssertEqual(expected.NextByteOffset, actual.NextByteOffset, "Native flare OAM admission");
            AssertTrue(actual.NextByteOffset > 0, "Focused flare placement fixture emits its native sprite");
            for (int index = 0; index < actual.NextByteOffset / 4; index++)
            {
                AssertEqual((actual.GetEntry(index).X + 7) & 511, edited.GetEntry(index).X, "Actual flare uses independently edited placement");
                AssertEqual(actual.GetEntry(index).Y, edited.GetEntry(index).Y, "X edit preserves actual flare Y");
            }
        }
        Console.WriteLine("Beam/Grapple flare origins:128 direct native words, zero stock overrides,128 independent edits, bounded aliases and four actual OAM fixtures pass.");
    }
    private sealed class LookupFlareForbiddenBus : ISnesAddressSpace
    {
        void ISnesAddressSpace.WriteByte(int address, byte value) => throw new InvalidOperationException($"Unexpected flare write {address:X6}");
    }
    private static void VerifyLookupStream1(ISnesAddressSpace rom)
    {
        for (int pose = 0; pose <= byte.MaxValue; pose++)
        {
            AssertEqual(rom.ReadByte(0x91b629 + pose * 8), SamusPoseDispatchDefinitions.ReadFacing((byte)pose), "Native named-pose facing case");
            AssertEqual(rom.ReadByte(0x91b62a + pose * 8), SamusPoseDispatchDefinitions.ReadMovement((byte)pose), "Native named-pose raw movement case");
            AssertEqual(rom.ReadByte(0x91b62b + pose * 8), SamusPoseDispatchDefinitions.ReadNoInputPose((byte)pose), "Native named-pose no-input case");
        }
        Suite(nameof(VerifyLookupStream1SamusPolicyDomains), () => VerifyLookupStream1SamusPolicyDomains(rom));
        Suite(nameof(VerifyLookupStream1MetroidLayout), () => VerifyLookupStream1MetroidLayout(rom));
        Suite(nameof(VerifyLookupStream1PuyoAndQuota), () => VerifyLookupStream1PuyoAndQuota(rom));
        Suite(nameof(VerifyLookupStream1OwtchStoke), () => VerifyLookupStream1OwtchStoke(rom));
        Suite(nameof(VerifyLookupStream1VisualCatalogs), () => VerifyLookupStream1VisualCatalogs(rom));
        Suite(nameof(VerifyLookupStream1NuclearWaffle), () => VerifyLookupStream1NuclearWaffle(rom));
        Suite(nameof(VerifyLookupStream1DeathDefinitions), () => VerifyLookupStream1DeathDefinitions(rom));
        Suite(nameof(VerifyLookupStream1Sciser), () => VerifyLookupStream1Sciser(rom));
        Suite(nameof(VerifyLookupStream1PowampMotion), () => VerifyLookupStream1PowampMotion(rom));
        Suite(nameof(VerifyLookupStream1HibashiDragonFireball), () => VerifyLookupStream1HibashiDragonFireball(rom));
        Suite(nameof(VerifyLookupStream1CommonFrames), () => VerifyLookupStream1CommonFrames(rom));
        Suite(nameof(VerifyLookupStream1EnemyMovement), () => VerifyLookupStream1EnemyMovement(rom));
        Suite(nameof(VerifyLookupStream1CadencePrograms), () => VerifyLookupStream1CadencePrograms(rom));
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (ushort medium = 0; medium < 3; medium++)
        {
            for (int variant = 0; variant < 4; variant++)
            {
                var actual = SamusVerticalMotionDefinitions.Launch(medium, (variant & 1) != 0, (variant & 2) != 0);
                int address = 0x909eb9 + 12 * variant + 2 * medium;
                AssertEqual(Word(address), actual.Whole, "stream1 native jump whole speed");
                AssertEqual(Word(address + 6), actual.Fraction, "stream1 native jump fractional speed");
            }
            var hurt = SamusVerticalMotionDefinitions.Knockback(medium);
            AssertEqual(Word(0x909ee9 + 2 * medium), hurt.Whole, "stream1 native knockback whole speed");
            AssertEqual(Word(0x909eef + 2 * medium), hurt.Fraction, "stream1 native knockback fractional speed");
            var bomb = SamusVerticalMotionDefinitions.BombJump(medium);
            AssertEqual(Word(0x909ef5 + 2 * medium), bomb.Whole, "stream1 native bomb whole speed");
            AssertEqual(Word(0x909efb + 2 * medium), bomb.Fraction, "stream1 native bomb fractional speed");
            var gravity = SamusVerticalMotionDefinitions.Gravity(medium);
            AssertEqual(Word(0x909ea7 + 2 * medium), gravity.Whole, "stream1 native whole gravity");
            AssertEqual(Word(0x909ea1 + 2 * medium), gravity.Fraction, "stream1 native fractional gravity");
        }
        foreach (ushort invalid in new ushort[] { 3, ushort.MaxValue })
        {
            for (int variant = 0; variant < 4; variant++)
                AssertThrows<IndexOutOfRangeException>(() => SamusVerticalMotionDefinitions.Launch(invalid, (variant & 1) != 0, (variant & 2) != 0), "stream1 invalid launch medium");
            AssertThrows<IndexOutOfRangeException>(() => SamusVerticalMotionDefinitions.Knockback(invalid), "stream1 invalid hurt medium");
            AssertThrows<IndexOutOfRangeException>(() => SamusVerticalMotionDefinitions.BombJump(invalid), "stream1 invalid bomb medium");
            AssertThrows<IndexOutOfRangeException>(() => SamusVerticalMotionDefinitions.Gravity(invalid), "stream1 invalid gravity medium");
        }
        for (int address = 0x90c254; address <= 0x90c28e; address++)
            AssertEqual(rom.ReadByte(address), SamusProjectileCooldownDefinitions.ReadByte(address), "stream1 all native cooldown bytes including padding");
        AssertEqual(rom.ReadByte(0x90c291), SamusProjectileCooldownDefinitions.ReadByte(0x90c291), "stream1 bounded spacetime beam cooldown");
        foreach (int invalid in new[] { int.MinValue, 0x90c253, 0x90c28f, 0x90c290, 0x90c292, int.MaxValue })
            AssertThrows<InvalidDataException>(() => SamusProjectileCooldownDefinitions.ReadByte(invalid), "stream1 invalid cooldown address");
    }
    private static void VerifyLookupStream1PowampMotion(ISnesAddressSpace rom)
    {
        short Word(int address) => unchecked((short)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));
        for (int phase = 0; phase < 12; phase++)
            AssertEqual(Word(0xa8c1a1 + 2 * phase), PowampMotionDefinitions.WiggleOffset(phase), "Powamp native wiggle triangle");
        for (int pose = 0; pose < 3; pose++)
        {
            AssertEqual(Word(0xa8c277 + 2 * pose), PowampMotionDefinitions.BalloonOffset(pose, false), "Powamp native rising balloon offset");
            AssertEqual(Word(0xa8c27d + 2 * pose), PowampMotionDefinitions.BalloonOffset(pose, true), "Powamp native sinking balloon offset");
        }
        for (int direction = 0; direction < 8; direction++)
        {
            AssertEqual(Word(0x86d21a + 2 * direction), PowampMotionDefinitions.SpikeXAcceleration(direction), "Powamp native spike X acceleration");
            AssertEqual(Word(0x86d22a + 2 * direction), PowampMotionDefinitions.SpikeYAcceleration(direction), "Powamp native spike Y acceleration");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 12, int.MaxValue })
            AssertThrows<InvalidDataException>(() => PowampMotionDefinitions.WiggleOffset(invalid), "Powamp wiggle domain");
        foreach (int invalid in new[] { int.MinValue, -1, 3, int.MaxValue })
            for (int sinking = 0; sinking < 2; sinking++)
                AssertThrows<IndexOutOfRangeException>(() => PowampMotionDefinitions.BalloonOffset(invalid, sinking != 0), "Powamp balloon pose domain");
        foreach (int invalid in new[] { int.MinValue, -1, 8, int.MaxValue })
        {
            AssertThrows<InvalidDataException>(() => PowampMotionDefinitions.SpikeXAcceleration(invalid), "Powamp X direction domain");
            AssertThrows<InvalidDataException>(() => PowampMotionDefinitions.SpikeYAcceleration(invalid), "Powamp Y direction domain");
        }
    }
    private static void VerifyLookupStream1EnemyMovement(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (ushort index = 0; index < 13; index++)
        {
            var intervals = BullMovementDefinitions.Intervals(index);
            AssertEqual(Word(0xa8d895 + 4 * index), intervals.Acceleration, "stream1 Bull acceleration interval");
            AssertEqual(Word(0xa8d897 + 4 * index), intervals.Deceleration, "stream1 Bull deceleration interval");
        }
        for (int direction = 0; direction < 10; direction++)
            AssertEqual(Word(0xa8d871 + 2 * direction), BullMovementDefinitions.ShotAngle(direction), "stream1 Bull compass angle");
        foreach (int invalid in new[] { -1, 10, int.MaxValue })
            AssertThrows<InvalidDataException>(() => BullMovementDefinitions.ShotAngle(invalid), "stream1 Bull invalid direction");
        AssertThrows<InvalidDataException>(() => BullMovementDefinitions.Intervals(13), "stream1 Bull invalid interval");
        for (byte index = 0; index < 6; index++)
            AssertEqual(Word(0xa29f36 + 2 * index), CacatacMovementDefinitions.TravelDistance(index), "stream1 Cacatac patrol blocks");
        foreach (byte invalid in new byte[] { 6, byte.MaxValue })
            AssertThrows<InvalidDataException>(() => CacatacMovementDefinitions.TravelDistance(invalid), "stream1 Cacatac invalid patrol selector");
        for (ushort direction = 0; direction < 20; direction += 2)
            AssertEqual(Word(0x86d96a + direction), CacatacProjectileDefinitions.InstructionList((CacatacSpikeDirection)direction), "stream1 Cacatac named direction program");
        foreach (ushort invalid in new ushort[] { 1, 19, 20, ushort.MaxValue })
            AssertThrows<InvalidDataException>(() => CacatacProjectileDefinitions.InstructionList((CacatacSpikeDirection)invalid), "stream1 Cacatac invalid direction selector");
    }
    private static void VerifyLookupStream1CadencePrograms(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (int address = 0x90c481; address < 0x90c4b5; address++)
        {
            AssertEqual(rom.ReadByte(address), ChargeFlareAnimationDefinitions.ReadByte(address), "stream1 original flare pointer/cadence byte");
            if (address < 0x90c4b4)
                AssertEqual(Word(address), ChargeFlareAnimationDefinitions.ReadWord(address), "stream1 original flare unaligned word");
        }
        foreach (int invalid in new[] { int.MinValue, 0x90c480, 0x90c4b5, int.MaxValue })
            AssertThrows<InvalidDataException>(() => ChargeFlareAnimationDefinitions.ReadByte(invalid), "stream1 flare byte domain");
        Check(0xa80000, 0xd841, 0xd871, BullInstructionProgramDefinitions.MechanicsWordCount,
            BullInstructionProgramDefinitionsTooling.PresentationWordCount,
            i => { var word = BullInstructionProgramDefinitions.MechanicsWord(i); return (word.Address, word.Value); },
            BullInstructionProgramDefinitionsTooling.PresentationWordAddress, BullInstructionProgramDefinitions.ReadMechanicsWord,
            BullInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
            a => a is 0xd843 or 0xd847 or 0xd84b or 0xd84f or 0xd85b or 0xd85f or 0xd863 or 0xd867);
        Check(0x860000, 0xd92e, 0xd96a, CacatacProjectileInstructionProgramDefinitionsTooling.MechanicsWordCount,
            CacatacProjectileInstructionProgramDefinitions.PresentationWordCount,
            i => { var word = CacatacProjectileInstructionProgramDefinitionsTooling.MechanicsWord(i); return (word.Address, word.Value); },
            CacatacProjectileInstructionProgramDefinitions.PresentationWordAddress, CacatacProjectileInstructionProgramDefinitions.ReadMechanicsWord,
            CacatacProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
            a => (a - 0xd930) % 6 == 0);
        for (int address = 0xd840; address <= 0xd871; address++)
            AssertEqual(address is 0xd843 or 0xd847 or 0xd84b or 0xd84f or 0xd85b or 0xd85f or 0xd863 or 0xd867,
                BullInstructionProgramDefinitions.IsPresentationWord((ushort)address), "stream1 Bull exact presentation classification");

        void Check(int bank, int first, int end, int mechanicsCount, int presentationCount,
            Func<int, (ushort Address, ushort Value)> mechanics, Func<int, ushort> presentation,
            Func<ushort, ushort> read, Func<int, bool> owns, Func<int, bool> isVisual)
        {
            int mechanical = 0, visual = 0;
            for (int address = first; address < end; address += 2)
            {
                bool selectedVisual = isVisual(address);
                if (selectedVisual)
                {
                    AssertEqual((ushort)address, presentation(visual++), "stream1 native presentation operand ordering");
                    AssertThrows<InvalidDataException>(() => read((ushort)address), "stream1 presentation excluded from mechanics");
                }
                else
                {
                    var actual = mechanics(mechanical++);
                    AssertEqual((ushort)address, actual.Address, "stream1 native mechanics address ordering");
                    AssertEqual(Word(bank | address), actual.Value, "stream1 original program mechanics value");
                    AssertEqual(actual.Value, read((ushort)address), "stream1 direct program mechanics value");
                }
                AssertEqual(!selectedVisual, owns(bank | address), "stream1 program low-byte ownership");
                AssertEqual(!selectedVisual, owns(bank | (address + 1)), "stream1 program high-byte ownership");
                AssertThrows<InvalidDataException>(() => read((ushort)(address + 1)), "stream1 unaligned program word rejection");
            }
            AssertEqual(mechanicsCount, mechanical, "stream1 exact mechanics count");
            AssertEqual(presentationCount, visual, "stream1 exact presentation count");
            AssertTrue(!owns(bank | (first - 1)) && !owns(bank | end) && !owns((bank ^ 0x10000) | first), "stream1 outside program byte rejection");
            foreach (int invalid in new[] { -1, mechanicsCount, int.MaxValue })
                AssertThrows<IndexOutOfRangeException>(() => mechanics(invalid), "stream1 mechanics enumeration bounds");
            foreach (int invalid in new[] { -1, presentationCount, int.MaxValue })
                AssertThrows<IndexOutOfRangeException>(() => presentation(invalid), "stream1 presentation enumeration bounds");
        }
    }
    private static void VerifyLookupStream1ProjectileMotion(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (int address = 0x90c2d1; address < 0x90c37b; address += 2)
            AssertEqual(Word(address), SamusProjectileMotionDefinitions.ReadWord(address), "stream1 all native projectile motion words");
        foreach (int invalid in new[] { int.MinValue, 0x90c2d0, 0x90c2d2, 0x90c37b, int.MaxValue })
            AssertThrows<InvalidDataException>(() => SamusProjectileMotionDefinitions.ReadWord(invalid), "stream1 exact projectile motion address domain");
        var initialize = typeof(SamusProjectileSystem).GetMethod("InitializePowerBeamVelocity",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!.CreateDelegate<Action<ISnesAddressSpace, SamusProjectileSlot>>();
        var guarded = new BeamSpeedRowAddressSpace(rom);
        for (ushort combination = 0; combination < 16; combination++)
        for (ushort direction = 0; direction < 10; direction++)
        {
            var slot = new SamusProjectileSlot(0) { Type = combination, Direction = direction };
            initialize(guarded, slot);
            int speed = unchecked((short)Word(0x90c2d1 + combination * 4 + (direction is 1 or 3 or 6 or 8 ? 2 : 0)));
            AssertEqual(unchecked((short)(direction is 1 or 2 or 3 ? speed : direction is 6 or 7 or 8 ? -speed : 0)), slot.XVelocity, "stream1 actual initializer X preserves adjacent missile-data overread");
            AssertEqual(unchecked((short)(direction is 0 or 1 or 8 or 9 ? -speed : direction is 3 or 4 or 5 or 6 ? speed : 0)), slot.YVelocity, "stream1 actual initializer Y preserves adjacent missile-data overread");
        }
    }
    private static void VerifyLookupStream1Selection(ISnesAddressSpace rom)
    {
        for (int address = 0x9383c1; address < 0x9386db; address += 2)
        {
            ushort expected = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
            AssertEqual(expected, SamusProjectileSelectionDefinitions.ReadWord(address),
                $"stream1 original projectile selector/header {address:X6}");
            AssertThrows<InvalidDataException>(() => SamusProjectileSelectionDefinitions.ReadWord(address + 1),
                "stream1 unaligned selector/header rejection");
        }
        foreach (int invalid in new[] { int.MinValue, 0x9283c1, 0x9383bf, 0x9386db, 0x9483c1, int.MaxValue })
            AssertThrows<InvalidDataException>(() => SamusProjectileSelectionDefinitions.ReadWord(invalid),
                "stream1 selector exact address-domain rejection");
    }
    private static void VerifyLookupStream1LaunchRoles(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (int index = 0; index < 5; index++)
        {
            var launch = SamusBombSpreadLaunchDefinitions.ForSlot(index);
            AssertEqual(Word(0x90d8cf + index * 2), launch.FuseTimer, "spread native fuse");
            AssertEqual(Word(0x90d8d9 + index * 2), launch.XVelocity, "spread native direction/magnitude X");
            AssertEqual(Word(0x90d8e3 + index * 2), launch.YSpeed, "spread native whole Y");
            AssertEqual(Word(0x90d8ed + index * 2), launch.YSubspeed, "spread native fraction Y");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 5, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => SamusBombSpreadLaunchDefinitions.ForSlot(invalid), "spread native slot rejection");
        for (int index = 0; index < 22; index++)
        {
            var frame = HibashiDefinitions.ActivityFrame(index);
            AssertEqual(Word(0xa68dbb + index * 2), frame.YOffset, "Hibashi native collision rise");
            AssertEqual(Word(0xa68de7 + index * 2), frame.YRadius, "Hibashi native collision radius");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 22, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => HibashiDefinitions.ActivityFrame(invalid), "Hibashi native frame rejection");
        for (ushort selector = 0; selector < 6; selector++)
        {
            AssertEqual(Word(0xa2e5ef + selector * 2), DragonAnimationDefinitions.InstructionList((DragonAnimationSelector)selector), "Dragon native phase/facing program");
            var role = EscapeEtecoonDefinitions.Initialization(selector);
            int offset = selector & ~1;
            AssertEqual(Word(0xb3e718 + offset), role.XPosition, "Etecoon native role X");
            AssertEqual(Word(0xb3e71e + offset), role.YPosition, "Etecoon native role Y");
            AssertEqual(Word(0xb3e724 + offset), (ushort)role.PreInstruction, "Etecoon native role action");
            AssertEqual(Word(0xb3e72a + offset), role.InstructionList, "Etecoon native role program");
            AssertEqual(Word(0xb3e730 + offset), role.HorizontalSpeed, "Etecoon native role speed");
        }
        foreach (ushort invalid in new ushort[] { 6, 7, ushort.MaxValue })
        {
            AssertThrows<InvalidDataException>(() => DragonAnimationDefinitions.InstructionList((DragonAnimationSelector)invalid), "Dragon invalid selector rejection");
            AssertThrows<ArgumentOutOfRangeException>(() => EscapeEtecoonDefinitions.Initialization(invalid), "Etecoon invalid role rejection");
        }
    }
    private static void VerifyLookupStream1SparkSpikePrograms(ISnesAddressSpace rom)
    {
        Check(0xf353, 0xf391, 17, 14,
            a => a < 0xf35f ? (a - 0xf353) % 4 == 2 : a >= 0xf363 && a < 0xf38f && (a - 0xf363) % 4 == 2,
            i => { var w = FallingSparkInstructionProgramDefinitionsTooling.MechanicsWord(i); return (w.Address, w.Value); },
            FallingSparkInstructionProgramDefinitions.PresentationWordAddress,
            FallingSparkInstructionProgramDefinitions.ReadMechanicsWord,
            FallingSparkInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte);
        Check(0xd208, 0xd21a, 6, 3,
            a => a < 0xd214 && (a - 0xd208) % 4 == 2,
            i => { var w = PowampSpikeInstructionProgramDefinitionsTooling.MechanicsWord(i); return (w.Address, w.Value); },
            PowampSpikeInstructionProgramDefinitions.PresentationWordAddress,
            PowampSpikeInstructionProgramDefinitions.ReadMechanicsWord,
            PowampSpikeInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte);

        void Check(int first, int end, int mechanicsCount, int presentationCount, Func<int, bool> isVisual,
            Func<int, (ushort Address, ushort Value)> mechanics, Func<int, ushort> presentation,
            Func<ushort, ushort> read, Func<int, bool> owns)
        {
            int mechanical = 0, visual = 0;
            for (int address = first; address < end; address += 2)
            {
                bool art = isVisual(address);
                if (art)
                {
                    AssertEqual((ushort)address, presentation(visual++), "spark/spike original presentation ordering");
                    AssertThrows<InvalidDataException>(() => read((ushort)address), "spark/spike presentation excluded from mechanics");
                }
                else
                {
                    var actual = mechanics(mechanical++);
                    AssertEqual((ushort)address, actual.Address, "spark/spike original mechanics ordering");
                    ushort native = (ushort)(rom.ReadByte(0x860000 | address) | rom.ReadByte(0x860000 | (address + 1)) << 8);
                    AssertEqual(native, actual.Value, "spark/spike original mechanics word");
                    AssertEqual(native, read((ushort)address), "spark/spike direct mechanics read");
                }
                AssertEqual(!art, owns(0x860000 | address), "spark/spike low-byte ownership");
                AssertEqual(!art, owns(0x860000 | (address + 1)), "spark/spike high-byte ownership");
                AssertThrows<InvalidDataException>(() => read((ushort)(address + 1)), "spark/spike unaligned word rejection");
            }
            AssertEqual(mechanicsCount, mechanical, "spark/spike mechanics extent");
            AssertEqual(presentationCount, visual, "spark/spike presentation extent");
            foreach (int invalid in new[] { -1, mechanicsCount, int.MaxValue })
                AssertThrows<IndexOutOfRangeException>(() => mechanics(invalid), "spark/spike mechanics index bounds");
            foreach (int invalid in new[] { -1, presentationCount, int.MaxValue })
                AssertThrows<IndexOutOfRangeException>(() => presentation(invalid), "spark/spike presentation index bounds");
            foreach (int invalid in new[] { 0x850000 | first, 0x860000 | (first - 1), 0x860000 | end, int.MinValue, int.MaxValue })
                AssertTrue(!owns(invalid), "spark/spike outside byte-domain rejection");
        }
    }
    private static void VerifyLookupStream1CommonFrames(ISnesAddressSpace rom)
    {
        byte[] originalBanks = [0xa0, 0xa2, 0xa3, 0xa4, 0xa5, 0xa6, 0xa7, 0xa8, 0xa9, 0xaa, 0xb2, 0xb3];
        AssertTrue(originalBanks.SequenceEqual(CommonEnemyEmptyExtendedFrameDefinitionsTooling.SupportedBanks), "common empty-frame original bank order");
        for (int bank = 0; bank <= byte.MaxValue; bank++)
        {
            bool expected = originalBanks.Contains((byte)bank);
            AssertEqual(expected, CommonEnemyEmptyExtendedFrameDefinitions.HasFrame((byte)bank, 0x804f), "common empty extended-frame exact bank domain");
            AssertEqual(expected, CommonEnemyEmptyExtendedFrameDefinitions.HasEmptySpritemap((byte)bank, 0x804d), "common empty OAM exact bank domain");
            AssertTrue(!CommonEnemyEmptyExtendedFrameDefinitions.HasFrame((byte)bank, 0x8050), "common empty frame excludes neighboring pointer");
            if (expected)
            {
                AssertEqual((byte)0, rom.ReadByte(bank << 16 | 0x804d), "native common OAM has zero components");
                AssertEqual((byte)1, rom.ReadByte(bank << 16 | 0x804f), "native common extended frame has one component");
            }
        }
        var deletion = CommonEnemyProjectileInstructionProgramDefinitionsTooling.MechanicsWord(0);
        AssertEqual((ushort)0x84fc, deletion.Address, "shared projectile delete identity");
        AssertEqual((ushort)(rom.ReadByte(0x8684fc) | rom.ReadByte(0x8684fd) << 8), deletion.Value, "shared projectile delete native instruction");
        foreach (int invalid in new[] { int.MinValue, -1, 1, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => CommonEnemyProjectileInstructionProgramDefinitionsTooling.MechanicsWord(invalid), "shared delete enumeration bounds");
    }
    private static void VerifyLookupStream1HibashiDragonFireball(ISnesAddressSpace rom)
    {
        Check(0x860000, 0xb4bf, 0xb4ef, 16, 8,
            address => (address - 0xb4bf) % 12 is 2 or 6,
            i => { var w = DragonFireballInstructionProgramDefinitionsTooling.MechanicsWord(i); return (w.Address, w.Value); },
            DragonFireballInstructionProgramDefinitions.PresentationWordAddress,
            DragonFireballInstructionProgramDefinitions.ReadMechanicsWord,
            DragonFireballInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte);
        Check(0xa60000, 0x8d1b, 0x8daf, 50, 24,
            address => address == 0x8dab || address >= 0x8d1f && address <= 0x8da3 && (address - 0x8d1f) % 6 == 0,
            i => { var w = HibashiInstructionProgramDefinitionsTooling.MechanicsWord(i); return (w.Address, w.Value); },
            HibashiInstructionProgramDefinitions.PresentationWordAddress,
            HibashiInstructionProgramDefinitions.ReadMechanicsWord,
            HibashiInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte);
        void Check(int bank, int first, int end, int count, int visualCount, Func<int, bool> isVisual,
            Func<int, (ushort Address, ushort Value)> mechanics, Func<int, ushort> presentation,
            Func<ushort, ushort> read, Func<int, bool> owns)
        {
            int m = 0, p = 0;
            for (int address = first; address < end; address += 2)
            {
                bool visual = isVisual(address);
                if (visual)
                {
                    AssertEqual((ushort)address, presentation(p++), "Hibashi/Dragon native presentation order");
                    AssertThrows<InvalidDataException>(() => read((ushort)address), "Hibashi/Dragon presentation excluded from mechanics");
                }
                else
                {
                    var word = mechanics(m++);
                    AssertEqual((ushort)address, word.Address, "Hibashi/Dragon native control order");
                    ushort value = (ushort)(rom.ReadByte(bank | address) | rom.ReadByte(bank | (address + 1)) << 8);
                    AssertEqual(value, word.Value, "Hibashi/Dragon original native control value");
                    AssertEqual(value, read((ushort)address), "Hibashi/Dragon direct control read");
                }
                AssertEqual(!visual, owns(bank | address), "Hibashi/Dragon low-byte ownership");
                AssertEqual(!visual, owns(bank | (address + 1)), "Hibashi/Dragon high-byte ownership");
                AssertThrows<InvalidDataException>(() => read((ushort)(address + 1)), "Hibashi/Dragon unaligned reads rejected");
            }
            AssertEqual(count, m, "Hibashi/Dragon control count");
            AssertEqual(visualCount, p, "Hibashi/Dragon presentation count");
            foreach (int invalid in new[] { -1, count, int.MaxValue })
                AssertThrows<IndexOutOfRangeException>(() => mechanics(invalid), "Hibashi/Dragon control index domain");
            foreach (int invalid in new[] { -1, visualCount, int.MaxValue })
                AssertThrows<IndexOutOfRangeException>(() => presentation(invalid), "Hibashi/Dragon presentation index domain");
            foreach (int invalid in new[] { bank | (first - 1), bank | end, (bank ^ 0x10000) | first })
                AssertTrue(!owns(invalid), "Hibashi/Dragon external byte domain");
        }
    }
    private static void VerifyLookupStream1Sciser(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var frames = SuperMetroid.Core.Assets.SciserVisualDefinitions.Frames();
        AssertEqual(12, frames.Length, "Sciser visual catalog size");
        string[] names = ["right", "left", "down", "up"];
        for (int surface = 0; surface < 4; surface++)
        for (int frame = 0; frame < 3; frame++)
        {
            var actual = frames[surface * 3 + frame];
            ushort pointer = Word(0xa39681 + surface * 24 + frame * 4);
            AssertEqual(pointer, actual.Pointer, "native Sciser visual identity");
            AssertEqual((byte)0xa3, actual.Bank, "Sciser bank");
            AssertEqual($"sciser_upside_{names[surface]}_{frame}", actual.Name, "Sciser stable editable name");
            AssertEqual((ushort)4, Word(0xa30000 | pointer), "native Sciser four-object record size");
        }
        var controls = new HashSet<ushort>();
        var visuals = new HashSet<ushort>();
        for (int surface = 0; surface < 4; surface++)
        {
            int start = 0x967b + surface * 24;
            foreach (int offset in new[] { 0, 2, 4, 8, 12, 16, 20, 22 }) controls.Add((ushort)(start + offset));
            for (int frame = 0; frame < 4; frame++) visuals.Add((ushort)(start + 6 + frame * 4));
        }
        int controlIndex = 0, visualIndex = 0;
        for (int pointer = 0x967a; pointer <= 0x96dc; pointer++)
        {
            AssertEqual(visuals.Contains((ushort)pointer), SciserInstructionProgramDefinitions.IsPresentationWord((ushort)pointer),
                "Sciser exact presentation domain");
            AssertEqual(controls.Contains((ushort)pointer) || controls.Contains((ushort)(pointer - 1)),
                SciserInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0xa30000 | pointer), "Sciser exact byte guard domain");
            if (controls.Contains((ushort)pointer))
            {
                var actual = SciserInstructionProgramDefinitionsTooling.MechanicsWord(controlIndex++);
                AssertEqual((ushort)pointer, actual.Address, "Sciser native control order");
                AssertEqual(Word(0xa30000 | pointer), actual.Value, "Sciser native control value");
            }
            else AssertThrows<InvalidDataException>(() => SciserInstructionProgramDefinitions.ReadMechanicsWord((ushort)pointer),
                "Sciser rejects every noncontrol address");
            if (visuals.Contains((ushort)pointer))
                AssertEqual((ushort)pointer, SciserInstructionProgramDefinitionsTooling.PresentationWordAddress(visualIndex++),
                    "Sciser native visual order");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 32, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => SciserInstructionProgramDefinitionsTooling.MechanicsWord(invalid), "Sciser control index domain");
        foreach (int invalid in new[] { int.MinValue, -1, 16, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => SciserInstructionProgramDefinitionsTooling.PresentationWordAddress(invalid), "Sciser visual index domain");
    }
    private static void VerifyLookupStream1DeathDefinitions(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var initialFrames = SamusSpecialSequenceRomData.Death.InitialFramesByMovementType;
        AssertEqual(28, initialFrames.Length, "death retail movement domain");
        for (int movement = 0; movement < initialFrames.Length; movement++)
            AssertEqual(rom.ReadByte(0x9bb420 + movement), initialFrames[movement], "native movement death phase");
        var segments = SamusSpecialSequenceRomData.Death.TileSegments;
        AssertEqual(5, segments.Length, "five death graphics transfers");
        for (int index = 0; index < segments.Length; index++)
        {
            AssertEqual(0x9b0000 | Word(0x9bb7bf + index * 2), segments[index].SourceAddress, "native death transfer source");
            AssertEqual(Word(0x9bb7c9 + index * 2), segments[index].EncodedVramDestination, "native death transfer destination");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 28, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => _ = initialFrames[invalid], "death movement exact domain");
        foreach (int invalid in new[] { int.MinValue, -1, 5, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => _ = segments[invalid], "death transfer exact domain");
        int frameIndex = 0, segmentIndex = 0;
        foreach (byte frame in initialFrames)
            AssertEqual(initialFrames[frameIndex++], frame, "death frame enumeration preserves ordering");
        foreach (var segment in segments)
            AssertEqual(segments[segmentIndex++], segment, "death transfer enumeration preserves ordering");
        AssertEqual(initialFrames.Length, frameIndex, "death frame enumerable count");
        AssertEqual(segments.Length, segmentIndex, "death transfer enumerable count");
    }
    private static void VerifyLookupStream1NuclearWaffle(ISnesAddressSpace rom)
    {
        Confirm(0xa6, 0x9490, NuclearWaffleInstructionProgramDefinitions.ReadMechanicsWord,
            NuclearWaffleInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
            index => { var word = NuclearWaffleInstructionProgramDefinitionsTooling.MechanicsWord(index); return (word.Address, word.Value); },
            NuclearWaffleInstructionProgramDefinitions.PresentationWordAddress);
        Confirm(0x86, 0xbb5e, NuclearWaffleProjectileInstructionProgramDefinitions.ReadMechanicsWord,
            NuclearWaffleProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
            index => { var word = NuclearWaffleProjectileInstructionProgramDefinitionsTooling.MechanicsWord(index); return (word.Address, word.Value); },
            NuclearWaffleProjectileInstructionProgramDefinitions.PresentationWordAddress);
        void Confirm(int bank, int start, Func<ushort, ushort> read, Func<int, bool> guarded,
            Func<int, (ushort Address, ushort Value)> mechanics, Func<int, ushort> visual)
        {
            var nativeControls = new HashSet<int>();
            for (int frame = 0; frame < 12; frame++)
            {
                nativeControls.Add(start + frame * 4);
                AssertEqual((ushort)(start + frame * 4 + 2), visual(frame), "Nuclear Waffle native visual positions");
            }
            nativeControls.Add(start + 48);
            nativeControls.Add(start + 50);
            int index = 0;
            for (int pointer = start - 1; pointer <= start + 52; pointer++)
            {
                AssertEqual(nativeControls.Contains(pointer) || nativeControls.Contains(pointer - 1),
                    guarded(bank << 16 | pointer), "Nuclear Waffle exact byte guard");
                if (nativeControls.Contains(pointer))
                {
                    var actual = mechanics(index++);
                    AssertEqual((ushort)pointer, actual.Address, "Nuclear Waffle control ordering");
                    AssertEqual((ushort)(rom.ReadByte(bank << 16 | pointer) | rom.ReadByte(bank << 16 | (pointer + 1)) << 8),
                        actual.Value, "Nuclear Waffle native controls");
                }
                else AssertThrows<InvalidDataException>(() => read((ushort)pointer), "Nuclear Waffle noncontrol rejection");
            }
            foreach (int invalid in new[] { int.MinValue, -1, 14, int.MaxValue })
                AssertThrows<IndexOutOfRangeException>(() => mechanics(invalid), "Nuclear Waffle control index bounds");
            foreach (int invalid in new[] { int.MinValue, -1, 12, int.MaxValue })
                AssertThrows<IndexOutOfRangeException>(() => visual(invalid), "Nuclear Waffle visual index bounds");
        }
    }
    private static void VerifyLookupStream1VisualCatalogs(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var selectors = ChargeFlareSpriteDefinitions.Selectors;
        var nativePointers = new List<ushort>();
        byte[] json = ChargeFlareSpriteExtractor.Extract(rom);
        var stock = ChargeFlareSpriteCatalog.Load(new MemoryStream(json));
        for (ushort selector = 0; selector < 54; selector++)
        {
            ushort pointer = Word(0x93a1a1 + selector * 2);
            AssertEqual(pointer, selectors[selector], "native charge flare phased selector");
            if (!nativePointers.Contains(pointer)) nativePointers.Add(pointer);
            var expected = new OamBuffer();
            var actual = new OamBuffer();
            DrawImportedFlareSpritemap((SuperMetroidAddressSpace)rom, expected, selector, 100, 100);
            stock.Draw(selector, actual, 100, 100);
            AssertTrue(expected.LowTable.SequenceEqual(actual.LowTable) && expected.HighTable.SequenceEqual(actual.HighTable),
                "calculated flare selector draws exact native composition");
            AssertEqual(expected.NextByteOffset, actual.NextByteOffset, "calculated flare native OAM cursor");
        }
        AssertEqual(28, nativePointers.Count, "native unique flare identity count");
        AssertTrue(nativePointers.SequenceEqual(ChargeFlareSpriteDefinitions.NativePointers), "native flare identity order preserved");
        for (int index = 0; index < nativePointers.Count; index++)
            AssertEqual((ushort)(index < 3 ? 1 : index == 3 ? 4 : 3), Word(0x930000 | nativePointers[index]), "native flare object record sizes");
        foreach (int invalid in new[] { int.MinValue, -1, 54, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => _ = selectors[invalid], "charge flare selector bounds");
        foreach (int invalid in new[] { int.MinValue, -1, 28, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => _ = ChargeFlareSpriteDefinitions.NativePointers[invalid], "flare pointer bounds");
        for (int digit = 0; digit < 10; digit++)
        {
            ushort pointer = EscapeTimerPresentationDefinitions.DigitSpritemapPointer(digit);
            AssertEqual(Word(0x809fd4 + digit * 2), pointer, "native escape timer digit pointer");
            AssertEqual((ushort)2, Word(0x800000 | pointer), "native two-object digit record");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 10, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => EscapeTimerPresentationDefinitions.DigitSpritemapPointer(invalid), "escape digit bounds");
        var dragon = DragonVisualDefinitions.Frames();
        ushort[] operands = [0xe59d, 0xe5a3, 0xe5a7, 0xe5af, 0xe5b5, 0xe5b9, 0xe5c1, 0xe5c5, 0xe5c9, 0xe5d9, 0xe5dd, 0xe5e1];
        string[] names = ["body_idle_left", "wing_left_0", "wing_left_1", "body_idle_right", "wing_right_0", "wing_right_1",
            "body_attack_left_0", "body_attack_left_1", "body_attack_left_2", "body_attack_right_0", "body_attack_right_1", "body_attack_right_2"];
        AssertEqual(12, dragon.Length, "Dragon visual identity count");
        for (int index = 0; index < dragon.Length; index++)
        {
            AssertEqual(Word(0xa20000 | operands[index]), dragon[index].Pointer, "native Dragon visual pointer");
            AssertEqual((byte)0xa2, dragon[index].Bank, "Dragon visual bank");
            AssertEqual("dragon_" + names[index], dragon[index].Name, "Dragon stable editable identity");
            AssertEqual((ushort)(index is 1 or 2 or 4 or 5 ? 1 : 8), Word(0xa20000 | dragon[index].Pointer), "native Dragon record size");
        }
    }
    private static void VerifyLookupStream1OwtchStoke(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var controls = new HashSet<int>();
        var visuals = new HashSet<int>();
        for (int side = 0; side < 2; side++)
        {
            foreach (int offset in new[] { 0, 2, 6, 10, 14, 16 }) controls.Add(0xa3ab + side * 18 + offset);
            foreach (int offset in new[] { 4, 8, 12 }) visuals.Add(0xa3ab + side * 18 + offset);
        }
        int controlIndex = 0, visualIndex = 0;
        for (int pointer = 0xa3aa; pointer <= 0xa3d0; pointer++)
        {
            AssertEqual(controls.Contains(pointer) || controls.Contains(pointer - 1),
                OwtchInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0xa20000 | pointer), "Owtch exact byte guard domain");
            if (controls.Contains(pointer))
            {
                var actual = OwtchInstructionProgramDefinitionsTooling.MechanicsWord(controlIndex++);
                AssertEqual((ushort)pointer, actual.Address, "Owtch native control order");
                AssertEqual(Word(0xa20000 | pointer), actual.Value, "Owtch native controls");
            }
            else AssertThrows<InvalidDataException>(() => OwtchInstructionProgramDefinitions.ReadMechanicsWord((ushort)pointer), "Owtch noncontrol rejection");
            if (visuals.Contains(pointer))
                AssertEqual((ushort)pointer, OwtchInstructionProgramDefinitionsTooling.PresentationWordAddress(visualIndex++), "Owtch native visual order");
            else AssertThrows<InvalidDataException>(() => OwtchStokeVisualDefinitions.FrameAt(EnemyDefinitionId.Owtch, (ushort)pointer), "Owtch nonvisual rejection");
        }
        var stokeVisuals = new HashSet<int>();
        for (int side = 0; side < 2; side++)
            foreach (int offset in new[] { 4, 8, 12, 16, 24, 32 }) stokeVisuals.Add(0x8932 + side * 38 + offset);
        for (int pointer = 0x8931; pointer <= 0x897f; pointer++)
            if (stokeVisuals.Contains(pointer))
                AssertEqual(Word(0xa20000 | pointer), OwtchStokeVisualDefinitions.FrameAt(EnemyDefinitionId.Stoke, (ushort)pointer), "Stoke native visual selection");
            else AssertThrows<InvalidDataException>(() => OwtchStokeVisualDefinitions.FrameAt(EnemyDefinitionId.Stoke, (ushort)pointer), "Stoke nonvisual rejection");
        foreach (int invalid in new[] { int.MinValue, -1, 12, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => OwtchInstructionProgramDefinitionsTooling.MechanicsWord(invalid), "Owtch control bounds");
        foreach (int invalid in new[] { int.MinValue, -1, 6, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => OwtchInstructionProgramDefinitionsTooling.PresentationWordAddress(invalid), "Owtch visual bounds");
        AssertThrows<InvalidDataException>(() => OwtchStokeVisualDefinitions.FrameAt(0, 0xa3af), "unknown visual owner rejected");
    }
    private static void VerifyLookupStream1PuyoAndQuota(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (ushort argument = 0; argument <= 24; argument += 2)
        {
            ushort pointer = Word(0x84db28 + argument);
            AssertEqual(pointer, SuperMetroid.Core.Rooms.MetroidsClearedPlmRomData.ResolvePreInstruction(argument), "native Metroid quota dispatcher identity");
            var eventNumber = SuperMetroid.Core.Rooms.MetroidsClearedPlmRomData.ResolveEvent(argument);
            if (argument < 18)
            {
                AssertTrue(eventNumber is null, "nine no-op quota identities have no event");
                AssertEqual((byte)0x60, rom.ReadByte(0x840000 | pointer), "native distinct no-op RTS");
            }
            else AssertEqual(Word(0x840000 | (pointer + 9)), (ushort)eventNumber!.Value, "native quota observer event operand");
        }
        foreach (ushort invalid in new ushort[] { 1, 17, 25, 26, ushort.MaxValue })
        {
            AssertThrows<InvalidDataException>(() => SuperMetroid.Core.Rooms.MetroidsClearedPlmRomData.ResolvePreInstruction(invalid), "quota pointer argument domain");
            AssertThrows<InvalidDataException>(() => SuperMetroid.Core.Rooms.MetroidsClearedPlmRomData.ResolveEvent(invalid), "quota event argument domain");
        }
        var controls = new HashSet<int>();
        var visuals = new HashSet<int>();
        for (int loop = 0; loop < 3; loop++)
        {
            foreach (int offset in new[] { 0, 4, 8, 12, 16, 18 }) controls.Add(0x99ad + loop * 20 + offset);
            foreach (int offset in new[] { 2, 6, 10, 14 }) visuals.Add(0x99ad + loop * 20 + offset);
        }
        for (int frame = 0; frame < 5; frame++)
        {
            controls.Add(0x99e9 + frame * 6);
            controls.Add(0x99ed + frame * 6);
            visuals.Add(0x99eb + frame * 6);
        }
        int controlIndex = 0, visualIndex = 0;
        for (int pointer = 0x99ac; pointer <= 0x9a08; pointer++)
        {
            AssertEqual(controls.Contains(pointer) || controls.Contains(pointer - 1),
                PuyoInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0xa20000 | pointer), "Puyo exact byte guard domain");
            AssertEqual(visuals.Contains(pointer), PuyoInstructionProgramDefinitions.IsPresentationWord((ushort)pointer), "Puyo exact visual domain");
            if (controls.Contains(pointer))
            {
                var actual = PuyoInstructionProgramDefinitionsTooling.MechanicsWord(controlIndex++);
                AssertEqual((ushort)pointer, actual.Address, "Puyo native control order");
                AssertEqual(Word(0xa20000 | pointer), actual.Value, "Puyo native control value");
            }
            else AssertThrows<InvalidDataException>(() => PuyoInstructionProgramDefinitions.ReadMechanicsWord((ushort)pointer), "Puyo noncontrol rejection");
            if (visuals.Contains(pointer)) AssertEqual((ushort)pointer, PuyoInstructionProgramDefinitionsTooling.PresentationWordAddress(visualIndex++), "Puyo native visual order");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 28, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => PuyoInstructionProgramDefinitionsTooling.MechanicsWord(invalid), "Puyo control bounds");
        foreach (int invalid in new[] { int.MinValue, -1, 17, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => PuyoInstructionProgramDefinitionsTooling.PresentationWordAddress(invalid), "Puyo visual bounds");
    }
    private static void VerifyLookupStream1MetroidLayout(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var frames = MetroidVisualDefinitions.Frames();
        AssertEqual(4, frames.Length, "Metroid body identities");
        for (int frame = 0; frame < 4; frame++)
        {
            AssertEqual(Word(0xa3e9d1 + frame * 4), frames[frame].Pointer, "native Metroid body identity");
            AssertEqual((byte)0xa3, frames[frame].Bank, "Metroid body bank");
            AssertEqual($"metroid_body_{frame}", frames[frame].Name, "Metroid stable editable name");
            AssertEqual((ushort)(frame == 1 ? 6 : 8), Word(0xa30000 | frames[frame].Pointer), "native Metroid OAM record size");
        }
        var controls = new HashSet<int>();
        var visuals = new HashSet<int>();
        foreach (var (start, count) in new[] { (0xe9cf, 20), (0xea25, 5) })
        {
            for (int frame = 0; frame < count; frame++)
            {
                controls.Add(start + frame * 4);
                visuals.Add(start + frame * 4 + 2);
            }
            for (int control = 0; control < 3; control++) controls.Add(start + count * 4 + control * 2);
        }
        int controlIndex = 0, visualIndex = 0;
        for (int pointer = 0xe9ce; pointer <= 0xea40; pointer++)
        {
            AssertEqual(controls.Contains(pointer) || controls.Contains(pointer - 1),
                MetroidInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0xa30000 | pointer), "Metroid exact byte guard");
            AssertEqual(visuals.Contains(pointer), MetroidInstructionProgramDefinitions.IsPresentationWord((ushort)pointer), "Metroid exact visual domain");
            if (controls.Contains(pointer))
            {
                var actual = MetroidInstructionProgramDefinitionsTooling.MechanicsWord(controlIndex++);
                AssertEqual((ushort)pointer, actual.Address, "Metroid original control order");
                AssertEqual(Word(0xa30000 | pointer), actual.Value, "Metroid original control value");
            }
            else AssertThrows<InvalidDataException>(() => MetroidInstructionProgramDefinitions.ReadMechanicsWord((ushort)pointer), "Metroid noncontrol rejection");
            if (visuals.Contains(pointer)) AssertEqual((ushort)pointer, MetroidInstructionProgramDefinitionsTooling.PresentationWordAddress(visualIndex++), "Metroid original visual order");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 31, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => MetroidInstructionProgramDefinitionsTooling.MechanicsWord(invalid), "Metroid control index bounds");
        foreach (int invalid in new[] { int.MinValue, -1, 25, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => MetroidInstructionProgramDefinitionsTooling.PresentationWordAddress(invalid), "Metroid visual index bounds");
    }
    private static void VerifyLookupStream1SamusPolicyDomains(ISnesAddressSpace rom)
    {
        for (int pose = 0; pose <= byte.MaxValue; pose++)
        {
            if (pose is >= 0xc9 and <= 0xce)
            {
                var angles = SamusShinesparkProjectileRomData.DepartureAngles((byte)pose);
                AssertEqual(rom.ReadByte(0x90d4c6 + (pose - 0xc9) * 2), angles.First.TableIndex, "native crash first departure angle");
                AssertEqual(rom.ReadByte(0x90d4c7 + (pose - 0xc9) * 2), angles.Second.TableIndex, "native crash opposite departure angle");
            }
            else AssertThrows<InvalidOperationException>(() => SamusShinesparkProjectileRomData.DepartureAngles((byte)pose), "crash departure exact pose domain");
        }
        foreach (byte invalid in new byte[] { 28, byte.MaxValue })
        {
            AssertThrows<IndexOutOfRangeException>(() => SamusHudDefinitions.MovementHandler((SamusMovementType)invalid), "HUD movement exact domain");
            AssertThrows<InvalidDataException>(() => SamusAtmosphericEffectDefinitions.WaterSplashFor((SamusMovementType)invalid), "splash exact movement domain");
        }
        foreach (ushort invalid in new ushort[] { 10, ushort.MaxValue })
            AssertThrows<InvalidDataException>(() => SamusAtmosphericEffectDefinitions.IsRunningFootContact(invalid), "foot-contact exact frame domain");
        foreach (byte invalid in new byte[] { 16, byte.MaxValue })
            AssertThrows<InvalidDataException>(() => SamusAtmosphericEffectDefinitions.ForCrateriaRoom(invalid), "atmospheric room exact domain");
        foreach (var (header, room) in new[] { (0x91f8, 0), (0x93fe, 5), (0x948c, 7), (0x94fd, 9), (0x9552, 10), (0x957d, 11), (0x95a8, 12), (0x95ff, 14) })
            AssertEqual((byte)room, rom.ReadByte(0x8f0000 | header), "native atmospheric room header identity");
    }
    private static void VerifyLookupStream1ArmCannonTileSources(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var nativeSources = new SortedSet<ushort>();
        for (int direction = 0; direction < 10; direction++)
        {
            int list = 0x900000 | Word(0x90c7a5 + direction * 2);
            for (int frame = 1; frame < 4; frame++) nativeSources.Add(Word(list + frame * 2));
        }
        ushort[] expected = nativeSources.ToArray();
        var sources = SamusArmCannonArtworkFormat.TileSourcePointers;
        AssertEqual(expected.Length, sources.Length, "Calculated cannon source count matches distinct native list operands");
        AssertTrue(expected.SequenceEqual(sources), "Calculated cannon source enumeration retains original ascending tile order");
        for (int index = 0; index < expected.Length; index++)
            AssertEqual(expected[index], sources[index], "Calculated cannon source indexing retains native identity");
        for (int word = 0; word <= ushort.MaxValue; word++)
        {
            int nativeIndex = Array.IndexOf(expected, (ushort)word);
            AssertEqual(nativeIndex, sources.IndexOf((ushort)word), "Calculated cannon reverse index preserves complete word domain");
            AssertEqual(nativeIndex >= 0, sources.Contains((ushort)word), "Calculated cannon membership rejects interior addresses");
        }
        foreach (int invalid in new[] { int.MinValue, -1, expected.Length, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => _ = sources[invalid], "Calculated cannon source index bounds preserve array contract");
        using var directory = new TestTempDirectory("map-catalog");
        SamusArmCannonArtworkFiles.Extract(rom, directory.Root, SupportedCartridge.Sha256);
        byte[] json = File.ReadAllBytes(Path.Combine(directory.Root, SamusArmCannonArtworkFormat.JsonFileName));
        byte[] png = File.ReadAllBytes(Path.Combine(directory.Root, SamusArmCannonArtworkFormat.TileFileName));
        var stock = SamusArmCannonArtworkCatalogTooling.Load(new MemoryStream(json), new MemoryStream(png));
        foreach (string fieldName in new[] { "attributes", "tileSources" })
        {
            var field = typeof(SamusArmCannonArtworkCatalog).GetField(fieldName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
            var overrides = (Dictionary<int, ushort>)field.GetValue(stock)!;
            AssertEqual(0, overrides.Count, "Stock cannon selectors require no stored fallback overrides");
        }
        for (int direction = 0; direction < 10; direction++)
        {
            var aim = (SamusProjectileDirection)direction;
            AssertEqual(Word(0x90c791 + direction * 2), SamusArmCannonArtworkFormat.StockSpriteAttributes(aim),
                "Direct calculated cannon OBJ defaults match original native words");
            int list = 0x900000 | Word(0x90c7a5 + direction * 2);
            for (int frame = 0; frame < 4; frame++)
                AssertEqual(Word(list + frame * 2), SamusArmCannonArtworkFormat.StockTileSource(aim, frame),
                    "Direct calculated cannon source defaults match original native operands");
        }
        byte[] nativePlanar = expected.SelectMany(pointer => Enumerable.Range(0, 32).Select(offset => rom.ReadByte((0x9a0000 | pointer) + offset))).ToArray();
        AssertEqual(ReferenceIdentity(json, nativePlanar), stock.ContentIdentity, "Calculated cannon selectors preserve canonical native content identity");
        Suite(nameof(VerifySelectors), () => VerifySelectors(stock, -1, 0));
        for (int direction = 0; direction < 10; direction++)
        {
            var document = System.Text.Json.Nodes.JsonNode.Parse(json)!;
            ushort replacement = expected[(sources.IndexOf(stock.TileSource(direction, 2)) + 1) % expected.Length];
            document["tileSources"]![direction]![2] = replacement;
            document["spriteAttributes"]![direction] = stock.SpriteAttributes(direction) ^ 0x4000;
            byte[] changedJson = System.Text.Encoding.UTF8.GetBytes(document.ToJsonString());
            var changed = SamusArmCannonArtworkCatalogTooling.Load(new MemoryStream(changedJson), new MemoryStream(png));
            VerifySelectors(changed, direction, replacement);
            AssertEqual(ReferenceIdentity(changedJson, nativePlanar), changed.ContentIdentity, "Independent cannon selector edits preserve canonical hash framing/order");
            VerifySelectors(stock, -1, 0);
        }
        foreach (int invalid in new[] { int.MinValue, -1, 10, int.MaxValue })
        {
            AssertThrows<IndexOutOfRangeException>(() => stock.SpriteAttributes(invalid), "Cannon OBJ selector bounds remain exact");
            AssertThrows<IndexOutOfRangeException>(() => stock.TileSource(invalid, 0), "Cannon tile direction bounds remain exact");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 4, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => stock.TileSource(0, invalid), "Cannon cover frame bounds remain exact");
        Verify(stock, false);
        var image = IndexedPng.Read(new MemoryStream(png), sources.Length * 8, 8);
        byte[] pixels = (byte[])image.Pixels.Clone();
        pixels[5 * 8] ^= 1;
        using var editedPng = new MemoryStream();
        IndexedPng.Write(editedPng, image.Width, image.Height, pixels, image.Palette);
        editedPng.Position = 0;
        var edited = SamusArmCannonArtworkCatalogTooling.Load(new MemoryStream(json), editedPng);
        Verify(edited, true);
        Verify(stock, false);
        AssertTrue(!stock.TryResolveTile(0x9b0000 | expected[0], 32, out _), "Cannon transfer retains native bank boundary");
        AssertTrue(!stock.TryResolveTile(0x9a0000 | expected[0], 31, out _), "Cannon transfer retains native byte-count boundary");
        AssertTrue(!stock.TryResolveTile((0x9a0000 | expected[0]) + 1, 32, out _), "Cannon transfer rejects interior source address");

        void VerifySelectors(SamusArmCannonArtworkCatalog catalog, int editedDirection, ushort replacement)
        {
            for (int direction = 0; direction < 10; direction++)
            {
                ushort expectedAttributes = Word(0x90c791 + direction * 2);
                if (direction == editedDirection) expectedAttributes ^= 0x4000;
                AssertEqual(expectedAttributes, catalog.SpriteAttributes(direction), "Calculated OBJ reflections preserve native attributes and independent edits");
                int list = 0x900000 | Word(0x90c7a5 + direction * 2);
                for (int frame = 0; frame < 4; frame++)
                {
                    ushort expectedSource = direction == editedDirection && frame == 2 ? replacement : Word(list + frame * 2);
                    AssertEqual(expectedSource, catalog.TileSource(direction, frame), "Calculated cannon orientation/frame preserves native selections and independent edits");
                }
            }
        }

        static string ReferenceIdentity(byte[] jsonBytes, byte[] planar)
        {
            using var document = System.Text.Json.JsonDocument.Parse(jsonBytes);
            var root = document.RootElement;
            return SelectedPresentationHash.Create(nameof(SamusArmCannonArtworkCatalog), content =>
            {
                content.AppendWords("pose pointers", root.GetProperty("posePointers").EnumerateArray().Select(value => (ushort)value.GetInt32()).ToArray());
                content.Append("drawing data", root.GetProperty("drawingData").EnumerateArray().Select(value => (byte)value.GetInt32()).ToArray());
                content.AppendWords("attributes", root.GetProperty("spriteAttributes").EnumerateArray().Select(value => (ushort)value.GetInt32()).ToArray());
                foreach (var direction in root.GetProperty("tileSources").EnumerateArray())
                    content.AppendWords("tile sources", direction.EnumerateArray().Select(value => (ushort)value.GetInt32()).ToArray());
                content.Append("characters", planar);
            });
        }
        void Verify(SamusArmCannonArtworkCatalog catalog, bool hasEdit)
        {
            for (int index = 0; index < expected.Length; index++)
            {
                int source = 0x9a0000 | expected[index];
                AssertTrue(catalog.TryResolveTile(source, 32, out var tile), "Actual cannon transfer resolves every calculated identity");
                for (int offset = 0; offset < 32; offset++)
                {
                    byte native = rom.ReadByte(source + offset);
                    if (hasEdit && index == 5 && offset == 0) native ^= 0x80;
                    AssertEqual(native, tile.Span[offset], "Calculated identity retains original tile bytes and isolated edited pixel");
                }
            }
        }
    }
    private static void VerifyLookupStream1CannonDrawingControls(ISnesAddressSpace rom)
    {
        using var directory = new TestTempDirectory("map-catalog");
        SamusArmCannonArtworkFiles.Extract(rom, directory.Root, SupportedCartridge.Sha256);
        byte[] json = File.ReadAllBytes(Path.Combine(directory.Root, SamusArmCannonArtworkFormat.JsonFileName));
        byte[] png = File.ReadAllBytes(Path.Combine(directory.Root, SamusArmCannonArtworkFormat.TileFileName));
        var document = System.Text.Json.JsonSerializer.Deserialize<SamusArmCannonArtworkDocument>(json,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        var tiles = RoomCharacterAtlas.Load(new MemoryStream(png), SamusArmCannonArtworkFormat.TileSourcePointers.Length * 32);
        SamusArmCannonArtworkCatalog Load(SamusArmCannonArtworkDocument value) => SamusArmCannonArtworkCatalog.FromPlacement(
            SamusArmCannonArtworkCatalog.LoadPlacement(new MemoryStream(SamusArmCannonArtworkCatalog.Write(value))), tiles);
        var stock = Load(document);
        var field = typeof(SamusArmCannonArtworkCatalog).GetField("drawingData",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        Dictionary<int, byte> Stored(SamusArmCannonArtworkCatalog value) => (Dictionary<int, byte>)field.GetValue(value)!;
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var headers = new HashSet<int>();
        var descriptors = new SortedSet<int>();
        for (int pose = 0; pose < 253; pose++)
        {
            int pointer = Word(0x90C7DF + pose * 2);
            descriptors.Add(pointer);
            headers.Add(pointer); headers.Add(pointer + 1);
            if ((rom.ReadByte(0x900000 | pointer) & 0x80) != 0)
            { headers.Add(pointer + 2); headers.Add(pointer + 3); }
        }
        AssertEqual(130, headers.Count, "cannon original distinct direction/mode control-byte count");
        for (int address = 0xCC21; address < 0xCC39; address++) headers.Add(address);
        var aliases = new Dictionary<int, int>();
        int[] boundaries = descriptors.Append(0xCC21).ToArray();
        for (int descriptor = 0; descriptor < boundaries.Length - 1; descriptor++)
        {
            int pointer = boundaries[descriptor], next = boundaries[descriptor + 1];
            if ((rom.ReadByte(0x900000 | pointer) & 0x80) != 0 || next - pointer <= 4) continue;
            bool constantPair = true;
            for (int address = pointer + 4; address < next; address++)
                constantPair &= rom.ReadByte(0x900000 | address) == rom.ReadByte(0x900000 | (pointer + 2 + ((address - pointer) & 1)));
            if (constantPair)
                for (int address = pointer + 4; address < next; address++) aliases.Add(address, pointer + 2 + ((address - pointer) & 1));
            else
            {
                bool fixedX = true;
                for (int address = pointer + 4; address < next; address += 2)
                    fixedX &= rom.ReadByte(0x900000 | address) == rom.ReadByte(0x900000 | (pointer + 2));
                if (fixedX)
                {
                    for (int address = pointer + 4; address < next; address += 2) aliases.Add(address, pointer + 2);
                    int frames = (next - pointer - 2) / 2;
                    if (frames is 6 or 10)
                    {
                        bool repeatedHalf = true;
                        for (int address = pointer + 3 + frames; address < next; address += 2)
                            repeatedHalf &= rom.ReadByte(0x900000 | address) == rom.ReadByte(0x900000 | (address - frames));
                        if (repeatedHalf)
                            for (int address = pointer + 3 + frames; address < next; address += 2)
                                aliases.Add(address, address - frames);
                    }
                }
            }
        }
        foreach ((int earlierPose, int laterPose) in new (int, int)[]
        {
            (0x01,0x02), (0x07,0x08), (0x0F,0x10), (0x11,0x12),
            (0x13,0x14), (0x17,0x18), (0x51,0x52), (0x69,0x6A),
            (0x6B,0x6C), (0x67,0x68), (0x2D,0x2E), (0x6D,0x6E),
            (0x6F,0x70), (0x27,0x28), (0x73,0x74), (0x49,0x4A),
            (0x75,0x76), (0x77,0x78), (0x55,0x56),
        })
        {
            int earlier = Word(0x90C7DF + earlierPose * 2), later = Word(0x90C7DF + laterPose * 2);
            int end = boundaries[Array.IndexOf(boundaries, later) + 1] - later;
            for (int offset = 3; offset < end; offset += 2)
            {
                AssertEqual(rom.ReadByte(0x900000 | (earlier + offset)), rom.ReadByte(0x900000 | (later + offset)),
                    "native horizontally reflected poses share this vertical profile");
                if (!aliases.ContainsKey(later + offset)) aliases.Add(later + offset, earlier + offset);
            }
        }
        foreach ((int earlierPose, int laterPose) in new (int, int)[] { (0x03,0x04), (0x15,0x16), (0x85,0x86) })
        {
            int earlierY = Word(0x90C7DF + earlierPose * 2) + 7, laterY = Word(0x90C7DF + laterPose * 2) + 7;
            AssertEqual(rom.ReadByte(0x900000 | earlierY), rom.ReadByte(0x900000 | laterY), "native fully upward secondary pose shares Y");
            aliases.Add(laterY, earlierY);
        }
        var reflections = new Dictionary<int, int>();
        // Native pose identities independently select the paired descriptors; no
        // production reflection selector participates in this expected mapping.
        foreach ((int earlierPose, int laterPose) in new (int, int)[]
        {
            (0x01,0x02), (0x03,0x04), (0x05,0x06), (0x07,0x08),
            (0x0B,0x0C), (0x0F,0x10), (0x11,0x12), (0x13,0x14),
            (0x15,0x16), (0x51,0x52), (0x69,0x6A), (0x6B,0x6C),
            (0x67,0x68), (0x2B,0x2C), (0x6D,0x6E), (0x6F,0x70),
            (0x27,0x28), (0x71,0x72), (0x73,0x74), (0x85,0x86),
            (0x49,0x4A), (0x75,0x76), (0x77,0x78), (0x55,0x56),
        })
        {
            int earlier = Word(0x90C7DF + earlierPose * 2), later = Word(0x90C7DF + laterPose * 2);
            bool variableDirection = (rom.ReadByte(0x900000 | later) & 0x80) != 0;
            int first = variableDirection ? 4 : 2;
            int end = variableDirection ? boundaries[Array.IndexOf(boundaries, later) + 1] - later : first + 2;
            for (int offset = first; offset < end; offset += 2)
            {
                int source = earlier + offset, target = later + offset;
                byte expected = unchecked((byte)(-unchecked((sbyte)rom.ReadByte(0x900000 | source)) - 8));
                AssertEqual(expected, rom.ReadByte(0x900000 | target), "native paired cover spans reflect around Samus origin");
                reflections.Add(target, source);
            }
        }
        AssertEqual(29, reflections.Count, "cannon native reflected origin count");
        AssertEqual(290, aliases.Count, "cannon original repeated coordinate-byte domain");
        const int start = 0xC9D9, count = 608, coordinateCount = 135;
        AssertEqual(coordinateCount, Stored(stock).Count, "cannon stores exactly required coordinate basis");
        for (int index = 0; index < count; index++)
        {
            ushort address = (ushort)(start + index);
            byte expected = rom.ReadByte(0x900000 | address);
            bool calculated = SamusArmCannonArtworkFormat.TryStockDrawingByte(address, out byte value);
            AssertEqual(headers.Contains(address), calculated, "cannon only original controls/cost aliases are calculated");
            if (calculated) AssertEqual(expected, value, "cannon direct original header/cost calculation");
            bool alias = SamusArmCannonArtworkFormat.TryStockCoordinateSource(address, out ushort source);
            AssertEqual(aliases.ContainsKey(address), alias, "cannon direct native constant-origin domain");
            if (alias)
            {
                AssertEqual((ushort)aliases[address], source, "cannon direct original first-pair source");
                AssertTrue(source < address, "cannon coordinate aliases strictly decrease");
            }
            bool reflection = SamusArmCannonArtworkFormat.TryStockReflectedXSource(address, out ushort reflectedSource);
            AssertEqual(reflections.ContainsKey(address), reflection, "cannon native reflected-origin domain");
            if (reflection)
            {
                AssertEqual((ushort)reflections[address], reflectedSource, "cannon original opposite-facing source");
                AssertTrue(reflectedSource < address && !calculated && !alias, "cannon reflection is an acyclic independent source relation");
                AssertEqual(expected, SamusArmCannonArtworkFormat.ReflectCoverX(rom.ReadByte(0x900000 | reflectedSource)), "cannon reflected native default");
            }
            AssertEqual(!calculated && !alias && !reflection, Stored(stock).ContainsKey(index), "cannon exact coordinate storage membership");
            AssertEqual(expected, stock.ReadDrawingByte(address), "cannon complete installed byte window");
            int[] drawing = document.DrawingData.ToArray(); drawing[index] ^= 0xFF;
            var edited = Load(document with { DrawingData = drawing });
            int expectedStored = 0;
            for (int selected = 0; selected < count; selected++)
            {
                int selectedAddress = start + selected;
                bool mandatory = !headers.Contains(selectedAddress) && !aliases.ContainsKey(selectedAddress) && !reflections.ContainsKey(selectedAddress);
                int selectedDefault = aliases.TryGetValue(selectedAddress, out int nativeSource)
                    ? drawing[nativeSource - start]
                    : reflections.TryGetValue(selectedAddress, out int reflectedNativeSource)
                        ? unchecked((byte)(-unchecked((sbyte)drawing[reflectedNativeSource - start]) - 8))
                        : document.DrawingData[selected];
                bool shouldStore = mandatory || drawing[selected] != selectedDefault;
                if (shouldStore) expectedStored++;
                AssertEqual(shouldStore, Stored(edited).ContainsKey(selected), "cannon exact supplied-basis exception membership");
            }
            AssertEqual(expectedStored, Stored(edited).Count, "cannon stores only basis and independently supplied differences");
            for (int other = 0; other < count; other++)
                AssertEqual((byte)(other == index ? drawing[index] : document.DrawingData[other]),
                    edited.ReadDrawingByte((ushort)(start + other)), "cannon independent control/coordinate/cost edit");
            AssertTrue(stock.ContentIdentity != edited.ContentIdentity, "cannon every supplied byte changes selected identity");
        }
        foreach (ushort address in new ushort[] { 0, start - 1, start + count, ushort.MaxValue })
            AssertThrows<InvalidDataException>(() => stock.ReadDrawingByte(address), "cannon drawing bounds remain exact");
        Console.WriteLine("Cannon drawing:130 native controls,24 cost aliases,290 shared coordinates,29 reflected origins,135 exact basis bytes and608 independent edits pass.");
    }

    private static void VerifyLookupStream1AtmosphericAttributes(ISnesAddressSpace rom)
    {
        ushort[] Native(int address) => Enumerable.Range(0, 4).Select(frame =>
            (ushort)(rom.ReadByte(address + frame * 2) | rom.ReadByte(address + frame * 2 + 1) << 8)).ToArray();
        ushort[] one = Native(0x908C0F), shared = Native(0x908C17);
        var stock = new SamusAtmosphericArtworkCatalog(one, shared);
        int Stored(SamusAtmosphericArtworkCatalog catalog, string name) =>
            ((System.Collections.IDictionary)typeof(SamusAtmosphericArtworkCatalog).GetField(name,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(catalog)!).Count;
        string Identity(ushort[] first, ushort[] second) => SelectedPresentationHash.Create(nameof(SamusAtmosphericArtworkCatalog), hash =>
        { hash.AppendWords("type one", first); hash.AppendWords("shared type four", second); });
        AssertEqual(Identity(one, shared), stock.ContentIdentity, "atmospheric canonical selected identity unchanged");
        AssertEqual(0, Stored(stock, "typeOne"), "atmospheric footsteps contain no stock overrides");
        AssertEqual(0, Stored(stock, "sharedTypeFour"), "atmospheric lava/dust contains no stock overrides");
        for (int kind = 0; kind < 2; kind++)
        for (int frame = 0; frame < 4; frame++)
        {
            ushort expected = (kind == 0 ? one : shared)[frame];
            AssertEqual(expected, SamusAtmosphericArtworkDefinitions.Attributes(kind == 0, frame), "atmospheric direct native packed default");
            var packed = new SnesObjAttributeWord(expected);
            AssertEqual(SamusAtmosphericArtworkDefinitions.Palette, packed.PaletteIndex, "atmospheric required palette basis");
            AssertEqual(SamusAtmosphericArtworkDefinitions.Priority, packed.Priority, "atmospheric required priority basis");
            AssertEqual((kind == 0 ? SamusAtmosphericArtworkDefinitions.FootstepFirstTile : SamusAtmosphericArtworkDefinitions.LavaDustFirstTile) + frame,
                packed.TileNumber, "atmospheric nine-bit tile progression");
            ushort[] editOne = one.ToArray(), editShared = shared.ToArray();
            ushort changedWord = (ushort)(expected ^ 0xFFFF);
            (kind == 0 ? editOne : editShared)[frame] = changedWord;
            var edited = new SamusAtmosphericArtworkCatalog(editOne, editShared);
            AssertEqual(1, Stored(edited, kind == 0 ? "typeOne" : "sharedTypeFour"), "atmospheric one full-word supplied exception");
            AssertEqual(0, Stored(edited, kind == 0 ? "sharedTypeFour" : "typeOne"), "atmospheric other list unchanged");
            AssertEqual(Identity(editOne, editShared), edited.ContentIdentity, "atmospheric edited identity uses exact supplied words");
            editOne[frame] = 0; editShared[frame] = 0;
            foreach (byte type in new byte[] { 1, 4, 6, 7 })
            for (byte other = 0; other < 4; other++)
            {
                AssertTrue(edited.TryResolve(type, other, out ushort value), "atmospheric admitted type/frame");
                AssertEqual((type == 1) == (kind == 0) && other == frame ? changedWord : (type == 1 ? one : shared)[other], value,
                    "atmospheric supplied full word and caller-input isolation");
            }
        }
        AssertTrue(!stock.TryResolve(2, 0, out _), "atmospheric null pointer remains outside installed domain");
        AssertTrue(!stock.TryResolve(1, 4, out _), "atmospheric next frame rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => _ = stock.TypeOne[-1], "atmospheric negative view index");
        Console.WriteLine("Atmospheric attributes: eight direct native defaults, exact OBJ field basis, zero stock overrides, eight full-word edits and canonical identities pass.");
    }

    private static void VerifyLookupStream1EscapeText(ISnesAddressSpace rom)
    {
        byte[] json = EscapeTypewriterExtractor.Extract(rom);
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var document = System.Text.Json.JsonSerializer.Deserialize<EscapeTypewriterDocument>(json, options)!;
        EscapeTypewriterPresentation Load(EscapeTypewriterDocument value)
        {
            using var output = new MemoryStream();
            EscapeTypewriterPresentation.Write(output, value);
            output.Position = 0;
            return EscapeTypewriterPresentation.Load(output);
        }
        var stock = Load(document);
        int Stored(EscapeTypewriterProgram program) => ((System.Collections.IDictionary)program.Lines.GetType()
            .GetField("overrides", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .GetValue(program.Lines)!).Count;
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        int totalFrames = 0;
        foreach (EscapeTypewriterProgramId id in new[] { EscapeTypewriterProgramId.Ceres, EscapeTypewriterProgramId.Zebes })
        {
            var program = stock.Get(id);
            var original = document.Programs[id.ToString()].Lines;
            AssertEqual(original.Length, EscapeTypewriterDefinitions.LineCount(id), "escape text required line grouping");
            AssertEqual(0, Stored(program), "escape text stock contains no sampled line overrides");
            for (int index = 0; index < original.Length; index++)
            {
                var expected = new EscapeTypewriterLine((ushort)original[index].Destination, original[index].Text);
                AssertEqual(expected, EscapeTypewriterDefinitions.Line(id, index), "escape text direct original placement and wording");
                AssertEqual(expected, program.Lines[index], "escape text installed line");
                var editedLines = original.ToArray();
                editedLines[index] = original[index] with { Text = "TEST!", Destination = original[index].Destination + 7 };
                var programs = new Dictionary<string, EscapeTypewriterProgramDocument>(document.Programs)
                { [id.ToString()] = new() { Lines = editedLines } };
                var edited = Load(document with { Programs = programs });
                AssertEqual(1, Stored(edited.Get(id)), "escape text single supplied line exception");
                for (int other = 0; other < original.Length; other++)
                    AssertEqual(other == index ? new EscapeTypewriterLine((ushort)editedLines[index].Destination, "TEST!")
                        : program.Lines[other], edited.Get(id).Lines[other], "escape text independent line edit");
            }
            // Independently interpret original control words and characters; the production
            // side receives only the installed calculated view and no cartridge reader.
            ushort tileBase = id == EscapeTypewriterProgramId.Ceres ? (ushort)0x3582 : (ushort)0x2610;
            var actual = new EscapeTypewriterState(program, tileBase);
            var actualVram = new SnesVram(); var expectedVram = new SnesVram();
            int cursor = EscapeTypewriterDefinitions.SourceAddress(id), glyphs = 0;
            ushort destination = 0, delay = 0, timer = 0;
            bool done = false;
            for (int frame = 0; !done && frame < 1000; frame++)
            {
                bool click = false;
                if (timer != 0) timer--;
                else
                {
                    timer = delay;
                    while (true)
                    {
                        ushort command = Word(cursor);
                        if (command == 0) { done = true; break; }
                        if (command == 1) { delay = Word(cursor + 2); cursor += 4; continue; }
                        if (command == 13) { destination = Word(cursor + 2); cursor += 4; continue; }
                        byte character = rom.ReadByte(cursor++);
                        if (character != ' ')
                        {
                            int glyph = character == '!' ? '[' : character;
                            expectedVram.ExecuteWordTransfer([(ushort)(tileBase + glyph - 'A')], destination, 1);
                            click = ++glyphs % 2 == 0;
                        }
                        destination++;
                        break;
                    }
                }
                AssertEqual(done, actual.Step(actualVram), "escape text native completion");
                AssertEqual(destination, actual.Destination, "escape text native destination");
                AssertEqual(delay, actual.Delay, "escape text native delay");
                AssertEqual(timer, actual.DelayTimer, "escape text native countdown");
                AssertEqual(glyphs, actual.GlyphsWritten, "escape text native glyph count");
                AssertEqual(click, actual.ClickRequested, "escape text native click order");
                AssertTrue(expectedVram.Bytes.SequenceEqual(actualVram.Bytes), "escape text native per-character VRAM");
                totalFrames++;
            }
            AssertTrue(done, "escape text native program terminates");
            var extraLines = original.Append(new EscapeTypewriterLineDocument { Destination = 0x6000, Text = "EXTRA!" }).ToArray();
            var extraPrograms = new Dictionary<string, EscapeTypewriterProgramDocument>(document.Programs)
            { [id.ToString()] = new() { Lines = extraLines } };
            var extra = Load(document with { Programs = extraPrograms }).Get(id);
            AssertEqual(original.Length + 1, extra.Lines.Count, "escape text accepts independently added line");
            AssertEqual(new EscapeTypewriterLine(0x6000, "EXTRA!"), extra.Lines[^1], "escape text preserves added content");
            AssertEqual(1, Stored(extra), "escape text stores only added line");
            extraPrograms[id.ToString()] = new() { Lines = original.Take(1).ToArray() };
            AssertEqual(1, Load(document with { Programs = extraPrograms }).Get(id).Lines.Count, "escape text preserves shortened document");
            AssertThrows<ArgumentOutOfRangeException>(() => _ = program.Lines[-1], "escape text negative line");
            AssertThrows<ArgumentOutOfRangeException>(() => _ = program.Lines[original.Length], "escape text next line");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Get(EscapeTypewriterProgramId.None), "escape text rejects absent program");
        Console.WriteLine($"Escape text: five direct native lines, zero stock overrides, independent edits/line counts and {totalFrames} actual native-oracle frames pass.");
    }

    private static void VerifyLookupStream1VisorColors(ISnesAddressSpace rom)
    {
        var document = System.Text.Json.JsonSerializer.Deserialize<SamusVisorColorDocument>(
            SamusVisorColorExtractor.Extract(rom), new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        SamusVisorColorCatalog Load(SamusVisorColorDocument value) =>
            SamusVisorColorCatalog.Load(new MemoryStream(SamusVisorColorCatalog.Write(value)));
        var stock = Load(document);
        var field = typeof(SamusVisorColorCatalog).GetField("colors",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        Dictionary<int, Bgr555> Stored(SamusVisorColorCatalog value) => (Dictionary<int, Bgr555>)field.GetValue(value)!;
        ushort Native(int index)
        {
            int address = SamusVisorColorFormat.SourceAddress + index * 2;
            return (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        }
        AssertEqual(0, Stored(stock).Count, "visor stock stores no sampled fallback colors");
        AssertEqual(Native(0), SamusVisorColorDefinitions.WideningStart, "visor required widening basis");
        AssertEqual(Native(3), SamusVisorColorDefinitions.FullBeamStart, "visor required full-beam basis");
        for (int channel = 0; channel < 3; channel++)
            AssertEqual(SamusVisorColorDefinitions.DarkeningStep,
                ((Native(3) >> (channel * 5)) & 31) - ((Native(4) >> (channel * 5)) & 31), "visor required darkening basis");
        for (int edited = 0; edited < SamusVisorColorFormat.ColorCount; edited++)
        {
            AssertEqual(Native(edited), SamusVisorColorDefinitions.Color(edited), $"visor direct native default {edited}");
            var changedColors = document.Colors.ToArray();
            ushort changedWord = (ushort)(Native(edited) ^ 0x7FFF);
            changedColors[edited] = new PaletteRgb5 { Red = changedWord & 31, Green = (changedWord >> 5) & 31, Blue = changedWord >> 10 };
            var changed = Load(document with { Colors = changedColors });
            AssertEqual(1, Stored(changed).Count, "visor independent edit stores exactly one exception");
            AssertEqual(changedWord, Stored(changed)[edited], "visor exception preserves supplied RGB5");
            for (int index = 0; index < SamusVisorColorFormat.ColorCount; index++)
                AssertEqual(index == edited ? changedWord : Native(index), changed.Resolve(index), "visor supplied edit is independent");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(-1), "visor rejects negative index");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(6), "visor rejects next index");
        AssertTrue(!stock.TryResolveByteOffset(-2, out _), "visor rejects negative offset");
        Console.WriteLine("Visor: six direct native defaults, exact two-color/step basis, zero stock overrides and six independent edits pass.");
    }

    private static void VerifyLookupStream1WorldForeground(ISnesAddressSpace rom)
    {
        byte[] Read(int address, int count) => Enumerable.Range(0, count).Select(i => rom.ReadByte(address + i)).ToArray();
        byte[] front = Read(WorldMapArtworkFormat.ForegroundSource, WorldMapArtworkFormat.ForegroundBytes);
        byte[] back = Read(WorldMapArtworkFormat.BackgroundSource, WorldMapArtworkFormat.BackgroundBytes);
        byte[] pixels = SnesGraphics.DecodePlanarTiles(front, 4, 16, out int width, out int height);
        byte[] backPixels = SnesGraphics.DecodePlanarTiles(back, 2, 16, out _, out int backHeight);
        byte[] Png(byte[] selected, int selectedHeight, int colors)
        {
            using var png = new MemoryStream(); IndexedPng.Write(png, width, selectedHeight, selected, SnesGraphics.DiagnosticPalette(colors));
            return png.ToArray();
        }
        byte[] backPng = Png(backPixels, backHeight, 4);
        WorldMapArtwork Create(byte[] selected) => WorldMapArtwork.Load(new MemoryStream(Png(selected, height, 16)), new MemoryStream(backPng));
        var field = typeof(WorldMapArtwork).GetField("foreground", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        var stock = Create(pixels);
        var stored = (Dictionary<int, byte>)field.GetValue(stock)!;
        ulong NativeMask(int tile)
        {
            ulong mask = 0;
            for (int y = 0; y < 8; y++) for (int x = 0; x < 8; x++)
                if (pixels[(tile / 16 * 8 + y) * width + tile % 16 * 8 + x] == 14) mask |= 1UL << (y * 8 + x);
            return mask;
        }
        var masks = (Dictionary<int, ulong>)typeof(WorldMapArtwork).GetField("fontFill",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(stock)!;
        AssertEqual(300, masks.Count, "Exact source font masks after blank and named glyph aliases");
        foreach (var pair in masks) AssertEqual(NativeMask(pair.Key), pair.Value, "Exact selected native font footprint");
        int calculated = 0, fontDifferences = 0;
        var edits = new HashSet<int>();
        for (int index = 0; index < pixels.Length; index++)
        {
            int source = WorldMapTileDefinitions.ForegroundSourcePixel(index);
            if (source >= 0)
                AssertEqual(source, WorldMapTileDefinitions.ForegroundSourcePixel(source), "Foreground source dependencies are fixed and acyclic");
            AssertEqual(pixels[index], source < 0 ? (byte)0 : pixels[source], $"Direct foreground relation at pixel{index}");
            bool font = WorldMapTileDefinitions.TryForegroundFontPixel(index, NativeMask, out byte fontValue);
            bool required = source == index && (!font || pixels[index] != fontValue);
            AssertEqual(required, stored.ContainsKey(index), "Exact selected source/contour basis; zero stock relation exceptions");
            if (source == index && font && pixels[index] != fontValue) fontDifferences++;
            if (source != index) { calculated++; if (source >= 0) edits.Add(source); }
        }
        AssertEqual(14347, stored.Count, "Exact drawing, contour and primitive source inputs");
        AssertEqual(10845, calculated, "Exact geometry and shared-source relations");

        AssertEqual(360, fontDifferences, "Exact selected glyph bevel/contour decisions");
        void Check(WorldMapArtwork art, byte[] expected)
        {
            var vram = new SnesVram(); art.LoadTo(vram);
            AssertTrue(vram.Bytes.Slice(WorldMapArtworkFormat.ForegroundDestination, front.Length).SequenceEqual(
                SnesPlanarTileEncoder.Encode(expected, width, height, 4)), "Complete independent foreground upload");
            AssertTrue(vram.Bytes.Slice(WorldMapArtworkFormat.BackgroundDestination, back.Length).SequenceEqual(back), "Completed BG3 remains exact");
        }
        Check(stock, pixels);
        for (int tile = 0; tile < WorldMapArtworkFormat.ForegroundTileCount; tile++)
            edits.Add(tile / 16 * 8 * width + tile % 16 * 8);
        foreach (int index in edits)
        {
            byte[] edited = (byte[])pixels.Clone();
            edited[index] ^= 15;
            Check(Create(edited), edited);
        }
        byte[] inverted = pixels.Select(p => (byte)(p ^ 15)).ToArray();
        Check(Create(inverted), inverted);
        AssertThrows<ArgumentOutOfRangeException>(() => WorldMapTileDefinitions.ForegroundSourcePixel(-1), "Foreground lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => WorldMapTileDefinitions.ForegroundSourcePixel(pixels.Length), "Foreground upper bound");
        Console.WriteLine($"World foreground:{pixels.Length} native pixels,{calculated} exact source relations,{stored.Count} source pixels/{masks.Count} masks/{fontDifferences} font differences,{edits.Count} independent tile/source edits/full inversion/full uploads pass.");
    }
    private static void VerifyLookupStream1ProjectileRadiusLayout(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var expected = new List<ushort>();
        for (int address = 0x9386db; address < 0x93a1a1;)
        {
            ushort word = Word(address);
            if (address == 0x93a117)
            {
                AssertEqual((ushort)0, word, "Native intervening empty spritemap record");
                address += 2;
            }
            else if ((word & 0x8000) == 0)
            {
                expected.Add(unchecked((ushort)address));
                address += 8;
            }
            else if (word == SamusProjectileRomData.Instructions.GoTo) address += 4;
            else
            {
                AssertEqual(SamusProjectileRomData.Instructions.Delete, word, "Only native terminal delete remains");
                address += 2;
            }
        }
        AssertEqual(805, expected.Count, "Independent native timed-record count");
        AssertTrue(SamusProjectileRadiusDefinitions.TimedRecordPointers.SequenceEqual(expected), "Exact calculated pointer domain/order");
        AssertEqual(expected.Count, SamusProjectileRadiusDefinitions.TimedRecordPointers.Count, "Calculated pointer view count");
        for (int index = 0; index < expected.Count; index++)
        {
            ushort pointer = expected[index];
            AssertEqual(pointer, SamusProjectileRadiusDefinitions.TimedRecordPointers[index], "Calculated indexed pointer");
            AssertTrue(SamusProjectileInstructionDefinitions.TryTimedFrame(0x930000 | pointer, out var frame), "Native timed frame classified");
            AssertTrue(frame.Axis >= 0 && frame.Phase >= 0, "Native semantic axes are nonnegative");
            AssertTrue(SamusProjectileRadiusDefinitions.TryCalculatedPair(frame, out ushort pair), "Every native frame has a calculated family policy");
            AssertEqual(Word(0x930000 | (pointer + 4)), pair, "Direct family policy has no native-table fallback");
            for (int component = 0; component < 2; component++)
                AssertEqual(rom.ReadByte(0x930000 | (pointer + 4 + component)),
                    SamusProjectileRadiusDefinitions.ReadByte(0x930000 | (pointer + 4 + component)), "Native physical radius bytes preserved");
        }
        var document = new ProjectileFrameBindingDocument
        {
            Version = 1,
            Frames = expected.ToDictionary(pointer => ProjectileFrameBindingFormat.FrameName(pointer),
                pointer => $"sprite_{Word(0x930000 | (pointer + 2)):X4}"),
        };
        var guard = new LookupFlareForbiddenBus();
        var runBomb = typeof(SamusBombProjectileSystem).GetMethod("RunProjectileInstructionHandler",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        int interpreted = 0, bombInterpreted = 0;
        foreach (bool edited in new[] { false, true })
        {
            if (edited)
                foreach (ushort pointer in expected) document.Frames[ProjectileFrameBindingFormat.FrameName(pointer)] = "sprite_A117";
            var bindings = ProjectileFrameBindingCatalog.Load(new MemoryStream(ProjectileFrameBindingCatalog.Write(document)));
            var shots = new SamusProjectileSystem { FrameBindings = bindings };
            var bombs = new SamusBombProjectileSystem { FrameBindings = bindings };
            foreach (ushort pointer in expected)
            {
                ushort duration = Word(0x930000 | pointer);
                ushort sprite = edited ? (ushort)0xa117 : Word(0x930000 | (pointer + 2));
                var slot = new SamusProjectileSlot(0) { InstructionTimer = 1, InstructionPointer = pointer };
                AssertTrue(!shots.RunProjectileInstructionHandler(slot), "Actual native timed frame survives");
                AssertEqual(duration, slot.InstructionTimer, "Actual timer unchanged");
                AssertEqual(sprite, slot.SpritemapPointer, "Independent installed artwork selection");
                AssertEqual((ushort)rom.ReadByte(0x930000 | (pointer + 4)), slot.XRadius, "Actual X radius independent of art edit");
                AssertEqual((ushort)rom.ReadByte(0x930000 | (pointer + 5)), slot.YRadius, "Actual Y radius independent of art edit");
                AssertEqual(Word(0x930000 | (pointer + 6)), slot.AnimationFrame, "Actual trail phase unchanged");
                AssertEqual(unchecked((ushort)(pointer + 8)), slot.InstructionPointer, "Actual record progression unchanged");
                interpreted++;
                if (duration == 0) continue; // Existing bomb contract excludes zero-duration timed records.
                var bomb = new SamusBombProjectileSlot(0) { InstructionTimer = 1, InstructionPointer = pointer, Type = 0x0500 };
                AssertTrue(!(bool)runBomb.Invoke(bombs, [bomb])!, "Actual bomb timed frame survives");
                AssertEqual(duration, bomb.InstructionTimer, "Actual bomb timer unchanged");
                AssertEqual(sprite, bomb.SpritemapPointer, "Independent bomb artwork selection");
                AssertEqual((ushort)rom.ReadByte(0x930000 | (pointer + 4)), bomb.XRadius, "Actual bomb X radius");
                AssertEqual((ushort)rom.ReadByte(0x930000 | (pointer + 5)), bomb.YRadius, "Actual bomb Y radius");
                AssertEqual(unchecked((ushort)(pointer + 8)), bomb.InstructionPointer, "Actual bomb record progression");
                bombInterpreted++;
            }
        }        for (int step = 0; step < 4; step++)
            AssertEqual(-4 - unchecked((sbyte)rom.ReadByte(0x93ae70 + 7 * step)),
                ProjectileWaveEnvelopeDefinitions.AxialLobeDistance(step), "Shared lobe source derives from centered native OAM");
        AssertEqual((Word(0x93d64c) & 0x1ff) + 4, ProjectileWaveEnvelopeDefinitions.SpazerInitialAxialSpread,
            "Shared initial Spazer lane source");
        var radiusAddresses = expected.SelectMany(pointer => new[] { 0x930000 | (pointer + 4), 0x930000 | (pointer + 5) }).ToHashSet();
        for (int address = 0x9386db; address < 0x93a1a1; address++)
            if (!radiusAddresses.Contains(address))
                AssertThrows<InvalidDataException>(() => SamusProjectileRadiusDefinitions.ReadByte(address), "Every non-radius operand/control byte remains rejected");        var identities = expected.ToHashSet();
        for (int address = 0x9386db; address < 0x93a1a1; address++)
            AssertEqual(identities.Contains(unchecked((ushort)address)),
                SamusProjectileInstructionDefinitions.TryTimedFrame(address, out _), "Exact full bounded classifier admission");
        foreach (int address in new[] { int.MinValue, -1, 0, 0x9386da, 0x93a1a1, int.MaxValue })
            AssertTrue(!SamusProjectileInstructionDefinitions.TryTimedFrame(address, out _), "Classifier rejects outside domain without overflow");
        AssertThrows<IndexOutOfRangeException>(() => _ = SamusProjectileRadiusDefinitions.TimedRecordPointers[-1], "Original lower indexed view rejection");
        AssertThrows<IndexOutOfRangeException>(() => _ = SamusProjectileRadiusDefinitions.TimedRecordPointers[805], "Original upper indexed view rejection");
        AssertEqual((byte)0, SamusProjectileRadiusDefinitions.ReadByte(SamusProjectileRadiusDefinitions.MurderBeamRadiusAddress), "Separate bounded MurderBeam X contract preserved");
        AssertEqual((byte)0, SamusProjectileRadiusDefinitions.ReadByte(SamusProjectileRadiusDefinitions.MurderBeamRadiusAddress + 1), "Separate bounded MurderBeam Y contract preserved");
        Console.WriteLine($"Projectile radii:805 native calculated records/1610 bytes,{interpreted} actual projectile/{bombInterpreted} bomb records with zero reads and independent art edits; exact domain/order/bounds pass.");
    }
    private static void VerifyLookupStream1ProjectilePrograms(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var programs = new List<(ushort Start, int Ticks)>();
        var records = new List<ushort>();
        var mechanics = new HashSet<int>();
        int programStart = 0x9386db, ticks = 1;
        for (int address = programStart; address < 0x93a1a1;)
        {
            if (address == 0x93a117) { address += 2; programStart = address; continue; }
            ushort word = Word(address);
            mechanics.Add(address);
            if ((word & 0x8000) == 0)
            {
                AssertTrue(word > 0, "Native projectile durations are positive");
                mechanics.Add(address + 6);
                records.Add(unchecked((ushort)address));
                ticks += word;
                address += 8;
            }
            else
            {
                if (word == 0x8239) { mechanics.Add(address + 2); address += 4; }
                else { AssertEqual((ushort)0x822f, word, "Native terminal deletion opcode"); address += 2; }
                programs.Add((unchecked((ushort)programStart), ticks));
                programStart = address; ticks = 1;
            }
        }
        AssertEqual(805, records.Count, "Original timed-record scope");
        AssertEqual(1816, mechanics.Count, "Original mechanics-word scope");
        AssertEqual(105, programs.Count, "Independent native complete program domains");
        foreach (int address in mechanics)
            AssertEqual(Word(address), SamusProjectileInstructionDefinitions.ReadWord(address), "Every native duration/trail/control/target word");
        for (int address = 0x9386db; address < 0x93a1a1; address++)
            if (!mechanics.Contains(address))
                AssertThrows<InvalidDataException>(() => SamusProjectileInstructionDefinitions.ReadWord(address), "Every nonmechanics byte remains rejected");
        var document = new ProjectileFrameBindingDocument
        {
            Version = 1,
            Frames = records.ToDictionary(pointer => ProjectileFrameBindingFormat.FrameName(pointer),
                pointer => $"sprite_{Word(0x930000 | (pointer + 2)):X4}"),
        };
        var bindings = ProjectileFrameBindingCatalog.Load(new MemoryStream(ProjectileFrameBindingCatalog.Write(document)));
        var guard = new LookupFlareForbiddenBus();
        var runBomb = typeof(SamusBombProjectileSystem).GetMethod("RunProjectileInstructionHandler",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        var readTrail = typeof(SamusProjectileSystem).GetMethod("GetTrailAnimationFrame",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!;
        int actualTicks = 0, loops = 0, deletions = 0;
        foreach (var program in programs)
        {
            var shots = new SamusProjectileSystem { FrameBindings = bindings };
            var bombs = new SamusBombProjectileSystem { FrameBindings = bindings };
            var slot = new SamusProjectileSlot(0) { InstructionTimer = 1, InstructionPointer = program.Start, Damage = 30 };
            var bomb = new SamusBombProjectileSlot(0) { InstructionTimer = 1, InstructionPointer = program.Start, Type = 0x0500 };
            ushort timer = 1, pointer = program.Start, sprite = 0, x = 0, y = 0, trail = 0;
            var visited = new HashSet<ushort>();
            bool finished = false;
            for (int tick = 0; tick < program.Ticks; tick++)
            {
                bool deleted = false, looped = false;
                timer--;
                if (timer == 0)
                {
                    ushort word = Word(0x930000 | pointer);
                    if (word == 0x8239)
                    {
                        pointer = Word(0x930000 | (pointer + 2));
                        word = Word(0x930000 | pointer);
                    }
                    if (word == 0x822f) deleted = true;
                    else
                    {
                        AssertTrue(word is > 0 and < 0x8000, "Native command reaches a timed frame");
                        looped = !visited.Add(pointer);
                        timer = word; sprite = Word(0x930000 | (pointer + 2));
                        x = rom.ReadByte(0x930000 | (pointer + 4)); y = rom.ReadByte(0x930000 | (pointer + 5));
                        trail = Word(0x930000 | (pointer + 6)); pointer += 8;
                    }
                }
                AssertEqual(deleted, shots.RunProjectileInstructionHandler(slot), "Exact native projectile deletion tick");
                AssertEqual(deleted, (bool)runBomb.Invoke(bombs, [bomb])!, "Exact native bomb deletion tick");
                actualTicks++;
                if (deleted)
                {
                    AssertEqual((ushort)0, slot.Damage, "Projectile deletion clears occupancy");
                    AssertEqual((ushort)0, bomb.Type, "Bomb deletion clears occupancy");
                    deletions++; finished = true; break;
                }
                AssertEqual((timer, pointer, sprite, x, y, trail),
                    (slot.InstructionTimer, slot.InstructionPointer, slot.SpritemapPointer, slot.XRadius, slot.YRadius, slot.AnimationFrame),
                    "Every scheduled projectile exposure/physical envelope/trail phase matches native");
                AssertEqual((timer, pointer, sprite, x, y),
                    (bomb.InstructionTimer, bomb.InstructionPointer, bomb.SpritemapPointer, bomb.XRadius, bomb.YRadius),
                    "Every scheduled bomb exposure/physical envelope matches native");
                slot.AnimationFrame = 0xbeef;
                AssertEqual(trail, (ushort)readTrail.Invoke(null, [slot])!, "Actual trail consumer reads prior record, not next phase or cached frame");
                slot.AnimationFrame = trail;
                if (looped) { loops++; finished = true; break; }
            }
            AssertTrue(finished, "Complete native startup and first loop or terminal deletion confirmed");
        }
        Console.WriteLine($"Projectile programs:1816 native words,805 records,105 complete programs,{actualTicks} exact ticks per owner,{loops} loop returns/{deletions} terminal deletions, zero runtime reads; original domain preserved.");
    }
    private static void VerifyLookupStream1CannonPlacement(ISnesAddressSpace rom)
    {
        using var directory = new TestTempDirectory("map-catalog");
        SamusBodyArtworkFiles.Extract(rom, directory.Root, SupportedCartridge.Sha256);
        SamusBodyArtworkCatalog body = SamusBodyArtworkFiles.Load(directory.Root, null);
        byte[] json = File.ReadAllBytes(Path.Combine(directory.Root, SamusArmCannonArtworkFormat.JsonFileName));
        byte[] png = File.ReadAllBytes(Path.Combine(directory.Root, SamusArmCannonArtworkFormat.TileFileName));
        var document = System.Text.Json.JsonSerializer.Deserialize<SamusArmCannonArtworkDocument>(json,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        var tiles = RoomCharacterAtlas.Load(new MemoryStream(png), SamusArmCannonArtworkFormat.TileSourcePointers.Length * 32);
        SamusArmCannonArtworkCatalog Load(SamusArmCannonArtworkDocument value) => SamusArmCannonArtworkCatalog.FromPlacement(
            SamusArmCannonArtworkCatalog.LoadPlacement(new MemoryStream(SamusArmCannonArtworkCatalog.Write(value))), tiles);
        var field = typeof(SamusArmCannonArtworkCatalog).GetField("drawingData",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        Dictionary<int, byte> Stored(SamusArmCannonArtworkCatalog value) => (Dictionary<int, byte>)field.GetValue(value)!;
        var stock = body.ArmCannon;
        AssertEqual(0, Stored(stock).Count, "Installed cannon has zero stock coordinate fallbacks: " +
            string.Join(", ", Stored(stock).Select(pair => $"{pair.Key + 0xc9d9:X4}={pair.Value:X2}")));
        AssertEqual(Load(document).ContentIdentity, stock.ContentIdentity, "Body binding preserves canonical cannon identity");
        int basis = 0;
        for (int index = 0; index < document.DrawingData.Length; index++)
        {
            ushort address = (ushort)(0xc9d9 + index);
            byte expected = rom.ReadByte(0x900000 | address);
            if (!SamusArmCannonArtworkFormat.TryStockDrawingByte(address, out _) &&
                !SamusArmCannonArtworkFormat.TryStockCoordinateSource(address, out _) &&
                !SamusArmCannonArtworkFormat.TryStockReflectedXSource(address, out _))
            {
                AssertTrue(SamusArmCannonPlacementDefinitions.TryCoordinate(body, address, out byte direct), "Every independent coordinate has a direct body/default source");
                AssertEqual(expected, direct, $"Direct native cannon attachment {address:X4}");
                basis++;
            }
            AssertEqual(expected, stock.ReadDrawingByte(address), "Every installed native cannon byte");
            int[] drawing = document.DrawingData.ToArray(); drawing[index] ^= 0xff;
            var edited = Load(document with { DrawingData = drawing }).WithBodyGeometry(body);
            for (int other = 0; other < drawing.Length; other++)
                AssertEqual((byte)drawing[other], edited.ReadDrawingByte((ushort)(0xc9d9 + other)), "Independent cover-byte edit preserves all supplied outputs");
            AssertEqual(Load(document with { DrawingData = drawing }).ContentIdentity, edited.ContentIdentity, "Bound edited cover retains exact canonical hash");
        }
        AssertEqual(135, basis, "All formerly independent coordinate basis bytes now have reviewed defaults");
        SamusBodyArtworkCatalog WithMaps(SamusSpritemapArtworkCatalog maps) => new(
            body.TopSetPointers.ToArray(), body.BottomSetPointers.ToArray(), body.PosePointers.ToArray(),
            body.GraphicsYOffsets.ToArray(), body.Frames.ToArray(),
            Enumerable.Range(0, SamusBodyArtworkCatalog.TopSetCount).Select(index => body.TopSet(index).ToArray()).ToArray(),
            Enumerable.Range(0, SamusBodyArtworkCatalog.BottomSetCount).Select(index => body.BottomSet(index).ToArray()).ToArray(),
            maps, body.Atmosphere, body.DeathPalettes, body.DeathTiles, stock,
            body.LandingYOffsets.ToArray(), body.PostureYOffsets.ToArray(), body.DrainedYOffsets.ToArray());
        var empty = new SamusSpritemapArtworkCatalog(body.Spritemaps.TopBases.ToArray(), body.Spritemaps.BottomBases.ToArray(),
            body.Spritemaps.Pointers.ToArray(), body.Spritemaps.Definitions.Select(map => new SamusSpritemapDefinition(map.Pointer, [])).ToArray());
        var zero = new SamusSpritemapArtworkCatalog(body.Spritemaps.TopBases.ToArray(), body.Spritemaps.BottomBases.ToArray(),
            new ushort[SamusSpritemapArtworkCatalog.PointerCount], []);
        var shifted = new SamusSpritemapArtworkCatalog(body.Spritemaps.TopBases.ToArray(), body.Spritemaps.BottomBases.ToArray(),
            body.Spritemaps.Pointers.ToArray(), body.Spritemaps.Definitions.Select(map => new SamusSpritemapDefinition(map.Pointer,
                map.Parts.Select(part => part with { X = (ushort)((part.X & ~511) | ((part.X + 1) & 511)), Y = unchecked((byte)(part.Y + 1)) }).ToArray())).ToArray());
        foreach (var maps in new[] { empty, zero, shifted })
        {
            var editedBody = WithMaps(maps);
            for (int index = 0; index < document.DrawingData.Length; index++)
                AssertEqual((byte)document.DrawingData[index], editedBody.ArmCannon.ReadDrawingByte((ushort)(0xc9d9 + index)), "Independent body geometry cannot rewrite supplied cover placement");
            AssertEqual(stock.ContentIdentity, editedBody.ArmCannon.ContentIdentity, "Body-only edits preserve complete cover identity");
            AssertTrue(Stored(editedBody.ArmCannon).Count > 0, "Different source art retains explicit independent cover offsets");
        }
        int draws = 0;
        foreach (byte pose in new byte[] { 1, 3, 5, 7, 11, 12, 15, 16, 17, 18, 0x15, 0x16, 0x17, 0x18, 0x2b, 0x2c, 0x2d, 0x2e, 0x49, 0x4a, 0x4b, 0x71, 0x72, 0x75, 0x76, 0xa4, 0xa6 })
        {
            int count = pose == 0xa4 ? 2 : pose == 0xa6 ? 3 : pose is 3 or 0x15 or 0x16 or 0x17 or 0x18 or 0x2b or 0x2c or 0x2d or 0x2e ? 2 : 1;
            for (ushort frame = 0; frame < count; frame++)
            {
                var samus = new SamusState { Pose = pose, AnimationFrame = frame, XPosition = 128, YPosition = 128, SelectedHudItem = 1 };
                samus.TileTransfers.BindArtwork(body);
                var cannon = new SamusArmCannonState { Artwork = stock };
                cannon.Update(rom, samus); cannon.Update(rom, samus);
                int pointer = rom.ReadByte(0x90c7df + pose * 2) | rom.ReadByte(0x90c7e0 + pose * 2) << 8;
                int offset = (rom.ReadByte(0x900000 | pointer) & 128) != 0 ? 4 : 2;
                int x = unchecked((sbyte)rom.ReadByte(0x900000 | (pointer + offset + frame * 2)));
                int y = unchecked((sbyte)rom.ReadByte(0x900000 | (pointer + offset + frame * 2 + 1)));
                int graphics = unchecked((sbyte)rom.ReadByte(0x91b629 + pose * 8 + 4));
                var cannonOam = new OamBuffer();
                var cannonWrites = new VramWriteQueue();
                cannon.Draw(rom, cannonOam, cannonWrites, samus, 0, 0, 0);
                AssertTrue(cannonOam.NextByteOffset == 4 && cannonWrites.Entries.Count == 1, "Actual cover draw and DMA publication");
                OamEntry cannonEntry = cannonOam.GetEntry(0);
                AssertEqual(128 + x, cannonEntry.X, "Actual native cover X");
                AssertEqual(128 + y - graphics, (int)cannonEntry.Y, "Actual native cover Y including pose origin");
                draws++;
            }
        }
        Console.WriteLine($"Cannon placement:135 direct body/tail defaults,608 native bytes/independent edits,zero stock fallbacks,canonical hashes,independent empty/zero/shifted body art and{draws} actual OAM draws pass.");
    }

    private static void VerifyLookupStream1OamPointers(ISnesAddressSpace rom)
    {
        using var directory = new TestTempDirectory("map-catalog");
        SamusBodyArtworkFiles.Extract(rom, directory.Root, SupportedCartridge.Sha256);
        var body = SamusBodyArtworkFiles.Load(directory.Root, null);
        var stock = body.Spritemaps;
        ushort[] native = Enumerable.Range(0, 2096).Select(index =>
            (ushort)(rom.ReadByte(0x92808d + index * 2) | rom.ReadByte(0x92808e + index * 2) << 8)).ToArray();
        int aliases = 0, directPointers = 0;
        var nativeDefinitions = stock.Definitions.ToDictionary(map => map.Pointer);
        for (int index = 0; index < native.Length; index++)
        {
            int source = SamusSpritemapFrameDefinitions.SourceIndex(index);
            AssertEqual(native[index], native[source], $"Source-identified native OAM sharing {index:X4}->{source:X4}");
            if (source != index) aliases++;
            else if (SamusSpritemapFrameDefinitions.TryPointer(index, nativeDefinitions, out ushort calculated))
            {
                AssertEqual(native[index], calculated, $"Direct native composition identity {index:X4}");
                directPointers++;
            }
        }
        AssertTrue(stock.Pointers.SequenceEqual(native), "All native pointer observations preserved");
        var stored = (Dictionary<int, ushort>)typeof(SamusSpritemapArtworkCatalog).GetField("pointers",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(stock)!;
        AssertEqual(native.Length - aliases - directPointers, stored.Count, "Exact independent pointer basis, no stock alias/allocation fallbacks");
        ushort[] top = stock.TopBases.ToArray(), bottom = stock.BottomBases.ToArray();
        SamusSpritemapDefinition[] definitions = stock.Definitions.ToArray();
        string Canonical(ushort[] pointers) => SelectedPresentationHash.Create(nameof(SamusSpritemapArtworkCatalog), content =>
        {
            content.AppendWords("top bases", top); content.AppendWords("bottom bases", bottom);
            content.AppendWords("pointers", pointers);
            foreach (var map in definitions.OrderBy(map => map.Pointer))
            {
                content.Append("pointer", map.Pointer); content.Append("part count", map.Parts.Length);
                foreach (var part in map.Parts)
                {
                    content.Append("x", part.X); content.Append("y", part.Y); content.Append("attributes", part.Attributes);
                }
            }
        });
        AssertEqual(Canonical(native), stock.ContentIdentity, "Original native OAM canonical hash");
        SamusSpritemapArtworkCatalog Create(ushort[] pointers) => new(top, bottom, pointers, definitions);
        var emptyCompositions = new SamusSpritemapArtworkCatalog(top, bottom, native,
            definitions.Select(map => new SamusSpritemapDefinition(map.Pointer, [])).ToArray());
        AssertTrue(emptyCompositions.Pointers.SequenceEqual(native), "Independent OAM part-count edits preserve every supplied pointer identity");
        for (int index = 0; index < native.Length; index++)
        {
            var supplied = native.ToArray(); supplied[index] = supplied[index] == 0 ? native[0] : (ushort)0;
            var edited = Create(supplied);
            AssertTrue(edited.Pointers.SequenceEqual(supplied), "Independent basis/alias edit preserves every supplied pointer");
            bool exists = edited.TryGet((ushort)index, out var map);
            AssertEqual(supplied[index] != 0, exists, "Actual edited selector retains mutable-memory versus installed record distinction");
            if (exists) AssertEqual(supplied[index], map!.Pointer, "Actual edited native record identity");
            if (index == native.Length - 1) AssertEqual(Canonical(supplied), edited.ContentIdentity, "Original independently edited OAM canonical hash");
        }
        Directory.CreateDirectory("csharp/test-temp");
        File.WriteAllLines("csharp/test-temp/samus-oam-pointer-required.csv", new[] { "Index,NativeAddress,CompositionPointer" }
            .Concat(stored.OrderBy(pair => pair.Key).Select(pair => $"{pair.Key:X4},{0x92808d + pair.Key * 2:X6},{pair.Value:X4}")));
        AssertThrows<InvalidDataException>(() => stock.TryGet(2096, out _), "Original upper index rejection");
        AssertThrows<InvalidDataException>(() => stock.TryGet(ushort.MaxValue, out _), "Original full ushort rejection");
        var memory = new TestAddressSpace();
        byte[] record = [1, 0, 3, 0, 2, 0x34, 0x12];
        for (int index = 0; index < record.Length; index++) memory.WriteByte(0x920000 + index, record[index]);
        var oam = new OamBuffer(); oam.BeginFrame();
        oam.AddSamusSpritemap(memory, 3, 127, 131, stock);
        AssertTrue(oam.LowTable[..4].SequenceEqual(new byte[] { 130, 133, 0x34, 0x12 }), "Native zero selector still reads mutable memory through actual renderer");
        Console.WriteLine($"Samus OAM pointers:2096 native selectors, {aliases} exact aliases/{directPointers} direct calculations/{stored.Count} retained composition inputs,2096 independent edits,empty OAM,actual identity/WRAM selection and domain checks pass.");
    }

    private static void VerifyLookupStream1BodyPixels(ISnesAddressSpace rom)
    {
        using var directory = new TestTempDirectory("map-catalog");
        SamusBodyArtworkFiles.Extract(rom, directory.Root, SupportedCartridge.Sha256);
        var body = SamusBodyArtworkFiles.Load(directory.Root, null);
        var field = typeof(SamusBodyTileDefinition).GetField("pixelInputs", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        var top = Enumerable.Range(0, 13).Select(set => body.TopSet(set).ToArray()).ToArray();
        var bottom = Enumerable.Range(0, 11).Select(set => body.BottomSet(set).ToArray()).ToArray();
        int bytes = 0, blankBytes = 0, sharedBytes = 0, retained = 0, circleBits = 0, diagnosticBytes = 0;
        var basisRows = new List<string> { "Half,Set,Position,NativeAddress,Byte,Mask" };
        for (int half = 0; half < 2; half++)
        {
            bool upper = half == 0;
            var groups = upper ? top : bottom;
            for (int set = 0; set < groups.Length; set++)
            for (int position = 0; position < groups[set].Length; position++)
            {
                var glyph = groups[set][position];
                var stored = (Dictionary<int, byte>)field.GetValue(glyph)!;
                byte[] image = glyph.Planar.ToArray();
                var contour = (Dictionary<int, byte>)typeof(SamusBodyTileDefinition).GetField("contourEdits", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(glyph)!;
                AssertEqual(0, contour.Count, "Zero stock morph-circle contour overrides");
                for (int index = 0; index < image.Length; index++)
                {
                    byte native = rom.ReadByte(glyph.SourceAddress + index);
                    AssertEqual(native, image[index], "Every native body pixel byte");
                    byte mask = SamusBodyPixelDefinitions.ContourMask(upper, set, position, index);
                    AssertEqual((byte)0, (byte)(native & ~mask), "Exact native circular silhouette");
                    circleBits += System.Numerics.BitOperations.PopCount((uint)(byte)~mask);
                    int tile = index / 32;
                    bool blank = SamusBodyPixelDefinitions.IsBlank(upper, set, position, tile);
                    bool relation = SamusBodyPixelDefinitions.TrySourceByte(upper, set, position, index, out int sourcePosition, out int sourceIndex);
                    bool shared = !blank && relation;
                    bool diagnostic = SamusBodyPixelDefinitions.TryDiagnosticByte(upper, set, position, index, out byte glyphByte);
                    if (diagnostic) { AssertEqual(native, glyphByte, "Direct native Ep drawing"); diagnosticBytes++; }
                    else if (blank) { AssertEqual((byte)0, native, "Direct native transparent allocation"); blankBytes++; }
                    else if (shared) { AssertEqual(rom.ReadByte(groups[set][sourcePosition].SourceAddress + sourceIndex), native, "Direct native shared anatomical patch"); sharedBytes++; }
                    else
                    {
                        retained++;
                        basisRows.Add($"{half},{set:X},{position:X},{glyph.SourceAddress + index:X6},{native:X2},{mask:X2}");
                    }
                    AssertEqual(!diagnostic && !blank && !shared, stored.ContainsKey(index), "Exact pixel basis; no stock defaults survive as overrides");
                    bytes++;
                }
            }
        }
        AssertEqual(130464, bytes, "Complete native body planar domain");
        AssertEqual(81 * 32, blankBytes, "Exact transparent tile domain");
        AssertEqual(357 * 32, sharedBytes, "Exact grapple/drained anatomy domain");
        AssertEqual(4 * 32, diagnosticBytes, "Exact bounded diagnostic domain");
        AssertEqual(1536, circleBits, "Eight morph silhouettes exclude48 pixels each in four planes");
        Directory.CreateDirectory("csharp/test-temp");
        File.WriteAllLines("csharp/test-temp/samus-body-pixel-basis.csv", basisRows);
        SamusBodyArtworkCatalog Create(SamusBodyTileDefinition[][] upper, SamusBodyTileDefinition[][]? lower = null) => new(
            body.TopSetPointers.ToArray(), body.BottomSetPointers.ToArray(), body.PosePointers.ToArray(), body.GraphicsYOffsets.ToArray(), body.Frames.ToArray(),
            upper, lower ?? bottom, body.Spritemaps, body.Atmosphere, body.DeathPalettes, body.DeathTiles, body.ArmCannon,
            body.LandingYOffsets.ToArray(), body.PostureYOffsets.ToArray(), body.DrainedYOffsets.ToArray());
        AssertEqual(CanonicalBodyHash(body, body.TopSetPointers.ToArray(), body.BottomSetPointers.ToArray(), body.PosePointers.ToArray(), body.Frames.ToArray(), top, bottom), body.ContentIdentity, "Original native body hash");
        foreach (var change in new (int Set, int Position, int Byte)[] { (0, 2, 128), (4, 2, 0), (4, 3, 0), (6, 13, 32), (6, 14, 32), (3, 4, 64), (10, 0, 0), (10, 4, 128), (1, 4, 0) })
        {
            var supplied = top.Select(group => group.ToArray()).ToArray();
            var original = supplied[change.Set][change.Position];
            byte[] edited = original.Planar.ToArray(); edited[change.Byte] ^= 255;
            supplied[change.Set][change.Position] = new(original.SourceAddress, original.FirstSize, original.SecondSize, edited);
            var actual = Create(supplied);
            for (int set = 0; set < supplied.Length; set++)
            for (int position = 0; position < supplied[set].Length; position++)
                AssertTrue(actual.TopSet(set)[position].Planar.Span.SequenceEqual(supplied[set][position].Planar.Span), "Source, derived and padding edits preserve every independently supplied glyph");
            AssertEqual(CanonicalBodyHash(body, body.TopSetPointers.ToArray(), body.BottomSetPointers.ToArray(), body.PosePointers.ToArray(), body.Frames.ToArray(), supplied, bottom), actual.ContentIdentity, "Exact independent body pixel hash");
        }
        var cableEdits = bottom.Select(group => group.ToArray()).ToArray();
        foreach (int position in new[] { 0, 1, 4, 5, 8, 9, 11, 12, 16 })
        {
            var glyph = cableEdits[3][position];
            byte[] pixels = glyph.Planar.ToArray(); pixels[0] ^= (byte)(1 << (position % 8));
            cableEdits[3][position] = new(glyph.SourceAddress, glyph.FirstSize, glyph.SecondSize, pixels);
        }
        var cableBody = Create(top, cableEdits);
        for (int set = 0; set < bottom.Length; set++)
        for (int position = 0; position < bottom[set].Length; position++)
            AssertTrue(cableEdits[set][position].Planar.Span.SequenceEqual(cableBody.BottomSet(set)[position].Planar.Span), "Independent repeated/swapped cable source and target edits remain exact");
        AssertEqual(CanonicalBodyHash(body, body.TopSetPointers.ToArray(), body.BottomSetPointers.ToArray(), body.PosePointers.ToArray(), body.Frames.ToArray(), top, cableEdits), cableBody.ContentIdentity, "Exact cable edit hash");
        Console.WriteLine($"Body pixels: {bytes} native bytes, {blankBytes} blank and {sharedBytes} shared bytes, exact {retained} selected inputs, nine individual upper edits plus nine simultaneous distinct cable edits, {diagnosticBytes} drawn diagnostic bytes, {circleBits} circular transparency bits and canonical hashes pass.");
    }
    private static void VerifyLookupStream1BodyTransfers(ISnesAddressSpace rom)
    {
        using var directory = new TestTempDirectory("map-catalog");
        SamusBodyArtworkFiles.Extract(rom, directory.Root, SupportedCartridge.Sha256);
        var body = SamusBodyArtworkFiles.Load(directory.Root, null);
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var sourceField = typeof(SamusBodyTileDefinition).GetField("sourceAddressOverride", flags)!;
        var sizeField = typeof(SamusBodyTileDefinition).GetField("firstSizeOverride", flags)!;
        var top = Enumerable.Range(0, 13).Select(set => body.TopSet(set).ToArray()).ToArray();
        var bottom = Enumerable.Range(0, 11).Select(set => body.BottomSet(set).ToArray()).ToArray();
        var changedTop = top.Select(group => group.ToArray()).ToArray();
        var changedBottom = bottom.Select(group => group.ToArray()).ToArray();
        int records = 0;
        for (int half = 0; half < 2; half++)
        {
            bool upper = half == 0;
            var groups = upper ? top : bottom;
            for (int set = 0; set < groups.Length; set++)
            for (int position = 0; position < groups[set].Length; position++)
            {
                var value = groups[set][position];
                int address = 0x920000 | (Word((upper ? 0x92d91e : 0x92d938) + set * 2) + position * 7);
                int source = Word(address) | rom.ReadByte(address + 2) << 16;
                ushort first = Word(address + 3), second = Word(address + 5);
                AssertEqual(source, SamusBodyTransferDefinitions.SourceAddress(body, upper, set, position), "Direct native packed allocation source");
                AssertTrue(SamusBodyTransferDefinitions.TryFirstSize(body, upper, set, position, value.Planar.Length, out ushort calculated), "Every native record has a direct row calculation");
                AssertEqual(first, calculated, $"Direct native row geometry {upper}/{set:X}/{position:X}");
                AssertEqual(source, value.SourceAddress, "Installed native source");
                AssertEqual(first, value.FirstSize, "Installed native first row");
                AssertEqual(second, value.SecondSize, "Installed native remaining row");
                AssertTrue(sourceField.GetValue(value) is null && sizeField.GetValue(value) is null, "Zero stock metadata overrides");
                byte[] pixels = value.Planar.ToArray();
                for (int index = 0; index < pixels.Length; index++) AssertEqual(rom.ReadByte(source + index), pixels[index], "Separate native pixel payload preserved");
                // Preserve schema-valid non-tile-aligned individual row splits too.
                ushort editedFirst = (ushort)(second == 0 ? first - 1 : first + 1);
                var supplied = new SamusBodyTileDefinition(source ^ 0x123456, editedFirst,
                    (ushort)(pixels.Length - editedFirst), pixels);
                (upper ? changedTop : changedBottom)[set][position] = supplied;
                var edited = supplied.WithTransferGeometry(body, upper, set, position);
                AssertEqual(supplied.SourceAddress, edited.SourceAddress, "Independent source identity edit");
                AssertEqual(supplied.FirstSize, edited.FirstSize, "Independent row split edit");
                AssertEqual(supplied.SecondSize, edited.SecondSize, "Edited complementary row");
                AssertTrue(edited.Planar.Span.SequenceEqual(pixels), "Metadata edit preserves every pixel");
                records++;
            }
        }
        AssertEqual(435, records, "Complete original metadata domain");
        SamusBodyArtworkCatalog Create(SamusBodyTileDefinition[][] upper, SamusBodyTileDefinition[][] lower, SamusSpritemapArtworkCatalog maps) => new(
            body.TopSetPointers.ToArray(), body.BottomSetPointers.ToArray(), body.PosePointers.ToArray(), body.GraphicsYOffsets.ToArray(), body.Frames.ToArray(),
            upper, lower, maps, body.Atmosphere, body.DeathPalettes, body.DeathTiles, body.ArmCannon,
            body.LandingYOffsets.ToArray(), body.PostureYOffsets.ToArray(), body.DrainedYOffsets.ToArray());
        var changed = Create(changedTop, changedBottom, body.Spritemaps);
        AssertEqual(CanonicalBodyHash(body, body.TopSetPointers.ToArray(), body.BottomSetPointers.ToArray(), body.PosePointers.ToArray(), body.Frames.ToArray(), top, bottom),
            body.ContentIdentity, "Exact canonical stock metadata hash");
        AssertEqual(CanonicalBodyHash(body, body.TopSetPointers.ToArray(), body.BottomSetPointers.ToArray(), body.PosePointers.ToArray(), body.Frames.ToArray(), changedTop, changedBottom),
            changed.ContentIdentity, "Exact canonical hash preserves every supplied metadata edit");
        var emptyMaps = new SamusSpritemapArtworkCatalog(body.Spritemaps.TopBases.ToArray(), body.Spritemaps.BottomBases.ToArray(),
            body.Spritemaps.Pointers.ToArray(), body.Spritemaps.Definitions.Select(map => new SamusSpritemapDefinition(map.Pointer, [])).ToArray());
        var empty = Create(top, bottom, emptyMaps);
        for (int half = 0; half < 2; half++)
        for (int set = 0; set < (half == 0 ? 13 : 11); set++)
        {
            var expected = half == 0 ? top[set] : bottom[set];
            var actual = half == 0 ? empty.TopSet(set) : empty.BottomSet(set);
            for (int index = 0; index < expected.Length; index++)
            {
                AssertEqual(expected[index].SourceAddress, actual[index].SourceAddress, "Empty independent OAM preserves source metadata");
                AssertEqual(expected[index].FirstSize, actual[index].FirstSize, "Empty independent OAM preserves first row");
                AssertEqual(expected[index].SecondSize, actual[index].SecondSize, "Empty independent OAM preserves second row");
            }
        }
        var shorterTop = top.Select(group => group.ToArray()).ToArray();
        var original = top[0][0];
        AssertTrue(original.SecondSize >= 32, "Chosen payload edit has a complete removable second-row tile");
        shorterTop[0][0] = new(original.SourceAddress, original.FirstSize, (ushort)(original.SecondSize - 32), original.Planar.Span[..^32].ToArray());
        var shorter = Create(shorterTop, bottom, body.Spritemaps);
        for (int position = 0; position < top[0].Length; position++)
        {
            AssertEqual(top[0][position].SourceAddress, shorter.TopSet(0)[position].SourceAddress, "Independent payload-length edit cannot rewrite supplied neighboring source identities");
            AssertEqual(shorterTop[0][position].FirstSize, shorter.TopSet(0)[position].FirstSize, "Independent payload length preserves supplied row split");
            AssertEqual(shorterTop[0][position].SecondSize, shorter.TopSet(0)[position].SecondSize, "Exact edited payload remainder");
        }
        // These exact native selectors include the known physical cross-group windows.
        var guard = new FrontendCartridgeReadGuard(rom);
        int transfers = 0;
        foreach (var selected in new (byte Pose, ushort Phase)[] { (1, 0), (0x65, 6), (0x65, 7), (0x66, 8), (0xd8, 0) })
        {
            var state = new SamusTileTransferState(); state.BindArtwork(body);
            state.SelectForPoseFrame(guard, selected.Pose, selected.Phase);
            int frameAddress = 0x920000 | (Word(0x92d94e + selected.Pose * 2) + selected.Phase * 4);
            int topAddress = 0x920000 | (Word(0x92d91e + rom.ReadByte(frameAddress) * 2) + rom.ReadByte(frameAddress + 1) * 7);
            AssertEqual(topAddress, state.TopDefinitionAddress, "Native physical upper selection including cross-group position");
            if (rom.ReadByte(frameAddress + 2) != 255)
                AssertEqual(0x920000 | (Word(0x92d938 + rom.ReadByte(frameAddress + 2) * 2) + rom.ReadByte(frameAddress + 3) * 7),
                    state.BottomDefinitionAddress, "Native physical lower selection including cross-group position");
            var actual = new SnesVram(); var expected = new SnesVram();
            void Copy(int address, int destination)
            {
                int source = Word(address) | rom.ReadByte(address + 2) << 16;
                int first = Word(address + 3), second = Word(address + 5);
                expected.LoadBytes(destination * 2, Enumerable.Range(0, first).Select(index => rom.ReadByte(source + index)).ToArray());
                expected.LoadBytes((destination + 0x100) * 2, Enumerable.Range(0, second).Select(index => rom.ReadByte(source + first + index)).ToArray());
            }
            Copy(state.TopDefinitionAddress, 0x6000);
            if (state.BottomTransferEnabled) Copy(state.BottomDefinitionAddress, 0x6080);
            state.TransferToVram(guard, actual);
            AssertTrue(expected.Bytes.SequenceEqual(actual.Bytes), "Actual complete native split DMA and physical cross-group aliases");
            AssertTrue(state.TopTransferEnabled, "Native NMI preserves transfer flags");
            transfers++;
        }
        Console.WriteLine($"Body transfers:{records} direct native records,zero stock metadata overrides,independent metadata/empty-OAM edits,canonical hash and{transfers} actual split DMA/cross-group selections pass.");
    }

}
