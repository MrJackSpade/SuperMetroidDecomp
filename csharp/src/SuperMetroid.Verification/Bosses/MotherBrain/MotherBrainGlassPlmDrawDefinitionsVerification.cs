using SuperMetroid.Core.Assets;
using SuperMetroid.Core;
using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.MotherBrain.LookupGlassLayoutsVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.LookupMotherBrainGlassStockVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainGlassPlmProgramVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.SyntheticRoomCartridgeFixturesVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Checks compiled Mother Brain glass draw layouts through the guarded production PLM path.</summary>
internal static class MotherBrainGlassPlmDrawDefinitionsVerification
{
    /// <summary>Compares eleven physical draw lists with cartridge data and verifies editable visual words leave collision blocks intact.</summary>
    internal static void VerifyMotherBrainGlassPlmDrawDefinitions(
        SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyMotherBrainGlassPlmProgram), () => VerifyMotherBrainGlassPlmProgram(rom));
        Suite(nameof(VerifyGlassLayoutGeometry), () => VerifyGlassLayoutGeometry(rom));
        Suite(nameof(VerifyGlassLayoutCollision), () => VerifyGlassLayoutCollision(rom));
        Suite(nameof(VerifyGlassLayoutVisuals), () => VerifyGlassLayoutVisuals(rom));

        RoomPlmShotBlockDrawDefinitions.DrawList[] lists =
            MotherBrainGlassPlmDrawDefinitions.All.OrderBy(list => PlmDrawAddressCodec.Of(list.Draw)).ToArray();
        RoomPlmVisualEntry[] entries = lists.Select(draw =>
            new RoomPlmVisualEntry(
                MotherBrainGlassPlmDrawDefinitions.VisualId(draw.Draw),
                draw.VisualWords())).ToArray();
        RoomPlmVisualEntry shatter = entries.Single(entry =>
            entry.Id == "shatter-1");
        ushort originalWord = shatter.Blocks[6];
        shatter.Blocks[6] = 0x0057;
        var edited = new RoomPlmMotherBrainGlassVisualCatalog(entries);
        shatter.Blocks[6] = 0x0058;
        AssertEqual((ushort)0x0057, edited.GetWord(NativePlmDraws.At<MotherBrainGlassDraw>(0x978f), 1, 2),
            "glass visual catalog copies author data");
        AssertEqual(originalWord,
            RoomPlmMotherBrainGlassVisualCatalog.Stock().GetWord(NativePlmDraws.At<MotherBrainGlassDraw>(0x978f), 1, 2),
            "stock glass visual catalog retains the native shatter tile");
        shatter.Blocks[6] = originalWord;
        AssertEqual(11, lists.Length,
            "Mother Brain glass program selects eleven distinct physical draw lists");
        var bank84 = new byte[0x8000];
        for (int index = 0; index < bank84.Length; index++)
            bank84[index] = rom.ReadCartridgeByte(0x848000 + index);
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in lists)
            VerifyMotherBrainGlassNativeDrawPath(bank84, lists, list,
                PlmDrawAddressCodec.Of(list.Draw) == 0x978f ? edited : null);
        Suite(nameof(VerifyMotherBrainGlassVisualInstallation), () => VerifyMotherBrainGlassVisualInstallation(rom));
        Console.WriteLine(
            "  Mother Brain glass PLM: 11 guarded native layouts and editable stock/override appearance preserve physical blocks.");
    }

    /// <summary>Draws one selected glass layout in a synthetic room while denying reads of its original cartridge payload.</summary>
    private static void VerifyMotherBrainGlassNativeDrawPath(
        byte[] bank84,
        RoomPlmShotBlockDrawDefinitions.DrawList[] lists,
        RoomPlmShotBlockDrawDefinitions.DrawList selected,
        RoomPlmMotherBrainGlassVisualCatalog? visuals)
    {
        const int width = 32;
        const int height = 16;
        const int originX = 9;
        const int originY = 5;
        TestAddressSpace bus = CreateBank84Cartridge(bank84);
        SeedRoomPlmPopulation(bus, 0x9000,
        [
            unchecked((byte)PlmNativeWords.Word(PlmHeaderId.MotherBrainGlass)),
            SnesLittleEndian.HighByte(PlmNativeWords.Word(PlmHeaderId.MotherBrainGlass)),
            originX, originY, 0x00, 0x80, 0x00, 0x00,
        ]);
        // The production control list is compiled and immutable. Draw each layout through the
        // production timed-frame draw instead of mutating a cartridge operand execution no longer reads.
        var guarded = NewMotherBrainGlassDrawReadGuard(bus, lists);
        BackgroundTile[] blockDefinitions = new BackgroundTile[0x400 * 4];
        blockDefinitions[0x57 * 4] = BackgroundTileWordBytes.Tile(0x57);
        var level = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(new ushort[width * height], new byte[width * height], new ushort[width * height]), blockDefinitions);
        var plms = PlmFixtures.Unbound(new PlmShotBlockVisuals { RoomPlmMotherBrainGlassVisuals = visuals });
        AssertEqual(1, PlmFixtures.LoadPopulation(ref plms, new PlmRoom(level, BgArtworkFixtures.For(new SnesVram()), new SharedGameState(), AreaId.Tourian), RoomPlmPopulationImporter.Read(guarded, 0x9000), new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState()), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false, IsBossDefeated = () => false, IsMiniBossDefeated = () => false }, Events = new FixtureEventFlags { HasEvent = _ => false, SetEvent = _ => { } } }),
            $"Mother Brain glass loads for draw ${PlmDrawAddressCodec.Of(selected.Draw):X4}");
        AssertTrue(plms.MotherBrainGlassWasLoaded,
            $"glass header retains its PLM owner for draw ${PlmDrawAddressCodec.Of(selected.Draw):X4}");
        plms.DrawSolePlmFrameForVerification(level, NativePlmDraws.At<MotherBrainGlassDraw>(PlmDrawAddressCodec.Of(selected.Draw)));

        int entryX = originX;
        int entryY = originY;
        var expected = new Dictionary<int, ushort>();
        foreach (RoomPlmShotBlockDrawDefinitions.Run run in selected.Runs.Span)
        {
            bool vertical = (run.DirectionAndCount & 0x8000) != 0;
            for (int block = 0; block < run.LevelWords.Length; block++)
            {
                int x = entryX + (vertical ? 0 : block);
                int y = entryY + (vertical ? block : 0);
                expected.Add(y * width + x, run.LevelWords.Span[block]);
            }
            entryX = originX + run.Next.X;
            entryY = originY + run.Next.Y;
        }
        foreach ((int index, ushort word) in expected)
            AssertEqual(word, level.GetCollisionBlockByIndex(index).PackedWord.Raw,
                $"glass draw ${PlmDrawAddressCodec.Of(selected.Draw):X4} writes native physical block {index}");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            $"glass draw ${PlmDrawAddressCodec.Of(selected.Draw):X4} avoids source payload reads");
        if (visuals is not null)
        {
            const int editedBlock = 7 * width + 6;
            AssertEqual((ushort)0x0057,
                level.DisplayedBlockTiles(editedBlock).TopLeft.PackedWord,
                "edited glass tile survives later camera streaming");
            AssertEqual((ushort)0x0ecf,
                level.GetCollisionBlockByIndex(editedBlock).PackedWord.Raw,
                "edited glass appearance does not change the physical shatter block");
        }
    }

    /// <summary>Checks extracted stock validation, override persistence, and rejection of malformed or tampered visual files.</summary>
    private static void VerifyMotherBrainGlassVisualInstallation(
        SuperMetroidAddressSpace rom)
    {
        using var testRootScratch = new TestScratchDirectory("mother-brain-glass-visual");
        string testRoot = testRootScratch.Root;
        try
        {
            var installation = new GameInstallation(testRoot);
            RoomPlmMotherBrainGlassVisualFiles.Extract(rom,
                installation.RoomPlmMotherBrainGlassVisualDirectory,
                SupportedCartridge.Sha256);
            RoomPlmMotherBrainGlassVisualFiles.ValidateStock(
                installation.RoomPlmMotherBrainGlassVisualDirectory);
            VerifyMotherBrainGlassStockMapping(rom, installation.LoadRoomPlmMotherBrainGlassVisuals());
            string stockPath = Path.Combine(
                installation.RoomPlmMotherBrainGlassVisualDirectory,
                RoomPlmMotherBrainGlassVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted glass JSON is empty.");
            JsonNode frame = document.Required("entries").AsArray().RequiredElements().Single(entry =>
                entry.Required("id").GetValue<string>() == "shatter-1");
            frame.Required("blocks")[6] = 0x0057;
            Directory.CreateDirectory(
                installation.RoomPlmMotherBrainGlassVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmMotherBrainGlassVisualOverrideDirectory,
                RoomPlmMotherBrainGlassVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertEqual((ushort)0x0057,
                installation.LoadRoomPlmMotherBrainGlassVisuals()
                    .GetWord(NativePlmDraws.At<MotherBrainGlassDraw>(0x978f), 1, 2),
                "installed glass override changes the multi-run shatter tile");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmMotherBrainGlassVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0057,
                RoomPlmMotherBrainGlassVisualFiles.Load(refreshed,
                    installation.RoomPlmMotherBrainGlassVisualOverrideDirectory)
                    .GetWord(NativePlmDraws.At<MotherBrainGlassDraw>(0x978f), 1, 2),
                "glass override survives stock replacement");

            frame.Required("blocks")[6] = 0xf057;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmMotherBrainGlassVisuals(),
                "invalid glass override fails loudly");
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmMotherBrainGlassVisuals(),
                "tampered glass stock fails manifest validation");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
    }

    /// <summary>Blocks cartridge reads across every compiled glass draw list's run headers and tile words.</summary>
    private static CartridgeReadGuard NewMotherBrainGlassDrawReadGuard(
        ISnesAddressSpace source, RoomPlmShotBlockDrawDefinitions.DrawList[] lists) =>
        new CartridgeReadGuard(source).Forbid(address => lists.Any(list =>
            {
                int first = 0x840000 | PlmDrawAddressCodec.Of(list.Draw);
                int length = list.Runs.Span.Sum(run =>
                    4 + run.LevelWords.Length * 2);
                return address >= first && address < first + length;
            }),
            address => $"Mother Brain glass reread draw payload ${address:X6}.");
}
