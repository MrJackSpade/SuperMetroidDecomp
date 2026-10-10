using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks eight native Mochtroid selectors against six installed editable OAM frames and rejects the adjacent mechanics operand as visual data.</summary>
    private static void VerifyInstalledMochtroidVisuals(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog stock)
    {
        var selectedFrames = new HashSet<ushort>();
        for (int index = 0;
             index < MochtroidInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort operand = MochtroidInstructionProgramDefinitionsTooling
                .PresentationWordAddress(index);
            int selectorAddress = (MochtroidVisualDefinitions.Bank << 16) | operand;
            ushort nativePointer = unchecked((ushort)(
                rom.ReadByte(selectorAddress) | rom.ReadByte(selectorAddress + 1) << 8));
            AssertEqual(nativePointer, MochtroidVisualDefinitions.FrameAt(operand),
                $"Mochtroid visual selector {index} matches pinned cartridge");
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    EnemyDefinitionPointers.Mochtroid, operand,
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
            DrawImportedEnemySpritemap(rom, nativeOam, MochtroidVisualDefinitions.Bank,
                nativePointer, 128, 128, 0, 0);
            installedOam.AddEnemySpritemap(installedParts, 128, 128, 0, 0);
            AssertTrue(nativeOam.LowTable.SequenceEqual(installedOam.LowTable) &&
                       nativeOam.HighTable.SequenceEqual(installedOam.HighTable) &&
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
