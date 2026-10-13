using SuperMetroid.Core.Assets;
using static SuperMetroid.Core.Hardware.SnesAddressMath;

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
[PpuLayoutBoundary("backend: OAM low/high-table construction and packing")]
public sealed class OamBuffer
{
    /// <summary>Number of hardware OBJ records representable by one OAM image.</summary>
    public const int SpriteCount = SnesPpuLayout.OamSpriteCount;
    /// <summary>Byte length of the 128 four-byte coordinate and attribute records.</summary>
    [PpuLayoutAccess]
    public const int LowTableByteCount = SnesPpuLayout.OamLowTableByteCount;
    /// <summary>Total 544-byte DMA payload containing the low table followed by the high table.</summary>
    [PpuLayoutAccess]
    public const int UploadByteCount = SnesPpuLayout.OamUploadByteCount;

    /// <summary>Each low-OAM sprite stores X, Y, tile, and attributes in four bytes.</summary>
    private const int LowRecordByteCount = 4;
    /// <summary>Four sprites share one high-OAM byte of X-high and size pairs.</summary>
    private const int HighPairsPerByte = 4;
    /// <summary>Mutable low-OAM bytes staged before the next hardware upload.</summary>
    internal readonly byte[] _lowTable = new byte[PpuMemoryLayout.OamLow.Count * LowRecordByteCount];
    /// <summary>Mutable two-bit sprite pairs staged after the low table.</summary>
    internal readonly byte[] _highTable = new byte[PpuMemoryLayout.OamHigh.Count / HighPairsPerByte];

    /// <summary>Raw 512-byte low OAM table used by PPU DMA.</summary>
    [PpuLayoutAccess]
    public ReadOnlySpan<byte> LowTable => _lowTable;

    /// <summary>Raw 32-byte high OAM table used by PPU DMA.</summary>
    [PpuLayoutAccess]
    public ReadOnlySpan<byte> HighTable => _highTable;

    /// <summary>
    /// Byte offset of the next low-table record, equivalent to WRAM <c>$0590</c>. It moves
    /// in four-byte increments and reaches <c>$0200</c> when all 128 sprites are occupied.
    /// </summary>
    [PpuLayoutAccess]
    public int NextByteOffset { get; private set; }

    /// <summary>
    /// Number of sprites written before the most recent <see cref="FinalizeFrame"/> reset.
    /// This host-side diagnostic has no separate original WRAM field.
    /// </summary>
    public int LastFinalizedSpriteCount { get; private set; }

    /// <summary>
    /// Starts the game-logic OAM construction phase. This ports the main-loop calls to
    /// <c>ClearHighOAM</c> and <c>STZ OAMStack</c> at <c>$82:8953-$82:8957</c>.
    /// </summary>
    public void BeginFrame()
    {
        Array.Clear(_highTable);
        NextByteOffset = 0;
    }

    /// <summary>Draws a compiled visual part with the same $81:879F clipping/packing as cartridge spritemaps.</summary>
    public void AddOnScreenSpritePart(SnesSpritemapXWord xOffset, byte yOffset,
        SnesObjAttributeWord attributes, Position16<OamPixels> origin)
        => AppendGenericSprite(xOffset, yOffset, attributes, origin, originIsOnScreen: true);

    /// <summary>Draws a compiled visual part with $81:8853's reversed vertical-wrap branch.</summary>
    public void AddOffScreenSpritePart(SnesSpritemapXWord xOffset, byte yOffset,
        SnesObjAttributeWord attributes, Position16<OamPixels> origin)
        => AppendGenericSprite(xOffset, yOffset, attributes, origin, originIsOnScreen: false);

