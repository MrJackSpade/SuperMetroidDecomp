using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Pause;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Map;

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
        /// The BG1 tiles the cartridge would show (#1467): the shown area map laid out on the layer's two pages, with the
        /// equipment page over the first while the equipment page shows, for comparison with native VRAM.
        /// </summary>
        internal BackgroundTile[] ShownBg1Tiles
        {
            get
            {
                BackgroundTile[] tiles = ShownAreaMapTiles.Of(self.ShownMap);
                if (self.ScreenMode != PauseScreenMode.Map)
                    self.screen.Tilemaps.Equipment.CopyTo(tiles);
                return tiles;
            }
        }

        /// <summary>
        /// The BG1 tile the cartridge would show at map cell <paramref name="cell"/> (#1467): the shown map's tile, or the
        /// equipment page's where the equipment page covers the first page.
        /// </summary>
        internal BackgroundTile ShownBg1Tile(Position16<MapTiles> cell)
        {
            int index = AreaMapPages.GetTilemapWordIndex(cell);
            if (self.ScreenMode != PauseScreenMode.Map && index < self.screen.Tilemaps.Equipment.Length)
                return self.screen.Tilemaps.Equipment[index];
            ShownMapCell shown = self.ShownMap[cell];
            return new BackgroundTile(shown.Cell.Shape.Picture, shown.ColourVariant, shown.Cell.InFront, shown.Cell.Mirroring);
        }

        /// <summary>
        /// The BG2 tiles the cartridge would show (#1467): the area's backdrop with the button rows over it, each label in
        /// the shade the highlight shows it in, laid out from the import's sources of the frame's pictures.
        /// </summary>
        internal BackgroundTile[] ShownBg2Tiles => PausePictureSheet.Frame(self.screen.Artwork, self.area, self.ButtonHighlight);

        /// <summary>
        /// Reads one native $7E:3000-relative button-label word as the shown BG2 rows hold it, proving the bottom pause
        /// chrome shows each label in its highlight's shade.
        /// </summary>
        internal ushort ReadPauseButtonLabelWord(int nativeWordIndex)
        {
            const int firstUploadedNativeWordIndex = 0x0320;
            int uploadedWordOffset = nativeWordIndex - firstUploadedNativeWordIndex;
            if ((uint)uploadedWordOffset >= PauseMenuLayout.ButtonRowsTileCount)
                throw new ArgumentOutOfRangeException(nameof(nativeWordIndex));
            return BackgroundTileWords.ToWord(self.ShownBg2Tiles[nativeWordIndex]);
        }

        /// <summary>
        /// Reads one of the three reserve-supply digits from the mutable equipment tilemap.
        /// This exposes rendered menu state to focused tests without exposing mutation.
        /// </summary>
        internal SnesBgTilemapWord ReadReserveSupplyDigit(int digitIndex)
        {
            if ((uint)digitIndex >= PauseMenuLayout.ReserveSupplyDigitCount)
                throw new ArgumentOutOfRangeException(nameof(digitIndex));

            int tile = PauseMenuLayout.ReserveSupplyDigitsByteOffset / sizeof(ushort) + digitIndex;
            return new SnesBgTilemapWord(BackgroundTileWords.ToWord(self.equipmentTilemap[tile]));
        }
    }
}
