using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static ImportedSpritemapsVerification;
using static WorkRobotInstructionProgramDefinitionsVerification;

/// <summary>Checks native Work Robot visual selectors, installed editable
/// OAM frames, and instruction-driven frame selection.</summary>
internal static class WorkRobotVisualsVerification
{
    /// <summary>Compares all 227 selectors with native words and rendered OAM,
    /// then ensures the instruction runner avoids cartridge presentation reads.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for installed Work Robot visuals")]
    internal static void VerifyInstalledWorkRobotVisuals(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog stock)
    {
        var selectedFrames = new HashSet<ushort>();
        var guard = NewWorkRobotInstructionReadGuard(rom, forbidPresentation: true);
        for (int index = 0;
             index < WorkRobotInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort operand = WorkRobotInstructionProgramDefinitionsTooling
                .PresentationWordAddress(index);
            int selectorAddress = ((int)new SnesAddress(WorkRobotVisualDefinitions.Bank, operand));
            ushort nativePointer = CartridgeImportSource.Require(rom).ReadCartridgeWord(selectorAddress);
            AssertEqual(nativePointer, WorkRobotVisualDefinitions.FrameAt(operand),
                $"Work Robot visual selector {index} matches pinned cartridge");
            EnemyDefinitionId definition = operand < WorkRobotInstructionProgramDefinitions.Initial
                ? EnemyDefinitionId.RobotNoPower
                : EnemyDefinitionId.Robot;
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    definition, operand, out ushort installedPointer),
                $"Work Robot selector {index} is installed for its actor");
            AssertEqual(nativePointer, installedPointer,
                $"Work Robot selector {index} preserves native frame identity");
            selectedFrames.Add(installedPointer);
            AssertTrue((stock.Spritemaps ?? throw new InvalidOperationException("Verification requires stock.Spritemaps.")).TryGetDisplay(
                    WorkRobotVisualDefinitions.Bank, installedPointer,
                    out EnemySpritemapParts installedParts),
                $"Work Robot selector {index} has editable OAM");
            var nativeOam = new OamBuffer();
            var installedOam = new OamBuffer();
            DrawImportedEnemySpritemap(rom, nativeOam, new EnemySpritemapDraw { Bank = WorkRobotVisualDefinitions.Bank, Pointer = nativePointer, Origin = new(128, 128), PaletteBits = 0, BaseTileIndex = 0 });
            installedOam.AddEnemySpritemap(installedParts, new Position16<OamPixels>(128, 128), 0, 0);
            AssertTrue(nativeOam.ImageEquals(installedOam) &&
                       nativeOam.NextByteOffset == installedOam.NextByteOffset,
                $"Work Robot selector {index} installed OAM matches cartridge");

            RoomEnemySystem enemies = CreateWorkRobotInstructionSystem(
                guard, out RoomEnemySlot robot);
            enemies.TileArtwork = stock;
            robot.EnemyDefinitionPointer = definition;
            robot.CurrentInstruction = unchecked((ushort)(operand - 2));
            robot.InstructionTimer = NativeCountdown.Start(1);
            enemies.Instructions.ProcessInstructions(robot, null, null, Position16<RoomPixels>.Zero, (ushort)0);
            AssertEqual(installedPointer, robot.SpritemapPointer,
                $"installed Work Robot instruction {index} selects native frame");
        }
        AssertEqual(WorkRobotVisualDefinitions.FrameCount, selectedFrames.Count,
            "227 Work Robot selectors choose 27 distinct editable frames");
        AssertEqual(0, guard.ObservedWords.Count,
            "all installed Work Robot programs avoid cartridge presentation words");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "all installed Work Robot programs retain compiled control words");
        AssertThrows<InvalidDataException>(
            () => WorkRobotVisualDefinitions.FrameAt(
                WorkRobotInstructionProgramDefinitions.Initial),
            "Work Robot visual catalog rejects adjacent mechanics data");
        Console.WriteLine(
            "  Work Robot visuals: 227 guarded native selectors and 27 editable OAM frames pass.");
    }
}
