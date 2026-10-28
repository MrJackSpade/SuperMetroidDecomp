using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Core;
using static SuperMetroid.Verification.Bosses.MotherBrain.LookupMotherBrainBackgroundVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.LookupMotherBrainBoundaryVerification;
using static SuperMetroid.Verification.Rooms.Plms.BossRoomPlmProgramsVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.LookupMotherBrainRegularDrawsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Mother Brain's fake-death terrain mutation programs, draws, and reachable production PLMs.</summary>
internal static class MotherBrainFakeDeathPlmsVerification
{
    /// <summary>Compares native programs and geometry, then exercises twenty room mutations without source reads.</summary>
    internal static void VerifyCompiledMotherBrainFakeDeathPlms()
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

        RoomPlmShotBlockDrawDefinitions.DrawList[] draws =
            MotherBrainFakeDeathPlmDrawDefinitions.All.ToArray();
        AssertEqual(22, draws.Length,
            "Mother Brain fake death owns twenty-two physical draws");
        foreach (MotherBrainFakeDeathProgram program in Enum.GetValues<MotherBrainFakeDeathProgram>())
            AssertTrue(MotherBrainFakeDeathPlmDrawDefinitions.TryGet(PlmDraw.Of(MotherBrainFakeDeathPlmProgramDefinitions.Layout(program)), out _),
                $"Mother Brain program {program} selects a compiled draw");
        Reachable[] reachable =
        [
            new Reachable(PlmHeaderId.FillMotherBrainsWall, MotherBrainFakeDeathProgram.FillWall),
            new Reachable(PlmHeaderId.MotherBrainsRoomEscapeDoor, MotherBrainFakeDeathProgram.EscapeDoor),
            new Reachable(PlmHeaderId.MotherBrainsBackgroundRow2, MotherBrainFakeDeathProgram.BackgroundRow2),
            new Reachable(PlmHeaderId.MotherBrainsBackgroundRow3, MotherBrainFakeDeathProgram.BackgroundRow3),
            new Reachable(PlmHeaderId.MotherBrainsBackgroundRow4, MotherBrainFakeDeathProgram.BackgroundRow4),
            new Reachable(PlmHeaderId.MotherBrainsBackgroundRow5, MotherBrainFakeDeathProgram.BackgroundRow5),
            new Reachable(PlmHeaderId.MotherBrainsBackgroundRow6, MotherBrainFakeDeathProgram.BackgroundRow6),
            new Reachable(PlmHeaderId.MotherBrainsBackgroundRow7, MotherBrainFakeDeathProgram.BackgroundRow7),
            new Reachable(PlmHeaderId.MotherBrainsBackgroundRow8, MotherBrainFakeDeathProgram.BackgroundRow8),
            new Reachable(PlmHeaderId.MotherBrainsBackgroundRow9, MotherBrainFakeDeathProgram.BackgroundRow9),
            new Reachable(PlmHeaderId.MotherBrainsBackgroundRowA, MotherBrainFakeDeathProgram.BackgroundRowA),
            new Reachable(PlmHeaderId.MotherBrainsBackgroundRowB, MotherBrainFakeDeathProgram.BackgroundRowB),
            new Reachable(PlmHeaderId.MotherBrainsBackgroundRowC, MotherBrainFakeDeathProgram.BackgroundRowC),
            new Reachable(PlmHeaderId.MotherBrainsBackgroundRowD, MotherBrainFakeDeathProgram.BackgroundRowD),
            new Reachable(PlmHeaderId.ClearMotherBrainCeilingBlock, MotherBrainFakeDeathProgram.ClearCeilingBlock),
            new Reachable(PlmHeaderId.ClearMotherBrainCeilingTube, MotherBrainFakeDeathProgram.ClearCeilingTube),
            new Reachable(PlmHeaderId.ClearMotherBrainBottomMiddleSideTube, MotherBrainFakeDeathProgram.ClearBottomMiddleSideTube),
            new Reachable(PlmHeaderId.ClearMotherBrainBottomMiddleTubes, MotherBrainFakeDeathProgram.ClearBottomMiddleTubes),
            new Reachable(PlmHeaderId.ClearMotherBrainBottomLeftTube, MotherBrainFakeDeathProgram.ClearBottomLeftTube),
            new Reachable(PlmHeaderId.ClearMotherBrainBottomRightTube, MotherBrainFakeDeathProgram.ClearBottomRightTube),
        ];
        foreach ((PlmHeaderId header, MotherBrainFakeDeathProgram program) in reachable)
            VerifyMotherBrainFakeDeathMutation(header, program);