    /// <summary>Applies the generic spritemap vertical-wrap clipping and packs one hardware OBJ record.</summary>
    private void AppendGenericSprite(SnesSpritemapXWord encodedXOffset, byte encodedYOffset,
        SnesObjAttributeWord finalAttributes, Position16<OamPixels> origin, bool originIsOnScreen)
    {
        if (NextByteOffset >= LowTableByteCount) return; // Native stops at the hardware OAM capacity.

        // Only the low nine X bits reach hardware. The independent size flag
        // survives even when vertical clipping parks the sprite off screen.
        ushort calculatedX = unchecked((ushort)(origin.X + encodedXOffset.Raw));
        int calculatedY = (byte)origin.Y + encodedYOffset;
        bool yOffsetIsNegative = (encodedYOffset & 0x80) != 0;
        bool hideForVerticalWrap = ShouldHideForOnScreenOrigin(calculatedY, yOffsetIsNegative);
        if (!originIsOnScreen)
            hideForVerticalWrap = !hideForVerticalWrap;

        if (hideForVerticalWrap)
        {
            // $81:8907 parks a clipped sprite beyond the visible screen.
            calculatedX = 0x0180;
            calculatedY = 0x00e0;
        }

        int spriteIndex = NextByteOffset >> 2;
        int lowOffset = NextByteOffset;
        _lowTable[lowOffset] = (byte)calculatedX;
        _lowTable[lowOffset + 1] = (byte)calculatedY;
        WriteAttributes(lowOffset, finalAttributes);
        SetHighTablePair(spriteIndex, SnesOamHighTablePair.FromSprite(calculatedX, encodedXOffset.IsLarge));
        NextByteOffset += 4;
    }

    /// <summary>
    /// Ports <c>AddSamusSpritemapToOAM</c> at <c>$81:89AE</c>.
    /// </summary>
    /// <remarks>
    /// This looks tantalizingly similar to <see cref="AddOnScreenSpritePart"/>, but the
    /// differences are part of the game data contract. The input is an index into the
    /// bank-$92 pointer table at <c>$92:808D</c>, not a direct address. More importantly,
    /// Samus entries keep their ROM-authored palette bits and do not use the generic
    /// routine's vertical-wrap hiding rule. Keeping a distinct method prevents a future
    /// cleanup from quietly changing the native draw behavior.
    /// </remarks>
    /// <param name="memory">Mutable memory for the native zero-pointer WRAM case.</param>
    /// <param name="spritemapIndex">Word index into <c>$92:808D</c>.</param>
    /// <param name="origin">Screen-space origin produced by bank $90.</param>
    /// <param name="artwork">Required installed Samus compositions indexed by the native pointer-table ordinal; a zero-pointer catalog entry selects the mutable WRAM spritemap case rather than an empty image.</param>
    public void AddSamusSpritemap(
        ISnesMutableMemory memory,
        ushort spritemapIndex,
        Position16<OamPixels> origin,
        SamusSpritemapArtworkCatalog artwork)
    {
        Ensure.NotNull(memory);
        Ensure.NotNull(artwork);

        if (artwork.TryGet(spritemapIndex, out SamusSpritemapDefinition? definition))
        {
            foreach (SamusSpritePart part in definition.Parts)
                AddSamusSpritePart(new SnesSpritemapXWord(part.X), part.Y,
                    part.Attributes, origin);
            return;
        }

        // A zero entry in the installed pointer table is not an empty picture: the
        // native bank-local dereference lands in the WRAM mirror at $92:0000.
        // Read that mutable state explicitly. No cartridge address is reachable here.
        foreach (Game.SamusZeroSpritemapEntry entry in Game.SamusOverreadMemory.ZeroPointerSpritemap(memory))
            AddSamusSpritePart(entry.EncodedX, entry.EncodedY, entry.Attributes, origin);
    }

