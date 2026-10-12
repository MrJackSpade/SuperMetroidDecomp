using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static ImportedSpritemapsVerification;
using static YardInstructionProgramDefinitionsVerification;

internal static class YardVisualsVerification
{
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for installed Yard visuals")]
    internal static void VerifyInstalledYardVisuals(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog stock)
    {
        var selectedFrames = new HashSet<ushort>();
        var guard = NewYardInstructionReadGuard(rom, forbidPresentation: true);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessInstructions", BindingFlags.Instance | BindingFlags.NonPublic)!;
        for (int index = 0;
             index < YardInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = YardInstructionProgramDefinitions.PresentationWordAddress(index);
            int selectorAddress = ((int)new SnesAddress(YardVisualDefinitions.Bank, operand));
            ushort nativePointer = CartridgeImportSource.Require(rom).ReadCartridgeWord(selectorAddress);
            AssertEqual(nativePointer, YardVisualDefinitions.FrameAt(operand),
                $"Yard visual selector {index} matches pinned cartridge");
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    EnemyDefinitionId.Yard, operand,
                    out ushort installedPointer),
                $"Yard selector {index} is installed");
            AssertEqual(nativePointer, installedPointer,
                $"Yard selector {index} preserves native frame identity");
            selectedFrames.Add(installedPointer);
            AssertTrue(stock.Spritemaps!.TryGetDisplay(
                    YardVisualDefinitions.Bank, installedPointer,
                    out EnemySpritemapParts installedParts),
                $"Yard selector {index} has editable OAM");
            var nativeOam = new OamBuffer();
            var installedOam = new OamBuffer();
            DrawImportedEnemySpritemap(rom, nativeOam, new EnemySpritemapDraw { Bank = YardVisualDefinitions.Bank, Pointer = nativePointer, Origin = new(128, 128), PaletteBits = 0, BaseTileIndex = 0 });
            installedOam.AddEnemySpritemap(installedParts, new Position16<OamPixels>(128, 128), 0, 0);
            AssertTrue(nativeOam.ImageEquals(installedOam) &&
                       nativeOam.NextByteOffset == installedOam.NextByteOffset,
                $"Yard selector {index} installed OAM matches cartridge");

            RoomEnemySystem enemies = CreateYardInstructionSystem(
                guard, out RoomEnemySlot yard);
            enemies.TileArtwork = stock;
            yard.CurrentInstruction = unchecked((ushort)(operand - 2));
            yard.InstructionTimer = 1;
            process.Invoke(enemies,
                [yard, null, null, Position16<RoomPixels>.Zero, (ushort)0]);
            AssertEqual(installedPointer, yard.SpritemapPointer,
                $"installed Yard instruction {index} selects native frame");
        }
        AssertEqual(YardVisualDefinitions.FrameCount, selectedFrames.Count,
            "112 Yard selectors choose 104 distinct editable frames");
        AssertEqual(0, guard.ObservedWords.Count,
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
