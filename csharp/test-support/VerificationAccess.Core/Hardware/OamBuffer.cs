using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Hardware;

/// <summary>
/// SNES Object Attribute Memory staging buffer used by Super Metroid at WRAM
/// <c>$0370-$058F</c>, including its awkward split low/high tables.
/// </summary>
/// <remarks>
/// The first 512 bytes contain 128 four-byte sprite records. The final 32 bytes contain
/// two extra bits per sprite: bit 0 is X coordinate bit 8 and bit 1 selects the large OBJ
/// size. Four sprites therefore share each high-table byte. NMI uploads all 544 bytes to
/// PPU OAM in one DMA at <c>$80:933A</c>.
/// </remarks>
public sealed class OamBuffer
{
    /// <summary>Number of hardware OBJ records representable by one OAM image.</summary>
    public const int SpriteCount = 128;
    /// <summary>Byte length of the 128 four-byte coordinate and attribute records.</summary>
    public const int LowTableByteCount = SnesPpuLayoutConstants.OamLowTableByteCount;

    /// <summary>The values of one byte: a coordinate sum at or past it carried out of the low byte.</summary>
    private const int ByteRange = 256;
    /// <summary>The X at which $81:8907 parks a clipped Samus part, beyond the visible screen.</summary>
    private const ushort ParkedX = 384;
    /// <summary>The Y that hides an unused or wrapped sprite below the visible screen.</summary>
    private const byte HiddenY = 240;

    /// <summary>Each low-OAM sprite stores X, Y, tile, and attributes in four bytes.</summary>
    internal const int LowRecordByteCount = 4;
    /// <summary>Four sprites share one high-OAM byte of X-high and size pairs.</summary>
    private const int HighPairsPerByte = 4;
    /// <summary>Mutable low-OAM bytes staged before the next hardware upload.</summary>
    internal readonly byte[] LowTableBytes = new byte[PpuMemoryLayoutConstants.OamLow.Count * LowRecordByteCount];
    /// <summary>Mutable two-bit sprite pairs staged after the low table.</summary>
    internal readonly byte[] HighTableBytes = new byte[PpuMemoryLayoutConstants.OamHigh.Count / HighPairsPerByte];

    /// <summary>
    /// Byte offset of the next low-table record, equivalent to WRAM <c>$0590</c>. It moves
    /// in four-byte increments and reaches <c>$0200</c> when all 128 sprites are occupied.
    /// </summary>
    public int NextByteOffset { get; internal set; }

    /// <summary>
    /// Number of sprites written before the most recent <see cref="FinalizeFrame"/> reset.
    /// This host-side diagnostic has no separate original WRAM field.
    /// </summary>
    public int LastFinalizedSpriteCount { get; internal set; }

    /// <summary>
    /// Starts the game-logic OAM construction phase. This ports the main-loop calls to
    /// <c>ClearHighOAM</c> and <c>STZ OAMStack</c> at <c>$82:8953-$82:8957</c>.
    /// </summary>
    public void BeginFrame()
    {
        Array.Clear(HighTableBytes);
        NextByteOffset = 0;
    }

    /// <summary>Draws a compiled visual part with the same $81:879F clipping/packing as cartridge spritemaps.</summary>
    public void AddOnScreenSpritePart(SnesSpritemapXWord xOffset, SnesSpritemapYByte yOffset,
        SnesObjAttributeWord attributes, Position16<SpritePixels> origin)
        => AppendGenericSprite(xOffset, yOffset, attributes, origin, originIsOnScreen: true);

    /// <summary>Draws a compiled visual part with $81:8853's reversed vertical-wrap branch.</summary>
    public void AddOffScreenSpritePart(SnesSpritemapXWord xOffset, SnesSpritemapYByte yOffset,
        SnesObjAttributeWord attributes, Position16<SpritePixels> origin)
        => AppendGenericSprite(xOffset, yOffset, attributes, origin, originIsOnScreen: false);

    /// <summary>Applies the generic spritemap vertical-wrap clipping and packs one hardware OBJ record.</summary>
    private void AppendGenericSprite(SnesSpritemapXWord encodedXOffset, SnesSpritemapYByte encodedYOffset,
        SnesObjAttributeWord finalAttributes, Position16<SpritePixels> origin, bool originIsOnScreen)
    {
        if (NextByteOffset >= LowTableByteCount) return; // Native stops at the hardware OAM capacity.

        // Only the low nine X bits reach hardware. The independent size flag
        // survives even when vertical clipping parks the sprite off screen.
        ushort calculatedX = unchecked((ushort)(origin.X + encodedXOffset.Raw));
        int calculatedY = (byte)origin.Y + encodedYOffset.Raw;
        bool hideForVerticalWrap = ShouldHideForOnScreenOrigin(calculatedY, encodedYOffset.IsNegative);
        if (!originIsOnScreen)
            hideForVerticalWrap = !hideForVerticalWrap;

        if (hideForVerticalWrap)
        {
            // $81:8907 parks a clipped sprite beyond the visible screen.
            calculatedX = ParkedX;
            calculatedY = ScreenGeometry.ScreenHeightPixels;
        }

        int spriteIndex = NextByteOffset >> 2;
        int lowOffset = NextByteOffset;
        LowTableBytes[lowOffset] = (byte)calculatedX;
        LowTableBytes[lowOffset + 1] = (byte)calculatedY;
        WriteAttributes(lowOffset, finalAttributes);
        SetHighTablePair(spriteIndex, SnesOamHighTablePair.FromSprite(calculatedX, encodedXOffset.IsLarge));
        NextByteOffset += 4;
    }

