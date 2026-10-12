using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.AssetExtraction;
using static AssertionsVerification;
using static LookupMotherBrainBackgroundVerification;
using static LookupMotherBrainBoundaryVerification;
using static LookupMotherBrainMutationProgramsVerification;
using static LookupMotherBrainRegularDrawsVerification;
using static SuiteTimingVerification;
using static VerificationHelpersVerification;

internal static class MotherBrainFakeDeathPlmsVerification
{
    internal static void VerifyCompiledMotherBrainFakeDeathPlms()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SuperMetroid.AssetExtraction.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Mother Brain mutation oracle revision");
        Suite(nameof(VerifyMotherBrainMutationProgramControls), () => VerifyMotherBrainMutationProgramControls(rom));
        Suite(nameof(VerifyMotherBrainMutationProgramDraws), () => VerifyMotherBrainMutationProgramDraws(rom));
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
        for (int programIndex = 0;
             programIndex < MotherBrainFakeDeathPlmProgramDefinitions.ProgramCount;
             programIndex++)
        {
            ushort pointer = checked((ushort)(
                MotherBrainFakeDeathPlmProgramDefinitions.Start +
                programIndex * MotherBrainFakeDeathPlmProgramDefinitions.ProgramByteLength + 2));
            AssertTrue(MotherBrainFakeDeathPlmProgramDefinitions.TryReadMechanicsWord(
                    pointer, out ushort drawPointer) &&
                MotherBrainFakeDeathPlmDrawDefinitions.TryGet(drawPointer, out _),
                $"Mother Brain program ${pointer - 2:X4} selects a compiled draw");
        }
        (PlmHeaderId Header, ushort Program)[] reachable =
        [
            (PlmHeaderId.FillMotherBrainsWall, RoomPlmInstructionLists.FillMotherBrainsWall),
            (PlmHeaderId.MotherBrainsRoomEscapeDoor, RoomPlmInstructionLists.MotherBrainsRoomEscapeDoor),
            (PlmHeaderId.MotherBrainsBackgroundRow2, RoomPlmInstructionLists.MotherBrainsBackgroundRow2),
            (PlmHeaderId.MotherBrainsBackgroundRow3, RoomPlmInstructionLists.MotherBrainsBackgroundRow3),
            (PlmHeaderId.MotherBrainsBackgroundRow4, RoomPlmInstructionLists.MotherBrainsBackgroundRow4),
            (PlmHeaderId.MotherBrainsBackgroundRow5, RoomPlmInstructionLists.MotherBrainsBackgroundRow5),
            (PlmHeaderId.MotherBrainsBackgroundRow6, RoomPlmInstructionLists.MotherBrainsBackgroundRow6),
            (PlmHeaderId.MotherBrainsBackgroundRow7, RoomPlmInstructionLists.MotherBrainsBackgroundRow7),
            (PlmHeaderId.MotherBrainsBackgroundRow8, RoomPlmInstructionLists.MotherBrainsBackgroundRow8),
            (PlmHeaderId.MotherBrainsBackgroundRow9, RoomPlmInstructionLists.MotherBrainsBackgroundRow9),
            (PlmHeaderId.MotherBrainsBackgroundRowA, RoomPlmInstructionLists.MotherBrainsBackgroundRowA),
            (PlmHeaderId.MotherBrainsBackgroundRowB, RoomPlmInstructionLists.MotherBrainsBackgroundRowB),
            (PlmHeaderId.MotherBrainsBackgroundRowC, RoomPlmInstructionLists.MotherBrainsBackgroundRowC),
            (PlmHeaderId.MotherBrainsBackgroundRowD, RoomPlmInstructionLists.MotherBrainsBackgroundRowD),
            (PlmHeaderId.ClearMotherBrainCeilingBlock, RoomPlmInstructionLists.ClearMotherBrainCeilingBlock),
            (PlmHeaderId.ClearMotherBrainCeilingTube, RoomPlmInstructionLists.ClearMotherBrainCeilingTube),
            (PlmHeaderId.ClearMotherBrainBottomMiddleSideTube, RoomPlmInstructionLists.ClearMotherBrainBottomMiddleSideTube),
            (PlmHeaderId.ClearMotherBrainBottomMiddleTubes, RoomPlmInstructionLists.ClearMotherBrainBottomMiddleTubes),
            (PlmHeaderId.ClearMotherBrainBottomLeftTube, RoomPlmInstructionLists.ClearMotherBrainBottomLeftTube),
            (PlmHeaderId.ClearMotherBrainBottomRightTube, RoomPlmInstructionLists.ClearMotherBrainBottomRightTube),
        ];
        foreach ((PlmHeaderId header, ushort program) in reachable)
            VerifyMotherBrainFakeDeathMutation(header, program);

