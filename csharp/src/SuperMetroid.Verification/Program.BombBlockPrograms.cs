using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyBombBlockPrograms()
    {
        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var forbidden = new HashSet<int>();
        int wordCount = 0;
        foreach (ushort address in RoomPlmBombBlockProgramDefinitions.MechanicsWordAddresses())
        {
            AssertTrue(RoomPlmBombBlockProgramDefinitions.TryReadMechanicsWord(
                address, out ushort compiled), $"bomb-block control ${address:X4} exists");
            ushort native = unchecked((ushort)(rom.ReadByte(0x840000 | address) |
                rom.ReadByte(0x840000 | (address + 1)) << 8));
            AssertEqual(native, compiled, $"bomb-block control ${address:X4} matches ROM");
            forbidden.Add(0x840000 | address);
            forbidden.Add(0x840000 | (address + 1));
            wordCount++;
        }

        int byteCount = 0;
        foreach (ushort address in RoomPlmBombBlockProgramDefinitions.MechanicsByteAddresses())
        {
            AssertTrue(RoomPlmBombBlockProgramDefinitions.TryReadMechanicsByte(
                address, out byte compiled), $"bomb-block sound ${address:X4} exists");
            AssertEqual(rom.ReadByte(0x840000 | address), compiled,
                $"bomb-block sound ${address:X4} matches ROM");
            forbidden.Add(0x840000 | address);
            byteCount++;
        }

        AssertEqual(88, wordCount, "sixteen bomb-block entries and eight tails have complete control words");
        AssertEqual(16, byteCount, "both sounds for each bomb-block variant are compiled");
        AssertTrue(!RoomPlmBombBlockProgramDefinitions.TryReadMechanicsWord(
                RoomPlmInstructionLists.ReactionBombBlock1x1Respawning + 5, out _),
            "interleaved draw pointer is not misclassified as control");

        int restoreCount = 0;
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in
                 RoomPlmBombBlockRestoreDrawDefinitions.All)
        {
            int cursor = list.Pointer;
            foreach (RoomPlmShotBlockDrawDefinitions.Run run in list.Runs.Span)
            {
                AssertEqual(run.DirectionAndCount,
                    unchecked((ushort)(rom.ReadByte(0x840000 | cursor) |
                        rom.ReadByte(0x840000 | (cursor + 1)) << 8)),
                    $"bomb restoration ${list.Pointer:X4} direction/count");
                forbidden.Add(0x840000 | cursor++);
                forbidden.Add(0x840000 | cursor++);
                foreach (ushort word in run.LevelWords.Span)
                {
                    AssertEqual(word,
                        unchecked((ushort)(rom.ReadByte(0x840000 | cursor) |
                            rom.ReadByte(0x840000 | (cursor + 1)) << 8)),
                        $"bomb restoration ${list.Pointer:X4} level word");
                    forbidden.Add(0x840000 | cursor++);
                    forbidden.Add(0x840000 | cursor++);
                }

                AssertEqual(unchecked((byte)run.NextX), rom.ReadByte(0x840000 | cursor),
                    $"bomb restoration ${list.Pointer:X4} next X");
                forbidden.Add(0x840000 | cursor++);
                AssertEqual(unchecked((byte)run.NextY), rom.ReadByte(0x840000 | cursor),
                    $"bomb restoration ${list.Pointer:X4} next Y");
                forbidden.Add(0x840000 | cursor++);
            }

            restoreCount++;
        }

        AssertEqual(3, restoreCount, "all linked bomb-block restoration lists are compiled");
        AssertTrue(!RoomPlmBombBlockRestoreDrawDefinitions.TryGet(0xa4c6, out _),
            "an unknown nearby bomb restoration pointer does not alias a compiled list");

        // All sixteen breakup frames are shared with the already compiled shot-block
        // family. Forbid their source bytes as well, so this production check covers
        // the complete bomb-block draw path rather than only its three unique restores.
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in
                 RoomPlmShotBlockDrawDefinitions.All)
        {
            int cursor = list.Pointer;
            foreach (RoomPlmShotBlockDrawDefinitions.Run run in list.Runs.Span)
            {
                int byteCountForRun = 2 + 2 * run.LevelWords.Length + 2;
                for (int offset = 0; offset < byteCountForRun; offset++)
                    forbidden.Add(0x840000 | (cursor + offset));
                cursor += byteCountForRun;
            }
        }

        foreach (byte behavior in Enumerable.Range(0, 8).Select(value => (byte)value))
        foreach (BombBlockProducer producer in Enum.GetValues<BombBlockProducer>())
        {
            BombBlockFixture native = NewBombBlockFixture(behavior, producer);
            BombBlockFixture compiled = NewBombBlockFixture(behavior, producer);
            var guarded = new ShotBlockProgramReadGuard(rom, forbidden);
            for (int frame = 0; frame < 420; frame++)
            {
                native.Plms.Step(rom, native.Level, native.Streamer, 0, 0, 0);
                compiled.Plms.Step(guarded, compiled.Level, compiled.Streamer, 0, 0, 0);
                AssertEqual(native.Plms.ActiveCount, compiled.Plms.ActiveCount,
                    $"bomb BTS {behavior}/{producer} active count, frame {frame}");
                AssertEqual(native.Plms.SoundRequests.Count, compiled.Plms.SoundRequests.Count,
                    $"bomb BTS {behavior}/{producer} sound count, frame {frame}");
                for (int block = 0; block < 64; block++)
                {
                    AssertEqual(native.Level.GetCollisionBlockByIndex(block).LevelWord,
                        compiled.Level.GetCollisionBlockByIndex(block).LevelWord,
                        $"bomb BTS {behavior}/{producer} block {block}, frame {frame}");
                    AssertEqual(native.Level.GetCollisionBlockByIndex(block).Behavior,
                        compiled.Level.GetCollisionBlockByIndex(block).Behavior,
                        $"bomb BTS {behavior}/{producer} BTS {block}, frame {frame}");
                }
            }

            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"bomb BTS {behavior}/{producer} never rereads compiled control bytes");
            AssertEqual(0, compiled.Plms.ActiveCount,
                $"bomb BTS {behavior}/{producer} completes its native timeline");
        }

        VerifyBombBlockVisualSeparation(rom, forbidden);

        Console.WriteLine($"Bomb-block PLMs: {wordCount} control words, {byteCount} sounds, {restoreCount} restoration lists, and all 24 collision/bomb/power-bomb timelines match ROM with source reads forbidden.");
    }

    private static void VerifyBombBlockVisualSeparation(
        SuperMetroidAddressSpace rom, HashSet<int> forbidden)
    {
        RoomPlmShotBlockVisualEntry[] entries = RoomPlmShotBlockDrawDefinitions.All
            .Select(list => new RoomPlmShotBlockVisualEntry(list.Pointer,
                list.Runs.Span.ToArray().Select(run =>
                    run.LevelWords.Span.ToArray().Select(word =>
                        new RoomLevelWord(word).VisualWord).ToArray()).ToArray()))
            .ToArray();
        RoomPlmShotBlockVisualEntry first = entries.Single(entry =>
            entry.DrawPointer == RoomPlmShotBlockDrawDefinitions.SingleFrame0);
        first.Runs[0][0] = 0x0054;
        var edited = new RoomPlmShotBlockVisualCatalog(entries);

        (ushort physical, ushort immediate, ushort streamed) Render(
            RoomPlmShotBlockVisualCatalog visuals)
        {
            const int width = 8;
            const int blockIndex = 27;
            var words = new ushort[width * width];
            words[blockIndex] = 0xf321;
            var definitions = new byte[0x400 * 8];
            for (int tile = 0; tile < 4; tile++)
            {
                definitions[0x53 * 8 + tile * 2] = 0x17;
                definitions[0x54 * 8 + tile * 2] = 0x18;
            }

            var level = new RoomLevelData(width, width, words,
                new byte[words.Length], new ushort[words.Length], definitions);
            var plms = new RoomPlmSystem { ShotBlockVisuals = visuals };
            AssertTrue(plms.TrySpawnCollisionBombBlock(level, blockIndex, 0),
                "bomb visual test installs the real collision-triggered PLM");
            plms.Step(new ShotBlockProgramReadGuard(rom, forbidden), level,
                level.CreateBackgroundStreamer(), 0, 0, 0);
            AssertEqual(1, plms.TilemapUpdates.Count,
                "first bomb-break frame publishes one immediate tile upload");
            return (level.GetCollisionBlockByIndex(blockIndex).LevelWord,
                plms.TilemapUpdates[0].TopRow[0],
                level.CreateBackgroundStreamer().BuildPlmLevelBlockUpdate(
                    blockIndex, 0).TopRow[0]);
        }

        var stock = Render(RoomPlmShotBlockVisualCatalog.Stock());
        var changed = Render(edited);
        AssertEqual((ushort)0x0053, stock.physical,
            "native bomb-break frame installs its original air collision word");
        AssertEqual(stock.physical, changed.physical,
            "shared breakup-art edit cannot change bomb-block physical collision");
        AssertEqual((ushort)0x0017, stock.immediate,
            "stock breakup frame reaches immediate tile upload");
        AssertEqual((ushort)0x0018, changed.immediate,
            "edited breakup frame reaches immediate bomb-block tile upload");
        AssertEqual((ushort)0x0018, changed.streamed,
            "edited bomb-block breakup frame survives later camera streaming");

        (ushort physical, ushort streamed) RenderRestoredParent(byte tileIndex)
        {
            const int width = 8;
            const int blockIndex = 27;
            var words = new ushort[width * width];
            words[blockIndex] = 0xf321;
            var definitions = new byte[0x400 * 8];
            for (int tile = 0; tile < 4; tile++)
                definitions[0x58 * 8 + tile * 2] = tileIndex;
            var level = new RoomLevelData(width, width, words,
                new byte[words.Length], new ushort[words.Length], definitions);
            var plms = new RoomPlmSystem();
            AssertTrue(plms.TrySpawnCollisionBombBlock(level, blockIndex, 1),
                "restored-art test installs linked respawning bomb block");
            var streamer = level.CreateBackgroundStreamer();
            var guarded = new ShotBlockProgramReadGuard(rom, forbidden);
            for (int frame = 0; frame < 420; frame++)
                plms.Step(guarded, level, streamer, 0, 0, 0);
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                "linked restoration uses no compiled source draw bytes");
            return (level.GetCollisionBlockByIndex(blockIndex).LevelWord,
                level.CreateBackgroundStreamer().BuildPlmLevelBlockUpdate(
                    blockIndex, 0).TopRow[0]);
        }

        var stockParent = RenderRestoredParent(0x17);
        var editedParent = RenderRestoredParent(0x18);
        AssertEqual((ushort)0xf058, stockParent.physical,
            "linked bomb block restores its native solid parent word");
        AssertEqual(stockParent.physical, editedParent.physical,
            "editing block $058 composition leaves restored collision intact");
        AssertEqual((ushort)0x0017, stockParent.streamed,
            "stock restored parent uses its room-block composition");
        AssertEqual((ushort)0x0018, editedParent.streamed,
            "edited block $058 composition reaches later streaming");
    }

    private enum BombBlockProducer { Collision, Bomb, PowerBomb }

    private sealed record BombBlockFixture(
        RoomLevelData Level, BackgroundTilemapStreamer Streamer, RoomPlmSystem Plms);

    private static BombBlockFixture NewBombBlockFixture(byte behavior, BombBlockProducer producer)
    {
        const int width = 8;
        const int blockIndex = 27;
        var words = new ushort[width * width];
        words[blockIndex] = 0xf321;
        var bts = new byte[words.Length];
        bts[blockIndex] = behavior;
        var level = new RoomLevelData(width, width, words, bts,
            new ushort[words.Length], new byte[0x400 * 8]);
        var plms = new RoomPlmSystem();
        bool spawned = producer switch
        {
            BombBlockProducer.Collision =>
                plms.TrySpawnCollisionBombBlock(level, blockIndex, behavior),
            BombBlockProducer.Bomb =>
                plms.TrySpawnBombReactionBlock(level, blockIndex, behavior,
                    (ushort)SamusProjectileFamily.Bomb),
            BombBlockProducer.PowerBomb =>
                plms.TrySpawnBombReactionBlock(level, blockIndex, behavior,
                    (ushort)SamusProjectileFamily.PowerBomb),
            _ => throw new ArgumentOutOfRangeException(nameof(producer)),
        };
        AssertTrue(spawned, $"bomb BTS {behavior}/{producer} installs its native program");
        return new BombBlockFixture(level, level.CreateBackgroundStreamer(), plms);
    }
}
