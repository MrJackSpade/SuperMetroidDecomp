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
}