    /// <summary>Emits one already-decoded Samus part with native OAM wrapping and authored attributes.</summary>
    public void AddSamusSpritePart(SnesSpritemapXWord encodedXOffset, byte encodedYOffset,
        ushort attributes, Position16<OamPixels> origin)
    {
        // The 65C816 adds the complete encoded X word, including size and old art-tool
        // bits. Only X bit 8 reaches high OAM; bit 15 independently chooses OBJ size.
        ushort calculatedX = unchecked((ushort)(origin.X + encodedXOffset.Raw));
        byte calculatedY = unchecked((byte)(origin.Y + encodedYOffset));
        int spriteIndex = NextByteOffset >> 2;
        int lowOffset = NextByteOffset;
        _lowTable[lowOffset] = (byte)calculatedX;
        _lowTable[lowOffset + 1] = calculatedY;
        // Unlike generic spritemaps, Samus parts retain their authored palette/priority.
        WriteAttributes(lowOffset, new SnesObjAttributeWord(attributes));
        SetHighTablePair(spriteIndex,
            SnesOamHighTablePair.FromSprite(calculatedX, encodedXOffset.IsLarge));
        // Native OAMStack is a nine-bit byte offset and wraps after $1FC.
        NextByteOffset = (NextByteOffset + 4) & 0x01ff;
    }

    /// <summary>
    /// Emits an authored projectile visual part using the $81:8A4B/$81:8A2B tail:
    /// preserve source attributes, wrap Y and wrap the write position after 128 OBJs.
    /// Unlike menu parts, it neither parks vertically wrapped parts nor stops at capacity.
    /// The caller owns whole-projectile viewport admission.
    /// </summary>
    public void AddProjectileSpritePart(SnesSpritemapXWord xOffset, byte yOffset,
        SnesObjAttributeWord attributes, Position16<OamPixels> origin)
    {
        ushort x = unchecked((ushort)(origin.X + xOffset.Raw));
        byte y = unchecked((byte)(origin.Y + yOffset));
        int spriteIndex = NextByteOffset >> 2;
        int lowOffset = NextByteOffset;
        _lowTable[lowOffset] = (byte)x;
        _lowTable[lowOffset + 1] = y;
        WriteAttributes(lowOffset, attributes);
        SetHighTablePair(spriteIndex, SnesOamHighTablePair.FromSprite(x, xOffset.IsLarge));
        NextByteOffset = (NextByteOffset + 4) & 0x01ff;
    }


    /// <summary>Draws an installed enemy frame without reading visual records from ROM.</summary>
    public void AddEnemySpritemap(
        ReadOnlySpan<EnemySpritemapPart> parts,
        Position16<OamPixels> origin,
        ushort paletteBits,
        ushort baseTileIndex,
        bool clipVerticalWrap = false,
        bool originYIsOnScreen = true)
    {
        _ = SnesObjAttributeWord.FromPaletteBits(paletteBits);
        foreach (EnemySpritemapPart part in parts)
            AppendEnemySpritemapPart(part, origin, paletteBits,
                baseTileIndex, clipVerticalWrap, originYIsOnScreen);
    }

    /// <summary>Appends an immutable calculated enemy composition in its original drawing order.</summary>
    public void AddEnemySpritemap(EnemySpritemapParts parts,
        Position16<OamPixels> origin, ushort paletteBits, ushort baseTileIndex,
        bool clipVerticalWrap = false, bool originYIsOnScreen = true)
    {
        Ensure.NotNull(parts);
        _ = SnesObjAttributeWord.FromPaletteBits(paletteBits);
        for (int index = 0; index < parts.Count; index++)
            AppendEnemySpritemapPart(parts[index], origin, paletteBits,
                baseTileIndex, clipVerticalWrap, originYIsOnScreen);
    }

