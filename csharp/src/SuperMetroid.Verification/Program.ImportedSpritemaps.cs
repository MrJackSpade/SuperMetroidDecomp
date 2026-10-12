using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rom;
using static AssertionsVerification;

/// <summary>Represents imported spritemaps verification.</summary>
internal static class ImportedSpritemapsVerification
{
    // These adapters require an import source at the diagnostic boundary. Core has no
    // address-based artwork loader, including when tests use constructed cartridge data.
    /// <summary>Draws imported enemy spritemap.</summary>
    internal static void DrawImportedEnemySpritemap(ISnesAddressSpace bus, OamBuffer oam, EnemySpritemapDraw draw) =>
        ImportedSpritemapOracle.DrawEnemy(CartridgeImportSource.Require(bus), oam, draw);

    /// <summary>
    /// The cartridge spritemap an enemy projectile draws this frame. Installed-presentation frames
    /// keep a blank spritemap and name the image by instruction operand; the cartridge's spritemap
    /// for such a frame is the word stored at that operand in bank $86.
    /// </summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the bank-$86 spritemap word an installed-presentation enemy projectile operand names")]
    internal static ushort NativeEnemyProjectileSpritemap(ISnesAddressSpace bus, RoomEnemyProjectileSlot projectile) =>
        projectile.PresentationOperandAddress != 0
            ? SuperMetroid.Core.Rom.RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                0x860000 | projectile.PresentationOperandAddress)
            : projectile.SpritemapPointer;

    /// <summary>Draws imported enemy projectile spritemap.</summary>
    internal static void DrawImportedEnemyProjectileSpritemap(ISnesAddressSpace bus, OamBuffer oam,
        ushort bank8dSpritemapPointer, Position16<OamPixels> origin,
        ushort graphicsIndex, bool originYIsOnScreen) =>
        ImportedSpritemapOracle.DrawEnemyProjectile(CartridgeImportSource.Require(bus), oam,
            bank8dSpritemapPointer, origin, graphicsIndex, originYIsOnScreen);

    /// <summary>Imports only the sprite records authored in a sparse projectile fixture.</summary>
    [CpuBusBoundary("exact bus test: imports the bank-$86 projectile frame operands a sparse fixture seeds")]
    private static EnemyProjectileSpritemapCatalog ImportFixtureProjectileSprites(TestAddressSpace bus)
    {
        var frames = EnemyProjectileSpritemapDefinitions.Frames.ToDictionary(
            frame => frame.Name, frame => EnemySpritemapFiles.ExtractParts(bus, 0x8d, frame.Pointer));
        var programs = new Dictionary<string, SpriteVisualPart[]>();
        foreach (EnemyProjectilePresentationFrameDefinition frame in EnemyProjectilePresentationFrameDefinitions.All)
        {
            ushort pointer = bus.ReadCartridgeWordFixedBank(0x860000 | frame.OperandAddress);
            // An unwritten fixture operand has no art. Do not turn a zero selector into
            // an import from WRAM, or silently substitute stock sprites into a fixture.
            programs.Add(frame.Name, pointer == 0 ? [] : EnemySpritemapFiles.ExtractParts(bus, 0x8d, pointer));
        }
        return EnemyProjectileSpritemapCatalog.Load(new MemoryStream(EnemyProjectileSpritemapCatalog.Write(
            new EnemyProjectileSpritemapDocument
            {
                Version = (int)EnemyProjectileSpritemapVersion.Current,
                Frames = frames,
                ProgramFrames = programs,
            })));
    }
    /// <summary>Verifies executed enemy selector.</summary>
    internal static void VerifyExecutedEnemySelector(ISnesAddressSpace rom,
        RoomEnemySlot slot, HashSet<ushort> executedOperands)
    {
        if (slot.InstructionTimer == 0) return;
        ushort operand = unchecked((ushort)(slot.CurrentInstruction - 2));
        int bank = (int)new SnesAddress(slot.Definition.Bank, 0);
        ushort native = (ushort)(SnesLittleEndian.Word(ExecutedEnemySelectorRomByte(rom, bank | operand), ExecutedEnemySelectorRomByte(rom, bank | unchecked((ushort)(operand + 1)))));
        AssertEqual(native, slot.SpritemapPointer,
            $"executed enemy visual ${bank | operand:X6} matches cartridge selector");
        executedOperands.Add(operand);
    }

    /// <summary>Executed enemy selector rom byte.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for executed enemy selector")]
    private static byte ExecutedEnemySelectorRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    // Confirm the visual actually selected by a timed instruction, whether its owner
    // carries an installed operand or a legacy compiled sprite identity.
    /// <summary>Verifies executed projectile frame.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyExecutedProjectileFrame(ISnesAddressSpace rom,
        RoomEnemyProjectileSlot projectile, EnemyProjectileSpritemapCatalog artwork,
        HashSet<ushort> executedOperands, ushort? expectedOperand = null)
    {
        if (!projectile.IsActive || projectile.InstructionTimer == 0) return;
        ushort operand = expectedOperand ?? unchecked((ushort)(projectile.InstructionPointer - 2));
        ushort nativePointer = (ushort)(SnesLittleEndian.Word(ExecutedProjectileFrameRomByte(rom, 0x860000 | operand), ExecutedProjectileFrameRomByte(rom, 0x860000 | unchecked((ushort)(operand + 1)))));
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
        DrawImportedEnemyProjectileSpritemap(rom, expected, nativePointer, new(128, 112), 0, true);
        actual.AddEnemySpritemap(parts.Span, new Position16<OamPixels>(128, 112), 0, 0,
            clipVerticalWrap: true, originYIsOnScreen: true);
        AssertEqual(expected.NextByteOffset, actual.NextByteOffset,
            $"projectile frame $86:{operand:X4} native sprite part count");
        AssertTrue(expected.ImageEquals(actual),
            $"projectile frame $86:{operand:X4} installed composition matches native OAM");
    }

    /// <summary>Executed projectile frame rom byte.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for executed projectile frame")]
    private static byte ExecutedProjectileFrameRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
