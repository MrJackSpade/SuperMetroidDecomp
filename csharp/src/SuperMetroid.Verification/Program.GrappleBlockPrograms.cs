using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyGrappleBlockControlMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses = [0xcd6a,0xcd6e,0xcd71,0xcd75,0xcd79,0xcd7d,0xcd81,0xcd85,0xcd89,0xcd8d,0xcd8f,0xcd91,
            0xcda9,0xcdad,0xcdb0,0xcdb4,0xcdb8,0xcdbc,0xcdc0];
        AssertTrue(addresses.SequenceEqual(RoomPlmGrappleBlockProgramDefinitions.MechanicsWordAddresses()), "Grapple native control enumeration");
        var known = addresses.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool found = RoomPlmGrappleBlockProgramDefinitions.TryReadMechanicsWord((ushort)address,out ushort value);
            AssertEqual(known.Contains((ushort)address),found,"Grapple complete control domain including callback gap");
            AssertEqual(found ? ReadBotwoonInstructionWord(rom,0x840000 | address) : (ushort)0,value,"Grapple native timing, sound queue, BTS restoration and deletion");
        }
    }

    private static void VerifyGrappleBlockDrawOperandMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses = [0xcd6c,0xcd73,0xcd77,0xcd7b,0xcd7f,0xcd83,0xcd87,0xcd8b,
            0xcdab,0xcdb2,0xcdb6,0xcdba,0xcdbe];
        var known = addresses.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool found = RoomPlmGrappleBlockProgramDefinitions.TryReadDrawPointerWord((ushort)address,out ushort value);
            AssertEqual(known.Contains((ushort)address),found,"Grapple complete draw operand domain");
            AssertEqual(found ? ReadBotwoonInstructionWord(rom,0x840000 | address) : (ushort)0,value,"Grapple native initial, forward and reverse draw selection");
        }
    }

    private static void VerifyGrappleBlockSoundMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses = [0xcd70,0xcdaf];
        AssertTrue(addresses.SequenceEqual(RoomPlmGrappleBlockProgramDefinitions.MechanicsByteAddresses()), "Grapple native sound enumeration");
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool found = RoomPlmGrappleBlockProgramDefinitions.TryReadMechanicsByte((ushort)address,out byte value);
            AssertEqual(addresses.Contains((ushort)address),found,"Grapple complete packed sound domain");
            AssertEqual(found ? rom.ReadByte(0x840000 | address) : (byte)0,value,"Grapple native packed sound bytes");
        }
    }

    private static void VerifyGrappleBlockPhysicalDrawMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] pointers = [0xa4f9,0xa4ff,0xa505,0xa50b,0xa511];
        var exported = RoomPlmGrappleBlockDrawDefinitions.All.ToArray();
        AssertTrue(pointers.SequenceEqual(exported.Select(draw => draw.Pointer)), "Grapple native draw export order");
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
        {
            bool found = RoomPlmGrappleBlockDrawDefinitions.TryGet((ushort)pointer,out var draw);
            AssertEqual(pointers.Contains((ushort)pointer),found,"Grapple full native draw pointer domain");
            if (!found)
            {
                AssertEqual(default(RoomPlmGrappleBlockDrawDefinitions.DrawList),draw,"Grapple missing draw cleared");
                continue;
            }
            AssertEqual((ushort)pointer,draw.Pointer,"Grapple draw identity");
            AssertEqual(ReadBotwoonInstructionWord(rom,0x840000 | pointer),RoomPlmGrappleBlockDrawDefinitions.DrawList.DirectionAndCount,"Grapple native single-block geometry");
            AssertEqual(ReadBotwoonInstructionWord(rom,0x840000 | (pointer + 2)),draw.LevelWord,"Grapple native collision and tile word");
            AssertEqual(unchecked((sbyte)rom.ReadByte(0x840000 | (pointer + 4))),RoomPlmGrappleBlockDrawDefinitions.DrawList.NextX,"Grapple native terminal X");
            AssertEqual(unchecked((sbyte)rom.ReadByte(0x840000 | (pointer + 5))),RoomPlmGrappleBlockDrawDefinitions.DrawList.NextY,"Grapple native terminal Y");
            AssertEqual(draw,exported.Single(entry => entry.Pointer == pointer),"Grapple exported draw matches calculation");
        }
    }

    private static void VerifyGrappleBlockStockVisualMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] pointers = [0xa4f9,0xa4ff,0xa505,0xa50b,0xa511];
        var native = pointers.ToDictionary(pointer => pointer, pointer =>
            (ushort)(ReadBotwoonInstructionWord(rom,0x840000 | (pointer + 2)) & 0xfff));
        var entries = native.Select(pair => new RoomPlmGrappleBlockVisualEntry(pair.Key,pair.Value)).ToArray();
        var stock = RoomPlmGrappleBlockVisualCatalog.Stock();
        var imported = new RoomPlmGrappleBlockVisualCatalog(entries);
        string expectedHash = SuperMetroid.Core.Assets.SelectedPresentationHash.FromWordFrames(nameof(RoomPlmGrappleBlockVisualCatalog),native);
        AssertEqual(expectedHash,stock.ContentIdentity,"Grapple calculated stock original hash");
        AssertEqual(expectedHash,imported.ContentIdentity,"Grapple imported stock original hash");
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
        {
            if (native.TryGetValue((ushort)pointer,out ushort word))
            {
                AssertEqual(word,stock.GetWord((ushort)pointer),"Grapple calculated native visual word");
                AssertEqual(word,imported.GetWord((ushort)pointer),"Grapple imported native visual word");
            }
            else
                AssertThrows<InvalidDataException>(() => stock.GetWord((ushort)pointer),"Grapple stock rejects unsupported draw identity");
        }
        entries[0] = new(pointers[0],0x0058);
        entries[4] = new(pointers[4],0x0453);
        var mixed = new RoomPlmGrappleBlockVisualCatalog(entries);
        var expected = entries.ToDictionary(entry => entry.DrawPointer,entry => entry.VisualWord);
        AssertEqual(SuperMetroid.Core.Assets.SelectedPresentationHash.FromWordFrames(nameof(RoomPlmGrappleBlockVisualCatalog),expected),
            mixed.ContentIdentity,"Grapple mixed stock/override identity");
        entries[0] = new(pointers[0],0x0054);
        foreach (var pair in expected)
            AssertEqual(pair.Value,mixed.GetWord(pair.Key),"Grapple selected artwork isolated from entry-array mutation");
        AssertThrows<InvalidDataException>(() => new RoomPlmGrappleBlockVisualCatalog(entries[..4]),"Grapple rejects incomplete selection");
        AssertThrows<InvalidDataException>(() => new RoomPlmGrappleBlockVisualCatalog(entries.Append(entries[0])),"Grapple rejects duplicate selection");
        entries[0] = new(pointers[0],0xe0b7);
        AssertThrows<InvalidDataException>(() => new RoomPlmGrappleBlockVisualCatalog(entries),"Grapple rejects artwork collision bits");
    }

    private static void VerifyGrappleBlockPrograms()
    {
        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyGrappleBlockStockVisualMapping), () => VerifyGrappleBlockStockVisualMapping(rom));
        Suite(nameof(VerifyGrappleBlockControlMapping), () => VerifyGrappleBlockControlMapping(rom));
        Suite(nameof(VerifyGrappleBlockDrawOperandMapping), () => VerifyGrappleBlockDrawOperandMapping(rom));
        Suite(nameof(VerifyGrappleBlockSoundMapping), () => VerifyGrappleBlockSoundMapping(rom));
        var forbidden = new HashSet<int>();
        foreach (ushort address in RoomPlmGrappleBlockProgramDefinitions.MechanicsWordAddresses())
        {
            forbidden.Add(0x840000 | address);
            forbidden.Add(0x840000 | (address + 1));
        }
        foreach (ushort address in RoomPlmGrappleBlockProgramDefinitions.MechanicsByteAddresses())
            forbidden.Add(0x840000 | address);

        Suite(nameof(VerifyGrappleBlockPhysicalDrawMapping), () => VerifyGrappleBlockPhysicalDrawMapping(rom));
        for (int address = 0xa4f9; address < 0xa517; address++)
            forbidden.Add(0x840000 | address);

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

        Suite(nameof(VerifyGrappleBlockVisualSeparation), () => VerifyGrappleBlockVisualSeparation(rom, forbidden));
        Suite(nameof(VerifyGrappleBlockVisualInstallation), () => VerifyGrappleBlockVisualInstallation(rom));

        Console.WriteLine($"Grapple-block PLMs: 19 control words, 2 sound bytes, and 5 draw lists match ROM; both programs run with source reads forbidden.");
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