        Console.WriteLine(
            "Mother Brain fake-death PLMs: 22 typed programs, 22 full draws, and 20 reachable production mutations match ROM without source reads.");
    }

    /// <summary>Checks one spawned PLM's exact collision blocks, draw-frame lifetime, deletion, and source-read guard.</summary>
    private static void VerifyMotherBrainFakeDeathMutation(
        PlmHeaderId header, MotherBrainFakeDeathProgram program)
    {
        const int width = 32;
        const int height = 16;
        ushort[] words = Enumerable.Repeat((ushort)0x8123,
            width * height).ToArray();
        RoomLevelData level = CreateRoom(new(width, height), words,
            new byte[words.Length]);
        var plms = PlmFixtures.Unbound();
        AssertTrue(plms.BossRoomMutations.TrySpawnMotherBrainMutation(level, new(5, 3), header),
            $"Mother Brain mutation {header} allocates");
        ushort[] expected = Enumerable.Range(0, words.Length)
            .Select(index => level.GetCollisionBlockByIndex(index).PackedWord.Raw)
            .ToArray();
        AssertTrue(plms.PopulationSlots.Single().Animation is MotherBrainFakeDeathPlmAnimation { Program: var started } && started == program,
            $"Mother Brain mutation {header} starts its typed program");
        AssertTrue(MotherBrainFakeDeathPlmDrawDefinitions.TryGet(
                PlmDraw.Of(MotherBrainFakeDeathPlmProgramDefinitions.Layout(program)), out var draw),
            $"Mother Brain mutation {header} has a physical draw");
        for (int runIndex = 0; runIndex < draw.Runs.Length; runIndex++)
        {
            RoomPlmShotBlockDrawDefinitions.Run run = draw.Runs.Span[runIndex];
            int x = 5 + (runIndex == 0 ? 0 :
                draw.Runs.Span[runIndex - 1].Next.X);
            int y = 3 + (runIndex == 0 ? 0 :
                draw.Runs.Span[runIndex - 1].Next.Y);
            bool vertical = (run.DirectionAndCount & 0x8000) != 0;
            for (int block = 0; block < run.LevelWords.Length; block++)
                expected[(y + (vertical ? block : 0)) * width +
                    x + (vertical ? 0 : block)] = run.LevelWords.Span[block];
        }
        var guard = NewMotherBrainFakeDeathSourceGuard(new TestAddressSpace());
        plms.Step(level);
        AssertEqual(1, plms.SlotTable.ActiveCount,
            $"Mother Brain mutation {header} persists through its draw frame");
        for (int index = 0; index < expected.Length; index++)
            AssertEqual(expected[index],
                level.GetCollisionBlockByIndex(index).PackedWord.Raw,
                $"Mother Brain mutation {header} level block {index}");
        plms.Step(level);
        AssertEqual(0, plms.SlotTable.ActiveCount,
            $"Mother Brain mutation {header} deletes on the following frame");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            $"Mother Brain mutation {header} reads no migrated source bytes");
    }

    /// <summary>Forbids bank-$84 reads of the migrated fake-death instruction and draw ranges.</summary>
    internal static CartridgeReadGuard NewMotherBrainFakeDeathSourceGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address =>
        {
            int pointer = SnesAddressMath.OffsetInBank(address);
            return (SnesAddressMath.BankNumber(address)) == 0x84 &&
                (BossRoomPlmNativeListAddresses.IsTypedProgramByte(pointer) ||
                 MotherBrainFakeDeathPlmDrawDefinitions.All.Any(draw =>
                     pointer >= PlmDrawAddressCodec.Of(draw.Draw) && pointer < PlmDrawAddressCodec.Of(draw.Draw) +
                         draw.Runs.Span.Sum(run =>
                             4 + run.LevelWords.Length * 2)));
        }, address => $"Mother Brain mutation reread migrated source ${address:X6}.");

    /// <summary>Runs the focused Mother Brain terrain mutation comparison against the retail ROM.</summary>
    [VerificationCommand("--mother-brain-fake-death-plms")]
    internal static int MotherBrainFakeDeathPlmsCommand()
    {
        Suite(nameof(VerifyCompiledMotherBrainFakeDeathPlms), () => VerifyCompiledMotherBrainFakeDeathPlms());
        return 0;
    }
}
