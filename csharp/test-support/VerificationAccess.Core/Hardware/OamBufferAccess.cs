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
        [PpuLayoutBoundary("exact hardware test: native OAM oracle for projectile parts")]
        public void AddProjectileSpritePart(SnesSpritemapXWord xOffset, byte yOffset,
            SnesObjAttributeWord attributes, Position16<OamPixels> origin)
        {
            ushort x = unchecked((ushort)(origin.X + xOffset.Raw));
            byte y = unchecked((byte)(origin.Y + yOffset));
            int spriteIndex = self.NextByteOffset >> 2;
            int lowOffset = self.NextByteOffset;
            self._lowTable[lowOffset] = (byte)x;
            self._lowTable[lowOffset + 1] = y;
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
            self._lowTable[lowOffset] = unchecked((byte)position.X);
            self._lowTable[lowOffset + 1] = unchecked((byte)position.Y);
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
    }
}
