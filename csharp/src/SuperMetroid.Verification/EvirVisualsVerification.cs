using System.Reflection;
using SuperMetroid.Core.Hardware;
using static EvirInstructionProgramDefinitionsVerification;
using static ImportedSpritemapsVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Evir;
using SuperMetroid.Core.Enemies.Drawing;

/// <summary>Checks that Evir instruction selectors resolve to installed frames with cartridge-equivalent OAM.</summary>
internal static class EvirVisualsVerification
{
    /// <summary>Checks all 49 Evir selectors against native pointers, editable spritemaps, and ROM-free instruction execution.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyInstalledEvirVisuals(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog stock)
    {
        var selectedFrames = new HashSet<ushort>();
        for (int index = 0;
             index < EvirInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = EvirInstructionProgramDefinitions
                .PresentationWordAddress(index);
            int selectorAddress = ((int)new SnesAddress(EvirVisualDefinitions.Bank, operand));
            ushort nativePointer = unchecked((ushort)(
                SnesLittleEndian.Word(InstalledEvirVisualsRomByte(rom, selectorAddress), InstalledEvirVisualsRomByte(rom, selectorAddress + 1))));
            AssertEqual(nativePointer, EvirVisualDefinitions.FrameAt(operand),
                $"Evir visual selector {index} matches pinned cartridge");
            EnemyDefinitionId definition = operand >=
                unchecked((ushort)(EvirInstructionProgramDefinitions.ProjectileNormal + 2))
                ? EnemyDefinitionId.EvirProjectile
                : EnemyDefinitionId.Evir;
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    definition, operand, out ushort installedPointer),
                $"Evir selector {index} is installed for its native actor");
            AssertEqual(nativePointer, installedPointer,
                $"Evir selector {index} preserves native frame identity");
            selectedFrames.Add(installedPointer);
            AssertTrue((stock.Spritemaps ?? throw new InvalidOperationException("Verification requires stock.Spritemaps.")).TryGetDisplay(
                    EvirVisualDefinitions.Bank, installedPointer,
                    out EnemySpritemapParts installedParts),
                $"Evir selector {index} has editable OAM");
            var nativeOam = new OamBuffer();
            var installedOam = new OamBuffer();
            DrawImportedEnemySpritemap(rom, nativeOam, new EnemySpritemapDraw { Bank = EvirVisualDefinitions.Bank, Pointer = nativePointer, Origin = new(128, 128), PaletteBits = 0, BaseTileIndex = 0 });
            installedOam.AddEnemySpritemap(installedParts, new Position16<OamPixels>(128, 128), 0, 0);
            AssertTrue(nativeOam.ImageEquals(installedOam) &&
                       nativeOam.NextByteOffset == installedOam.NextByteOffset,
                $"Evir selector {index} installed OAM matches cartridge");
        }
        AssertEqual(EvirVisualDefinitions.FrameCount, selectedFrames.Count,
            "49 Evir selectors choose 24 distinct editable OAM frames");
        AssertThrows<InvalidDataException>(
            () => EvirVisualDefinitions.FrameAt(
                EvirInstructionProgramDefinitions.AdjacentCallbackCode),
            "Evir visual catalog rejects adjacent AI callback code");
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = NewEvirInstructionGuard(rom, forbidPresentation: true);
        for (int index = 0;
             index < EvirInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = EvirInstructionProgramDefinitions
                .PresentationWordAddress(index);
            EnemyDefinitionId definition = operand >=
                unchecked((ushort)(EvirInstructionProgramDefinitions.ProjectileNormal + 2))
                    ? EnemyDefinitionId.EvirProjectile
                    : EnemyDefinitionId.Evir;
            RoomEnemySystem enemies = NewEvirInstructionSystem(guard);
            enemies.TileArtwork = stock;
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            PrepareEvirInstructionSlot(slot, definition,
                unchecked((ushort)(operand - 2)));
            object?[] arguments =
                [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
            EnemyInstructionRunner.Process(enemies, arguments);
            AssertEqual(EvirVisualDefinitions.FrameAt(operand), slot.SpritemapPointer,
                $"installed Evir program {index} selects its native visual frame");
        }
        AssertEqual(0, guard.ObservedWords.Count,
            "all 49 installed Evir programs avoid cartridge presentation words");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "all 49 installed Evir programs retain compiled timing words");
        Console.WriteLine(
            "  Evir visuals: 49 guarded native selectors and 24 editable OAM frames pass.");
    }

    /// <summary>Reads a cartridge byte solely for the native Evir frame and OAM comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for installed Evir visuals")]
    private static byte InstalledEvirVisualsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
