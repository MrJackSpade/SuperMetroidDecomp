using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Frontend.Pause;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Progression;

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

        /// <summary>
        /// The BG1 tiles the cartridge would show (#1467): the shown area map laid out on the layer's two pages, with the
        /// equipment page over the first while the equipment page shows, for comparison with native VRAM.
        /// </summary>
        /// <param name="artwork">The map presentation the fixture bound the menu to.</param>
        internal BackgroundTile[] ShownBg1Tiles(AreaMapPresentation artwork)
        {
            BackgroundTile[] tiles = ShownAreaMapTiles.Of(self.ShownMap);
            if (self.ScreenMode != PauseScreenMode.Map)
                self.ShownEquipmentTiles(artwork).CopyTo(tiles, 0);
            return tiles;
        }

        /// <summary>
        /// The BG1 tile the cartridge would show at map cell <paramref name="cell"/> (#1467): the shown map's tile, or the
        /// equipment page's where the equipment page covers the first page.
        /// </summary>
        /// <param name="artwork">The map presentation the fixture bound the menu to.</param>
        /// <param name="cell">The map cell.</param>
        internal BackgroundTile ShownBg1Tile(AreaMapPresentation artwork, Position16<MapTiles> cell)
        {
            int index = AreaMapPages.GetTilemapWordIndex(cell);
            if (self.ScreenMode != PauseScreenMode.Map && index < PauseEquipmentBaseDefinitions.Cells)
                return self.ShownEquipmentTiles(artwork)[index];
            return MapCellCharacters.Tile(self.ShownMap.Cells[cell.Y * AreaMapSize.WidthInMapTiles + cell.X]);
        }

        /// <summary>
        /// The BG2 tiles the cartridge would show (#1467): the area's backdrop with the button rows over it, each label in
        /// the shade the highlight shows it in, laid out from the import's sources of the frame's pictures.
        /// </summary>
        /// <param name="artwork">The map presentation the fixture bound the menu to.</param>
        internal BackgroundTile[] ShownBg2Tiles(AreaMapPresentation artwork) => PausePictureSheetConstants.Frame(artwork, self.area, self.ButtonHighlight);

        /// <summary>
        /// Reads one native $7E:3000-relative button-label word as the shown BG2 rows hold it, proving the bottom pause
        /// chrome shows each label in its highlight's shade.
        /// </summary>
        /// <param name="artwork">The map presentation the fixture bound the menu to.</param>
        /// <param name="nativeWordIndex">The label word's index from the start of the native BG2 rows.</param>
        internal ushort ReadPauseButtonLabelWord(AreaMapPresentation artwork, int nativeWordIndex)
        {
            const int firstUploadedNativeWordIndex = 0x0320;
            int uploadedWordOffset = nativeWordIndex - firstUploadedNativeWordIndex;
            if ((uint)uploadedWordOffset >= PauseMenuLayoutConstants.ButtonRowsTileCount)
                throw new ArgumentOutOfRangeException(nameof(nativeWordIndex));
            return BackgroundTileWords.ToWord(self.ShownBg2Tiles(artwork)[nativeWordIndex]);
        }

        /// <summary>
        /// The equipment page's tiles as the cartridge's tilemap holds what the page shows this frame (#1467, #1515), laid
        /// out the way the import lays out a shown page.
        /// </summary>
        /// <param name="artwork">The map presentation the fixture bound the menu to.</param>
        internal BackgroundTile[] ShownEquipmentTiles(AreaMapPresentation artwork) => PauseEquipmentPageLayout.LayOut(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.ImportOf(artwork), self.EquipmentPage);

        /// <summary>
        /// Reads one of the three reserve-supply digits as the equipment page shows it (#1467, #1515): the tile the import
        /// lays out at the digit's place.
        /// </summary>
        /// <param name="artwork">The map presentation the fixture bound the menu to.</param>
        /// <param name="digitIndex">The digit, from the hundreds.</param>
        internal SnesBgTilemapWord ReadReserveSupplyDigit(AreaMapPresentation artwork, int digitIndex)
        {
            if ((uint)digitIndex >= PauseMenuLayoutConstants.ReserveSupplyDigitCount)
                throw new ArgumentOutOfRangeException(nameof(digitIndex));

            int tile = PauseMenuLayoutConstants.ReserveSupplyDigitsByteOffset / sizeof(ushort) + digitIndex;
            return new SnesBgTilemapWord(BackgroundTileWords.ToWord(self.ShownEquipmentTiles(artwork)[tile]));
        }
    }
}