        Console.WriteLine(
            "Mother Brain fake-death PLMs: 66 instruction words, 22 full draws, and 20 reachable production mutations match ROM without source reads.");
    }

    private static void VerifyMotherBrainFakeDeathMutation(
        PlmHeaderId header, ushort program)
    {
        const int width = 32;
        const int height = 16;
        ushort[] words = Enumerable.Repeat((ushort)0x8123,
            width * height).ToArray();
        RoomLevelData level = CreateRoom(new(width, height), words,
            new byte[words.Length], blockDefinitions: new byte[0x400 * 8]);
        var plms = new RoomPlmSystem();
        AssertTrue(plms.TrySpawnMotherBrainMutation(level, new(5, 3), header),
            $"Mother Brain mutation ${(int)header:X4} allocates");
        ushort[] expected = Enumerable.Range(0, words.Length)
            .Select(index => level.GetCollisionBlockByIndex(index).LevelWord)
            .ToArray();
        AssertTrue(MotherBrainFakeDeathPlmProgramDefinitions.TryReadMechanicsWord(
                checked((ushort)(program + 2)), out ushort drawPointer),
            $"Mother Brain mutation ${(int)header:X4} selects a compiled draw");
        AssertTrue(MotherBrainFakeDeathPlmDrawDefinitions.TryGet(
                drawPointer, out var draw),
            $"Mother Brain mutation ${(int)header:X4} has a physical draw");
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
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        plms.Step(guard, level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(1, plms.ActiveCount,
            $"Mother Brain mutation ${(int)header:X4} persists through its draw frame");
        for (int index = 0; index < expected.Length; index++)
            AssertEqual(expected[index],
                level.GetCollisionBlockByIndex(index).LevelWord,
                $"Mother Brain mutation ${(int)header:X4} level block {index}");
        plms.Step(guard, level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(0, plms.ActiveCount,
            $"Mother Brain mutation ${(int)header:X4} deletes on the following frame");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            $"Mother Brain mutation ${(int)header:X4} reads no migrated source bytes");
    }

    internal static CartridgeReadGuard NewMotherBrainFakeDeathSourceGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address =>
        {
            int pointer = SnesAddressMath.OffsetInBank(address);
            return (SnesAddressMath.BankNumber(address)) == 0x84 &&
                ((pointer >= MotherBrainFakeDeathPlmProgramDefinitions.Start &&
                  pointer < MotherBrainFakeDeathPlmProgramDefinitions.EndExclusive) ||
                 MotherBrainFakeDeathPlmDrawDefinitions.All.Any(draw =>
                     pointer >= draw.Pointer && pointer < draw.Pointer +
                         draw.Runs.Span.ToArray().Sum(run =>
                             4 + run.LevelWords.Length * 2)));
        }, address => $"Mother Brain mutation reread migrated source ${address:X6}.");

    [VerificationCommand("--mother-brain-fake-death-plms")]
    internal static int MotherBrainFakeDeathPlmsCommand()
    {
        Suite(nameof(VerifyCompiledMotherBrainFakeDeathPlms), () => VerifyCompiledMotherBrainFakeDeathPlms());
        return 0;
    }
}
