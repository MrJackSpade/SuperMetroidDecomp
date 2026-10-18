using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.MotherBrain.BabyMetroidRouteDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.Plms.ShotBlockPlmProgramsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Samus.Grapple;
using SuperMetroid.Verification.Harness;

/// <summary>Retail and guarded-runtime checks for respawning/permanent grapple-block PLMs and editable visuals.</summary>
internal static class GrappleBlockProgramsVerification
{
    /// <summary>Checks all nineteen compiled control addresses and values across the full 16-bit bank-local domain.</summary>
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
            AssertEqual(found ? ReadRomWord(rom,0x840000 | address) : (ushort)0,value,"Grapple native timing, sound queue, BTS restoration and deletion");
        }
    }

    /// <summary>Checks the thirteen interleaved draw-pointer words while rejecting non-operand addresses.</summary>
    private static void VerifyGrappleBlockDrawOperandMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses = [0xcd6c,0xcd73,0xcd77,0xcd7b,0xcd7f,0xcd83,0xcd87,0xcd8b,
            0xcdab,0xcdb2,0xcdb6,0xcdba,0xcdbe];
        var known = addresses.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool found = RoomPlmGrappleBlockProgramDefinitions.TryReadDrawPointerWord((ushort)address,out ushort value);
            AssertEqual(known.Contains((ushort)address),found,"Grapple complete draw operand domain");
            AssertEqual(found ? ReadRomWord(rom,0x840000 | address) : (ushort)0,value,"Grapple native initial, forward and reverse draw selection");
        }
    }

    /// <summary>Checks both packed sound operands and exact byte-address ownership.</summary>
    private static void VerifyGrappleBlockSoundMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses = [0xcd70,0xcdaf];
        AssertTrue(addresses.SequenceEqual(RoomPlmGrappleBlockProgramDefinitions.MechanicsByteAddresses()), "Grapple native sound enumeration");
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool found = RoomPlmGrappleBlockProgramDefinitions.TryReadMechanicsByte((ushort)address,out byte value);
            AssertEqual(addresses.Contains((ushort)address),found,"Grapple complete packed sound domain");
            AssertEqual(found ? GrappleBlockSoundMappingRomByte(rom, 0x840000 | address) : (byte)0,value,"Grapple native packed sound bytes");
        }
    }

    /// <summary>Reads a cartridge sound byte solely as the compiled-operand oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for grapple block sound mapping")]
    private static byte GrappleBlockSoundMappingRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Compares five one-block draw lists with native geometry, collision words, and terminal offsets.</summary>
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
            AssertEqual(ReadRomWord(rom,0x840000 | pointer),RoomPlmGrappleBlockDrawDefinitions.DrawList.DirectionAndCount,"Grapple native single-block geometry");
            AssertEqual(ReadRomWord(rom,0x840000 | (pointer + 2)),draw.LevelWord,"Grapple native collision and tile word");
            AssertEqual(unchecked((sbyte)GrappleBlockPhysicalDrawMappingRomByte(rom, 0x840000 | (pointer + 4))),RoomPlmGrappleBlockDrawDefinitions.DrawList.NextX,"Grapple native terminal X");
            AssertEqual(unchecked((sbyte)GrappleBlockPhysicalDrawMappingRomByte(rom, 0x840000 | (pointer + 5))),RoomPlmGrappleBlockDrawDefinitions.DrawList.NextY,"Grapple native terminal Y");
            AssertEqual(draw,exported.Single(entry => entry.Pointer == pointer),"Grapple exported draw matches calculation");
        }
    }

    /// <summary>Reads native draw-list terminal offsets as the physical-geometry oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for grapple block physical draw mapping")]
    private static byte GrappleBlockPhysicalDrawMappingRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks calculated stock tile choices and content identity, then rejects missing, duplicate, or collision-bit edits.</summary>
    private static void VerifyGrappleBlockStockVisualMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] pointers = [0xa4f9,0xa4ff,0xa505,0xa50b,0xa511];
        var native = pointers.ToDictionary(pointer => pointer, pointer =>
            (ushort)(ReadRomWord(rom,0x840000 | (pointer + 2)) & 0xfff));
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

    /// <summary>Runs both BTS programs under read guards for 300 frames and checks edited art remains separate from physical collision.</summary>
    internal static void VerifyGrappleBlockPrograms()
    {
        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
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
            var guarded = NewShotBlockProgramReadGuard(rom, forbidden);
            for (int frame = 0; frame < 300; frame++)
            {
                native.Plms.Step(native.Level, native.Streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
                compiled.Plms.Step(compiled.Level, compiled.Streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
                AssertEqual(native.Plms.SlotTable.ActiveCount, compiled.Plms.SlotTable.ActiveCount,
                    $"Grapple BTS {bts} active count, frame {frame}");
                AssertEqual(native.Plms.Sounds.SoundRequests.Count, compiled.Plms.Sounds.SoundRequests.Count,
                    $"Grapple BTS {bts} sound count, frame {frame}");
                AssertEqual(native.Level.GetCollisionBlockByIndex(27).PackedWord.Raw,
                    compiled.Level.GetCollisionBlockByIndex(27).PackedWord.Raw,
                    $"Grapple BTS {bts} level word, frame {frame}");
                AssertEqual(native.Level.GetCollisionBlockByIndex(27).Bts.Value,
                    compiled.Level.GetCollisionBlockByIndex(27).Bts.Value,
                    $"Grapple BTS {bts} BTS, frame {frame}");
            }

            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"Grapple BTS {bts} never rereads compiled control bytes");
            AssertEqual(0, compiled.Plms.SlotTable.ActiveCount,
                $"Grapple BTS {bts} finishes its cartridge timeline");
        }

        Suite(nameof(VerifyGrappleBlockVisualSeparation), () => VerifyGrappleBlockVisualSeparation());
        Suite(nameof(VerifyGrappleBlockVisualInstallation), () => VerifyGrappleBlockVisualInstallation(rom));

        Console.WriteLine($"Grapple-block PLMs: 19 control words, 2 sound bytes, and 5 draw lists match ROM; both programs run with source reads forbidden.");
    }

    /// <summary>Confirms a first-frame tile edit changes immediate and streamed visuals while retaining the same grapple collision word.</summary>
    private static void VerifyGrappleBlockVisualSeparation(
)
    {
        RoomPlmGrappleBlockVisualEntry[] entries =
            RoomPlmGrappleBlockDrawDefinitions.All.Select(draw =>
                new RoomPlmGrappleBlockVisualEntry(draw.Pointer,
                    new RoomLevelWord(draw.LevelWord).VisualWord)).ToArray();
        int first = Array.FindIndex(entries, entry =>
            entry.DrawPointer == RoomPlmGrappleBlockDrawDefinitions.Grapple);
        entries[first] = entries[first] with { VisualWord = 0x00b8 };
        var edited = new RoomPlmGrappleBlockVisualCatalog(entries);

        // Run the first PLM draw and capture collision, immediate tile upload, and later camera-streamed tile output.
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

            var level = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(words, new byte[words.Length], new ushort[words.Length]), definitions);
            var plms = new RoomPlmSystem { Drawing = { GrappleBlockVisuals = visuals } };
            AssertTrue(plms.TrySpawnBreakableGrappleBlock(level, blockIndex, 1),
                "visual test starts the real respawning Grapple PLM");
            plms.Step(level, level.CreateBackgroundStreamer(), new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
            AssertEqual(1, plms.TilemapUpdates.Count,
                "first Grapple draw publishes one immediate tile upload");
            return (level.GetCollisionBlockByIndex(blockIndex).PackedWord.Raw,
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

    /// <summary>Checks extracted stock, edited override, stock refresh, and rejection of invalid art or corrupt stock manifest.</summary>
    private static void VerifyGrappleBlockVisualInstallation(SuperMetroidAddressSpace rom)
    {
        using var testRootScratch = new TestScratchDirectory("grapple-block-visual");
        string testRoot = testRootScratch.Root;
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
            JsonNode first = document.Required("entries").AsArray().RequiredElements().Single(entry =>
                entry.Required("drawPointer").GetValue<int>() ==
                RoomPlmGrappleBlockDrawDefinitions.Grapple);
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

    /// <summary>Constructed room collision, tile streamer, and PLM system for one grapple BTS program.</summary>
    internal sealed record GrappleBlockFixture(
        RoomLevelData Level, BackgroundTilemapStreamer Streamer, RoomPlmSystem Plms);

    /// <summary>Spawns one grapple block at cell 27 in an eight-column synthetic room.</summary>
    private static GrappleBlockFixture NewGrappleBlockFixture(byte bts)
    {
        const int width = 8;
        const int blockIndex = 27;
        var words = new ushort[width * width];
        words[blockIndex] = 0xe123;
        var level = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(words, new byte[words.Length], new ushort[words.Length]), new byte[0x400 * 8]);
        var plms = new RoomPlmSystem();
        AssertTrue(plms.TrySpawnBreakableGrappleBlock(level, blockIndex, bts),
            $"Grapple BTS {bts} installs its native program");
        return new GrappleBlockFixture(level, level.CreateBackgroundStreamer(), plms);
    }

    /// <summary>Runs the grapple-block mechanics and visual checks from their command-line switch.</summary>
    [VerificationCommand("--grapple-block-programs")]
    internal static int GrappleBlockProgramsCommand()
    {
        Suite(nameof(VerifyGrappleBlockPrograms), () => VerifyGrappleBlockPrograms());
        return 0;
    }
}
