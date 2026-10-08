using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    // These adapters require an import source at the diagnostic boundary. Core has no
    // address-based artwork loader, including when tests use constructed cartridge data.
    private static void DrawImportedEnemySpritemap(ISnesAddressSpace bus, OamBuffer oam,
        byte bank, ushort spritemapPointer, ushort originX, ushort originY,
        ushort paletteBits, ushort baseTileIndex,
        bool clipVerticalWrap = false, bool originYIsOnScreen = true) =>
        ImportedSpritemapOracle.DrawEnemy(CartridgeImportSource.Require(bus), oam, bank,
            spritemapPointer, originX, originY, paletteBits, baseTileIndex,
            clipVerticalWrap, originYIsOnScreen);

    /// <summary>
    /// The cartridge spritemap an enemy projectile draws this frame. Installed-presentation frames
    /// keep a blank spritemap and name the image by instruction operand; the cartridge's spritemap
    /// for such a frame is the word stored at that operand in bank $86.
    /// </summary>
    private static ushort NativeEnemyProjectileSpritemap(ISnesAddressSpace bus, RoomEnemyProjectileSlot projectile) =>
        projectile.PresentationOperandAddress != 0
            ? SuperMetroid.Core.Rom.RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                0x860000 | projectile.PresentationOperandAddress)
            : projectile.SpritemapPointer;

    private static void DrawImportedEnemyProjectileSpritemap(ISnesAddressSpace bus, OamBuffer oam,
        ushort bank8dSpritemapPointer, ushort originX, ushort originY,
        ushort graphicsIndex, bool originYIsOnScreen) =>
        ImportedSpritemapOracle.DrawEnemyProjectile(CartridgeImportSource.Require(bus), oam,
            bank8dSpritemapPointer, originX, originY, graphicsIndex, originYIsOnScreen);

    /// <summary>Imports only the sprite records authored in a sparse projectile fixture.</summary>
    private static EnemyProjectileSpritemapCatalog ImportFixtureProjectileSprites(TestAddressSpace bus)
    {
        var frames = EnemyProjectileSpritemapDefinitions.Frames.ToDictionary(
            frame => frame.Name, frame => EnemySpritemapFiles.ExtractParts(bus, 0x8d, frame.Pointer));
        var programs = new Dictionary<string, SpriteVisualPart[]>();
        foreach (EnemyProjectilePresentationFrameDefinition frame in EnemyProjectilePresentationFrameDefinitions.All)
        {
            ushort pointer = (ushort)(bus.ReadCartridgeByte(0x860000 | frame.OperandAddress) |
                bus.ReadCartridgeByte(0x860000 | (ushort)(frame.OperandAddress + 1)) << 8);
            // An unwritten fixture operand has no art. Do not turn a zero selector into
            // an import from WRAM, or silently substitute stock sprites into a fixture.
            programs.Add(frame.Name, pointer == 0 ? [] : EnemySpritemapFiles.ExtractParts(bus, 0x8d, pointer));
        }
        return EnemyProjectileSpritemapCatalog.Load(new MemoryStream(EnemyProjectileSpritemapCatalog.Write(
            new EnemyProjectileSpritemapDocument
            {
                Version = EnemyProjectileSpritemapDefinitions.Version,
                Frames = frames,
                ProgramFrames = programs,
            })));
    }
    private static void VerifyExecutedEnemySelector(ISnesAddressSpace rom,
        RoomEnemySlot slot, HashSet<ushort> executedOperands)
    {
        if (slot.InstructionTimer == 0) return;
        ushort operand = unchecked((ushort)(slot.CurrentInstruction - 2));
        int bank = slot.Definition.Bank << 16;
        ushort native = (ushort)(rom.ReadByte(bank | operand) |
            rom.ReadByte(bank | unchecked((ushort)(operand + 1))) << 8);
        AssertEqual(native, slot.SpritemapPointer,
            $"executed enemy visual ${bank | operand:X6} matches cartridge selector");
        executedOperands.Add(operand);
    }
    // Confirm the visual actually selected by a timed instruction, whether its owner
    // carries an installed operand or a legacy compiled sprite identity.
    private static void VerifyExecutedProjectileFrame(ISnesAddressSpace rom,
        RoomEnemyProjectileSlot projectile, EnemyProjectileSpritemapCatalog artwork,
        HashSet<ushort> executedOperands, ushort? expectedOperand = null)
    {
        if (!projectile.IsActive || projectile.InstructionTimer == 0) return;
        ushort operand = expectedOperand ?? unchecked((ushort)(projectile.InstructionPointer - 2));
        ushort nativePointer = (ushort)(rom.ReadByte(0x860000 | operand) |
            rom.ReadByte(0x860000 | unchecked((ushort)(operand + 1))) << 8);
        executedOperands.Add(operand);
        ReadOnlyMemory<EnemySpritemapPart> parts;
        if (projectile.PresentationOperandAddress != 0)
        {
            AssertEqual(operand, projectile.PresentationOperandAddress,
                "executed projectile frame retains its native presentation operand");
            parts = artwork.GetProgramFrame(projectile.PresentationOperandAddress);
        }
        else
        {
            AssertEqual(nativePointer, projectile.SpritemapPointer,
                "executed projectile frame retains its native compiled selector");
            parts = artwork.Get(projectile.SpritemapPointer);
        }
        var expected = new OamBuffer();
        var actual = new OamBuffer();
        expected.BeginFrame();
        actual.BeginFrame();
        DrawImportedEnemyProjectileSpritemap(rom, expected, nativePointer, 128, 112, 0, true);
        actual.AddEnemySpritemap(parts.Span, 128, 112, 0, 0,
            clipVerticalWrap: true, originYIsOnScreen: true);
        AssertEqual(expected.NextByteOffset, actual.NextByteOffset,
            $"projectile frame $86:{operand:X4} native sprite part count");
        AssertTrue(expected.LowTable.SequenceEqual(actual.LowTable) &&
            expected.HighTable.SequenceEqual(actual.HighTable),
            $"projectile frame $86:{operand:X4} installed composition matches native OAM");
    }
}
