using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Verifies Work Robot selectors retain native frame identity and OAM while installed programs avoid runtime presentation-word reads.</summary>
    private static void VerifyInstalledWorkRobotVisuals(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog stock)
    {
        var selectedFrames = new HashSet<ushort>();
        var guard = new WorkRobotInstructionReadGuard(rom, forbidPresentation: true);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessInstructions", BindingFlags.Instance | BindingFlags.NonPublic)!;
        for (int index = 0;
             index < WorkRobotInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort operand = WorkRobotInstructionProgramDefinitionsTooling
                .PresentationWordAddress(index);
            int selectorAddress = (WorkRobotVisualDefinitions.Bank << 16) | operand;
            ushort nativePointer = unchecked((ushort)(
                rom.ReadByte(selectorAddress) | rom.ReadByte(selectorAddress + 1) << 8));
            AssertEqual(nativePointer, WorkRobotVisualDefinitions.FrameAt(operand),
                $"Work Robot visual selector {index} matches pinned cartridge");
            ushort definition = operand < WorkRobotInstructionProgramDefinitions.Initial
                ? RoomEnemySystem.WorkRobotNoPowerDefinition
                : RoomEnemySystem.WorkRobotDefinition;
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    definition, operand, out ushort installedPointer),
                $"Work Robot selector {index} is installed for its actor");
            AssertEqual(nativePointer, installedPointer,
                $"Work Robot selector {index} preserves native frame identity");
            selectedFrames.Add(installedPointer);
            AssertTrue(stock.Spritemaps!.TryGetDisplay(
                    WorkRobotVisualDefinitions.Bank, installedPointer,
                    out EnemySpritemapParts installedParts),
                $"Work Robot selector {index} has editable OAM");
            var nativeOam = new OamBuffer();
            var installedOam = new OamBuffer();
            DrawImportedEnemySpritemap(rom, nativeOam, WorkRobotVisualDefinitions.Bank,
                nativePointer, 128, 128, 0, 0);
            installedOam.AddEnemySpritemap(installedParts, 128, 128, 0, 0);
            AssertTrue(nativeOam.LowTable.SequenceEqual(installedOam.LowTable) &&
                       nativeOam.HighTable.SequenceEqual(installedOam.HighTable) &&
                       nativeOam.NextByteOffset == installedOam.NextByteOffset,
                $"Work Robot selector {index} installed OAM matches cartridge");

            RoomEnemySystem enemies = CreateWorkRobotInstructionSystem(
                guard, out RoomEnemySlot robot);
            enemies.TileArtwork = stock;
            robot.EnemyDefinitionPointer = definition;
            robot.CurrentInstruction = unchecked((ushort)(operand - 2));
            robot.InstructionTimer = 1;
            process.Invoke(enemies,
                [robot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0]);
            AssertEqual(installedPointer, robot.SpritemapPointer,
                $"installed Work Robot instruction {index} selects native frame");
        }
        AssertEqual(WorkRobotVisualDefinitions.FrameCount, selectedFrames.Count,
            "227 Work Robot selectors choose 27 distinct editable frames");
        AssertEqual(0, guard.ObservedPresentationWords.Count,
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
