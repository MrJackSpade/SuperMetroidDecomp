using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Frontend.Pause;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Progression;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>Verification access to <see cref="PauseMenuState"/> members production does not use.</summary>
internal static class PauseMenuStateAccess
{
    /// <summary>Reads pause transitions, selection, animation, and native PPU state.</summary>
    extension(PauseMenuState self)
    {
        /// <summary>
        /// Runs one pause update as a check drives the menu alone: the map's arrows read <paramref name="heldInput"/>, or
        /// the delayed-held buttons when none are given, the short frame count is <paramref name="frameCount"/>, and
        /// no bombs are frozen under the pause beyond an empty set of bomb slots.
        /// </summary>
        internal bool Step(ControllerButton delayedHeldInput, ControllerButton newlyPressedInput, ControllerButton? heldInput = null,
            int frameCount = 0) =>
            self.Step(delayedHeldInput, newlyPressedInput, heldInput ?? delayedHeldInput, new Compatibility.ShortFrameCount(frameCount), NoFrozenBombs());

<<<<<<< HEAD
||||||| 418ca79935
        /// <summary>Frames the map page by <paramref name="scroll"/> in place of the framing the menu set up, for a check of a map's edges.</summary>
        internal void FrameMap(MapScroll scroll) =>
            PrivateFrontendState.Set(self, nameof(PauseMenuState.CurrentMapScroll), Ensure.NotNull(scroll));

=======
        /// <summary>Frames the map page by <paramref name="scroll"/> in place of the framing the menu set up, for a check of a map's edges.</summary>
        internal void FrameMap(MapScroll scroll) =>
            self.CurrentMapScroll = Ensure.NotNull(scroll);

>>>>>>> origin/main
        /// <summary>
        /// Moves the map page's scroll to <paramref name="scroll"/> under the framing and limits the menu set up, as a check
        /// or an imported snapshot places it between scroll steps.
        /// </summary>
<<<<<<< HEAD
        internal void ScrollMapTo(Offset16<MapPixels> scroll) => self.ActiveMapScroll.Scroll = scroll;
||||||| 418ca79935
        internal void ScrollMapTo(Offset16<MapPixels> scroll) =>
            self.FrameMap(global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Frontend.Map.MapScroll>([typeof(global::SuperMetroid.Core.Frontend.Map.MapFraming), typeof(global::SuperMetroid.Core.Frontend.Map.MapScrollLimits), typeof(global::SuperMetroid.Core.Spatial.Offset16<global::SuperMetroid.Core.Spatial.MapPixels>)], new object?[] { self.ActiveMapScroll.Framing, self.ActiveMapScroll.Limits, scroll }));
=======
        internal void ScrollMapTo(Offset16<MapPixels> scroll) =>
            self.FrameMap(new global::SuperMetroid.Core.Frontend.Map.MapScroll(self.ActiveMapScroll.Framing, self.ActiveMapScroll.Limits, scroll));
>>>>>>> origin/main

        /// <summary>True while a map/equipment page change is fading or loading; page input waits for it.</summary>
        internal bool IsPageTransitionActive => self.Transition is not null;

        /// <summary>The column the cursor is in: its selected control's category.</summary>
        internal PauseEquipmentCategory SelectedCategory => self.SelectedControl.Category;

        /// <summary>Read-only diagnostic timing for the native equipment-selector animation.</summary>
        internal SelectorAnimation ItemSelectorAnimationState =>
            new(self.itemSelector.Playback.Place, self.itemSelector.Shown ?? throw new InvalidOperationException("The item selector shows no look."), self.itemSelector.UpdatesLeft);

        /// <summary>Number of cartridge OBJ records emitted by the most recent render.</summary>
        internal int LastRenderedSpriteCount => self.screen.Sprites.LastFinalizedSpriteCount;

        /// <summary>
        /// The BG1 tiles the cartridge would show (#1467): the shown area map laid out on the layer's two pages, with the
        /// equipment page over the first while the equipment page shows, for comparison with native VRAM.
        /// </summary>
        /// <param name="artwork">The map presentation the fixture bound the menu to.</param>
        internal BackgroundTile[] ShownBg1Tiles(InstalledScreenContent artwork)
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
        internal BackgroundTile ShownBg1Tile(InstalledScreenContent artwork, Position16<MapTiles> cell)
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
        internal BackgroundTile[] ShownBg2Tiles(InstalledScreenContent artwork) => PausePictureSheetConstants.Frame(artwork, self.area, self.ButtonHighlight);

        /// <summary>
        /// Reads one native $7E:3000-relative button-label word as the shown BG2 rows hold it, proving the bottom pause
        /// chrome shows each label in its highlight's shade.
        /// </summary>
        /// <param name="artwork">The map presentation the fixture bound the menu to.</param>
        /// <param name="nativeWordIndex">The label word's index from the start of the native BG2 rows.</param>
        internal ushort ReadPauseButtonLabelWord(InstalledScreenContent artwork, int nativeWordIndex)
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
        internal BackgroundTile[] ShownEquipmentTiles(InstalledScreenContent artwork) => PauseEquipmentPageLayout.LayOut(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.ImportOf(artwork), self.EquipmentPage);

        /// <summary>
        /// Reads one of the three reserve-supply digits as the equipment page shows it (#1467, #1515): the tile the import
        /// lays out at the digit's place.
        /// </summary>
        /// <param name="artwork">The map presentation the fixture bound the menu to.</param>
        /// <param name="digitIndex">The digit, from the hundreds.</param>
        internal SnesBgTilemapWord ReadReserveSupplyDigit(InstalledScreenContent artwork, int digitIndex)
        {
            if ((uint)digitIndex >= PauseMenuLayoutConstants.ReserveSupplyDigitCount)
                throw new ArgumentOutOfRangeException(nameof(digitIndex));

            int tile = PauseMenuLayoutConstants.ReserveSupplyDigitsByteOffset / sizeof(ushort) + digitIndex;
            return new SnesBgTilemapWord(BackgroundTileWords.ToWord(self.ShownEquipmentTiles(artwork)[tile]));
        }
    }

    /// <summary>
    /// An empty set of Samus's bomb slots for a menu a check drives alone; the pause repaints only their values, so the
    /// glows they would show are never read.
    /// </summary>
    private static SamusBombProjectileSystem NoFrozenBombs() =>
        new(GameplayAudioRequests.Standalone(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork());
}
