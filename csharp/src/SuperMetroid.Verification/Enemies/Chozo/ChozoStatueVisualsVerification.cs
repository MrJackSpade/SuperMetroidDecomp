using SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Core;
using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using static SuperMetroid.Verification.Enemies.Chozo.LookupChozoStockVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.World;
using SuperMetroid.Verification.Harness;
namespace SuperMetroid.Verification.Enemies.Chozo;

/// <summary>Checks extracted Chozo statue draw data, editable visual overrides, and compiled PLM execution.</summary>
internal static class ChozoStatueVisualsVerification
{
    /// <summary>Verifies stock words against the cartridge, then exercises edited slope art through the room tilemap.</summary>
    internal static void VerifyChozoStatueVisualInstallation(
        SuperMetroidAddressSpace rom)
    {
        using var testRootScratch = new TestScratchDirectory("chozo-statue-visual");
        string testRoot = testRootScratch.Root;
        try
        {
            var installation = new GameInstallation(testRoot);
            // Extraction checks every physical word, run direction and offset
            // against the pinned cartridge before writing editable art.
            RoomPlmChozoStatueVisualFiles.Extract(rom,
                installation.RoomPlmChozoStatueVisualDirectory,
                SupportedCartridge.Sha256);
            RoomPlmChozoStatueVisualFiles.ValidateStock(
                installation.RoomPlmChozoStatueVisualDirectory);
            VerifyChozoStatueStockMapping(rom, installation.LoadRoomPlmChozoStatueVisuals());

            string stockPath = Path.Combine(
                installation.RoomPlmChozoStatueVisualDirectory,
                RoomPlmChozoStatueVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted Chozo JSON is empty.");
            JsonNode clearSlope = document.Required("entries").AsArray().RequiredElements().Single(entry =>
                entry.Required("id").GetValue<string>() ==
                "wrecked-ship-clear-slope-access");
            clearSlope.Required("blocks")[0] = InstalledAppearanceJson.Cell(0x53);
            JsonNode blockSlope = document.Required("entries").AsArray().RequiredElements().Single(entry =>
                entry.Required("id").GetValue<string>() ==
                "wrecked-ship-block-slope-access");
            blockSlope.Required("blocks")[0] = InstalledAppearanceJson.Cell(0x54);
            Directory.CreateDirectory(
                installation.RoomPlmChozoStatueVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmChozoStatueVisualOverrideDirectory,
                RoomPlmChozoStatueVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            var selected = installation.LoadRoomPlmChozoStatueVisuals();
            AssertEqual(RoomBlockAppearance.Of(0x53), selected.GetAppearance(
                PlmDraw.Of(ChozoStatueDraw.ClearSlopeAccess), 0, 0),
                "Chozo art override selects its edited first block");

            // Run both real Chozo PLM instruction lists. The bus refuses reads
            // of compiled draw bytes, so the production dispatcher must reach
            // each extracted override without falling back to ROM data.
            const int width = 32;
            var guarded = NewChozoProgramAndDrawReadGuard(rom);
            foreach ((PlmHeaderId header, RoomTerrainBlock physical, RoomBlockAppearance visual) in new[]
            {
                new VerifyChozoStatueVisualInstallationCase(PlmHeaderId.ClearSlopeAccessForWreckedShipChozo, RoomTerrainBlock.Of(0x12b, BlockMirroring.None, RoomCollisionType.Air), RoomBlockAppearance.Of(0x53)),
                new VerifyChozoStatueVisualInstallationCase(PlmHeaderId.BlockSlopeAccessForWreckedShipChozo, RoomTerrainBlock.Of(0x12b, BlockMirroring.None, RoomCollisionType.SpikeBlock), RoomBlockAppearance.Of(0x54)),
            })
            {
                var level = CreateEmptyRoom(new(width, 16));
                var plms = PlmFixtures.Unbound(new PlmShotBlockVisuals { RoomPlmChozoStatueVisuals = selected });
                AssertTrue(plms.ChozoStatue.TrySpawnChozoStatuePlm(level,
                    new ChozoStatuePlmRequest(header, new Position16<RoomBlocks>(3, 4), IsHardcoded: false)),
                    $"Chozo actor {header} spawns in a bounded test room");
                plms.Step(level);
                int origin = 4 * width + 3;
                AssertEqual(physical,
                    level.GetCollisionBlockByIndex(origin).Terrain,
                    $"edited Chozo art retains native physical block for {header}");
                AssertEqual(visual,
                    level.DisplayedAppearance(origin),
                    $"edited Chozo art reaches streamed tilemap for {header}");
            }
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                "Chozo slope PLMs did not reread compiled program or draw bytes");

            var crumbleLevel = CreateEmptyRoom(new(width, 16));
            var crumblePlms = PlmFixtures.Unbound();
            AssertTrue(crumblePlms.ChozoStatue.TrySpawnChozoStatuePlm(crumbleLevel,
                new ChozoStatuePlmRequest(PlmHeaderId.CrumbleLowerNorfairChozoRoomPlug,
                    new Position16<RoomBlocks>(3, 4), IsHardcoded: false)),
                "Chozo crumble plug spawns in a bounded test room");
            var seenCrumbleBlocks = new HashSet<RoomTerrainBlock>();
            for (int frame = 0; frame < 15; frame++)
            {
                crumblePlms.Step(crumbleLevel);
                seenCrumbleBlocks.Add(crumbleLevel.GetCollisionBlock(new(3, 4)).Terrain);
            }
            foreach (int block in new[] { 0x53, 0x54, 0x55, 0xff })
                AssertTrue(seenCrumbleBlocks.Contains(RoomTerrainBlock.Of(block, BlockMirroring.None, RoomCollisionType.Air)),
                    $"Chozo crumble program draws air block ${block:X3}");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                "Chozo crumble PLM did not reread compiled program or draw bytes");

            var wreckedHandLevel = CreateEmptyRoom(new(width, 16));
            var wreckedHandPlms = PlmFixtures.Unbound();
            AssertTrue(wreckedHandPlms.ChozoStatue.TrySpawnChozoStatuePlm(wreckedHandLevel,
                new ChozoStatuePlmRequest(PlmHeaderId.WreckedShipChozoHand,
                    new Position16<RoomBlocks>(3, 4), IsHardcoded: false)),
                "Wrecked Ship hand spawns from its compiled header");
            wreckedHandPlms.Step(wreckedHandLevel);
            AssertTrue(wreckedHandPlms.PopulationSlots.Count == 0,
                "Wrecked Ship hand executes the shared compiled delete list");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                "shared delete list did not reread its bank-$84 bytes");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmChozoStatueVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual(RoomBlockAppearance.Of(0x53),
                RoomPlmChozoStatueVisualFiles.Load(refreshed,
                    installation.RoomPlmChozoStatueVisualOverrideDirectory)
                    .GetAppearance(PlmDraw.Of(ChozoStatueDraw.ClearSlopeAccess), 0, 0),
                "Chozo override survives stock replacement");

            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomPlmChozoStatueVisualFiles.ValidateStock(
                    installation.RoomPlmChozoStatueVisualDirectory),
                "tampered Chozo stock fails manifest validation");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
    }

    /// <summary>Rejects cartridge reads from the compiled Chozo instruction, delete, and draw ranges.</summary>
    internal static CartridgeReadGuard NewChozoProgramAndDrawReadGuard(ISnesAddressSpace source) =>
        new MutableCartridgeReadGuard(source)
            .Forbid(address =>
                    (address >= (0x840000 | ChozoStatuePlmProgramDefinitions.CrumblePlugStart) &&
                     address <= (0x840000 | ChozoStatuePlmProgramDefinitions.CrumblePlugEnd)) ||
                    (address >= (0x840000 | ChozoStatuePlmProgramDefinitions.LowerNorfairHandStart) &&
                     address <= (0x840000 | ChozoStatuePlmProgramDefinitions.LowerNorfairHandEnd)) ||
                    (address >= (0x840000 | ChozoStatuePlmProgramDefinitions.ClearSlopeStart) &&
                     address <= (0x840000 | ChozoStatuePlmProgramDefinitions.ClearSlopeEnd)) ||
                    (address >= (0x840000 | ChozoStatuePlmProgramDefinitions.BlockSlopeStart) &&
                     address <= (0x840000 | ChozoStatuePlmProgramDefinitions.BlockSlopeEnd)) ||
                    (address >= (0x840000 | PlmProgramStartOracle.SharedDeleteEntry) &&
                     address <= (0x840000 | (PlmProgramStartOracle.SharedDeleteEntry + 1))) ||
                    ChozoStatueDraws.All.Any(draw =>
                    {
                        int first = 0x840000 | PlmDrawAddressCodec.Of(draw.Draw);
                        int length = draw.Runs.Span.Sum(run =>
                            4 + run.Blocks.Length * 2);
                        return address >= first && address < first + length;
                    }),
                address => $"Chozo PLM reread compiled program or draw byte ${address:X6}.");
}
