using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyShotBlockProgramOperands()
    {
        // #1157: the first draw operand of a permanent 1x1 shot block ($84:CBBC)
        // was absent from the compiled program. Exercise the actual interpreter
        // against RAM-only memory, not the old import-capable reference guard.
        ShotBlockFixture reported = NewShotBlockFixture(4);
        reported.Plms.Step(new SuperMetroidAddressSpace(), reported.Level,
            reported.Streamer, 0, 0, 0);
        AssertEqual((ushort)0x0053,
            reported.Level.GetCollisionBlockByIndex(27).LevelWord,
            "reported permanent shot block draws its first break frame without a cartridge read");

        // The same timed-record contract owns all eight ordinary shot-block lists.
        // Confirm their break/restore pointer sequence, including the multi-block
        // parent restoration, rather than merely asserting that execution survived.
        for (byte behavior = 0; behavior < 8; behavior++)
        {
            ShotBlockFixture fixture = NewShotBlockFixture(behavior);
            var memory = new SuperMetroidAddressSpace();
            fixture.Plms.Step(memory, fixture.Level, fixture.Streamer, 0, 0, 0);
            AssertEqual((ushort)0x0053,
                fixture.Level.GetCollisionBlockByIndex(27).LevelWord,
                $"shot-block BTS {behavior} first physical break frame");
            for (int frame = 1; frame < 420; frame++)
                fixture.Plms.Step(memory, fixture.Level, fixture.Streamer, 0, 0, 0);
            ushort expected = behavior switch
            {
                0 => 0xc052,
                1 => 0xc096,
                2 => 0xc098,
                3 => 0xc099,
                _ => 0x00ff,
            };
            AssertEqual(expected, fixture.Level.GetCollisionBlockByIndex(27).LevelWord,
                $"shot-block BTS {behavior} restores or permanently clears the native parent");
            AssertEqual(0, fixture.Plms.ActiveCount,
                $"shot-block BTS {behavior} finishes its compiled program");
        }
        Console.WriteLine("#1157: permanent-shot-block failure and all eight shared timed-record operand contracts confirmed on RAM-only memory.");
    }

    private static void VerifyShotBlockPlmPrograms()
    {
        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var forbidden = new HashSet<int>();
        ushort[] nativeDraws = [0xa345,0xa35d,0xa37d,0xa39d,0xa34b,0xa365,0xa385,0xa3ad,
            0xa351,0xa36d,0xa38d,0xa3bd,0xa357,0xa375,0xa395,0xa3cd,0xa47b,0xa483,0xa48b];
        AssertTrue(nativeDraws.SequenceEqual(RoomPlmShotBlockDrawDefinitions.All.Select(draw => draw.Pointer)),
            "shot-block independent native draw order");
        var knownDraws = nativeDraws.ToHashSet();
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
        {
            bool found = RoomPlmShotBlockDrawDefinitions.TryGet((ushort)pointer, out var draw);
            AssertEqual(knownDraws.Contains((ushort)pointer), found, "shot-block full draw-pointer domain");
            if (!found) AssertEqual(default(RoomPlmShotBlockDrawDefinitions.Draw), draw, "shot-block unknown draw output cleared");
        }
        int wordCount = 0;
        foreach (ushort address in RoomPlmShotBlockProgramDefinitions.MechanicsWordAddresses())
        {
            AssertTrue(RoomPlmShotBlockProgramDefinitions.TryReadMechanicsWord(address, out ushort value),
                $"shot-block control word ${address:X4} is compiled");
            ushort sourceWord = unchecked((ushort)(
                rom.ReadByte(0x840000 | address) |
                (rom.ReadByte(0x840000 | (address + 1)) << 8)));
            AssertEqual(sourceWord, value,
                $"shot-block control word ${address:X4} matches pinned cartridge");
            forbidden.Add(0x840000 | address);
            forbidden.Add(0x840000 | (address + 1));
            wordCount++;
        }

        int byteCount = 0;
        foreach (ushort address in RoomPlmShotBlockProgramDefinitions.MechanicsByteAddresses())
        {
            AssertTrue(RoomPlmShotBlockProgramDefinitions.TryReadMechanicsByte(address, out byte value),
                $"shot-block sound byte ${address:X4} is compiled");
            AssertEqual(rom.ReadByte(0x840000 | address), value,
                $"shot-block sound byte ${address:X4} matches pinned cartridge");
            forbidden.Add(0x840000 | address);
            byteCount++;
        }

        AssertEqual(102, wordCount, "thirteen ordinary/gated/enemy-breakable shot programs have control words");
        AssertEqual(13, byteCount, "thirteen shot programs have sound operands");
        AssertTrue(!RoomPlmShotBlockProgramDefinitions.TryReadMechanicsWord(0xa345, out _),
            "interleaved draw-list pointers are not misclassified as control words");

        int drawListCount = 0;
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in
                 RoomPlmShotBlockDrawDefinitions.All)
        {
            AssertTrue(RoomPlmShotBlockDrawDefinitions.TryGet(list.Pointer, out var calculated), "shot-block calculated draw exists");
            AssertEqual(list.Runs.Length, calculated.RunCount, "shot-block calculated run count");
            int cursor = list.Pointer;
            int runIndex = 0;
            foreach (RoomPlmShotBlockDrawDefinitions.Run run in list.Runs.Span)
            {
                AssertEqual(run.LevelWords.Length, calculated.WordsPerRun, "shot-block calculated run width");
                AssertEqual((run.DirectionAndCount & 0x8000) != 0, calculated.Vertical, "shot-block calculated direction");
                int blockIndex = 0;
                AssertEqual(run.DirectionAndCount,
                    unchecked((ushort)(rom.ReadByte(0x840000 | cursor) |
                        (rom.ReadByte(0x840000 | (cursor + 1)) << 8))),
                    $"shot-block draw list ${list.Pointer:X4} direction/count");
                forbidden.Add(0x840000 | cursor++);
                forbidden.Add(0x840000 | cursor++);
                foreach (ushort word in run.LevelWords.Span)
                {
                    AssertEqual(word, calculated.WordAt(runIndex, blockIndex++), "shot-block direct and exported word agree");
                    AssertEqual(word,
                        unchecked((ushort)(rom.ReadByte(0x840000 | cursor) |
                            (rom.ReadByte(0x840000 | (cursor + 1)) << 8))),
                        $"shot-block draw list ${list.Pointer:X4} level word at ${cursor:X4}");
                    forbidden.Add(0x840000 | cursor++);
                    forbidden.Add(0x840000 | cursor++);
                }

                AssertEqual(unchecked((byte)run.NextX), rom.ReadByte(0x840000 | cursor),
                    $"shot-block draw list ${list.Pointer:X4} next X");
                forbidden.Add(0x840000 | cursor++);
                AssertEqual(unchecked((byte)run.NextY), rom.ReadByte(0x840000 | cursor),
                    $"shot-block draw list ${list.Pointer:X4} next Y");
                forbidden.Add(0x840000 | cursor++);
                runIndex++;
            }

            foreach (int invalid in new[] {int.MinValue,-1,int.MaxValue})
            {
                AssertThrows<IndexOutOfRangeException>(() => calculated.WordAt(invalid, 0), "shot-block invalid run");
                AssertThrows<IndexOutOfRangeException>(() => calculated.WordAt(0, invalid), "shot-block invalid block");
            }
            AssertThrows<IndexOutOfRangeException>(() => calculated.WordAt(calculated.RunCount, 0), "shot-block run upper bound");
            AssertThrows<IndexOutOfRangeException>(() => calculated.WordAt(0, calculated.WordsPerRun), "shot-block word upper bound");

            drawListCount++;
        }

        AssertEqual(19, drawListCount, "all shot-block animation and restoration lists are compiled");
        AssertTrue(!RoomPlmShotBlockDrawDefinitions.TryGet(0xa344, out _),
            "uncatalogued draw pointers do not alias a nearby list");

        // Compare the real PLM handler frame by frame with and without precisely the
        // compiled control bytes forbidden. ROM equality above supplies source parity;
        // this production-path check covers the sound handoff,
        // timed frames, 384-tick respawn hold, word restoration, and permanent deletion.
        for (byte behavior = 0; behavior < 8; behavior++)
        {
            ShotBlockFixture stock = NewShotBlockFixture(behavior);
            ShotBlockFixture compiled = NewShotBlockFixture(behavior);
            var guarded = new ShotBlockProgramReadGuard(rom, forbidden);
            for (int frame = 0; frame < 420; frame++)
            {
                stock.Plms.Step(rom, stock.Level, stock.Streamer, 0, 0, 0);
                compiled.Plms.Step(guarded, compiled.Level, compiled.Streamer, 0, 0, 0);
                AssertEqual(stock.Plms.ActiveCount, compiled.Plms.ActiveCount,
                    $"shot-block BTS {behavior} active slots at frame {frame}");
                AssertEqual(stock.Plms.SoundRequests.Count, compiled.Plms.SoundRequests.Count,
                    $"shot-block BTS {behavior} sound count at frame {frame}");
                for (int index = 0; index < 64; index++)
                    AssertEqual(stock.Level.GetCollisionBlockByIndex(index).LevelWord,
                        compiled.Level.GetCollisionBlockByIndex(index).LevelWord,
                        $"shot-block BTS {behavior} block {index} at frame {frame}");
            }

            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"shot-block BTS {behavior} executes without control-byte ROM reads");
            AssertEqual(0, compiled.Plms.ActiveCount,
                $"shot-block BTS {behavior} completes within the retail timer window");
        }

        VerifyShotBlockVisualSeparation(rom, forbidden);
        VerifyShotBlockVisualInstallation(rom);

        Console.WriteLine($"Shot-block PLMs: {wordCount} control words, {byteCount} sound bytes and {drawListCount} draw lists match ROM; all eight programs execute with source bytes forbidden.");
    }

    private static void VerifyShotBlockVisualSeparation(
        ISnesAddressSpace rom, HashSet<int> forbidden)
    {
        RoomPlmShotBlockVisualCatalog stock = RoomPlmShotBlockVisualCatalog.Stock();
        RoomPlmShotBlockVisualEntry[] editedEntries = RoomPlmShotBlockDrawDefinitions.All
            .Select(list => new RoomPlmShotBlockVisualEntry(list.Pointer,
                list.Runs.Span.ToArray()
                    .Select(run => run.LevelWords.Span.ToArray()
                        .Select(word => unchecked((ushort)(word & 0x0fff))).ToArray())
                    .ToArray()))
            .ToArray();
        RoomPlmShotBlockVisualEntry first = editedEntries.Single(entry =>
            entry.DrawPointer == RoomPlmShotBlockDrawDefinitions.SingleFrame0);
        first.Runs[0][0] = 0x0054;
        var edited = new RoomPlmShotBlockVisualCatalog(editedEntries);
        first.Runs[0][0] = 0x0055;
        AssertEqual((ushort)0x0054,
            edited.GetWord(RoomPlmShotBlockDrawDefinitions.SingleFrame0, 0, 0),
            "visual catalog defensively copies author data");
        AssertEqual((ushort)0x0053,
            stock.GetWord(RoomPlmShotBlockDrawDefinitions.SingleFrame0, 0, 0),
            "stock visual catalog retains the native first break frame");

        const int width = 8;
        const int block = 3 + 3 * width;
        var words = new ushort[width * width];
        words[block] = 0xc321;
        byte[] definitions = new byte[0x400 * 8];
        foreach (int visualIndex in new[] { 0x53, 0x54 })
        for (int tile = 0; tile < 4; tile++)
            definitions[visualIndex * 8 + tile * 2] = unchecked((byte)visualIndex);

        (ushort physical, ushort immediate, ushort streamed) Render(
            RoomPlmShotBlockVisualCatalog visuals)
        {
            var level = new RoomLevelData(width, width, words,
                new byte[words.Length], new ushort[words.Length], definitions);
            var streamer = level.CreateBackgroundStreamer();
            var plms = new RoomPlmSystem { ShotBlockVisuals = visuals };
            AssertTrue(plms.TrySpawnProjectileShotBlock(level, block, 0, 0, true),
                "visual-only test allocates the production shot-block PLM");
            plms.Step(new ShotBlockProgramReadGuard(rom, forbidden), level, streamer, 0, 0, 0);
            AssertEqual(1, plms.TilemapUpdates.Count,
                "visual-only edit publishes one immediate PLM tilemap update");
            return (level.GetCollisionBlockByIndex(block).LevelWord,
                plms.TilemapUpdates[0].TopRow[0],
                level.CreateBackgroundStreamer().BuildPlmLevelBlockUpdate(block, 0).TopRow[0]);
        }

        var native = Render(stock);
        var changed = Render(edited);
        AssertEqual((ushort)0x0053, native.physical,
            "stock first break frame has its native physical level word");
        AssertEqual(native.physical, changed.physical,
            "visual edit cannot change first-frame collision or level data");
        AssertEqual((ushort)0x0053, native.immediate,
            "stock draw reaches the immediate tilemap upload");
        AssertEqual((ushort)0x0054, changed.immediate,
            "edited draw reaches the immediate tilemap upload");
        AssertEqual((ushort)0x0054, changed.streamed,
            "edited draw persists in later camera streaming");

        AssertThrows<InvalidDataException>(
            () => new RoomPlmShotBlockVisualCatalog(editedEntries.Skip(1)),
            "visual catalog rejects missing compiled draw lists");
        first.Runs[0][0] = 0xf054;
        AssertThrows<InvalidDataException>(
            () => new RoomPlmShotBlockVisualCatalog(editedEntries),
            "visual catalog rejects collision bits in editable data");
    }

    private static void VerifyShotBlockVisualInstallation(ISnesAddressSpace rom)
    {
        string testRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "shot-block-visual-" + Guid.NewGuid().ToString("N")));
        string allowedRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp")) +
            Path.DirectorySeparatorChar;
        if (!testRoot.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Shot-block visual test root escaped test-temp.");
        try
        {
            var installation = new GameInstallation(testRoot);
            RoomPlmShotBlockVisualFiles.Extract(rom,
                installation.RoomPlmShotBlockVisualDirectory, SupportedCartridge.Sha256);
            RoomPlmShotBlockVisualFiles.ValidateStock(
                installation.RoomPlmShotBlockVisualDirectory);
            AssertEqual((ushort)0x0053,
                installation.LoadRoomPlmShotBlockVisuals().GetWord(
                    RoomPlmShotBlockDrawDefinitions.SingleFrame0, 0, 0),
                "installed stock shot-block frame matches native visual block");

            string stockPath = Path.Combine(installation.RoomPlmShotBlockVisualDirectory,
                RoomPlmShotBlockVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted shot-block JSON is empty.");
            JsonNode first = document["entries"]!.AsArray().Single(entry =>
                entry!["drawPointer"]!.GetValue<int>() ==
                RoomPlmShotBlockDrawDefinitions.SingleFrame0)!;
            first["runs"]![0]![0] = 0x0054;
            Directory.CreateDirectory(installation.RoomPlmShotBlockVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmShotBlockVisualOverrideDirectory,
                RoomPlmShotBlockVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertEqual((ushort)0x0054,
                installation.LoadRoomPlmShotBlockVisuals().GetWord(
                    RoomPlmShotBlockDrawDefinitions.SingleFrame0, 0, 0),
                "installed shot-block override replaces only the selected visual word");

            // Stock repair/upgrade replaces files under game/, not overrides/. Extract
            // into a new stock directory and prove the same user edit remains selected.
            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmShotBlockVisualFiles.Extract(rom, refreshed, SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0054,
                RoomPlmShotBlockVisualFiles.Load(refreshed,
                    installation.RoomPlmShotBlockVisualOverrideDirectory).GetWord(
                    RoomPlmShotBlockDrawDefinitions.SingleFrame0, 0, 0),
                "shot-block override survives replacement of stock content");

            first["runs"]![0]![0] = 0xf054;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmShotBlockVisuals(),
                "shot-block visual override rejects collision/type bits");
            first["runs"]![0]![0] = 0x0054;
            File.WriteAllText(overridePath, document.ToJsonString());
            File.WriteAllText(stockPath, "corrupt stock");
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmShotBlockVisuals(),
                "shot-block stock manifest hash rejects corrupted content");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
    }

    private sealed record ShotBlockFixture(
        RoomLevelData Level,
        BackgroundTilemapStreamer Streamer,
        RoomPlmSystem Plms);

    private static ShotBlockFixture NewShotBlockFixture(byte behavior)
    {
        const int width = 8;
        const int height = 8;
        const int blockIndex = 3 + 3 * width;
        var words = new ushort[width * height];
        words[blockIndex] = 0xc321;
        var level = new RoomLevelData(
            width, height, words, new byte[words.Length],
            new ushort[words.Length], new byte[0x400 * 8]);
        var plms = new RoomPlmSystem();
        AssertTrue(plms.TrySpawnProjectileShotBlock(
                level, blockIndex, behavior, 0, solidBlock: true),
            $"shot-block BTS {behavior} allocates its native PLM program");
        return new ShotBlockFixture(level, level.CreateBackgroundStreamer(), plms);
    }

    private sealed class ShotBlockProgramReadGuard(ISnesAddressSpace source, HashSet<int> forbidden)
        : ISnesAddressSpace, IImportCartridgeSource
    {
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (forbidden.Contains(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production PLM handler read compiled control byte ${address:X6}.");
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
