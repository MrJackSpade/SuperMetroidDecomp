using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyInstalledYardVisuals(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog stock)
    {
        var selectedFrames = new HashSet<ushort>();
        var guard = new YardInstructionReadGuard(rom, forbidPresentation: true);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessInstructions", BindingFlags.Instance | BindingFlags.NonPublic)!;
        for (int index = 0;
             index < YardInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = YardInstructionProgramDefinitions.PresentationWordAddress(index);
            int selectorAddress = (YardVisualDefinitions.Bank << 16) | operand;
            ushort nativePointer = unchecked((ushort)(
                rom.ReadByte(selectorAddress) | rom.ReadByte(selectorAddress + 1) << 8));
            AssertEqual(nativePointer, YardVisualDefinitions.FrameAt(operand),
                $"Yard visual selector {index} matches pinned cartridge");
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    RoomEnemySystem.YardDefinition, operand,
                    out ushort installedPointer),
                $"Yard selector {index} is installed");
            AssertEqual(nativePointer, installedPointer,
                $"Yard selector {index} preserves native frame identity");
            selectedFrames.Add(installedPointer);
            AssertTrue(stock.Spritemaps!.TryGetDisplay(
                    YardVisualDefinitions.Bank, installedPointer,
                    out ReadOnlyMemory<EnemySpritemapPart> installedParts),
                $"Yard selector {index} has editable OAM");
            var nativeOam = new OamBuffer();
            var installedOam = new OamBuffer();
            nativeOam.AddEnemySpritemap(rom, YardVisualDefinitions.Bank,
                nativePointer, 128, 128, 0, 0);
            installedOam.AddEnemySpritemap(installedParts.Span, 128, 128, 0, 0);
            AssertTrue(nativeOam.LowTable.SequenceEqual(installedOam.LowTable) &&
                       nativeOam.HighTable.SequenceEqual(installedOam.HighTable) &&
                       nativeOam.NextByteOffset == installedOam.NextByteOffset,
                $"Yard selector {index} installed OAM matches cartridge");

            RoomEnemySystem enemies = CreateYardInstructionSystem(
                guard, out RoomEnemySlot yard);
            enemies.TileArtwork = stock;
            yard.CurrentInstruction = unchecked((ushort)(operand - 2));
            yard.InstructionTimer = 1;
            process.Invoke(enemies,
                [yard, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0]);
            AssertEqual(installedPointer, yard.SpritemapPointer,
                $"installed Yard instruction {index} selects native frame");
        }
        AssertEqual(YardVisualDefinitions.FrameCount, selectedFrames.Count,
            "112 Yard selectors choose 104 distinct editable frames");
        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "all installed Yard programs avoid cartridge presentation words");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "all installed Yard programs retain compiled control words");
        AssertThrows<InvalidDataException>(
            () => YardVisualDefinitions.FrameAt(
                YardInstructionProgramDefinitions.CrawlingUpsideUpMovingLeft),
            "Yard visual catalog rejects adjacent mechanics data");
        Console.WriteLine(
            "  Yard visuals: 112 guarded native selectors and 104 editable OAM frames pass.");
    }
}
