using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.TestSupport.VerificationOracles;

/// <summary>
/// Import-only packed sprite decoder for cartridge-versus-installed OAM comparisons.
/// Linked into diagnostic executables, never the gameplay Core or shipped hosts.
/// </summary>
internal static class ImportedSpritemapOracle
{
    /// <summary>Decodes packed five-byte parts with native bank wrap into on-screen or off-screen OAM.</summary>
    public static void DrawGeneric(IImportCartridgeSource source, OamBuffer oam,
        int address, Position16<SpritePixels> origin, ushort paletteBits,
        bool originIsOnScreen = true)
    {
        Ensure.NotNull(source);
        Ensure.NotNull(oam);
        _ = SnesAddress.FromBusAddress(address);
        _ = SnesObjAttributeWordConstants.FromPaletteBits(paletteBits);

        // Native pointers wrap within their data bank. The immutable reference side
        // deliberately accepts only import data; live-memory OAM has separate APIs.
        int BankOffset(int offset) => SnesAddressMath.AddWithinBank(address, offset);
        byte Byte(int offset) => source.ReadCartridgeByte(BankOffset(offset));
        ushort Word(int offset) => LittleEndianBytes.Word(Byte(offset), Byte(offset + 1));
        ushort count = Word(0);
        for (int part = 0; part < count && oam.NextByteOffset < OamBuffer.LowTableByteCount; part++)
        {
            int offset = 2 + part * 5;
            var x = new SnesSpritemapXWord(Word(offset));
            byte y = Byte(offset + 2);
            var attributes = new SnesObjAttributeWord(Word(offset + 3)).WithPaletteBits(paletteBits);
            if (originIsOnScreen)
                oam.AddOnScreenSpritePart(x, new SnesSpritemapYByte(y), attributes, origin);
            else
                oam.AddOffScreenSpritePart(x, new SnesSpritemapYByte(y), attributes, origin);
        }
    }

    /// <summary>Decodes only the reference artwork; production accepts typed visual parts.</summary>
    public static void DrawEnemy(IImportCartridgeSource source, OamBuffer oam, NativeEnemySpritemapDraw draw) =>
        oam.AddEnemySpritemap(ReadEnemyParts(source, draw.Bank, draw.Pointer), draw.Origin,
            checked((ushort)draw.PaletteBits), checked((ushort)draw.BaseTileIndex), draw.ClipVerticalWrap,
            draw.OriginYIsOnScreen);

    /// <summary>Uses bank $8D enemy-projectile graphics bits for palette and base tile selection.</summary>
    public static void DrawEnemyProjectile(IImportCartridgeSource source, OamBuffer oam,
        ushort bank8dSpritemapPointer, Position16<SpritePixels> origin,
        ushort graphicsIndex, bool originYIsOnScreen) =>
        DrawEnemy(source, oam, new NativeEnemySpritemapDraw { Bank = 0x8d, Pointer = bank8dSpritemapPointer, Origin = origin, PaletteBits = new SnesObjAttributeWord(graphicsIndex).PaletteBits, BaseTileIndex = unchecked((byte)graphicsIndex), ClipVerticalWrap = true, OriginYIsOnScreen = originYIsOnScreen });

    /// <summary>Reads a packed enemy spritemap's part count and five-byte records from its data bank.</summary>
    private static SpritemapPart[] ReadEnemyParts(IImportCartridgeSource source,
        byte bank, ushort pointer)
    {
        Ensure.NotNull(source);
        byte Byte(int offset) => source.ReadCartridgeByte(
            ((int)new SnesAddress(bank, checked((ushort)(SnesAddressMath.OffsetInBank(pointer + offset))))));
        ushort Word(int offset) => LittleEndianBytes.Word(Byte(offset), Byte(offset + 1));
        var parts = new SpritemapPart[Word(0)];
        for (int part = 0; part < parts.Length; part++)
        {
            int offset = 2 + part * 5;
            parts[part] = new(new SnesSpritemapXWord(Word(offset)), new SnesSpritemapYByte(Byte(offset + 2)),
                new SnesObjAttributeWord(Word(offset + 3)));
        }
        return parts;
    }
}
