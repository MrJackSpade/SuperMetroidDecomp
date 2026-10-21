using SuperMetroid.Core;
using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.MotherBrain.BabyMetroidRouteDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks shot-block PLM programs, editable draw frames, and ROM-free runtime execution against cartridge data.</summary>
internal static class ShotBlockPlmProgramsVerification
{
    /// <summary>Reproduces the permanent-shot-block first-frame regression and checks all eight timed BTS programs on RAM-only memory.</summary>
    internal static void VerifyShotBlockProgramOperands()
    {
        // #1157: the first draw operand of a permanent 1x1 shot block ($84:CBBC)
        // was absent from the compiled program. Exercise the actual interpreter
        // against RAM-only memory, not the old import-capable reference guard.
        ShotBlockFixture reported = NewShotBlockFixture(4);
        reported.Plms.Step(reported.Level);
        AssertEqual((ushort)0x0053,
            reported.Level.GetCollisionBlockByIndex(27).PackedWord.Raw,
            "reported permanent shot block draws its first break frame without a cartridge read");

        // The same timed-record contract owns all eight ordinary shot-block lists.
        // Confirm their break/restore pointer sequence, including the multi-block
        // parent restoration, rather than merely asserting that execution survived.
        for (byte behavior = 0; behavior < 8; behavior++)
        {
            ShotBlockFixture fixture = NewShotBlockFixture(behavior);
            fixture.Plms.Step(fixture.Level);
            AssertEqual((ushort)0x0053,
                fixture.Level.GetCollisionBlockByIndex(27).PackedWord.Raw,
                $"shot-block BTS {behavior} first physical break frame");
            for (int frame = 1; frame < 420; frame++)
                fixture.Plms.Step(fixture.Level);
            ushort expected = behavior switch
            {
                0 => 0xc052,
                1 => 0xc096,
                2 => 0xc098,
                3 => 0xc099,
                _ => 0x00ff,
            };
            AssertEqual(expected, fixture.Level.GetCollisionBlockByIndex(27).PackedWord.Raw,
                $"shot-block BTS {behavior} restores or permanently clears the native parent");
            AssertEqual(0, fixture.Plms.SlotTable.ActiveCount,
                $"shot-block BTS {behavior} finishes its compiled program");
        }
        Console.WriteLine("#1157: permanent-shot-block failure and all eight shared timed-record operand contracts confirmed on RAM-only memory.");
    }

