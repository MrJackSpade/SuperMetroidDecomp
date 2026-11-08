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
    /// <summary>Compares native programs and geometry, then exercises twenty room mutations without source reads.</summary>
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
        Suite(nameof(VerifyMotherBrainWallVisuals), () => VerifyMotherBrainWallVisuals(rom));
        Suite(nameof(VerifyMotherBrainDoorVisuals), () => VerifyMotherBrainDoorVisuals(rom));

        RoomObjectShotBlockDrawDefinitions.DrawList[] draws =
            RoomObjectMotherBrainFakeDeathVisualFiles.All.ToArray();
        AssertEqual(22, draws.Length,
            "Mother Brain fake death owns twenty-two physical draws");
        foreach (MotherBrainFakeDeathProgram program in Enum.GetValues<MotherBrainFakeDeathProgram>())
            AssertTrue(MotherBrainFakeDeathDraws.TryGet(RoomObjectDraw.Of(MotherBrainFakeDeathPrograms.Layout(program)), out _),
                $"Mother Brain program {program} selects a compiled draw");
        Reachable[] reachable =
        [
            new Reachable(MotherBrainRoomMutation.FillWall, MotherBrainFakeDeathProgram.FillWall),
            new Reachable(MotherBrainRoomMutation.EscapeDoor, MotherBrainFakeDeathProgram.EscapeDoor),
            new Reachable(MotherBrainRoomMutation.BackgroundRow2, MotherBrainFakeDeathProgram.BackgroundRow2),
            new Reachable(MotherBrainRoomMutation.BackgroundRow3, MotherBrainFakeDeathProgram.BackgroundRow3),
            new Reachable(MotherBrainRoomMutation.BackgroundRow4, MotherBrainFakeDeathProgram.BackgroundRow4),
            new Reachable(MotherBrainRoomMutation.BackgroundRow5, MotherBrainFakeDeathProgram.BackgroundRow5),
            new Reachable(MotherBrainRoomMutation.BackgroundRow6, MotherBrainFakeDeathProgram.BackgroundRow6),
            new Reachable(MotherBrainRoomMutation.BackgroundRow7, MotherBrainFakeDeathProgram.BackgroundRow7),
            new Reachable(MotherBrainRoomMutation.BackgroundRow8, MotherBrainFakeDeathProgram.BackgroundRow8),
            new Reachable(MotherBrainRoomMutation.BackgroundRow9, MotherBrainFakeDeathProgram.BackgroundRow9),
            new Reachable(MotherBrainRoomMutation.BackgroundRowA, MotherBrainFakeDeathProgram.BackgroundRowA),
            new Reachable(MotherBrainRoomMutation.BackgroundRowB, MotherBrainFakeDeathProgram.BackgroundRowB),
            new Reachable(MotherBrainRoomMutation.BackgroundRowC, MotherBrainFakeDeathProgram.BackgroundRowC),
            new Reachable(MotherBrainRoomMutation.BackgroundRowD, MotherBrainFakeDeathProgram.BackgroundRowD),
            new Reachable(MotherBrainRoomMutation.ClearCeilingBlock, MotherBrainFakeDeathProgram.ClearCeilingBlock),
            new Reachable(MotherBrainRoomMutation.ClearCeilingTube, MotherBrainFakeDeathProgram.ClearCeilingTube),
            new Reachable(MotherBrainRoomMutation.ClearBottomMiddleSideTube, MotherBrainFakeDeathProgram.ClearBottomMiddleSideTube),
            new Reachable(MotherBrainRoomMutation.ClearBottomMiddleTubes, MotherBrainFakeDeathProgram.ClearBottomMiddleTubes),
            new Reachable(MotherBrainRoomMutation.ClearBottomLeftTube, MotherBrainFakeDeathProgram.ClearBottomLeftTube),
            new Reachable(MotherBrainRoomMutation.ClearBottomRightTube, MotherBrainFakeDeathProgram.ClearBottomRightTube),
        ];
        foreach ((MotherBrainRoomMutation mutation, MotherBrainFakeDeathProgram program) in reachable)
            VerifyMotherBrainFakeDeathMutation(mutation, program);

        Console.WriteLine(
            "Mother Brain fake-death PLMs: 22 typed programs, 22 full draws, and 20 reachable production mutations match ROM without source reads.");
    }

    /// <summary>Checks one spawned PLM's exact collision blocks, draw-frame lifetime, deletion, and source-read guard.</summary>
    private static void VerifyMotherBrainFakeDeathMutation(
        MotherBrainRoomMutation mutation, MotherBrainFakeDeathProgram program)
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
        AssertTrue(roomObjects.PopulationSlots.Single().Animation is MotherBrainFakeDeathObjectAnimation { Program: var started } && started == program,
            $"Mother Brain mutation {mutation} starts its typed program");
        AssertTrue(MotherBrainFakeDeathDraws.TryGet(
                RoomObjectDraw.Of(MotherBrainFakeDeathPrograms.Layout(program)), out var draw),
            $"Mother Brain mutation {mutation} has a physical draw");
        for (int runIndex = 0; runIndex < draw.Runs.Length; runIndex++)
        {
            RoomObjectShotBlockDrawDefinitions.Run run = draw.Runs.Span[runIndex];
            int x = 5 + run.Start.X;
            int y = 3 + run.Start.Y;
            bool vertical = run.Direction == RoomObjectRunDirection.Vertical;
            for (int block = 0; block < run.Blocks.Length; block++)
                expected[(y + (vertical ? block : 0)) * width +
                    x + (vertical ? 0 : block)] = run.Blocks.Span[block];
        }
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
                 RoomObjectMotherBrainFakeDeathVisualFiles.All.Any(draw =>
                     pointer >= RoomObjectDrawAddressCodec.Of(draw.Draw) && pointer < RoomObjectDrawAddressCodec.Of(draw.Draw) +
                         draw.Runs.Span.Sum(run =>
                             4 + run.Blocks.Length * 2)));
        }, address => $"Mother Brain mutation reread migrated source ${address:X6}.");

    /// <summary>Runs the focused Mother Brain terrain mutation comparison against the retail ROM.</summary>
    [VerificationCommand("--mother-brain-fake-death-plms")]
    internal static int MotherBrainFakeDeathObjectsCommand()
    {
        Suite(nameof(VerifyCompiledMotherBrainFakeDeathObjects), () => VerifyCompiledMotherBrainFakeDeathObjects());
        return 0;
    }
}
