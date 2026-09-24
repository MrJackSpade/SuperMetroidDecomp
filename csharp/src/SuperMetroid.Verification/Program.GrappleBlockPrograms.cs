using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyGrappleBlockPrograms()
    {
        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var forbidden = new HashSet<int>();
        int wordCount = 0;
        foreach (ushort address in RoomPlmGrappleBlockProgramDefinitions.MechanicsWordAddresses())
        {
            AssertTrue(RoomPlmGrappleBlockProgramDefinitions.TryReadMechanicsWord(
                address, out ushort compiled), $"Grapple-block control ${address:X4} exists");
            ushort native = unchecked((ushort)(rom.ReadByte(0x840000 | address) |
                rom.ReadByte(0x840000 | (address + 1)) << 8));
            AssertEqual(native, compiled, $"Grapple-block control ${address:X4} matches ROM");
            forbidden.Add(0x840000 | address);
            forbidden.Add(0x840000 | (address + 1));
            wordCount++;
        }

        int byteCount = 0;
        foreach (ushort address in RoomPlmGrappleBlockProgramDefinitions.MechanicsByteAddresses())
        {
            AssertTrue(RoomPlmGrappleBlockProgramDefinitions.TryReadMechanicsByte(
                address, out byte compiled), $"Grapple-block sound ${address:X4} exists");
            AssertEqual(rom.ReadByte(0x840000 | address), compiled,
                $"Grapple-block sound ${address:X4} matches ROM");
            forbidden.Add(0x840000 | address);
            byteCount++;
        }

        AssertEqual(19, wordCount, "both Grapple-block programs contain all control words");
        AssertEqual(2, byteCount, "both Grapple-block programs contain sound bytes");
        AssertTrue(!RoomPlmGrappleBlockProgramDefinitions.TryReadMechanicsWord(
                RoomPlmInstructionLists.RespawningBreakableGrappleBlock + 2, out _),
            "first draw pointer remains outside the compiled control domain");

        int drawCount = 0;
        foreach (RoomPlmGrappleBlockDrawDefinitions.DrawList draw in
                 RoomPlmGrappleBlockDrawDefinitions.All)
        {
            ushort[] native = new ushort[2];
            for (int word = 0; word < native.Length; word++)
            {
                int address = 0x840000 | (draw.Pointer + word * 2);
                native[word] = unchecked((ushort)(rom.ReadByte(address) |
                    rom.ReadByte(address + 1) << 8));
                forbidden.Add(address);
                forbidden.Add(address + 1);
            }

            AssertEqual(RoomPlmGrappleBlockDrawDefinitions.DrawList.DirectionAndCount,
                native[0], $"Grapple draw ${draw.Pointer:X4} native block count");
            AssertEqual(draw.LevelWord, native[1],
                $"Grapple draw ${draw.Pointer:X4} native level word");
            int terminator = 0x840000 | (draw.Pointer + 4);
            AssertEqual(unchecked((byte)RoomPlmGrappleBlockDrawDefinitions.DrawList.NextX),
                rom.ReadByte(terminator),
                $"Grapple draw ${draw.Pointer:X4} native X terminator");
            AssertEqual(unchecked((byte)RoomPlmGrappleBlockDrawDefinitions.DrawList.NextY),
                rom.ReadByte(terminator + 1),
                $"Grapple draw ${draw.Pointer:X4} native Y terminator");
            forbidden.Add(terminator);
            forbidden.Add(terminator + 1);
            drawCount++;
        }

        AssertEqual(5, drawCount, "all breakable-Grapple-block draw lists are compiled");
        AssertTrue(!RoomPlmGrappleBlockDrawDefinitions.TryGet(0xa4f8, out _),
            "a nearby unknown draw pointer does not alias a compiled Grapple list");

        for (byte bts = 1; bts <= 2; bts++)
        {
            GrappleBlockFixture native = NewGrappleBlockFixture(bts);
            GrappleBlockFixture compiled = NewGrappleBlockFixture(bts);
            var guarded = new ShotBlockProgramReadGuard(rom, forbidden);
            for (int frame = 0; frame < 300; frame++)
            {
                native.Plms.Step(rom, native.Level, native.Streamer, 0, 0, 0);
                compiled.Plms.Step(guarded, compiled.Level, compiled.Streamer, 0, 0, 0);
                AssertEqual(native.Plms.ActiveCount, compiled.Plms.ActiveCount,
                    $"Grapple BTS {bts} active count, frame {frame}");
                AssertEqual(native.Plms.SoundRequests.Count, compiled.Plms.SoundRequests.Count,
                    $"Grapple BTS {bts} sound count, frame {frame}");
                AssertEqual(native.Level.GetCollisionBlockByIndex(27).LevelWord,
                    compiled.Level.GetCollisionBlockByIndex(27).LevelWord,
                    $"Grapple BTS {bts} level word, frame {frame}");
                AssertEqual(native.Level.GetCollisionBlockByIndex(27).Behavior,
                    compiled.Level.GetCollisionBlockByIndex(27).Behavior,
                    $"Grapple BTS {bts} BTS, frame {frame}");
            }

            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"Grapple BTS {bts} never rereads compiled control bytes");
            AssertEqual(0, compiled.Plms.ActiveCount,
                $"Grapple BTS {bts} finishes its cartridge timeline");
        }

        VerifyGrappleBlockVisualSeparation(rom, forbidden);
        VerifyGrappleBlockVisualInstallation(rom);

        Console.WriteLine($"Grapple-block PLMs: {wordCount} control words, {byteCount} sound bytes, and {drawCount} draw lists match ROM; both programs run with source reads forbidden.");
    }

    private static void VerifyGrappleBlockVisualSeparation(
        SuperMetroidAddressSpace rom, HashSet<int> forbidden)
    {
        RoomPlmGrappleBlockVisualEntry[] entries =
            RoomPlmGrappleBlockDrawDefinitions.All.Select(draw =>
                new RoomPlmGrappleBlockVisualEntry(draw.Pointer,
                    new RoomLevelWord(draw.LevelWord).VisualWord)).ToArray();
        int first = Array.FindIndex(entries, entry =>
            entry.DrawPointer == RoomPlmGrappleBlockDrawDefinitions.Grapple);
        entries[first] = entries[first] with { VisualWord = 0x00b8 };
        var edited = new RoomPlmGrappleBlockVisualCatalog(entries);

        (ushort physical, ushort immediate, ushort streamed) Render(
            RoomPlmGrappleBlockVisualCatalog visuals)
        {
            const int width = 8;
            const int blockIndex = 27;
            var words = new ushort[width * width];
            words[blockIndex] = 0xe123;
            var definitions = new byte[0x400 * 8];
            for (int tile = 0; tile < 4; tile++)
            {
                definitions[0xb7 * 8 + tile * 2] = 0x17;
                definitions[0xb8 * 8 + tile * 2] = 0x18;
            }

            var level = new RoomLevelData(width, width, words,
                new byte[words.Length], new ushort[words.Length], definitions);
            var plms = new RoomPlmSystem { GrappleBlockVisuals = visuals };
            AssertTrue(plms.TrySpawnBreakableGrappleBlock(level, blockIndex, 1),
                "visual test starts the real respawning Grapple PLM");
            plms.Step(new ShotBlockProgramReadGuard(rom, forbidden), level,
                level.CreateBackgroundStreamer(), 0, 0, 0);
            AssertEqual(1, plms.TilemapUpdates.Count,
                "first Grapple draw publishes one immediate tile upload");
            return (level.GetCollisionBlockByIndex(blockIndex).LevelWord,
                plms.TilemapUpdates[0].TopRow[0],
                level.CreateBackgroundStreamer().BuildPlmLevelBlockUpdate(
                    blockIndex, 0).TopRow[0]);
        }

        var stock = Render(RoomPlmGrappleBlockVisualCatalog.Stock());
        var changed = Render(edited);
        AssertEqual((ushort)0xe0b7, stock.physical,
            "native first Grapple frame installs Grapple collision");
        AssertEqual(stock.physical, changed.physical,
            "art edit does not change the physical Grapple collision word");
        AssertEqual((ushort)0x0017, stock.immediate,
            "stock visual block reaches immediate tile upload");
        AssertEqual((ushort)0x0018, changed.immediate,
            "edited visual block reaches immediate tile upload");
        AssertEqual((ushort)0x0018, changed.streamed,
            "edited visual block persists through later camera streaming");
        AssertThrows<InvalidDataException>(
            () => new RoomPlmGrappleBlockVisualCatalog(entries.Skip(1)),
            "Grapple catalog rejects incomplete draw-list coverage");
        entries[first] = entries[first] with { VisualWord = 0xf0b8 };
        AssertThrows<InvalidDataException>(
            () => new RoomPlmGrappleBlockVisualCatalog(entries),
            "Grapple catalog rejects collision bits in editable data");
    }

    private static void VerifyGrappleBlockVisualInstallation(SuperMetroidAddressSpace rom)
    {
        string testRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "grapple-block-visual-" + Guid.NewGuid().ToString("N")));
        string allowedRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp")) +
            Path.DirectorySeparatorChar;
        if (!testRoot.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Grapple-block test root escaped test-temp.");
        try
        {
            var installation = new GameInstallation(testRoot);
            RoomPlmGrappleBlockVisualFiles.Extract(rom,
                installation.RoomPlmGrappleBlockVisualDirectory, SupportedCartridge.Sha256);
            RoomPlmGrappleBlockVisualFiles.ValidateStock(
                installation.RoomPlmGrappleBlockVisualDirectory);
            AssertEqual((ushort)0x00b7,
                installation.LoadRoomPlmGrappleBlockVisuals().GetWord(
                    RoomPlmGrappleBlockDrawDefinitions.Grapple),
                "installed Grapple frame matches native stock art");

            string stockPath = Path.Combine(
                installation.RoomPlmGrappleBlockVisualDirectory,
                RoomPlmGrappleBlockVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted Grapple-block JSON is empty.");
            JsonNode first = document["entries"]!.AsArray().Single(entry =>
                entry!["drawPointer"]!.GetValue<int>() ==
                RoomPlmGrappleBlockDrawDefinitions.Grapple)!;
            first["visualWord"] = 0x00b8;
            Directory.CreateDirectory(installation.RoomPlmGrappleBlockVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmGrappleBlockVisualOverrideDirectory,
                RoomPlmGrappleBlockVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertEqual((ushort)0x00b8,
                installation.LoadRoomPlmGrappleBlockVisuals().GetWord(
                    RoomPlmGrappleBlockDrawDefinitions.Grapple),
                "Grapple-block override selects the edited art");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmGrappleBlockVisualFiles.Extract(rom, refreshed, SupportedCartridge.Sha256);
            AssertEqual((ushort)0x00b8,
                RoomPlmGrappleBlockVisualFiles.Load(refreshed,
                    installation.RoomPlmGrappleBlockVisualOverrideDirectory).GetWord(
                        RoomPlmGrappleBlockDrawDefinitions.Grapple),
                "Grapple edit survives stock-content replacement");
            first["visualWord"] = 0xf0b8;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmGrappleBlockVisuals(),
                "Grapple override rejects collision-bit edits");
            first["visualWord"] = 0x00b8;
            File.WriteAllText(overridePath, document.ToJsonString());
            File.WriteAllText(stockPath, "corrupt stock");
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmGrappleBlockVisuals(),
                "Grapple stock manifest hash rejects corruption");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
    }

    private sealed record GrappleBlockFixture(
        RoomLevelData Level, BackgroundTilemapStreamer Streamer, RoomPlmSystem Plms);

    private static GrappleBlockFixture NewGrappleBlockFixture(byte bts)
    {
        const int width = 8;
        const int blockIndex = 27;
        var words = new ushort[width * width];
        words[blockIndex] = 0xe123;
        var level = new RoomLevelData(width, width, words,
            new byte[words.Length], new ushort[words.Length], new byte[0x400 * 8]);
        var plms = new RoomPlmSystem();
        AssertTrue(plms.TrySpawnBreakableGrappleBlock(level, blockIndex, bts),
            $"Grapple BTS {bts} installs its native program");
        return new GrappleBlockFixture(level, level.CreateBackgroundStreamer(), plms);
    }
}
