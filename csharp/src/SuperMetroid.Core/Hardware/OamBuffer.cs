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
    public const int SpriteCount = 128;
    public const int LowTableByteCount = SpriteCount * 4;
    public const int HighTableByteCount = SpriteCount / 4;
    public const int UploadByteCount = LowTableByteCount + HighTableByteCount;

    private readonly byte[] _lowTable = new byte[LowTableByteCount];
    private readonly byte[] _highTable = new byte[HighTableByteCount];

    /// <summary>Raw 512-byte low OAM table used by PPU DMA.</summary>
    public ReadOnlySpan<byte> LowTable => _lowTable;

    /// <summary>Raw 32-byte high OAM table used by PPU DMA.</summary>
    public ReadOnlySpan<byte> HighTable => _highTable;

    /// <summary>
    /// Byte offset of the next low-table record, equivalent to WRAM <c>$0590</c>. It moves
    /// in four-byte increments and reaches <c>$0200</c> when all 128 sprites are occupied.
    /// </summary>
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

    /// <summary>
    /// Ports the on-screen-origin spritemap loader at <c>$81:879F</c>.
    /// </summary>
    /// <param name="bus">Address space containing the packed spritemap.</param>
    /// <param name="spritemapAddress">24-bit address of its two-byte entry count.</param>
    /// <param name="originX">Unsigned sprite origin; arithmetic wraps at 16 bits.</param>
    /// <param name="originY">Only the low byte participates in the original routine.</param>
    /// <param name="paletteBits">
    /// Palette selection already shifted into OBJ attribute bits <c>$0E00</c>.
    /// </param>
    public void AddOnScreenSpritemap(
        ISnesAddressSpace bus,
        int spritemapAddress,
        ushort originX,
        ushort originY,
        ushort paletteBits)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ValidateAddress(spritemapAddress);
        if ((paletteBits & ~0x0e00) != 0)
            throw new ArgumentOutOfRangeException(nameof(paletteBits), paletteBits, "OBJ palette bits must fit mask $0E00.");

        ushort entryCount = ReadWordInFixedBank(bus, spritemapAddress);
        int entryAddress = AddWithinBank(spritemapAddress, 2);

        for (int entryIndex = 0; entryIndex < entryCount && NextByteOffset < LowTableByteCount; entryIndex++)
        {
            ushort encodedXOffset = ReadWordInFixedBank(bus, entryAddress);
            byte encodedYOffset = bus.ReadByte(AddWithinBank(entryAddress, 2));
            ushort sourceAttributes = ReadWordInFixedBank(bus, AddWithinBank(entryAddress, 3));

            // The low nine bits form a signed/modular X offset. Bit 15 is simultaneously
            // the large-sprite flag, and bits 9-14 are zero in valid records. Adding the
            // complete word looks odd but exactly reproduces the 16-bit ADC in $81:87B8.
            ushort calculatedX = unchecked((ushort)(originX + encodedXOffset));
            bool isLarge = (encodedXOffset & 0x8000) != 0;

            int calculatedY = (byte)originY + encodedYOffset;
            bool yOffsetIsNegative = (encodedYOffset & 0x80) != 0;
            bool hideForVerticalWrap = ShouldHideForOnScreenOrigin(calculatedY, yOffsetIsNegative);

            if (hideForVerticalWrap)
            {
                // $81:8907 parks a clipped sprite at X=$180/Y=$E0. X low becomes $80 and
                // its high-table X bit is set, placing the object safely beyond the screen.
                calculatedX = 0x0180;
                calculatedY = 0x00e0;
            }

            int spriteIndex = NextByteOffset >> 2;
            int lowOffset = NextByteOffset;
            _lowTable[lowOffset] = (byte)calculatedX;
            _lowTable[lowOffset + 1] = (byte)calculatedY;

            // Mask $F1FF preserves tile number, priority, and flips while clearing the
            // spritemap's palette bits. The caller-provided palette then replaces them.
            ushort finalAttributes = (ushort)((sourceAttributes & 0xf1ff) | paletteBits);
            _lowTable[lowOffset + 2] = (byte)finalAttributes;
            _lowTable[lowOffset + 3] = (byte)(finalAttributes >> 8);
            SetHighTablePair(spriteIndex, (calculatedX & 0x0100) != 0, isLarge);

            NextByteOffset += 4;
            entryAddress = AddWithinBank(entryAddress, 5);
        }
    }

    /// <summary>
    /// Ports <c>AddSamusSpritemapToOAM</c> at <c>$81:89AE</c>.
    /// </summary>
    /// <remarks>
    /// This looks tantalizingly similar to <see cref="AddOnScreenSpritemap"/>, but the
    /// differences are part of the game data contract. The input is an index into the
    /// bank-$92 pointer table at <c>$92:808D</c>, not a direct address. More importantly,
    /// Samus entries keep their ROM-authored palette bits and do not use the generic
    /// routine's vertical-wrap hiding rule. Keeping a distinct method prevents a future
    /// cleanup from quietly changing the native draw behavior.
    /// </remarks>
    /// <param name="bus">CPU address space containing the pointer table and spritemap.</param>
    /// <param name="spritemapIndex">Word index into <c>$92:808D</c>.</param>
    /// <param name="originX">Screen-space X origin produced by bank $90.</param>
    /// <param name="originY">Screen-space Y origin produced by bank $90.</param>
    public void AddSamusSpritemap(
        ISnesAddressSpace bus,
        ushort spritemapIndex,
        ushort originX,
        ushort originY)
    {
        ArgumentNullException.ThrowIfNull(bus);

        // $81:89B8 doubles A because every table element is a 16-bit pointer. The table
        // and pointed-to records both live in bank $92, so 16-bit address arithmetic wraps
        // without carrying into a different bank.
        int pointerAddress = 0x920000 | ((0x808d + spritemapIndex * 2) & 0xffff);
        ushort spritemapPointer = ReadWordInFixedBank(bus, pointerAddress);

        // Zero pointers are intentional empty frames. The assembly tests the entry count
        // after following the pointer; accepting a zero pointer here would read $92:0000,
        // whereas the actual retail table uses pointers to records whose count may be zero.
        // Follow the native data literally rather than assigning a host-side meaning to 0.
        int spritemapAddress = 0x920000 | spritemapPointer;
        ushort entryCount = ReadWordInFixedBank(bus, spritemapAddress);
        if (entryCount == 0)
            return;

        int entryAddress = AddWithinBank(spritemapAddress, 2);
        for (int entryIndex = 0; entryIndex < entryCount; entryIndex++)
        {
            ushort encodedXOffset = ReadWordInFixedBank(bus, entryAddress);
            byte encodedYOffset = bus.ReadByte(AddWithinBank(entryAddress, 2));
            ushort attributes = ReadWordInFixedBank(bus, AddWithinBank(entryAddress, 3));

            // The 65C816 adds the complete encoded X word, including the size bit and any
            // irrelevant high bits produced by old art tools. Only X bit 8 reaches high
            // OAM; bit 15 of the original offset independently chooses the large OBJ size.
            ushort calculatedX = unchecked((ushort)(originX + encodedXOffset));
            byte calculatedY = unchecked((byte)(originY + encodedYOffset));
            bool xHigh = (calculatedX & 0x0100) != 0;
            bool isLarge = (encodedXOffset & 0x8000) != 0;

            int spriteIndex = NextByteOffset >> 2;
            int lowOffset = NextByteOffset;
            _lowTable[lowOffset] = (byte)calculatedX;
            _lowTable[lowOffset + 1] = calculatedY;

            // Unlike $81:879F, $81:8A1A performs no $F1FF mask and no caller palette OR.
            // Pose spritemaps carry their complete tile/palette/priority/flip attributes.
            _lowTable[lowOffset + 2] = (byte)attributes;
            _lowTable[lowOffset + 3] = (byte)(attributes >> 8);
            SetHighTablePair(spriteIndex, xHigh, isLarge);

            // Native OAMStack is a nine-bit byte offset and therefore wraps after $1FC.
            // Normal gameplay stays below that limit, but retaining the mask makes the
            // behavior inspectable instead of replacing it with a host collection limit.
            NextByteOffset = (NextByteOffset + 4) & 0x01ff;
            entryAddress = AddWithinBank(entryAddress, 5);
        }
    }

    /// <summary>
    /// Ports <c>AddProjectileSpritemapToOAM</c> at <c>$81:8A4B</c> for a direct bank-$93
    /// spritemap pointer. Projectile records keep their own palette, priority, and flips.
    /// </summary>
    /// <remarks>
    /// The bank-$93 draw caller has already performed room-relative off-screen checks, so
    /// this routine intentionally has no vertical-wrap parking rule. It is closer to the
    /// common tail used by the original than <see cref="AddOnScreenSpritemap"/>, whose
    /// caller supplies replacement palette bits.
    /// </remarks>
    public void AddProjectileSpritemap(
        ISnesAddressSpace bus,
        ushort bank93SpritemapPointer,
        ushort originX,
        ushort originY)
    {
        ArgumentNullException.ThrowIfNull(bus);

        int spritemapAddress = 0x930000 | bank93SpritemapPointer;
        ushort entryCount = ReadWordInFixedBank(bus, spritemapAddress);
        if (entryCount == 0)
            return;

        int entryAddress = AddWithinBank(spritemapAddress, 2);
        for (int entryIndex = 0; entryIndex < entryCount; entryIndex++)
        {
            ushort encodedXOffset = ReadWordInFixedBank(bus, entryAddress);
            byte encodedYOffset = bus.ReadByte(AddWithinBank(entryAddress, 2));
            ushort attributes = ReadWordInFixedBank(bus, AddWithinBank(entryAddress, 3));

            ushort calculatedX = unchecked((ushort)(originX + encodedXOffset));
            byte calculatedY = unchecked((byte)(originY + encodedYOffset));
            bool xHigh = (calculatedX & 0x0100) != 0;
            bool isLarge = (encodedXOffset & 0x8000) != 0;

            int spriteIndex = NextByteOffset >> 2;
            int lowOffset = NextByteOffset;
            _lowTable[lowOffset] = (byte)calculatedX;
            _lowTable[lowOffset + 1] = calculatedY;
            _lowTable[lowOffset + 2] = (byte)attributes;
            _lowTable[lowOffset + 3] = (byte)(attributes >> 8);
            SetHighTablePair(spriteIndex, xHigh, isLarge);

            // $81:8A2B masks the byte-address stack to nine bits after each entry.
            NextByteOffset = (NextByteOffset + 4) & 0x01ff;
            entryAddress = AddWithinBank(entryAddress, 5);
        }
    }

    /// <summary>
    /// Ports <c>$81:8A37</c>: follows an index through bank-$93 table <c>$A1A1</c> and
    /// emits the selected charge/grapple-flare spritemap without off-screen correction.
    /// </summary>
    public void AddFlareSpritemap(
        ISnesAddressSpace bus,
        ushort tableIndex,
        ushort originX,
        ushort originY)
    {
        ArgumentNullException.ThrowIfNull(bus);

        // The native entry point doubles A, then reads a same-bank pointer. Reusing the
        // projectile loader below is exact after that one lookup: both routes keep the
        // ROM-authored palette/priority bits and wrap the nine-bit OAM stack identically.
        ushort pointer = ReadWordInFixedBank(
            bus,
            0x930000 | ((0xa1a1 + tableIndex * 2) & 0xffff));
        AddProjectileSpritemap(bus, pointer, originX, originY);
    }

    /// <summary>
    /// Ports the bank-$86 enemy-projectile loaders at <c>$81:8C0A/$81:8C7F</c> for a direct
    /// bank-$8D spritemap pointer and the projectile slot's packed graphics index.
    /// </summary>
    /// <remarks>
    /// Enemy projectiles do not use bank-$93's “attributes are final” contract. The low byte
    /// of <paramref name="graphicsIndex"/> is added to every ROM tile/attribute word as a
    /// base tile number, and its high byte is ORed afterward as palette bits. The caller also
    /// chooses between two opposite vertical-wrap rules depending on whether the projectile
    /// origin itself is in screen Y <c>$00-$FF</c>. Those details are what let a multi-object
    /// breath or explosion straddle the top/bottom edge without wrapping onto the wrong side.
    /// </remarks>
    public void AddEnemyProjectileSpritemap(
        ISnesAddressSpace bus,
        ushort bank8dSpritemapPointer,
        ushort originX,
        ushort originY,
        ushort graphicsIndex,
        bool originYIsOnScreen)
    {
        ArgumentNullException.ThrowIfNull(bus);

        int spritemapAddress = 0x8d0000 | bank8dSpritemapPointer;
        ushort entryCount = ReadWordInFixedBank(bus, spritemapAddress);
        if (entryCount == 0)
            return;

        ushort baseTileNumber = unchecked((byte)graphicsIndex);
        ushort paletteBits = unchecked((ushort)(graphicsIndex & 0xff00));
        int entryAddress = AddWithinBank(spritemapAddress, 2);
        for (int entryIndex = 0; entryIndex < entryCount; entryIndex++)
        {
            ushort encodedXOffset = ReadWordInFixedBank(bus, entryAddress);
            byte encodedYOffset = bus.ReadByte(AddWithinBank(entryAddress, 2));
            ushort sourceAttributes = ReadWordInFixedBank(bus, AddWithinBank(entryAddress, 3));

            ushort calculatedX = unchecked((ushort)(originX + encodedXOffset));
            int unsignedYSum = unchecked((byte)originY) + encodedYOffset;
            bool yOffsetIsNegative = (encodedYOffset & 0x80) != 0;
            bool hideForVerticalWrap = originYIsOnScreen
                ? yOffsetIsNegative ? unsignedYSum < 0x100 : unsignedYSum >= 0x100
                : yOffsetIsNegative ? unsignedYSum >= 0x100 : unsignedYSum < 0x100;
            byte calculatedY = hideForVerticalWrap ? (byte)0xf0 : unchecked((byte)unsignedYSum);

            int spriteIndex = NextByteOffset >> 2;
            int lowOffset = NextByteOffset;
            _lowTable[lowOffset] = unchecked((byte)calculatedX);
            _lowTable[lowOffset + 1] = calculatedY;

            // `$81:8C60` uses ADC, not OR, for the base tile. Carry is explicitly clear at
            // this point, but an overflowing tile number may carry into attribute bits.
            ushort finalAttributes = unchecked((ushort)(sourceAttributes + baseTileNumber));
            finalAttributes |= paletteBits;
            _lowTable[lowOffset + 2] = unchecked((byte)finalAttributes);
            _lowTable[lowOffset + 3] = unchecked((byte)(finalAttributes >> 8));
            SetHighTablePair(
                spriteIndex,
                (calculatedX & 0x0100) != 0,
                (encodedXOffset & 0x8000) != 0);

            // The native OAM stack is a wrapping nine-bit byte index, including within a
            // single large spritemap. Preserve that diagnostic edge rather than truncating.
            NextByteOffset = (NextByteOffset + 4) & 0x01ff;
            entryAddress = AddWithinBank(entryAddress, 5);
        }
    }

    /// <summary>
    /// Ports <c>DrawSpritemapWithBaseTile</c> at <c>$81:8AB8</c>, the common enemy
    /// spritemap writer used by <c>WriteEnemyOams</c>.
    /// </summary>
    /// <remarks>
    /// Enemy definitions choose a fixed program/data bank, while each animation frame is
    /// only a 16-bit pointer inside that bank. Unlike Samus, an enemy's graphics-set loader
    /// supplies both a base tile number and replacement OBJ palette bits. The native code
    /// adds the base tile to the complete ROM attribute word (so tile overflow can carry),
    /// then ORs the palette selection. Coordinates and the nine-bit OAM stack wrap exactly
    /// as they do on the 65C816.
    /// </remarks>
    public void AddEnemySpritemap(
        ISnesAddressSpace bus,
        byte bank,
        ushort spritemapPointer,
        ushort originX,
        ushort originY,
        ushort paletteBits,
        ushort baseTileIndex)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if ((paletteBits & ~0x0e00) != 0)
            throw new ArgumentOutOfRangeException(nameof(paletteBits), paletteBits, "OBJ palette bits must fit mask $0E00.");

        int spritemapAddress = (bank << 16) | spritemapPointer;
        ushort entryCount = ReadWordInFixedBank(bus, spritemapAddress);
        int entryAddress = AddWithinBank(spritemapAddress, 2);
        for (int entryIndex = 0; entryIndex < entryCount; entryIndex++)
        {
            ushort encodedXOffset = ReadWordInFixedBank(bus, entryAddress);
            byte encodedYOffset = bus.ReadByte(AddWithinBank(entryAddress, 2));
            ushort sourceAttributes = ReadWordInFixedBank(bus, AddWithinBank(entryAddress, 3));

            ushort calculatedX = unchecked((ushort)(originX + encodedXOffset));
            byte calculatedY = unchecked((byte)(originY + encodedYOffset));
            ushort finalAttributes = unchecked((ushort)(sourceAttributes + baseTileIndex));
            finalAttributes |= paletteBits;

            int spriteIndex = NextByteOffset >> 2;
            int lowOffset = NextByteOffset;
            _lowTable[lowOffset] = unchecked((byte)calculatedX);
            _lowTable[lowOffset + 1] = calculatedY;
            _lowTable[lowOffset + 2] = unchecked((byte)finalAttributes);
            _lowTable[lowOffset + 3] = unchecked((byte)(finalAttributes >> 8));
            SetHighTablePair(
                spriteIndex,
                (calculatedX & 0x0100) != 0,
                (encodedXOffset & 0x8000) != 0);

            NextByteOffset = (NextByteOffset + 4) & 0x01ff;
            entryAddress = AddWithinBank(entryAddress, 5);
        }
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
    public void AddRawSmallSprite(ushort x, ushort y, ushort attributes)
    {
        int spriteIndex = NextByteOffset >> 2;
        int lowOffset = NextByteOffset;
        _lowTable[lowOffset] = unchecked((byte)x);
        _lowTable[lowOffset + 1] = unchecked((byte)y);
        _lowTable[lowOffset + 2] = unchecked((byte)attributes);
        _lowTable[lowOffset + 3] = unchecked((byte)(attributes >> 8));
        SetHighTablePair(spriteIndex, (x & 0x0100) != 0, isLarge: false);
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
    public void AddProjectileTrailSprite(byte x, byte y, ushort attributes)
    {
        if (NextByteOffset >= LowTableByteCount)
            return;

        int lowOffset = NextByteOffset;
        _lowTable[lowOffset] = x;
        _lowTable[lowOffset + 1] = y;
        _lowTable[lowOffset + 2] = unchecked((byte)attributes);
        _lowTable[lowOffset + 3] = unchecked((byte)(attributes >> 8));
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
        int highPair = (_highTable[spriteIndex >> 2] >> highShift) & 3;
        ushort attributes = (ushort)(_lowTable[lowOffset + 2] | (_lowTable[lowOffset + 3] << 8));

        return new OamEntry(
            X: _lowTable[lowOffset] | ((highPair & 1) << 8),
            Y: _lowTable[lowOffset + 1],
            TileNumber: attributes & 0x01ff,
            Palette: (attributes >> 9) & 7,
            Priority: (attributes >> 12) & 3,
            FlipX: (attributes & 0x4000) != 0,
            FlipY: (attributes & 0x8000) != 0,
            IsLarge: (highPair & 2) != 0);
    }

    /// <summary>Copies the exact contiguous 544-byte payload sent to PPU OAM.</summary>
    public byte[] CreateUploadPayload()
    {
        var payload = new byte[UploadByteCount];
        _lowTable.CopyTo(payload, 0);
        _highTable.CopyTo(payload, LowTableByteCount);
        return payload;
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
        ArgumentNullException.ThrowIfNull(stagingBuffer);

        stagingBuffer._lowTable.CopyTo(_lowTable, 0);
        stagingBuffer._highTable.CopyTo(_highTable, 0);
        LastFinalizedSpriteCount = stagingBuffer.LastFinalizedSpriteCount;

        // Hardware OAM has no construction cursor. Keeping the displayed model at zero
        // prevents callers from accidentally appending logic records to the uploaded copy.
        NextByteOffset = 0;
    }

    private void SetHighTablePair(int spriteIndex, bool xHigh, bool isLarge)
    {
        int byteIndex = spriteIndex >> 2;
        int shift = (spriteIndex & 3) * 2;
        int pairMask = 3 << shift;
        int pairValue = ((xHigh ? 1 : 0) | (isLarge ? 2 : 0)) << shift;

        // The ROM only ORs these bits because ClearHighOAM ran earlier. Clear-and-replace
        // is equivalent for a fresh frame and prevents stale debugger edits from leaking.
        _highTable[byteIndex] = (byte)((_highTable[byteIndex] & ~pairMask) | pairValue);
    }

    private static bool ShouldHideForOnScreenOrigin(int unsignedYSum, bool offsetIsNegative)
    {
        byte wrappedY = (byte)unsignedYSum;
        if (!offsetIsNegative)
            return unsignedYSum >= 0x100 || wrappedY >= 0xe0;

        // This is the deliberately literal form of $81:8800-$81:881C. Negative offsets
        // may legitimately wrap into $E0-$FF to appear partly above the screen.
        return (unsignedYSum & 0x100) != 0 ? wrappedY >= 0xe0 : wrappedY < 0xe0;
    }

    private static ushort ReadWordInFixedBank(ISnesAddressSpace bus, int address)
    {
        byte low = bus.ReadByte(address);
        byte high = bus.ReadByte(AddWithinBank(address, 1));
        return (ushort)(low | (high << 8));
    }

    private static int AddWithinBank(int address, int byteCount)
    {
        int bank = address & 0xff0000;
        return bank | ((address + byteCount) & 0xffff);
    }

    private static void ValidateAddress(int address)
    {
        if ((uint)address > 0x00ff_ffff)
            throw new ArgumentOutOfRangeException(nameof(address));
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
