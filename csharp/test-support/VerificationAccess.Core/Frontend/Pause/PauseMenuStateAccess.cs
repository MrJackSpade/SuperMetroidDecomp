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

        /// <summary>
        /// Scrolls the map page toward <paramref name="target"/> as a player does (#2554): the horizontal arrow and then the
        /// vertical one are each held for the eight-pixel steps that bring the shown scroll nearest the target, and an arrow
        /// the map can no longer scroll takes no step, so the scroll stops at its limit. Each update is a pause update with
        /// only that arrow held.
        /// </summary>
        /// <returns>The pause updates run.</returns>
        internal int ScrollMapToward(Offset16<MapPixels> target)
        {
            Offset16<MapPixels> start = self.MapScroll;
            int updates = HoldArrow(self, target.X - start.X, ControllerButton.Left, ControllerButton.Right);
            return updates + HoldArrow(self, target.Y - start.Y, ControllerButton.Up, ControllerButton.Down);
        }

        /// <summary>
        /// Scrolls the map page to exactly <paramref name="target"/> with the arrows (<see cref="ScrollMapToward"/>), as a
        /// check places it or an imported snapshot brings it where the native menu's steps left it.
        /// </summary>
        /// <returns>The pause updates run.</returns>
        /// <exception cref="InvalidOperationException">
        /// The target does not lie a whole number of steps from the shown scroll, or the arrows stop short of it.
        /// </exception>
        internal int ScrollMapTo(Offset16<MapPixels> target)
        {
            Offset16<MapPixels> start = self.MapScroll;
            if ((target.X - start.X) % MapScrollDefinitions.StepPixels != 0 || (target.Y - start.Y) % MapScrollDefinitions.StepPixels != 0)
                throw new InvalidOperationException(
                    $"The map scrolls from {start} in {MapScrollDefinitions.StepPixels}-pixel steps, which never reach {target}.");
            int updates = self.ScrollMapToward(target);
            Offset16<MapPixels> reached = self.MapScroll;
            if (reached != target)
                throw new InvalidOperationException($"The map's arrows stop at {reached} on the way from {start} to {target}.");
            return updates;
        }

        /// <summary>True while a map/equipment page change is fading or loading; page input waits for it.</summary>
        internal bool IsPageTransitionActive => self.Transition is not null;

        /// <summary>The column the cursor is in: its selected control's category.</summary>
        internal PauseEquipmentCategory SelectedCategory => self.SelectedControl.Category;

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
    /// Holds <paramref name="backward"/>'s arrow, for a negative <paramref name="distance"/>, or <paramref name="forward"/>'s
    /// for a positive one, for as many whole steps as bring the scroll nearest that distance along its axis: each step runs
    /// its <see cref="MapScrollDefinitions.StepTicks"/> updates and the next starts on the update after it finishes.
    /// </summary>
    /// <returns>The pause updates run.</returns>
    private static int HoldArrow(PauseMenuState menu, int distance, ControllerButton backward, ControllerButton forward)
    {
        int steps = (Math.Abs(distance) + MapScrollDefinitions.StepPixels / 2 - 1) / MapScrollDefinitions.StepPixels;
        ControllerButton arrow = distance < 0 ? backward : forward;
        int updates = steps * MapScrollDefinitions.StepTicks;
        SamusBombProjectileSystem bombs = NoFrozenBombs();
        for (int update = 0; update < updates; update++)
            _ = menu.Step(ControllerButton.None, ControllerButton.None, arrow, Compatibility.ShortFrameCount.Zero, bombs);
        return updates;
    }

    /// <summary>
    /// An empty set of Samus's bomb slots for a menu a check drives alone; the pause repaints only their values, so the
    /// glows they would show are never read.
    /// </summary>
    private static SamusBombProjectileSystem NoFrozenBombs() =>
        new(GameplayAudioRequests.Standalone(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork());
}
