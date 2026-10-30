using SuperMetroid.AssetExtraction;
using SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Verification.Areas.Escape;
using SuperMetroid.Core;
using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using static SuperMetroid.Verification.Bosses.MotherBrain.BabyMetroidRouteDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.Plms.ShotBlockPlmProgramsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.World;
namespace SuperMetroid.Verification.Rooms.LevelData;

/// <summary>Checks bomb and crumble block restore artwork against native draw lists and production PLM behavior.</summary>
internal static class LinkedRestoreVisualsVerification
{
    /// <summary>The bomb-block or contact-crumble restoration draw the cartridge stores at <paramref name="pointer"/>.</summary>
    private static PlmDraw LinkedAt(int pointer) => NativePlmDraws.IsDrawAt<BombBlockRestoreDraw>(pointer)
        ? NativePlmDraws.At<BombBlockRestoreDraw>(pointer) : NativePlmDraws.At<ContactCrumbleRestoreDraw>(pointer);

    /// <summary>Compares six flattened stock layouts with bank-$84 draw words and checks catalog bounds and edit isolation.</summary>
    private static void VerifyLinkedRestoreStockMapping(SuperMetroidAddressSpace rom)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0xa4c7,"bomb-horizontal"), new LookupEscapeGateStockFrame(0xa4cf,"bomb-vertical"),
            new LookupEscapeGateStockFrame(0xa4d7,"bomb-square"), new LookupEscapeGateStockFrame(0xa4a1,"crumble-horizontal"), new LookupEscapeGateStockFrame(0xa4a9,"crumble-vertical"), new LookupEscapeGateStockFrame(0xa4b1,"crumble-square")];
        var native = new Dictionary<ushort, RoomBlockAppearance[]>();
        foreach (var frame in frames)
        {
            var words = new List<ushort>();
            int cursor = frame.Pointer;
            while (true)
            {
                int count = ReadRomWord(rom, 0x840000 | cursor) & 0x7fff;
                for (int block = 0; block < count; block++)
                    words.Add(RoomLevelWordCodec.DecodeAppearance(ReadRomWord(rom, 0x840000 | (cursor + 2 + block * 2))));
                cursor += 2 + count * 2;
                ushort offset = ReadRomWord(rom, 0x840000 | cursor);
                cursor += 2;
                if (offset == 0) break;
            }
            native.Add(frame.Pointer, words.ToArray());
        }
        var entries = frames.Select(frame => new RoomPlmVisualEntry(frame.Id, native[frame.Pointer].ToArray())).ToArray();
        var stock = InstalledRoomPlmLinkedRestoreVisuals.Stock();
        var imported = new InstalledRoomPlmLinkedRestoreVisuals(entries);
        string expectedHash = SuperMetroid.Core.Assets.SelectedPresentationHash.FromAppearanceFrames(nameof(InstalledRoomPlmLinkedRestoreVisuals), native.ToDictionary(pair => LinkedRestorePlmDraws.VisualId(LinkedAt(pair.Key)), pair => pair.Value));
        AssertEqual(expectedHash, stock.ContentIdentity, "linked restore calculated stock preserves original flattened hash");
        AssertEqual(expectedHash, imported.ContentIdentity, "linked restore imported stock hash");
        foreach (var frame in frames)
        {
            for (int word = 0; word < native[frame.Pointer].Length; word++)
            {
                AssertEqual(native[frame.Pointer][word], stock.GetAppearance(LinkedAt(frame.Pointer), word / 2, word % 2), "linked restore native stock word");
                AssertEqual(native[frame.Pointer][word], imported.GetAppearance(LinkedAt(frame.Pointer), word / 2, word % 2), "linked restore imported native word");
            }
            foreach (int bad in new[] {int.MinValue,-1,native[frame.Pointer].Length / 2,int.MaxValue})
                AssertThrows<ArgumentOutOfRangeException>(() => stock.GetAppearance(LinkedAt(frame.Pointer),bad,0), "linked restore stock run bounds");
            foreach (int bad in new[] {int.MinValue,-1,2,int.MaxValue})
                AssertThrows<ArgumentOutOfRangeException>(() => stock.GetAppearance(LinkedAt(frame.Pointer),0,bad), "linked restore stock block bounds");
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (!native.ContainsKey((ushort)pointer))
                AssertThrows<InvalidDataException>(() => stock.GetAppearance(LinkedAt(pointer),0,0), "linked restore rejects unknown frame");
        entries[2].Blocks[3] = RoomBlockAppearance.Of(0x053);
        entries[5].Blocks[0] = RoomBlockAppearance.Of(0x054);
        var mixed = new InstalledRoomPlmLinkedRestoreVisuals(entries);
        var expected = frames.ToDictionary(frame => frame.Pointer, frame => entries.Single(entry => entry.Id == frame.Id).Blocks.ToArray());
        AssertEqual(SuperMetroid.Core.Assets.SelectedPresentationHash.FromAppearanceFrames(nameof(InstalledRoomPlmLinkedRestoreVisuals), expected.ToDictionary(pair => LinkedRestorePlmDraws.VisualId(LinkedAt(pair.Key)), pair => pair.Value)),
            mixed.ContentIdentity, "linked restore mixed custom/stock hash");
        entries[2].Blocks[3] = RoomBlockAppearance.Of(0x055);
        foreach (var frame in frames)
        for (int word = 0; word < expected[frame.Pointer].Length; word++)
            AssertEqual(expected[frame.Pointer][word], mixed.GetAppearance(LinkedAt(frame.Pointer),word / 2,word % 2), "linked restore custom clone isolation and stock fallback");
        AssertThrows<InvalidDataException>(() => new InstalledRoomPlmLinkedRestoreVisuals(entries[..5]), "linked restore rejects incomplete stock");
        AssertThrows<InvalidDataException>(() => new InstalledRoomPlmLinkedRestoreVisuals(entries.Append(entries[0])), "linked restore rejects duplicate stock");
    }

    /// <summary>Checks stock extraction, editable override persistence, and rejection of malformed restore artwork.</summary>
    internal static void VerifyLinkedRestoreVisualInstallation(
        SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyLinkedRestoreStockMapping), () => VerifyLinkedRestoreStockMapping(rom));
        using var testRootScratch = new TestScratchDirectory("linked-restore-visual");
        string testRoot = testRootScratch.Root;
        try
        {
            var installation = new GameInstallation(testRoot);
            // Extraction checks all six compiled row shapes, full physical words,
            // and signed offsets against this pinned retail cartridge.
            RoomPlmLinkedRestoreVisualFiles.Extract(rom,
                installation.RoomPlmLinkedRestoreVisualDirectory,
                SupportedCartridge.Sha256);
            RoomPlmLinkedRestoreVisualFiles.ValidateStock(
                installation.RoomPlmLinkedRestoreVisualDirectory);
            AssertEqual(RoomBlockAppearance.Of(0x058),
                installation.LoadRoomPlmLinkedRestoreVisuals().GetAppearance(
                    PlmDraw.Of(BombBlockRestoreDraw.Horizontal), 0, 0),
                "stock bomb restore selects cartridge block 058");
            AssertEqual(RoomBlockAppearance.Of(0x0bc),
                installation.LoadRoomPlmLinkedRestoreVisuals().GetAppearance(
                    PlmDraw.Of(ContactCrumbleRestoreDraw.Horizontal), 0, 0),
                "stock crumble restore selects cartridge block 0BC");
            AssertThrows<InvalidDataException>(
                () => new InstalledRoomPlmLinkedRestoreVisuals(
                    [new RoomPlmVisualEntry("bomb-horizontal",
                        new RoomBlockAppearance[2])]),
                "restore catalog rejects missing linked layouts");

            string stockPath = Path.Combine(
                installation.RoomPlmLinkedRestoreVisualDirectory,
                RoomPlmLinkedRestoreVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted restore JSON is empty.");
            JsonNode bomb = document.Required("entries").AsArray().RequiredElements().Single(entry =>
                entry.Required("id").GetValue<string>() == "bomb-horizontal");
            bomb.Required("blocks")[0] = PlmVisualJson.Appearance(0x053);
            Directory.CreateDirectory(
                installation.RoomPlmLinkedRestoreVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmLinkedRestoreVisualOverrideDirectory,
                RoomPlmLinkedRestoreVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertEqual(RoomBlockAppearance.Of(0x053),
                installation.LoadRoomPlmLinkedRestoreVisuals().GetAppearance(
                    PlmDraw.Of(BombBlockRestoreDraw.Horizontal), 0, 0),
                "installed restore override selects edited block");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmLinkedRestoreVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual(RoomBlockAppearance.Of(0x053),
                RoomPlmLinkedRestoreVisualFiles.Load(refreshed,
                    installation.RoomPlmLinkedRestoreVisualOverrideDirectory)
                    .GetAppearance(PlmDraw.Of(BombBlockRestoreDraw.Horizontal), 0, 0),
                "restore override survives stock replacement");
            bomb.Required("blocks")[0] = 0xf053;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmLinkedRestoreVisuals(),
                "restore override rejects physical collision bits");
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomPlmLinkedRestoreVisualFiles.ValidateStock(
                    installation.RoomPlmLinkedRestoreVisualDirectory),
                "tampered restore stock fails manifest validation");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
    }

    /// <summary>Runs a real linked bomb or crumble PLM with edited appearance, ensuring its physical parent word stays native.</summary>
    internal static void VerifyLinkedRestoreVisualSeparation(
        SuperMetroidAddressSpace rom, HashSet<int> forbidden, bool bomb)
    {
        RoomPlmVisualEntry[] entries =
            RoomPlmLinkedRestoreDrawDefinitions.All.Select(draw =>
                new RoomPlmVisualEntry(
                    LinkedRestorePlmDraws.VisualId(draw.Draw),
                    draw.Appearances()))
                .ToArray();
        string editedId = bomb ? "bomb-horizontal" : "crumble-horizontal";
        RoomBlockAppearance editedWord = RoomBlockAppearance.Of(bomb ? 0x053 : 0x054);
        entries.Single(entry => entry.Id == editedId).Blocks[0] = editedWord;
        var visuals = new InstalledRoomPlmLinkedRestoreVisuals(entries);

        const int width = 8;
        const int blockIndex = 27;
        var words = new RoomTerrainBlock[width * width];
        words[blockIndex] = bomb ? RoomTerrainBlock.Of(0x321, BlockMirroring.None, RoomCollisionType.BombableBlock) : RoomTerrainBlock.Of(0x321, BlockMirroring.None, RoomCollisionType.SpecialBlock);
        var level = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(words, new RoomBlockBehavior[words.Length], new RoomBlockAppearance[words.Length]));
        var plms = PlmFixtures.Unbound(new PlmShotBlockVisuals { RoomPlmLinkedRestoreVisuals = visuals });
        bool spawned = bomb
            ? plms.TrySpawnCollisionBombBlock(level, blockIndex, 1)
            : plms.BlockReactions.TrySpawnSamusContactCrumbleBlock(level, blockIndex,
                new RoomBlockBehavior(1));
        AssertTrue(spawned, $"{editedId} installs its real linked PLM");

        var guarded = NewShotBlockProgramReadGuard(rom, forbidden);
        bool sawImmediate = false;
        for (int frame = 0; frame < (bomb ? 420 : 100); frame++)
        {
            plms.Step(level);
            sawImmediate |= level.DisplayedAppearance(blockIndex) == editedWord;
        }
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            $"{editedId} does not reread compiled program or draw bytes");
        AssertTrue(sawImmediate,
            $"{editedId} reaches an immediate tilemap update");
        AssertEqual(bomb ? RoomTerrainBlock.Of(0x058, BlockMirroring.None, RoomCollisionType.BombableBlock) : RoomTerrainBlock.Of(0x0bc, BlockMirroring.None, RoomCollisionType.SpecialBlock),
            level.GetCollisionBlockByIndex(blockIndex).Terrain,
            $"{editedId} preserves its native physical parent word");
        AssertEqual(editedWord,
            level.DisplayedAppearance(blockIndex),
            $"{editedId} survives later room streaming");
    }
}
