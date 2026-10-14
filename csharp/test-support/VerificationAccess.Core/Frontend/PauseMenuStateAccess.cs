using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

/// <summary>Verification access to <see cref="PauseMenuState"/> members production does not use.</summary>
internal static class PauseMenuStateAccess
{
    /// <summary>Reads pause transitions, selection, animation, and native PPU state.</summary>
    extension(PauseMenuState self)
    {
        /// <summary>True while a map/equipment page change is fading or loading; page input waits for it.</summary>
        internal bool IsPageTransitionActive => self.transition != default;

        /// <summary>Low byte of the native category/item selector word.</summary>
        internal PauseEquipmentCategory SelectedCategory => self.selectedCategory;

        /// <summary>High byte of the native category/item selector word.</summary>
        internal int SelectedItem => self.selectedItem;

        /// <summary>Read-only diagnostic timing for the native equipment-selector animation.</summary>
        internal (int Frame, int Timer) ItemSelectorAnimationState => (self.itemSelectorAnimationFrame, self.itemSelectorAnimationTimer);

        /// <summary>Number of cartridge OBJ records emitted by the most recent render.</summary>
        internal int LastRenderedSpriteCount => self.oam.LastFinalizedSpriteCount;

        /// <summary>Last bank-$82 menu spritemap ID selected by a pause draw routine.</summary>
        internal ushort LastIndicatorSpritemapId => self.lastIndicatorSpritemapId;

        /// <summary>
        /// Reads one native $7E:3000-relative button-label word after its queued-equivalent
        /// upload. This narrow friend-test seam proves the bottom pause chrome reached VRAM;
        /// callers cannot mutate the private menu PPU.
        /// </summary>
        [PpuLayoutBoundary("exact hardware test: checks native PPU geometry, native VRAM words")]
        internal ushort ReadPauseButtonLabelWord(int nativeWordIndex)
        {
            const int firstUploadedNativeWordIndex = 0x0320;
            int uploadedWordOffset = nativeWordIndex - firstUploadedNativeWordIndex;
            if ((uint)uploadedWordOffset >= PauseMenuLayout.ButtonRowsByteCount / 2)
                throw new ArgumentOutOfRangeException(nameof(nativeWordIndex));
            return self.vram.ReadWord(PauseVramLayout.ButtonRows.Offset + uploadedWordOffset);
        }

        /// <summary>
        /// Reads one of the three reserve-supply digits from the mutable equipment tilemap.
        /// This exposes rendered menu state to focused tests without exposing mutation.
        /// </summary>
        internal SnesBgTilemapWord ReadReserveSupplyDigit(int digitIndex)
        {
            if ((uint)digitIndex >= PauseMenuLayout.ReserveSupplyDigitCount)
                throw new ArgumentOutOfRangeException(nameof(digitIndex));

            int byteOffset = PauseMenuLayout.ReserveSupplyDigitsByteOffset + digitIndex * 2;
            return unchecked((ushort)(
                SnesLittleEndian.Word(self.equipmentTilemap[byteOffset], self.equipmentTilemap[byteOffset + 1])));
        }
    }
}
