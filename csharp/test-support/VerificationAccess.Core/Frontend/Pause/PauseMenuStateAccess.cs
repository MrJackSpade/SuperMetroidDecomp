using SuperMetroid.Core.Hardware;

using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>Verification access to <see cref="PauseMenuState"/> members production does not use.</summary>
internal static class PauseMenuStateAccess
{
    /// <summary>Reads pause transitions, selection, animation, and native PPU state.</summary>
    extension(PauseMenuState self)
    {
        /// <summary>True while a map/equipment page change is fading or loading; page input waits for it.</summary>
        internal bool IsPageTransitionActive => self.Transition != default;

        /// <summary>Low byte of the native category/item selector word.</summary>
        internal PauseEquipmentCategory SelectedCategory => self.SelectedCategory;

        /// <summary>High byte of the native category/item selector word.</summary>
        internal int SelectedItem => self.SelectedItem;

        /// <summary>Read-only diagnostic timing for the native equipment-selector animation.</summary>
        internal SelectorAnimation ItemSelectorAnimationState => new SelectorAnimation(self.ItemSelectorAnimationFrame, self.ItemSelectorAnimationTimer);

        /// <summary>Number of cartridge OBJ records emitted by the most recent render.</summary>
        internal int LastRenderedSpriteCount => self.screen.Objects.LastFinalizedSpriteCount;

        /// <summary>Pose of the Samus map marker when it was the last indicator a pause render drew.</summary>
        internal SuperMetroid.Core.Frontend.Map.MapSpriteId? LastMapIndicatorSprite => self.LastMapIndicatorSprite;

        /// <summary>Category of the equipment selector when it was the last indicator a pause render drew.</summary>
        internal PauseEquipmentCategory? LastSelectorCategory => self.LastSelectorCategory;

        /// <summary>
        /// Reads one native $7E:3000-relative button-label word after its queued-equivalent
        /// upload, proving the bottom pause chrome reached the shown BG2 rows.
        /// </summary>
        internal ushort ReadPauseButtonLabelWord(int nativeWordIndex)
        {
            const int firstUploadedNativeWordIndex = 0x0320;
            int uploadedWordOffset = nativeWordIndex - firstUploadedNativeWordIndex;
            if ((uint)uploadedWordOffset >= PauseMenuLayout.ButtonRowsByteCount / 2)
                throw new ArgumentOutOfRangeException(nameof(nativeWordIndex));
            return self.screen.Tilemaps.ButtonRows[uploadedWordOffset];
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
            return unchecked(
                SnesLittleEndian.Word(self.equipmentTilemap[byteOffset], self.equipmentTilemap[byteOffset + 1]));
        }
    }
}