    /// <summary>Packs an enemy part after adding its tile base, palette, and optional vertical clipping.</summary>
    private void AppendEnemySpritemapPart(EnemySpritemapPart part,
        Position16<OamPixels> origin, ushort paletteBits, ushort baseTileIndex,
        bool clipVerticalWrap, bool originYIsOnScreen)
    {
        SnesSpritemapXWord encodedXOffset = part.X;
        byte encodedYOffset = part.Y;
        ushort calculatedX = unchecked((ushort)(origin.X + encodedXOffset.Raw));
        int unsignedYSum = unchecked((byte)origin.Y) + encodedYOffset;
        byte calculatedY;
        if (!clipVerticalWrap)
        {
            calculatedY = unchecked((byte)unsignedYSum);
        }
        else
        {
            // Extended enemy spritemaps route each component through $81:8B22 or
            // $81:8B96. Their complementary carry/sign tests prevent a piece on one
            // side of the vertical boundary from wrapping onto the other side.
            bool yOffsetIsNegative = (encodedYOffset & 0x80) != 0;
            bool hideForVerticalWrap = originYIsOnScreen
                ? yOffsetIsNegative ? unsignedYSum < 0x100 : unsignedYSum >= 0x100
                : yOffsetIsNegative ? unsignedYSum >= 0x100 : unsignedYSum < 0x100;
            calculatedY = hideForVerticalWrap ? (byte)0xf0 : unchecked((byte)unsignedYSum);
        }
        SnesObjAttributeWord finalAttributes = part.Attributes
            .AddPackedTileBase(baseTileIndex)
            .Or(SnesObjAttributeWord.FromPaletteBits(paletteBits));

        int spriteIndex = NextByteOffset >> 2;
        int lowOffset = NextByteOffset;
        _lowTable[lowOffset] = unchecked((byte)calculatedX);
        _lowTable[lowOffset + 1] = calculatedY;
        WriteAttributes(lowOffset, finalAttributes);
        SetHighTablePair(
            spriteIndex,
            SnesOamHighTablePair.FromSprite(calculatedX, encodedXOffset.IsLarge));

        NextByteOffset = (NextByteOffset + 4) & 0x01ff;
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
    public void AddRawSmallSprite(Position16<OamPixels> position, SnesObjAttributeWord attributes)
    {
        int spriteIndex = NextByteOffset >> 2;
        int lowOffset = NextByteOffset;
        _lowTable[lowOffset] = unchecked((byte)position.X);
        _lowTable[lowOffset + 1] = unchecked((byte)position.Y);
        WriteAttributes(lowOffset, attributes);
        SetHighTablePair(spriteIndex, SnesOamHighTablePair.FromSprite(position.X, isLarge: false));
        NextByteOffset = (NextByteOffset + 4) & 0x01ff;
    }

    /// <summary>
    /// Appends one projectile-trail OBJ exactly as <c>$90:B700/$90:B766</c> do: four low-OAM
    /// bytes, no high-OAM write, and a non-wrapping stack increment.
    /// </summary>
    /// <remarks>
    /// The distinction from <see cref="AddRawSmallSprite"/> is small but observable at the
    /// 128th OBJ. The trail handler permits offset <c>$01FC</c>, advances the native stack to
    /// <c>$0200</c>, and then rejects every later trail. It also knows that both screen
    /// coordinates have zero high bytes, so the cleared high-OAM pair already describes a
    /// small object at X 0..255 and is deliberately left untouched.
    /// </remarks>
    public void AddProjectileTrailSprite(Position16<OamPixels> position, SnesObjAttributeWord attributes)
    {
        if (NextByteOffset >= LowTableByteCount)
            return;

        // Only the low bytes are stored: the caller has already admitted an on-screen point.
        int lowOffset = NextByteOffset;
        _lowTable[lowOffset] = unchecked((byte)position.X);
        _lowTable[lowOffset + 1] = unchecked((byte)position.Y);
        WriteAttributes(lowOffset, attributes);
        NextByteOffset += 4;
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
            _lowTable[spriteIndex * 4 + 1] = 0xf0;

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
            (_highTable[spriteIndex >> 2] >> highShift) & 3)));
        SnesObjAttributeWord attributes = (ushort)(
            SnesLittleEndian.Word(_lowTable[lowOffset + 2], _lowTable[lowOffset + 3]));

        return new OamEntry(
            X: _lowTable[lowOffset] | (highPair.XHigh ? 0x0100 : 0),
            Y: _lowTable[lowOffset + 1],
            TileNumber: attributes.TileNumber,
            Palette: attributes.PaletteIndex,
            Priority: attributes.Priority,
            FlipX: attributes.FlipHorizontally,
            FlipY: attributes.FlipVertically,
            IsLarge: highPair.IsLarge);
    }

    /// <summary>
    /// Restores a complete physical OAM image for a software render consumer. The
    /// physical image has no construction cursor. Retain the producer's modeled
    /// sprite count because existing raster kernels deliberately stop at that count.
    /// </summary>
    [PpuLayoutAccess]
    public void LoadUploadPayload(ReadOnlySpan<byte> payload, int modeledSpriteCount)
    {
        if ((uint)modeledSpriteCount > SpriteCount)
            throw new ArgumentOutOfRangeException(nameof(modeledSpriteCount));
        Ensure.LengthEqual(payload, UploadByteCount);
        payload[..LowTableByteCount].CopyTo(_lowTable);
        payload[LowTableByteCount..].CopyTo(_highTable);
        NextByteOffset = 0;
        LastFinalizedSpriteCount = modeledSpriteCount;
    }

    /// <summary>
    /// Copies a finalized 544-byte staging image into a second OAM model, corresponding to
    /// DMA channel 0 in <c>UpdateOAM_CGRAM</c> at <c>$80:933A</c>.
    /// </summary>
    /// <remarks>
    /// Super Metroid builds OAM during the main loop and the following NMI makes that image
    /// PPU-visible. A distinct destination matters for animated Samus: her staged spritemap
    /// and staged graphics definition must become visible in the same NMI, not one on either
    /// side of a desktop renderer call.
    /// </remarks>
    public void CopyFinalizedFrom(OamBuffer stagingBuffer)
    {
        Ensure.NotNull(stagingBuffer);

        stagingBuffer._lowTable.CopyTo(_lowTable, 0);
        stagingBuffer._highTable.CopyTo(_highTable, 0);
        LastFinalizedSpriteCount = stagingBuffer.LastFinalizedSpriteCount;

        // Hardware OAM has no construction cursor. Keeping the displayed model at zero
        // prevents callers from accidentally appending logic records to the uploaded copy.
        NextByteOffset = 0;
    }

    /// <summary>Replaces one sprite's two-bit high-OAM pair without disturbing its three neighbors.</summary>
    private void SetHighTablePair(int spriteIndex, SnesOamHighTablePair pair)
    {
        int byteIndex = spriteIndex >> 2;
        int shift = (spriteIndex & 3) * 2;
        int pairMask = 3 << shift;
        int pairValue = pair.Raw << shift;

        // The ROM only ORs these bits because ClearHighOAM ran earlier. Clear-and-replace
        // is equivalent for a fresh frame and prevents stale debugger edits from leaking.
        _highTable[byteIndex] = (byte)((_highTable[byteIndex] & ~pairMask) | pairValue);
    }

    /// <summary>Stores one typed packed attribute word in the low-OAM record.</summary>
    private void WriteAttributes(int lowOffset, SnesObjAttributeWord attributes)
    {
        _lowTable[lowOffset + 2] = unchecked((byte)attributes.Raw);
        _lowTable[lowOffset + 3] = SnesLittleEndian.HighByte(attributes.Raw);
    }

    /// <summary>Implements the native carry and sign test for hiding vertically wrapped generic parts.</summary>
    private static bool ShouldHideForOnScreenOrigin(int unsignedYSum, bool offsetIsNegative)
    {
        byte wrappedY = (byte)unsignedYSum;
        if (!offsetIsNegative)
            return unsignedYSum >= 0x100 || wrappedY >= 0xe0;

        // This is the deliberately literal form of $81:8800-$81:881C. Negative offsets
        // may legitimately wrap into $E0-$FF to appear partly above the screen.
        return (unsignedYSum & 0x100) != 0 ? wrappedY >= 0xe0 : wrappedY < 0xe0;
    }
}

/// <summary>A readable projection of one hardware OAM record.</summary>
public readonly record struct OamEntry(
    int X,
    byte Y,
    int TileNumber,
    int Palette,
    int Priority,
    bool FlipX,
    bool FlipY,
    bool IsLarge);
