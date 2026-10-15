using SuperMetroid.Core.Enemies.Evir.Artwork;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Enemies.Evir.EvirInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Evir;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Verification.Enemies.Evir;

/// <summary>Checks that Evir instruction selectors resolve to installed frames with cartridge-equivalent OAM.</summary>
internal static class EvirVisualsVerification
{
    /// <summary>
    /// Checks every pose the Evir's programs show against editable stock spritemaps with cartridge-equivalent OAM, and
    /// that running each program with the stock artwork shows exactly its authored poses.
    /// </summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyInstalledEvirVisuals(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog stock)
    {
        var shownFrames = EvirAnimationsConstants.Spritemaps(Enum.GetValues<EvirProgram>()).ToHashSet();
        foreach (ushort pointer in shownFrames)
        {
            AssertTrue((stock.Spritemaps ?? throw new InvalidOperationException("Verification requires stock.Spritemaps.")).TryGetDisplay(
                    EvirVisualDefinitions.Bank, pointer,
                    out EnemySpritemapParts installedParts),
                $"Evir pose $A8:{pointer:X4} has editable OAM");
            var nativeOam = new OamBuffer();
            var installedOam = new OamBuffer();
            DrawImportedEnemySpritemap(rom, nativeOam, new EnemySpritemapDraw { Bank = EvirVisualDefinitions.Bank, Pointer = pointer, Origin = new(128, 128), PaletteBits = 0, BaseTileIndex = 0 });
            installedOam.AddEnemySpritemap(installedParts, new Position16<OamPixels>(128, 128), 0, 0);
            AssertTrue(nativeOam.ImageEquals(installedOam) &&
                       nativeOam.NextByteOffset == installedOam.NextByteOffset,
                $"Evir pose $A8:{pointer:X4} installed OAM matches cartridge");
        }
        AssertEqual(EvirVisualDefinitions.FrameCount, shownFrames.Count,
            "the Evir programs show 24 distinct editable OAM frames");
        AssertTrue(EvirVisualDefinitions.Frames().Select(frame => frame.Pointer).ToHashSet().SetEquals(shownFrames),
            "every exported Evir frame is shown by a program");
        foreach (EvirProgram loop in new[] { EvirProgram.BodyFacingLeft, EvirProgram.ArmsFacingLeft, EvirProgram.BodyFacingRight, EvirProgram.ArmsFacingRight })
        {
            RoomEnemySystem enemies = NewEvirInstructionSystem(rom);
            enemies.TileArtwork = stock;
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            PrepareEvirInstructionSlot(enemies, slot, EnemyDefinitionId.Evir, loop);
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(EvirAnimations.Programs, loop, EvirFrameDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(enemies, slot, EnemyInstructionRunner.LoopUpdates(EvirAnimations.Programs, loop)),
                $"installed Evir {loop} shows its authored poses");
        }
        Console.WriteLine(
            "  Evir visuals: 24 editable OAM frames match the cartridge and every installed loop shows its authored poses.");
    }

    /// <summary>Reads a cartridge byte solely for the native Evir frame and OAM comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for installed Evir visuals")]
    private static byte InstalledEvirVisualsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