    /// <summary>Checks compiled shot-block control, sound, and drawing data against retail ROM and executes the programs with source reads forbidden.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for shot block PLM programs")]
    internal static void VerifyShotBlockPlmPrograms()
    {
        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyShotBlockStockVisualMapping), () => VerifyShotBlockStockVisualMapping(rom));
        Suite(nameof(VerifyShotBlockProgramControlMapping), () => VerifyShotBlockProgramControlMapping(rom));
        Suite(nameof(VerifyShotBlockProgramDrawMapping), () => VerifyShotBlockProgramDrawMapping(rom));
        Suite(nameof(VerifyShotBlockProgramSoundMapping), () => VerifyShotBlockProgramSoundMapping(rom));
        Suite(nameof(VerifyRespawningShotSizeSelection), () => VerifyRespawningShotSizeSelection(rom));
        Suite(nameof(VerifyPermanentShotSizeSelection), () => VerifyPermanentShotSizeSelection(rom));
        Suite(nameof(RoomPlmProgramsVerification.VerifyBlockReactionPrograms), () => RoomPlmProgramsVerification.VerifyBlockReactionPrograms(rom));
        Suite(nameof(RoomPlmProgramsVerification.VerifyScriptedRoomPrograms), () => RoomPlmProgramsVerification.VerifyScriptedRoomPrograms(rom));
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
            ushort sourceWord = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0x840000 | address);
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
            AssertEqual(rom.ReadCartridgeByte(0x840000 | address), value,
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
                    CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0x840000 | cursor),
                    $"shot-block draw list ${list.Pointer:X4} direction/count");
                forbidden.Add(0x840000 | cursor++);
                forbidden.Add(0x840000 | cursor++);
                foreach (ushort word in run.LevelWords.Span)
                {
                    AssertEqual(word, calculated.WordAt(runIndex, blockIndex++), "shot-block direct and exported word agree");
                    AssertEqual(word,
                        CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0x840000 | cursor),
                        $"shot-block draw list ${list.Pointer:X4} level word at ${cursor:X4}");
                    forbidden.Add(0x840000 | cursor++);
                    forbidden.Add(0x840000 | cursor++);
                }

                AssertEqual(unchecked((byte)run.Next.X), rom.ReadCartridgeByte(0x840000 | cursor),
                    $"shot-block draw list ${list.Pointer:X4} next X");
                forbidden.Add(0x840000 | cursor++);
                AssertEqual(unchecked((byte)run.Next.Y), rom.ReadCartridgeByte(0x840000 | cursor),
                    $"shot-block draw list ${list.Pointer:X4} next Y");
                forbidden.Add(0x840000 | cursor++);
                runIndex++;
            }

            foreach (int invalid in new[] {int.MinValue,-1,int.MaxValue})
            {
                AssertThrows<ArgumentOutOfRangeException>(() => calculated.WordAt(invalid, 0), "shot-block invalid run");
                AssertThrows<ArgumentOutOfRangeException>(() => calculated.WordAt(0, invalid), "shot-block invalid block");
            }
            AssertThrows<ArgumentOutOfRangeException>(() => calculated.WordAt(calculated.RunCount, 0), "shot-block run upper bound");
            AssertThrows<ArgumentOutOfRangeException>(() => calculated.WordAt(0, calculated.WordsPerRun), "shot-block word upper bound");

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
            var guarded = NewShotBlockProgramReadGuard(rom, forbidden);
            for (int frame = 0; frame < 420; frame++)
            {
                stock.Plms.Step(stock.Level);
                compiled.Plms.Step(compiled.Level);
                AssertEqual(stock.Plms.SlotTable.ActiveCount, compiled.Plms.SlotTable.ActiveCount,
                    $"shot-block BTS {behavior} active slots at frame {frame}");
                AssertEqual(stock.Plms.Sounds.SoundRequests.Count, compiled.Plms.Sounds.SoundRequests.Count,
                    $"shot-block BTS {behavior} sound count at frame {frame}");
                for (int index = 0; index < 64; index++)
                    AssertEqual(stock.Level.GetCollisionBlockByIndex(index).PackedWord.Raw,
                        compiled.Level.GetCollisionBlockByIndex(index).PackedWord.Raw,
                        $"shot-block BTS {behavior} block {index} at frame {frame}");
            }

            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"shot-block BTS {behavior} executes without control-byte ROM reads");
            AssertEqual(0, compiled.Plms.SlotTable.ActiveCount,
                $"shot-block BTS {behavior} completes within the retail timer window");
        }

        Suite(nameof(VerifyShotBlockVisualSeparation), () => VerifyShotBlockVisualSeparation());
        Suite(nameof(VerifyShotBlockVisualInstallation), () => VerifyShotBlockVisualInstallation(rom));

        Console.WriteLine($"Shot-block PLMs: {wordCount} control words, {byteCount} sound bytes and {drawListCount} draw lists match ROM; all eight programs execute with source bytes forbidden.");
    }

    /// <summary>Checks the four respawning shapes against native selector words and rejects out-of-range BTS sizes.</summary>
    private static void VerifyRespawningShotSizeSelection(SuperMetroidAddressSpace rom)
    {
        ushort[] fields = [0xd066,0xd06a,0xd06e,0xd072];
        for (int size = 0; size < fields.Length; size++)
            AssertEqual(ReadRomWord(rom, 0x840000 | fields[size]),
                RoomPlmNativeListAddresses.BlockReactionEntry(BlockReactionPlmPrograms.RespawningShot(size)), "respawning shot native shape selection");
        for (int size = 4; size <= byte.MaxValue; size++)
            AssertThrows<ArgumentOutOfRangeException>(() => BlockReactionPlmPrograms.RespawningShot(size), "respawning shot rejects other BTS values without masking");
        foreach (int size in new[] {int.MinValue,-1,256,int.MaxValue})
            AssertThrows<ArgumentOutOfRangeException>(() => BlockReactionPlmPrograms.RespawningShot(size), "respawning shot size domain");
    }

    /// <summary>Checks the four permanent shapes after the caller's BTS offset and rejects invalid selector inputs.</summary>
    private static void VerifyPermanentShotSizeSelection(SuperMetroidAddressSpace rom)
    {
        ushort[] fields = [0xd076,0xd07a,0xd07e,0xd082];
        for (int size = 0; size < fields.Length; size++)
            AssertEqual(ReadRomWord(rom, 0x840000 | fields[size]),
                RoomPlmNativeListAddresses.BlockReactionEntry(BlockReactionPlmPrograms.PermanentShot(size)), "permanent shot native shape selection after caller subtracts four");
        for (int size = 4; size <= byte.MaxValue; size++)
            AssertThrows<ArgumentOutOfRangeException>(() => BlockReactionPlmPrograms.PermanentShot(size), "permanent shot rejects out-of-range size without masking");
        foreach (int size in new[] {int.MinValue,-1,256,int.MaxValue})
            AssertThrows<ArgumentOutOfRangeException>(() => BlockReactionPlmPrograms.PermanentShot(size), "permanent shot size domain");
    }

    /// <summary>Lists the thirteen native timed-list layouts used to derive control and draw operand addresses.</summary>
    private static ShotBlockNativeProgramLayoutsResult[] ShotBlockNativeProgramLayouts() =>
        [new ShotBlockNativeProgramLayoutsResult(0xcadf,7,true),new ShotBlockNativeProgramLayoutsResult(0xcb02,8,false),new ShotBlockNativeProgramLayoutsResult(0xcb27,8,false),new ShotBlockNativeProgramLayoutsResult(0xcb4c,8,false),
         new ShotBlockNativeProgramLayoutsResult(0xcbb7,4,false),new ShotBlockNativeProgramLayoutsResult(0xcbcc,4,false),new ShotBlockNativeProgramLayoutsResult(0xcbe1,4,false),new ShotBlockNativeProgramLayoutsResult(0xcbf6,4,false),
         new ShotBlockNativeProgramLayoutsResult(0xcb71,7,true),new ShotBlockNativeProgramLayoutsResult(0xcb94,7,true),new ShotBlockNativeProgramLayoutsResult(0xcc0b,4,false),new ShotBlockNativeProgramLayoutsResult(0xcc20,4,false),new ShotBlockNativeProgramLayoutsResult(0xcd53,4,false)];

    /// <summary>Checks the full address domain of compiled timers and control words against bank $84.</summary>
    private static void VerifyShotBlockProgramControlMapping(SuperMetroidAddressSpace rom)
    {
        var addresses = new List<ushort>();
        foreach (var layout in ShotBlockNativeProgramLayouts())
        {
            addresses.Add(layout.Start);
            for (int frame = 0; frame <= layout.Frames; frame++)
                addresses.Add((ushort)(layout.Start + 3 + 4 * frame));
            if (layout.RestoreWord) addresses.Add((ushort)(layout.Start + 5 + 4 * layout.Frames));
        }
        AssertTrue(addresses.SequenceEqual(RoomPlmShotBlockProgramDefinitions.MechanicsWordAddresses()), "shot-block native control enumeration order");
        var known = addresses.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool found = RoomPlmShotBlockProgramDefinitions.TryReadMechanicsWord((ushort)address, out ushort value);
            AssertEqual(known.Contains((ushort)address), found, "shot-block full control-word domain");
            AssertEqual(found ? ReadRomWord(rom, 0x840000 | address) : (ushort)0, value,
                "shot-block native control value including respawn hold and Power Bomb cadence");
        }
    }

    /// <summary>Checks that draw operands occur only at timed-frame offsets and retain native shape choices.</summary>
    private static void VerifyShotBlockProgramDrawMapping(SuperMetroidAddressSpace rom)
    {
        var addresses = new HashSet<ushort>();
        foreach (var layout in ShotBlockNativeProgramLayouts())
        for (int frame = 0; frame < layout.Frames; frame++)
            addresses.Add((ushort)(layout.Start + 5 + 4 * frame));
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool found = RoomPlmShotBlockProgramDefinitions.TryReadDrawPointerWord((ushort)address, out ushort value);
            AssertEqual(addresses.Contains((ushort)address), found, "shot-block full draw-operand domain");
            AssertEqual(found ? ReadRomWord(rom, 0x840000 | address) : (ushort)0, value,
                "shot-block native shape, reverse breakup and restoration draw selection");
        }
    }

    /// <summary>Checks the thirteen packed sound operands and rejects every unrelated bank-$84 address.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for shot block program sound mapping")]
    private static void VerifyShotBlockProgramSoundMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses = [0xcae1,0xcb04,0xcb29,0xcb4e,0xcbb9,0xcbce,0xcbe3,0xcbf8,
            0xcb73,0xcb96,0xcc0d,0xcc22,0xcd55];
        AssertTrue(addresses.SequenceEqual(RoomPlmShotBlockProgramDefinitions.MechanicsByteAddresses()), "shot-block native sound operand order");
        var known = addresses.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool found = RoomPlmShotBlockProgramDefinitions.TryReadMechanicsByte((ushort)address, out byte value);
            AssertEqual(known.Contains((ushort)address), found, "shot-block full packed sound domain");
            AssertEqual(found ? rom.ReadCartridgeByte(0x840000 | address) : (byte)0, value, "shot-block native packed sound IDs");
        }
    }

    /// <summary>Compares stock draw runs to ROM and checks editable visual identity, cloning, and bounds.</summary>
    private static void VerifyShotBlockStockVisualMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] pointers = [0xa345,0xa34b,0xa351,0xa357,0xa35d,0xa365,0xa36d,0xa375,
            0xa37d,0xa385,0xa38d,0xa395,0xa39d,0xa3ad,0xa3bd,0xa3cd,0xa47b,0xa483,0xa48b];
        var native = new Dictionary<ushort, ushort[][]>();
        foreach (ushort pointer in pointers)
        {
            int cursor = pointer;
            var runs = new List<ushort[]>();
            do
            {
                int count = ReadRomWord(rom, 0x840000 | cursor) & 0x7fff;
                cursor += 2;
                ushort[] row = new ushort[count];
                for (int i = 0; i < count; i++, cursor += 2)
                    row[i] = (ushort)(ReadRomWord(rom, 0x840000 | cursor) & 0x0fff);
                runs.Add(row);
                ushort next = ReadRomWord(rom, 0x840000 | cursor);
                cursor += 2;
                if (next == 0) break;
                AssertTrue(runs.Count < 2, "shot-block native oracle has at most two runs");
            } while (true);
            native.Add(pointer, runs.ToArray());
        }
        var stock = RoomPlmShotBlockVisualCatalog.Stock();
        var loaded = new RoomPlmShotBlockVisualCatalog(native.Select(pair => new RoomPlmShotBlockVisualEntry(pair.Key, pair.Value)));
        string identity = SuperMetroid.Core.Assets.SelectedPresentationHash.FromWordFrames(nameof(RoomPlmShotBlockVisualCatalog), native);
        AssertEqual(identity, stock.ContentIdentity, "shot-block stock preserves original native content identity");
        AssertEqual(identity, loaded.ContentIdentity, "shot-block imported stock preserves identity");
        foreach (var pair in native)
        for (int run = 0; run < pair.Value.Length; run++)
        for (int word = 0; word < pair.Value[run].Length; word++)
        {
            AssertEqual(pair.Value[run][word], stock.GetWord(pair.Key, run, word), "shot-block calculated native stock word");
            AssertEqual(pair.Value[run][word], loaded.GetWord(pair.Key, run, word), "shot-block imported native stock word");
        }
        native[0xa345][0][0] = 0x0054;
        native[0xa48b][1][1] = 0x0258;
        var edited = new RoomPlmShotBlockVisualCatalog(native.Select(pair => new RoomPlmShotBlockVisualEntry(pair.Key, pair.Value)));
        AssertEqual(SuperMetroid.Core.Assets.SelectedPresentationHash.FromWordFrames(nameof(RoomPlmShotBlockVisualCatalog), native), edited.ContentIdentity,
            "shot-block mixed stock/custom identity preserves original framing");
        foreach (var pair in native)
        for (int run = 0; run < pair.Value.Length; run++)
        for (int word = 0; word < pair.Value[run].Length; word++)
            AssertEqual(pair.Value[run][word], edited.GetWord(pair.Key, run, word), "shot-block mixed payload output");
        native[0xa345][0][0] = 0;
        AssertEqual((ushort)0x0054, edited.GetWord(0xa345, 0, 0), "shot-block custom data cloned");
        AssertEqual((ushort)0x0053, loaded.GetWord(0xa345, 0, 0), "shot-block normalized stock isolated from caller mutation");
        foreach (var catalog in new[] {stock, loaded, edited})
        foreach (var pair in native)
        {
            foreach (int invalid in new[] {int.MinValue,-1,pair.Value.Length,int.MaxValue})
                AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetWord(pair.Key, invalid, 0), "shot-block visual run bounds");
            foreach (int invalid in new[] {int.MinValue,-1,pair.Value[0].Length,int.MaxValue})
                AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetWord(pair.Key, 0, invalid), "shot-block visual word bounds");
        }
        AssertThrows<InvalidDataException>(() => new RoomPlmShotBlockVisualCatalog(native.Skip(1).Select(pair => new RoomPlmShotBlockVisualEntry(pair.Key, pair.Value))), "shot-block imported stock still requires complete coverage");
    }

    /// <summary>Checks that edited shot-block art reaches both immediate and streamed tiles without changing collision bits or compiled mechanics.</summary>
    private static void VerifyShotBlockVisualSeparation(
)
    {
        RoomPlmShotBlockVisualCatalog stock = RoomPlmShotBlockVisualCatalog.Stock();
        RoomPlmShotBlockVisualEntry[] editedEntries = RoomPlmShotBlockDrawDefinitions.All
            .Select(list => new RoomPlmShotBlockVisualEntry(list.Pointer,
                list.Runs.Span
                    .SelectToArray(run => run.LevelWords.Span
                        .SelectToArray(word => unchecked((ushort)(word & 0x0fff))))
                    ))
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

        ShotBlockPlmProgramsRenderResult Render(
            RoomPlmShotBlockVisualCatalog visuals)
        {
            var level = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(words, new byte[words.Length], new ushort[words.Length]), definitions);
            var plms = PlmFixtures.Unbound(new PlmShotBlockVisuals { RoomPlmShotBlockVisuals = visuals });
            AssertTrue(plms.TrySpawnProjectileShotBlock(level, block, 0, 0, true),
                "visual-only test allocates the production shot-block PLM");
            plms.Step(level);
            return new ShotBlockPlmProgramsRenderResult(level.GetCollisionBlockByIndex(block).PackedWord.Raw,
                level.DisplayedBlockTiles(block).TopLeft.Raw,
                level.DisplayedBlockTiles(block).TopLeft.Raw);
        }

        var native = Render(stock);
        var changed = Render(edited);
        AssertEqual((ushort)0x0053, native.Physical,
            "stock first break frame has its native physical level word");
        AssertEqual(native.Physical, changed.Physical,
            "visual edit cannot change first-frame collision or level data");
        AssertEqual((ushort)0x0053, native.Immediate,
            "stock draw reaches the immediate tilemap upload");
        AssertEqual((ushort)0x0054, changed.Immediate,
            "edited draw reaches the immediate tilemap upload");
        AssertEqual((ushort)0x0054, changed.Streamed,
            "edited draw persists in later camera streaming");

        AssertThrows<InvalidDataException>(
            () => new RoomPlmShotBlockVisualCatalog(editedEntries.Skip(1)),
            "visual catalog rejects missing compiled draw lists");
        first.Runs[0][0] = 0xf054;
        AssertThrows<InvalidDataException>(
            () => new RoomPlmShotBlockVisualCatalog(editedEntries),
            "visual catalog rejects collision bits in editable data");
    }

    /// <summary>Checks installed stock art, user overrides, stock replacement, and corruption rejection.</summary>
    private static void VerifyShotBlockVisualInstallation(ISnesAddressSpace rom)
    {
        using var testRootScratch = new TestScratchDirectory("shot-block-visual");
        string testRoot = testRootScratch.Root;
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
            JsonNode first = document.Required("entries").AsArray().RequiredElements().Single(entry =>
                entry.Required("drawPointer").GetValue<int>() ==
                RoomPlmShotBlockDrawDefinitions.SingleFrame0);
            first.Required("runs").Required(0)[0] = 0x0054;
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

            first.Required("runs").Required(0)[0] = 0xf054;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmShotBlockVisuals(),
                "shot-block visual override rejects collision/type bits");
            first.Required("runs").Required(0)[0] = 0x0054;
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

    /// <summary>Minimal room, and PLM owners for stepping a shot-block program.</summary>
    internal sealed record ShotBlockFixture(
        RoomLevelData Level,
        RoomPlmSystem Plms);

    /// <summary>Spawns the chosen BTS behavior at a fixed 8-by-8 room block for deterministic frame checks.</summary>
    private static ShotBlockFixture NewShotBlockFixture(byte behavior)
    {
        const int width = 8;
        const int height = 8;
        const int blockIndex = 3 + 3 * width;
        var words = new ushort[width * height];
        words[blockIndex] = 0xc321;
        var level = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(words, new byte[words.Length], new ushort[words.Length]), new byte[0x400 * 8]);
        var plms = PlmFixtures.Unbound();
        AssertTrue(plms.TrySpawnProjectileShotBlock(
                level, blockIndex, behavior, 0, solidBlock: true),
            $"shot-block BTS {behavior} allocates its native PLM program");
        return new ShotBlockFixture(level, plms);
    }

    /// <summary>Rejects runtime reads of the compiled control and sound bytes recorded from bank $84.</summary>
    internal static CartridgeReadGuard NewShotBlockProgramReadGuard(ISnesAddressSpace source, HashSet<int> forbidden) =>
        new CartridgeReadGuard(source).Forbid(forbidden.Contains,
            address => $"Production PLM handler read compiled control byte ${address:X6}.");

    /// <summary>Runs the RAM-only regression for the permanent first draw frame and all shot-block timers.</summary>
    [VerificationCommand("--shot-block-program-operands")]
    internal static int ShotBlockProgramOperandsCommand()
    {
        Suite(nameof(VerifyShotBlockProgramOperands), () => VerifyShotBlockProgramOperands());
        return 0;
    }

    /// <summary>Runs the cartridge oracle and editable-art checks for shot-block programs.</summary>
    [VerificationCommand("--shot-block-plm-programs")]
    internal static int ShotBlockPlmProgramsCommand()
    {
        Suite(nameof(VerifyShotBlockPlmPrograms), () => VerifyShotBlockPlmPrograms());
        return 0;
    }
}
