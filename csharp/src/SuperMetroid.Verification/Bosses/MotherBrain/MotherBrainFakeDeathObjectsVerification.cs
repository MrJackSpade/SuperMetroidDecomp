using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Verification.Rooms.RoomObjects;
using SuperMetroid.Core;
using static SuperMetroid.Verification.Bosses.MotherBrain.LookupMotherBrainBackgroundVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.LookupMotherBrainBoundaryVerification;
using static SuperMetroid.Verification.Rooms.RoomObjects.BossRoomObjectProgramsVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.LookupMotherBrainRegularDrawsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Mother Brain's fake-death terrain mutation programs, draws, and reachable production PLMs.</summary>
internal static class MotherBrainFakeDeathObjectsVerification
{
    /// <summary>Compares native lists and geometry, then exercises twenty room mutations without source reads.</summary>
    internal static void VerifyCompiledMotherBrainFakeDeathObjects()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SuperMetroid.AssetExtraction.Cartridge.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Mother Brain mutation oracle revision");
        Suite(nameof(VerifyMotherBrainFakeDeathPrograms), () => VerifyMotherBrainFakeDeathPrograms(rom));
        Suite(nameof(VerifyMotherBrainRegularDrawMapping), () => VerifyMotherBrainRegularDrawMapping(rom));
        Suite(nameof(VerifyMotherBrainBackgroundGeometry), () => VerifyMotherBrainBackgroundGeometry(rom));
        Suite(nameof(VerifyMotherBrainBackgroundCollision), () => VerifyMotherBrainBackgroundCollision(rom));
        Suite(nameof(VerifyMotherBrainBoundaryGeometry), () => VerifyMotherBrainBoundaryGeometry(rom));
        Suite(nameof(VerifyMotherBrainBoundaryCollision), () => VerifyMotherBrainBoundaryCollision(rom));

        AssertEqual(20, RoomObjectMotherBrainFakeDeathVisualFiles.Family.Layouts.Count,
            "Mother Brain fake death owns twenty physical draws");
        foreach (MotherBrainRoomMutation mutation in Enum.GetValues<MotherBrainRoomMutation>())
        {
            TerrainChange change = MotherBrainRoomMutationDefinitions.Changes[mutation];
            AssertTrue(MotherBrainFakeDeathDraws.TryGet(change.Frames.Single().Draw, out _),
                $"Mother Brain mutation {mutation} draws a compiled layout");
            VerifyMotherBrainFakeDeathMutation(rom, mutation, change);
        }

        Console.WriteLine(
            "Mother Brain fake-death room objects: 22 full draws and 20 production mutations match ROM without source reads.");
    }

    /// <summary>
    /// Checks one spawned PLM's exact blocks, drawn from its installed picture, against the terrain the cartridge's draw
    /// list writes, and its draw-frame lifetime, deletion and source-read guard.
    /// </summary>
    private static void VerifyMotherBrainFakeDeathMutation(CartridgeImportAddressSpace rom,
        MotherBrainRoomMutation mutation, TerrainChange change)
    {
        const int width = 32;
        const int height = 16;
        RoomTerrainBlock[] terrain = Enumerable.Repeat(RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.SolidBlock),
            width * height).ToArray();
        RoomLevelData level = CreateRoom(new(width, height), terrain, new RoomBlockBehavior[terrain.Length]);
        var roomObjects = RoomObjectFixtures.Unbound();
        AssertTrue(roomObjects.BossRoomMutations.MotherBrainRoom.TrySpawn(level, new(new(5, 3), mutation)),
            $"Mother Brain mutation {mutation} allocates");
        RoomTerrainBlock[] expected = Enumerable.Range(0, terrain.Length)
            .Select(index => level.GetCollisionBlockByIndex(index).Terrain)
            .ToArray();
        AssertTrue(roomObjects.PopulationSlots.Single().Behaviour is TerrainChangeObject { Change: var started } && started == change,
            $"Mother Brain mutation {mutation} starts its terrain change");
        AssertTrue(MotherBrainFakeDeathDraws.TryGet(change.Frames.Single().Draw, out var draw),
            $"Mother Brain mutation {mutation} has a physical draw");
        foreach (KeyValuePair<Offset16<RoomBlocks>, RoomTerrainBlock> cell in
                 InstalledFramePictureAssertions.NativeTerrain(rom, RoomObjectDrawAddressCodec.Of(draw.Draw), out _))
            expected[(3 + cell.Key.Y) * width + 5 + cell.Key.X] = cell.Value;
        var guard = NewMotherBrainFakeDeathSourceGuard(new TestAddressSpace());
        roomObjects.Step(level);
        AssertEqual(1, roomObjects.SlotTable.ActiveCount,
            $"Mother Brain mutation {mutation} persists through its draw frame");
        for (int index = 0; index < expected.Length; index++)
            AssertEqual(expected[index],
                level.GetCollisionBlockByIndex(index).Terrain,
                $"Mother Brain mutation {mutation} level block {index}");
        roomObjects.Step(level);
        AssertEqual(0, roomObjects.SlotTable.ActiveCount,
            $"Mother Brain mutation {mutation} deletes on the following frame");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            $"Mother Brain mutation {mutation} reads no migrated source bytes");
    }

    /// <summary>Forbids bank-$84 reads of the migrated fake-death instruction and draw ranges.</summary>
    internal static CartridgeReadGuard NewMotherBrainFakeDeathSourceGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address =>
        {
            int pointer = SnesAddressMath.OffsetInBank(address);
            return (SnesAddressMath.BankNumber(address)) == 0x84 &&
                (BossRoomObjectNativeListAddresses.IsTypedProgramByte(pointer) ||
                 RoomObjectMotherBrainFakeDeathVisualFiles.Family.Layouts.Values.Any(draw =>
                     pointer >= RoomObjectDrawAddressCodec.Of(draw.Draw) && pointer < RoomObjectDrawAddressCodec.Of(draw.Draw) +
                         draw.Runs.Span.Sum(run =>
                             4 + run.Count * 2)));
        }, address => $"Mother Brain mutation reread migrated source ${address:X6}.");

    /// <summary>Runs the focused Mother Brain terrain mutation comparison against the retail ROM.</summary>
    [VerificationCommand("--mother-brain-fake-death-plms")]
    internal static int MotherBrainFakeDeathObjectsCommand()
    {
        Suite(nameof(VerifyCompiledMotherBrainFakeDeathObjects), () => VerifyCompiledMotherBrainFakeDeathObjects());
        return 0;
    }
}