    /// <summary>
    /// Appends one already-packed small OBJ record. Bank $94's grapple renderer writes
    /// these four bytes directly instead of routing through a spritemap loader.
    /// </summary>
    /// <remarks>
    /// This deliberately accepts the complete attribute word: tile, palette, priority,
    /// and flips are calculated by the native caller. X remains a nine-bit hardware value;
    /// Y wraps to its low byte exactly like a direct store to OAMLow.
    /// </remarks>
    public void AddRawSmallSprite(Position16<SpritePixels> position, SnesObjAttributeWord attributes)
    {
        int spriteIndex = NextByteOffset >> 2;
        int lowOffset = NextByteOffset;
        LowTableBytes[lowOffset] = unchecked((byte)position.X);
        LowTableBytes[lowOffset + 1] = unchecked((byte)position.Y);
        WriteAttributes(lowOffset, attributes);
        SetHighTablePair(spriteIndex, SnesOamHighTablePair.FromSprite(position.X, isLarge: false));
        NextByteOffset = (NextByteOffset + 4) % LowTableByteCount;
    }

    /// <summary>
    /// Ports <c>$80:896E</c>: move every unused sprite to Y=<c>$F0</c> and reset the OAM
    /// stack pointer for the following construction pass.
    /// </summary>
    public void FinalizeFrame()
    {
        int usedSpriteCount = Math.Min(NextByteOffset >> 2, SpriteCount);
        LastFinalizedSpriteCount = usedSpriteCount;

        // The assembly uses a spectacular 128-store unrolled jump table. A loop expresses
        // the same writes without hiding the important rule: only each record's Y byte is
        // changed, leaving its other stale bytes harmless while it is parked off-screen.
        for (int spriteIndex = usedSpriteCount; spriteIndex < SpriteCount; spriteIndex++)
            LowTableBytes[spriteIndex * 4 + 1] = HiddenY;

        NextByteOffset = 0;
    }

    /// <summary>Decodes one sprite from the split low/high tables for debugger inspection.</summary>
    public OamEntry GetEntry(int spriteIndex)
    {
        if ((uint)spriteIndex >= SpriteCount)
            throw new ArgumentOutOfRangeException(nameof(spriteIndex));

        int lowOffset = spriteIndex * 4;
        int highShift = (spriteIndex & 3) * 2;
        var highPair = new SnesOamHighTablePair(unchecked((byte)(
            (HighTableBytes[spriteIndex >> 2] >> highShift) & 3)));
        SnesObjAttributeWord attributes =
            LittleEndianBytes.Word(LowTableBytes[lowOffset + 2], LowTableBytes[lowOffset + 3]);

        return new OamEntry(
            Position: new Position16<SpritePixels>(
                checked((ushort)(LowTableBytes[lowOffset] + (highPair.XHigh ? ByteRange : 0))),
                LowTableBytes[lowOffset + 1]),
            TileNumber: attributes.TileNumber,
            Palette: attributes.PaletteIndex,
            Priority: attributes.Priority,
            FlipX: attributes.FlipHorizontally,
            FlipY: attributes.FlipVertically,
            IsLarge: highPair.IsLarge);
    }

    /// <summary>Replaces one sprite's two-bit high-OAM pair without disturbing its three neighbors.</summary>
    internal void SetHighTablePair(int spriteIndex, SnesOamHighTablePair pair)
    {
        int byteIndex = spriteIndex >> 2;
        int shift = (spriteIndex & 3) * 2;
        int pairMask = 3 << shift;
        int pairValue = pair.Raw << shift;

        // The ROM only ORs these bits because ClearHighOAM ran earlier. Clear-and-replace
        // is equivalent for a fresh frame and prevents stale debugger edits from leaking.
        HighTableBytes[byteIndex] = (byte)((HighTableBytes[byteIndex] & ~pairMask) | pairValue);
    }

    /// <summary>Stores one typed packed attribute word in the low-OAM record.</summary>
    internal void WriteAttributes(int lowOffset, SnesObjAttributeWord attributes)
    {
        LowTableBytes[lowOffset + 2] = unchecked((byte)attributes.Raw);
        LowTableBytes[lowOffset + 3] = LittleEndianBytes.HighByte(attributes.Raw);
    }

    /// <summary>Implements the native carry and sign test for hiding vertically wrapped generic parts.</summary>
    private static bool ShouldHideForOnScreenOrigin(int unsignedYSum, bool offsetIsNegative)
    {
        byte wrappedY = (byte)unsignedYSum;
        if (!offsetIsNegative)
            return unsignedYSum >= ByteRange || wrappedY >= ScreenGeometry.ScreenHeightPixels;

        // This is the deliberately literal form of $81:8800-$81:881C. Negative offsets
        // may legitimately wrap into $E0-$FF to appear partly above the screen.
        return (unsignedYSum & ByteRange) != 0 ? wrappedY >= ScreenGeometry.ScreenHeightPixels : wrappedY < ScreenGeometry.ScreenHeightPixels;
    }
}
