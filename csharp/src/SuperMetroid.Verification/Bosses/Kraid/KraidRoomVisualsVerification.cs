using SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using static SuperMetroid.Verification.Bosses.Kraid.KraidRoomPlmsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidRoomVisualSelectionVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Areas.Maridia;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.Kraid;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Kraid ceiling and spike PLM visuals, edited block streaming, and isolation from the Maridia elevatube catalog.</summary>
internal static class KraidRoomVisualsVerification
{
    /// <summary>Extracts ten retail Kraid draw lists, edits three visual cells, and checks live redraws preserve physical words and stock integrity.</summary>
    internal static void VerifyKraidRoomVisuals()
    {
        Suite(nameof(VerifyKraidRoomVisualSelection), () => VerifyKraidRoomVisualSelection());
        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        using var testRootScratch = new TestScratchDirectory("kraid-room-visual");
        string testRoot = testRootScratch.Root;
        try
        {
            var installation = new GameInstallation(testRoot);
            RoomPlmKraidVisualFiles.Extract(rom,
                installation.RoomPlmKraidVisualDirectory,
                SupportedCartridge.Sha256);
            RoomPlmKraidVisualFiles.ValidateStock(
                installation.RoomPlmKraidVisualDirectory);
            InstalledRoomPlmKraidVisuals stock =
                installation.LoadRoomPlmKraidVisuals();
            foreach (RoomPlmShotBlockDrawDefinitions.DrawList draw in
                     KraidRoomPlmDraws.All)
            {
                for (int block = 0; block < draw.Runs.Span[0].Count;
                     block++)
                    AssertEqual(draw.Runs.Span[0].Blocks.Span[block].Appearance,
                        stock.GetAppearance(draw.Draw, 0, block),
                        $"Kraid stock draw ${PlmDrawAddressCodec.Of(draw.Draw):X4}, block {block}");
            }
            AssertThrows<InvalidDataException>(
                () => new InstalledRoomPlmKraidVisuals([]),
                "Kraid visual catalog rejects missing frames");

            string stockPath = Path.Combine(
                installation.RoomPlmKraidVisualDirectory,
                RoomPlmKraidVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted Kraid room JSON is empty.");
            JsonNode first = Entry("crumble-first");
            JsonNode ceiling = Entry("clear-ceiling");
            JsonNode spikes = Entry("clear-spikes");
            first.Required("blocks")[0] = PlmVisualJson.Appearance(0x058);
            ceiling.Required("blocks")[14] = PlmVisualJson.Appearance(0x059);
            spikes.Required("blocks")[21] = PlmVisualJson.Appearance(0x05a);
            Directory.CreateDirectory(
                installation.RoomPlmKraidVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmKraidVisualOverrideDirectory,
                RoomPlmKraidVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            InstalledRoomPlmKraidVisuals edited =
                installation.LoadRoomPlmKraidVisuals();
            AssertEqual(RoomBlockAppearance.Of(0x058),
                edited.GetAppearance(PlmDraw.Of(KraidRoomDraw.CrumbleFirst), 0, 0),
                "Kraid override edits first crumble frame");
            AssertEqual(RoomBlockAppearance.Of(0x059),
                edited.GetAppearance(PlmDraw.Of(KraidRoomDraw.ClearCeiling), 0, 14),
                "Kraid override edits far-right ceiling clear block");
            AssertEqual(RoomBlockAppearance.Of(0x05a),
                edited.GetAppearance(PlmDraw.Of(KraidRoomDraw.ClearSpikes), 0, 21),
                "Kraid override edits far-right spike clear block");
            VerifyKraidVisualOwnerIsolation(edited);

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmKraidVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual(RoomBlockAppearance.Of(0x05a),
                RoomPlmKraidVisualFiles.Load(refreshed,
                    installation.RoomPlmKraidVisualOverrideDirectory)
                    .GetAppearance(PlmDraw.Of(KraidRoomDraw.ClearSpikes), 0, 21),
                "Kraid override survives stock replacement");

            first.Required("blocks")[0] = 0xf058;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmKraidVisuals(),
                "Kraid override cannot change physical collision bits");
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomPlmKraidVisualFiles.ValidateStock(
                    installation.RoomPlmKraidVisualDirectory),
                "tampered Kraid stock fails manifest validation");

            JsonNode Entry(string id) => document.Required("entries").AsArray().RequiredElements()
                .Single(entry => entry.Required("id").GetValue<string>() == id);
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
        Console.WriteLine(
            "Kraid room visuals: ten native draws, live ceiling/spike edits, elevatube owner isolation, physical/timing separation, stock repair and strict failures pass.");
    }

    /// <summary>Checks Kraid redraws use their selected art while a simultaneous elevatube retains its own visual catalog.</summary>
    private static void VerifyKraidVisualOwnerIsolation(
        InstalledRoomPlmKraidVisuals edited)
    {
        var tube = new InstalledRoomPlmMaridiaElevatubeVisuals(
            [new RoomPlmVisualEntry(
                MaridiaElevatubePlmDefinitions.VisualId, [RoomBlockAppearance.Of(0x05b)])]);
        Suite(nameof(VerifyKraidDraw), () => VerifyKraidDraw(PlmHeaderId.CrumbleKraidCeilingIntoBackground1, PlmDrawAddressCodec.Of(PlmDraw.Of(KraidRoomDraw.CrumbleFirst)), 0, RoomTerrainBlock.Of(0x180, BlockMirroring.None, RoomCollisionType.SolidBlock), RoomBlockAppearance.Of(0x058), new KraidRoomVisualCatalogs { Kraid = edited, Tube = tube }));
        Suite(nameof(VerifyKraidDraw), () => VerifyKraidDraw(PlmHeaderId.ClearKraidCeiling, PlmDrawAddressCodec.Of(PlmDraw.Of(KraidRoomDraw.ClearCeiling)), 14, RoomTerrainBlock.Of(0x130, BlockMirroring.None, RoomCollisionType.Air), RoomBlockAppearance.Of(0x059), new KraidRoomVisualCatalogs { Kraid = edited, Tube = tube }));
        Suite(nameof(VerifyKraidDraw), () => VerifyKraidDraw(PlmHeaderId.ClearKraidSpikes, PlmDrawAddressCodec.Of(PlmDraw.Of(KraidRoomDraw.ClearSpikes)), 21, RoomTerrainBlock.Of(0x110, BlockMirroring.None, RoomCollisionType.Air), RoomBlockAppearance.Of(0x05a), new KraidRoomVisualCatalogs { Kraid = edited, Tube = tube }));
        Suite(nameof(VerifyKraidDraw), () => VerifyKraidDraw(PlmHeaderId.CrumbleKraidCeilingIntoBackground1, PlmDrawAddressCodec.Of(PlmDraw.Of(KraidRoomDraw.CrumbleFirst)), 0, RoomTerrainBlock.Of(0x180, BlockMirroring.None, RoomCollisionType.SolidBlock), RoomBlockAppearance.Of(0x180), new KraidRoomVisualCatalogs { Kraid = null, Tube = tube }));

        RoomLevelData tubeLevel = CreateRoom(new(4, 4), new RoomTerrainBlock[16], new RoomBlockBehavior[16]);
        var tubePlms = PlmFixtures.Unbound(new PlmShotBlockVisuals { RoomPlmKraidVisuals = edited, RoomPlmMaridiaElevatubeVisuals = tube });
        AssertTrue(tubePlms.WreckedShipTreadmill.TrySpawnMaridiaElevatube(tubeLevel),
            "elevatube allocates alongside Kraid appearance resource");
        var tubeGuard = NewKraidRoomSourceGuard(new TestAddressSpace());
        tubePlms.Step(tubeLevel);
        int tubeBlock = tubeLevel.GetBlockIndex(MaridiaElevatubeDefinitions.PlmBlock);
        AssertEqual(RoomTerrainBlock.Of(0x180, BlockMirroring.None, RoomCollisionType.SolidBlock),
            tubeLevel.GetCollisionBlockByIndex(tubeBlock).Terrain,
            "elevatube retains shared native physical word");
        // A PLM redraw identifies its block by the BG1 ring destination of that block.
        AssertEqual(RoomBlockAppearance.Of(0x05b), tubeLevel.DisplayedAppearance(tubeBlock),
            "elevatube retains its own edited appearance, not Kraid's first frame");
        AssertEqual(0, tubeGuard.ForbiddenReadAttempts,
            "elevatube does not read migrated Kraid source data");
    }

    /// <summary>Spawns a Kraid mutation and checks its collision word and streamed tile after an owner-specific art edit.</summary>
    private static void VerifyKraidDraw(PlmHeaderId header, ushort drawPointer, int blockOffset, RoomTerrainBlock physicalWord, RoomBlockAppearance visualWord, KraidRoomVisualCatalogs catalogs)
    {
        const int width = 32;
        var words = new RoomTerrainBlock[width * 16];
        for (int x = 5; x <= 26; x++)
            words[5 * width + x] = RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.SolidBlock);
        RoomLevelData level = CreateRoom(new(width, 16), words,
            new RoomBlockBehavior[words.Length]);
        var plms = PlmFixtures.Unbound(new PlmShotBlockVisuals { RoomPlmKraidVisuals = catalogs.Kraid, RoomPlmMaridiaElevatubeVisuals = catalogs.Tube });
        AssertTrue(plms.BossRoomMutations.TrySpawnKraidRoomMutation(level, new(5, 5), header),
            $"Kraid visual owner {header} allocates");
        var guard = NewKraidRoomSourceGuard(new TestAddressSpace());
        plms.Step(level);
        int blockIndex = 5 * width + 5 + blockOffset;
        AssertEqual(physicalWord,
            level.GetCollisionBlockByIndex(blockIndex).Terrain,
            $"Kraid draw ${drawPointer:X4} retains physical block {blockOffset}");
        AssertEqual(visualWord,
            level.DisplayedAppearance(blockIndex),
            $"Kraid draw ${drawPointer:X4} edit survives streaming");
        AssertEqual(visualWord, level.DisplayedAppearance(blockIndex),
            $"Kraid draw ${drawPointer:X4} uses owner-specific visible block {blockOffset}");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            $"Kraid draw ${drawPointer:X4} reads no migrated source bytes");
    }

    /// <summary>Runs the Kraid room visual installation and ownership verification command.</summary>
    [VerificationCommand("--kraid-room-visuals")]
    internal static int KraidRoomVisualsCommand()
    {
        Suite(nameof(VerifyKraidRoomVisuals), () => VerifyKraidRoomVisuals());
        return 0;
    }
}
