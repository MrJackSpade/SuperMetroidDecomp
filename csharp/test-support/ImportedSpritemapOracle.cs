using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;

/// <summary>
/// Import-only packed sprite decoder for cartridge-versus-installed OAM comparisons.
/// Linked into diagnostic executables, never the gameplay Core or shipped hosts.
/// </summary>
internal static class ImportedSpritemapOracle
{
    /// <summary>
    /// Decodes a packed generic spritemap whose count is followed by five-byte sprite records,
    /// applies the requested palette and origin placement, and appends parts until the low OAM
    /// table is full. Record reads wrap within the spritemap's data bank.
    /// </summary>
    /// <param name="source">Import-only cartridge source containing the spritemap bytes.</param>
    /// <param name="oam">Destination OAM buffer for the decoded sprite parts.</param>
    /// <param name="address">Bus address of the packed spritemap count and records.</param>
    /// <param name="originX">Horizontal origin added to each part's relative position.</param>
    /// <param name="originY">Vertical origin added to each part's relative position.</param>
    /// <param name="paletteBits">Object palette selector applied to every decoded part.</param>
    /// <param name="originIsOnScreen">Selects on-screen or off-screen coordinate placement for the origin.</param>
    public static void DrawGeneric(IImportCartridgeSource source, OamBuffer oam,
        int address, ushort originX, ushort originY, ushort paletteBits,
        bool originIsOnScreen = true)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(oam);
        _ = SnesAddress.FromBusAddress(address);
        _ = SnesObjAttributeWord.FromPaletteBits(paletteBits);

        // Native pointers wrap within their data bank. The immutable reference side
        // deliberately accepts only import data; live-memory OAM has separate APIs.
        int BankOffset(int offset) => (address & 0xff0000) | ((address + offset) & 0xffff);
        byte Byte(int offset) => source.ReadCartridgeByte(BankOffset(offset));
        ushort Word(int offset) => (ushort)(Byte(offset) | Byte(offset + 1) << 8);
        ushort count = Word(0);
        for (int part = 0; part < count && oam.NextByteOffset < OamBuffer.LowTableByteCount; part++)
        {
            int offset = 2 + part * 5;
            var x = new SnesSpritemapXWord(Word(offset));
            byte y = Byte(offset + 2);
            var attributes = new SnesObjAttributeWord(Word(offset + 3)).WithPaletteBits(paletteBits);
            if (originIsOnScreen)
                oam.AddOnScreenSpritePart(x, y, attributes, originX, originY);
            else
                oam.AddOffScreenSpritePart(x, y, attributes, originX, originY);
        }
    }

    /// <summary>Decodes only the reference artwork; production accepts typed visual parts.</summary>
    public static void DrawEnemy(IImportCartridgeSource source, OamBuffer oam,
        byte bank, ushort spritemapPointer, ushort originX, ushort originY,
        ushort paletteBits, ushort baseTileIndex,
        bool clipVerticalWrap = false, bool originYIsOnScreen = true) =>
        oam.AddEnemySpritemap(ReadEnemyParts(source, bank, spritemapPointer), originX, originY,
            paletteBits, baseTileIndex, clipVerticalWrap, originYIsOnScreen);

    /// <summary>
    /// Decodes a projectile spritemap from bank $8D and draws it with vertical-wrap clipping;
    /// the graphics index supplies both the object palette bits and the base tile index.
    /// </summary>
    /// <param name="source">Import-only cartridge source containing the bank-$8D spritemap.</param>
    /// <param name="oam">Destination OAM buffer for the projectile's sprite parts.</param>
    /// <param name="bank8dSpritemapPointer">Bank-local pointer to the projectile spritemap.</param>
    /// <param name="originX">Horizontal origin added to each part's relative position.</param>
    /// <param name="originY">Vertical origin added to each part's relative position.</param>
    /// <param name="graphicsIndex">Projectile graphics selector supplying palette bits and the base tile index.</param>
    /// <param name="originYIsOnScreen">Whether the origin uses on-screen vertical coordinates.</param>
    public static void DrawEnemyProjectile(IImportCartridgeSource source, OamBuffer oam,
        ushort bank8dSpritemapPointer, ushort originX, ushort originY,
        ushort graphicsIndex, bool originYIsOnScreen) =>
        DrawEnemy(source, oam, 0x8d, bank8dSpritemapPointer, originX, originY,
            new SnesObjAttributeWord(graphicsIndex).PaletteBits, unchecked((byte)graphicsIndex),
            clipVerticalWrap: true, originYIsOnScreen);

    /// <summary>
    /// Reads the count-prefixed, five-byte enemy sprite records at a bank-local pointer and
    /// returns their relative positions and object attributes without drawing them.
    /// </summary>
    /// <param name="source">Import-only cartridge source containing the enemy spritemap.</param>
    /// <param name="bank">Bank containing the spritemap pointer's data.</param>
    /// <param name="pointer">Bank-local address of the count-prefixed spritemap.</param>
    /// <returns>The decoded records in their original spritemap order.</returns>
    private static EnemySpritemapPart[] ReadEnemyParts(IImportCartridgeSource source,
        byte bank, ushort pointer)
    {
        ArgumentNullException.ThrowIfNull(source);
        byte Byte(int offset) => source.ReadCartridgeByte(
            (bank << 16) | ((pointer + offset) & 0xffff));
        ushort Word(int offset) => (ushort)(Byte(offset) | Byte(offset + 1) << 8);
        var parts = new EnemySpritemapPart[Word(0)];
        for (int part = 0; part < parts.Length; part++)
        {
            int offset = 2 + part * 5;
            parts[part] = new(new SnesSpritemapXWord(Word(offset)), Byte(offset + 2),
                new SnesObjAttributeWord(Word(offset + 3)));
        }
        return parts;
    }
}
