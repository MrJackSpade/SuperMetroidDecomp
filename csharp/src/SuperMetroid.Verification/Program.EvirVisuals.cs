using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyInstalledEvirVisuals(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog stock)
    {
        var selectedFrames = new HashSet<ushort>();
        for (int index = 0;
             index < EvirInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = EvirInstructionProgramDefinitions
                .PresentationWordAddress(index);
            int selectorAddress = (EvirVisualDefinitions.Bank << 16) | operand;
            ushort nativePointer = unchecked((ushort)(
                rom.ReadByte(selectorAddress) | rom.ReadByte(selectorAddress + 1) << 8));
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
            AssertTrue(stock.Spritemaps!.TryGetDisplay(
                    EvirVisualDefinitions.Bank, installedPointer,
                    out EnemySpritemapParts installedParts),
                $"Evir selector {index} has editable OAM");
            var nativeOam = new OamBuffer();
            var installedOam = new OamBuffer();
            DrawImportedEnemySpritemap(rom, nativeOam, EvirVisualDefinitions.Bank,
                nativePointer, 128, 128, 0, 0);
            installedOam.AddEnemySpritemap(installedParts, 128, 128, 0, 0);
            AssertTrue(nativeOam.LowTable.SequenceEqual(installedOam.LowTable) &&
                       nativeOam.HighTable.SequenceEqual(installedOam.HighTable) &&
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
        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        var guard = new EvirInstructionReadGuard(rom, forbidPresentation: true);
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
            RoomEnemySystem enemies = NewEvirInstructionSystem(guard, flags);
            enemies.TileArtwork = stock;
            RoomEnemySlot slot = enemies.Slots[0];
            PrepareEvirInstructionSlot(slot, definition,
                unchecked((ushort)(operand - 2)));
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0];
            process.Invoke(enemies, arguments);
            AssertEqual(EvirVisualDefinitions.FrameAt(operand), slot.SpritemapPointer,
                $"installed Evir program {index} selects its native visual frame");
        }
        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "all 49 installed Evir programs avoid cartridge presentation words");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "all 49 installed Evir programs retain compiled timing words");
        Console.WriteLine(
            "  Evir visuals: 49 guarded native selectors and 24 editable OAM frames pass.");
    }
}
