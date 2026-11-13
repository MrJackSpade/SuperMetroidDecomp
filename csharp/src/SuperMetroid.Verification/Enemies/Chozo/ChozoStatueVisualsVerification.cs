using SuperMetroid.Verification.Rooms.RoomObjects;
using SuperMetroid.Core;
using static SuperMetroid.Verification.Enemies.Chozo.LookupChozoStockVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
namespace SuperMetroid.Verification.Enemies.Chozo;

/// <summary>Checks extracted Chozo statue draw data and compiled PLM execution with the installed art.</summary>
internal static class ChozoStatueVisualsVerification
{
    /// <summary>
    /// Verifies the installed stock words against the cartridge, runs the compiled Chozo PLMs with the installed art and
    /// checks that a malformed installed visual file fails loudly where it is read.
    /// </summary>
    internal static void VerifyChozoStatueVisualInstallation(
        SuperMetroidAddressSpace rom)
    {
        // A private installation copy, whose stock file is tampered below.
        using (PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomObjectChozoStatueVisualDirectory))
        {
            GameInstallation installation = copy.Installation;
            RoomObjectChozoStatueVisualFilesConstants.ValidateStock(
                installation.RoomObjectChozoStatueVisualDirectory);
            InstalledRoomObjectChozoStatueVisuals installed = installation.LoadRoomObjectChozoStatueVisuals();
            VerifyChozoStatueStockMapping(rom, installed);

            // Run both real Chozo PLM instruction lists. The bus refuses reads of compiled draw bytes, so the
            // production dispatcher must reach the installed art without falling back to ROM data.
            const int width = 32;
            var guarded = NewChozoProgramAndDrawReadGuard(rom);
            foreach ((ChozoStatueRoomObject header, RoomTerrainBlock physical) in new[]
            {
                new VerifyChozoStatueVisualInstallationCase(ChozoStatueRoomObject.SlopeAccessOpening, RoomTerrainBlock.Of(0x12b, BlockMirroring.None, RoomCollisionType.Air)),
                new VerifyChozoStatueVisualInstallationCase(ChozoStatueRoomObject.SlopeAccessClosing, RoomTerrainBlock.Of(0x12b, BlockMirroring.None, RoomCollisionType.SpikeBlock)),
            })
            {
                var level = CreateEmptyRoom(new(width, 16));
                var roomObjects = RoomObjectFixtures.Unbound(new RoomObjectVisuals { RoomObjectChozoStatueVisuals = installed });
                AssertTrue(roomObjects.ChozoStatue.TrySpawnChozoStatueObject(level,
                    new RoomObjectSpawnRequest<ChozoStatueRoomObject>(new Position16<RoomBlocks>(3, 4), header)),
                    $"Chozo actor {header} spawns in a bounded test room");
                roomObjects.Step(level);
                int origin = 4 * width + 3;
                AssertEqual(physical,
                    level.GetCollisionBlockByIndex(origin).Terrain,
                    $"Chozo art retains native physical block for {header}");
            }
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                "Chozo slope PLMs did not reread compiled program or draw bytes");

            var crumbleLevel = CreateEmptyRoom(new(width, 16));
            var crumbleObjects = RoomObjectFixtures.Unbound();
            AssertTrue(crumbleObjects.ChozoStatue.TrySpawnChozoStatueObject(crumbleLevel,
                new RoomObjectSpawnRequest<ChozoStatueRoomObject>(new Position16<RoomBlocks>(3, 4), ChozoStatueRoomObject.PlugCrumbleUnderFootstep)),
                "Chozo crumble plug spawns in a bounded test room");
            var seenCrumbleBlocks = new HashSet<RoomTerrainBlock>();
            for (int frame = 0; frame < 15; frame++)
            {
                crumbleObjects.Step(crumbleLevel);
                seenCrumbleBlocks.Add(crumbleLevel.GetCollisionBlock(new(3, 4)).Terrain);
            }
            foreach (int block in new[] { 0x53, 0x54, 0x55, 0xff })
                AssertTrue(seenCrumbleBlocks.Contains(RoomTerrainBlock.Of(block, BlockMirroring.None, RoomCollisionType.Air)),
                    $"Chozo crumble program draws air block ${block:X3}");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                "Chozo crumble PLM did not reread compiled program or draw bytes");

            var wreckedHandLevel = CreateEmptyRoom(new(width, 16));
            var wreckedHandObjects = RoomObjectFixtures.Unbound();
            AssertTrue(wreckedHandObjects.ChozoStatue.TrySpawnChozoStatueObject(wreckedHandLevel,
                new RoomObjectSpawnRequest<ChozoStatueRoomObject>(new Position16<RoomBlocks>(3, 4), ChozoStatueRoomObject.WreckedShipHand)),
                "Wrecked Ship hand spawns");
            wreckedHandObjects.Step(wreckedHandLevel);
            AssertTrue(wreckedHandObjects.PopulationSlots.Count == 0,
                "Wrecked Ship hand executes the shared compiled delete list");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                "shared delete list did not reread its bank-$84 bytes");

            string stockPath = Path.Combine(
                installation.RoomObjectChozoStatueVisualDirectory,
                RoomObjectChozoStatueVisualFiles.VisualFileName);
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomObjectChozoStatueVisualFilesConstants.ValidateStock(
                    installation.RoomObjectChozoStatueVisualDirectory),
                "tampered Chozo stock fails loudly where it is read");
        }
    }

    /// <summary>Rejects cartridge reads from the compiled Chozo instruction, delete, and draw ranges.</summary>
    internal static CartridgeReadGuard NewChozoProgramAndDrawReadGuard(ISnesAddressSpace source) =>
        new MutableCartridgeReadGuard(source)
            .Forbid(address =>
                    (address >= (0x840000 | ChozoStatueObjectProgramDefinitions.CrumblePlugStart) &&
                     address <= (0x840000 | ChozoStatueObjectProgramDefinitions.CrumblePlugEnd)) ||
                    (address >= (0x840000 | ChozoStatueObjectProgramDefinitions.LowerNorfairHandStart) &&
                     address <= (0x840000 | ChozoStatueObjectProgramDefinitions.LowerNorfairHandEnd)) ||
                    (address >= (0x840000 | ChozoStatueObjectProgramDefinitions.ClearSlopeStart) &&
                     address <= (0x840000 | ChozoStatueObjectProgramDefinitions.ClearSlopeEnd)) ||
                    (address >= (0x840000 | ChozoStatueObjectProgramDefinitions.BlockSlopeStart) &&
                     address <= (0x840000 | ChozoStatueObjectProgramDefinitions.BlockSlopeEnd)) ||
                    (address >= (0x840000 | RoomObjectNativeListAddresses.DeleteList) &&
                     address <= (0x840000 | (RoomObjectNativeListAddresses.DeleteList + 1))) ||
                    RoomObjectChozoStatueVisualFiles.All.Any(draw =>
                    {
                        int first = 0x840000 | RoomObjectDrawAddressCodec.Of(draw.Draw);
                        int length = draw.Runs.Span.Sum(run =>
                            4 + run.Count * 2);
                        return address >= first && address < first + length;
                    }),
                address => $"Chozo PLM reread compiled program or draw byte ${address:X6}.");

}
