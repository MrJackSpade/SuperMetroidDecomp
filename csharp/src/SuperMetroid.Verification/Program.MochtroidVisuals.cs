using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static ImportedSpritemapsVerification;

internal static class MochtroidVisualsVerification
{
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for Mochtroid visuals")]
    internal static void VerifyInstalledMochtroidVisuals(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog stock)
    {
        var selectedFrames = new HashSet<ushort>();
        for (int index = 0;
             index < MochtroidInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort operand = MochtroidInstructionProgramDefinitionsTooling
                .PresentationWordAddress(index);
            int selectorAddress = ((int)new SnesAddress(MochtroidVisualDefinitions.Bank, operand));
            ushort nativePointer = CartridgeImportSource.Require(rom).ReadCartridgeWord(selectorAddress);
            AssertEqual(nativePointer, MochtroidVisualDefinitions.FrameAt(operand),
                $"Mochtroid visual selector {index} matches pinned cartridge");
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    EnemyDefinitionId.Mochtroid, operand,
                    out ushort installedPointer),
                $"Mochtroid selector {index} is installed");
            AssertEqual(nativePointer, installedPointer,
                $"Mochtroid selector {index} preserves native frame identity");
            selectedFrames.Add(installedPointer);
            AssertTrue(stock.Spritemaps!.TryGetDisplay(
                    MochtroidVisualDefinitions.Bank, installedPointer,
                    out EnemySpritemapParts installedParts),
                $"Mochtroid selector {index} has editable OAM");
            var nativeOam = new OamBuffer();
            var installedOam = new OamBuffer();
            DrawImportedEnemySpritemap(rom, nativeOam, new EnemySpritemapDraw { Bank = MochtroidVisualDefinitions.Bank, Pointer = nativePointer, Origin = new(128, 128), PaletteBits = 0, BaseTileIndex = 0 });
            installedOam.AddEnemySpritemap(installedParts, new Position16<OamPixels>(128, 128), 0, 0);
            AssertTrue(nativeOam.ImageEquals(installedOam) &&
                       nativeOam.NextByteOffset == installedOam.NextByteOffset,
                $"Mochtroid selector {index} installed OAM matches cartridge");
        }
        AssertEqual(MochtroidVisualDefinitions.FrameCount, selectedFrames.Count,
            "eight Mochtroid selectors choose six distinct editable OAM frames");
        AssertThrows<InvalidDataException>(
            () => MochtroidVisualDefinitions.FrameAt(
                MochtroidInstructionProgramDefinitions.FirstAdjacentMechanicsData),
            "Mochtroid visual catalog rejects adjacent shake physics");
        Console.WriteLine("  Mochtroid visuals: eight native selectors, six editable OAM frames and stock parity pass.");
    }
}
