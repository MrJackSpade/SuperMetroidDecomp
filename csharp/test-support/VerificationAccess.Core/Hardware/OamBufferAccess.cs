namespace SuperMetroid.Core.Hardware;

/// <summary>Verification access to <see cref="OamBuffer"/> members production does not use.</summary>
internal static class OamBufferAccess
{
    /// <summary>Exposes <see cref="OamBuffer"/> state that only verification reads.</summary>
    extension(OamBuffer self)
    {
        /// <summary>
        /// Emits an authored projectile visual part using the $81:8A4B/$81:8A2B tail:
        /// preserve source attributes, wrap Y and wrap the write position after 128 OBJs.
        /// Unlike menu parts, it neither parks vertically wrapped parts nor stops at capacity.
        /// The caller owns whole-projectile viewport admission.
        /// </summary>
        /// <summary>The low table as the native oracles write it; production only reads it.</summary>
        internal Span<byte> WritableLowTable() =>
            System.Runtime.InteropServices.MemoryMarshal.CreateSpan(
                ref System.Runtime.InteropServices.MemoryMarshal.GetReference(self.LowTable), self.LowTable.Length);

        [PpuLayoutBoundary("exact hardware test: native OAM oracle for projectile parts")]
        public void AddProjectileSpritePart(SnesSpritemapXWord xOffset, byte yOffset,
            SnesObjAttributeWord attributes, Position16<OamPixels> origin)
        {
            ushort x = unchecked((ushort)(origin.X + xOffset.Raw));
            byte y = unchecked((byte)(origin.Y + yOffset));
            int spriteIndex = self.NextByteOffset >> 2;
            int lowOffset = self.NextByteOffset;
            self.WritableLowTable()[lowOffset] = (byte)x;
            self.WritableLowTable()[lowOffset + 1] = y;
            self.WriteAttributes(lowOffset, attributes);
            self.SetHighTablePair(spriteIndex, SnesOamHighTablePair.FromSprite(x, xOffset.IsLarge));
            self.NextByteOffset = (self.NextByteOffset + 4) % OamBuffer.LowTableByteCount;
        }
        /// <summary>
        /// Appends one projectile-trail OBJ exactly as <c>$90:B700/$90:B766</c> do: four low-OAM
        /// bytes, no high-OAM write, and a non-wrapping stack increment.
        /// </summary>
        /// <remarks>
        /// The distinction from <see cref="OamBuffer.AddRawSmallSprite"/> is small but observable at the
        /// 128th OBJ. The trail handler permits offset <c>$01FC</c>, advances the native stack to
        /// <c>$0200</c>, and then rejects every later trail. It also knows that both screen
        /// coordinates have zero high bytes, so the cleared high-OAM pair already describes a
        /// small object at X 0..255 and is deliberately left untouched.
        /// </remarks>
        [PpuLayoutBoundary("exact hardware test: native OAM oracle for projectile trails")]
        public void AddProjectileTrailSprite(Position16<OamPixels> position, SnesObjAttributeWord attributes)
        {
            if (self.NextByteOffset >= OamBuffer.LowTableByteCount)
                return;

            // Only the low bytes are stored: the caller has already admitted an on-screen point.
            int lowOffset = self.NextByteOffset;
            self.WritableLowTable()[lowOffset] = unchecked((byte)position.X);
            self.WritableLowTable()[lowOffset + 1] = unchecked((byte)position.Y);
            self.WriteAttributes(lowOffset, attributes);
            self.NextByteOffset += 4;
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
                self.AppendEnemySpritemapPart(part, origin, paletteBits,
                    baseTileIndex, clipVerticalWrap, originYIsOnScreen);
        }

        /// <summary>Appends an installed enemy composition in its original drawing order, as the native writer does.</summary>
        [PpuLayoutBoundary("exact hardware test: native OAM oracle for enemy compositions")]
        public void AddEnemySpritemap(EnemySpritemapParts parts,
            Position16<OamPixels> origin, ushort paletteBits, ushort baseTileIndex,
            bool clipVerticalWrap = false, bool originYIsOnScreen = true)
        {
            Ensure.NotNull(parts);
            _ = SnesObjAttributeWord.FromPaletteBits(paletteBits);
            for (int index = 0; index < parts.Count; index++)
                self.AppendEnemySpritemapPart(parts[index], origin, paletteBits,
                    baseTileIndex, clipVerticalWrap, originYIsOnScreen);
        }

        /// <summary>Packs an enemy part after adding its tile base, palette, and optional vertical clipping.</summary>
        [PpuLayoutBoundary("exact hardware test: native OAM oracle for enemy parts")]
        internal void AppendEnemySpritemapPart(EnemySpritemapPart part,
            Position16<OamPixels> origin, ushort paletteBits, ushort baseTileIndex,
            bool clipVerticalWrap, bool originYIsOnScreen)
        {
            SnesSpritemapXWord encodedXOffset = part.X;
            byte encodedYOffset = part.Y.Raw;
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
                bool yOffsetIsNegative = unchecked((sbyte)encodedYOffset) < 0;
                bool hideForVerticalWrap = originYIsOnScreen
                    ? yOffsetIsNegative ? unsignedYSum < ByteRange : unsignedYSum >= ByteRange
                    : yOffsetIsNegative ? unsignedYSum >= ByteRange : unsignedYSum < ByteRange;
                calculatedY = hideForVerticalWrap ? HiddenY : unchecked((byte)unsignedYSum);
            }
            SnesObjAttributeWord finalAttributes = part.Attributes
                .AddPackedTileBase(baseTileIndex)
                .Or(SnesObjAttributeWord.FromPaletteBits(paletteBits));

            int spriteIndex = self.NextByteOffset >> 2;
            int lowOffset = self.NextByteOffset;
            self.WritableLowTable()[lowOffset] = unchecked((byte)calculatedX);
            self.WritableLowTable()[lowOffset + 1] = calculatedY;
            self.WriteAttributes(lowOffset, finalAttributes);
            self.SetHighTablePair(spriteIndex, SnesOamHighTablePair.FromSprite(calculatedX, encodedXOffset.IsLarge));
            self.NextByteOffset = (self.NextByteOffset + 4) % OamBuffer.LowTableByteCount;
        }
    }

    /// <summary>The span of one byte coordinate, which a vertical sum crosses when it wraps.</summary>
    private const int ByteRange = 256;

    /// <summary>The Y at which the native writer hides a part that would wrap across the screen's edge.</summary>
    private const byte HiddenY = 240;
}
