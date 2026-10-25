using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.VerificationOracles;
namespace SuperMetroid.Verification.Hardware;
/// <summary>Uses cartridge spritemaps as a diagnostic oracle for installed enemy artwork.</summary>
internal static class ImportedSpritemapsVerification
{
    /// <summary>Draws a native enemy map through the import boundary, including constructed cartridge fixtures.</summary>
    internal static void DrawImportedEnemySpritemap(ISnesAddressSpace bus, OamBuffer oam, NativeEnemySpritemapDraw draw) =>
        ImportedSpritemapOracle.DrawEnemy(CartridgeImportSource.Require(bus), oam, draw);

    /// <summary>
    /// The cartridge spritemap an enemy projectile draws this frame. Installed-presentation frames
    /// keep a blank spritemap and name the image by instruction operand; the cartridge's spritemap
    /// for such a frame is the word stored at that operand in bank $86.
    /// </summary>
    internal static ushort NativeEnemyProjectileSpritemap(ISnesAddressSpace bus, RoomEnemyProjectileSlot projectile) =>
        projectile.PresentationOperandAddress != 0
            ? SuperMetroid.AssetExtraction.Cartridge.RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                0x860000 | projectile.PresentationOperandAddress)
            : projectile.SpritemapPointer;

    /// <summary>Draws a bank-$8D projectile map with its native graphics selection and vertical-wrap clipping.</summary>
    internal static void DrawImportedEnemyProjectileSpritemap(ISnesAddressSpace bus, OamBuffer oam,
        ushort bank8dSpritemapPointer, Position16<SpritePixels> origin,
        ushort graphicsIndex, bool originYIsOnScreen) =>
        ImportedSpritemapOracle.DrawEnemyProjectile(CartridgeImportSource.Require(bus), oam,
            bank8dSpritemapPointer, origin, graphicsIndex, originYIsOnScreen);

    /// <summary>Checks a timed enemy instruction's executed sprite operand against its retail word.</summary>
    internal static void VerifyExecutedEnemySelector(ISnesAddressSpace rom,
        RoomEnemySlot slot, HashSet<ushort> executedOperands)
    {
        if (slot.InstructionTimer.IsZero) return;
        ushort operand = unchecked((ushort)(slot.CurrentInstruction - 2));
        int bank = (int)new SnesAddress(slot.NativeHeader.Bank, 0);
        ushort native = SnesLittleEndian.Word(ExecutedEnemySelectorRomByte(rom, bank | operand), ExecutedEnemySelectorRomByte(rom, bank | unchecked((ushort)(operand + 1))));
        AssertEqual(native, slot.SpritemapPointer,
            $"executed enemy visual ${bank | operand:X6} matches cartridge selector");
        executedOperands.Add(operand);
    }

    /// <summary>Reads one native enemy frame-selector byte for the executed-instruction check.</summary>
    private static byte ExecutedEnemySelectorRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Compares the executed projectile frame's installed OAM with its native selector and spritemap.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyExecutedProjectileFrame(ISnesAddressSpace rom,
        RoomEnemyProjectileSlot projectile, EnemyProjectileSpritemapCatalog artwork,
        HashSet<ushort> executedOperands, ushort? expectedOperand = null)
    {
        if (!projectile.IsActive || projectile.InstructionTimer.IsZero) return;
        ushort operand = expectedOperand ?? throw new InvalidOperationException("An executed typed frame names its native operand.");
        ushort nativePointer = SnesLittleEndian.Word(ExecutedProjectileFrameRomByte(rom, 0x860000 | operand), ExecutedProjectileFrameRomByte(rom, 0x860000 | unchecked((ushort)(operand + 1))));
        executedOperands.Add(operand);
        ReadOnlyMemory<SpritemapPart> parts;
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
        actual.AddEnemySpritemap(parts.Span, new Position16<SpritePixels>(128, 112), 0, 0,
            clipVerticalWrap: true, originYIsOnScreen: true);
        AssertEqual(expected.NextByteOffset, actual.NextByteOffset,
            $"projectile frame $86:{operand:X4} native sprite part count");
        AssertTrue(expected.ImageEquals(actual),
            $"projectile frame $86:{operand:X4} installed composition matches native OAM");
    }

    /// <summary>Reads a native bank-$86 projectile frame-selector byte for the OAM comparison.</summary>
    private static byte ExecutedProjectileFrameRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
