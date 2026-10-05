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

    private static void DrawImportedEnemyProjectileSpritemap(ISnesAddressSpace bus, OamBuffer oam,
        ushort bank8dSpritemapPointer, ushort originX, ushort originY,
        ushort graphicsIndex, bool originYIsOnScreen) =>
        ImportedSpritemapOracle.DrawEnemyProjectile(CartridgeImportSource.Require(bus), oam,
            bank8dSpritemapPointer, originX, originY, graphicsIndex, originYIsOnScreen);

    /// <summary>Resolves a fixture's selected frame without restoring a visual-pointer reader in Core.</summary>
    private static ushort ReadImportedMotherBrainVisualSelector(TestAddressSpace bus,
        MotherBrainEnemyProjectileSlot slot) => slot.PresentationOperandAddress == 0
            ? EnemyProjectileSpritemapDefinitions.BlankSpritemap
            : (ushort)(bus.ReadCartridgeByte(0x860000 | slot.PresentationOperandAddress) |
                bus.ReadCartridgeByte(0x860000 | (ushort)(slot.PresentationOperandAddress + 1)) << 8);

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
    // Confirm the visual actually selected by a timed instruction, whether its owner
    // carries an installed operand or a legacy compiled sprite identity.
    private static void VerifyExecutedProjectileFrame(ISnesAddressSpace rom,
        RoomEnemyProjectileSlot projectile, EnemyProjectileSpritemapCatalog artwork,
        HashSet<ushort> executedOperands)
    {
        if (!projectile.IsActive || projectile.InstructionTimer == 0) return;
        ushort operand = unchecked((ushort)(projectile.InstructionPointer - 2));
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
