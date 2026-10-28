using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Map;

namespace SuperMetroid.Core.Hud;

/// <summary>
/// The three mutable 32-tile rows of Super Metroid's gameplay HUD tilemap, corresponding
/// exactly to WRAM <c>$7E:C608-$7E:C6C7</c>.
/// </summary>
public sealed class HudState
{
    /// <summary>Number of eight-pixel BG3 cells per HUD row, matching the native 32-word tilemap stride.</summary>
    public const int WidthInTiles = 32;
    /// <summary>Number of mutable HUD rows the gameplay update rewrites, excluding the preceding static row.</summary>
    public const int MutableRowCount = 3;
    /// <summary>Ninety-six mutable BG3 tile words, stored row-major across the three 32-cell HUD rows.</summary>
    public const int MutableTileCount = WidthInTiles * MutableRowCount;

    /// <summary>Three mutable BG3 HUD rows in native row-major tilemap order.</summary>
    internal readonly ushort[] Tiles = new ushort[MutableTileCount];

    /// <summary>The three mutable HUD rows the frame shows, row-major (#1348).</summary>
    public ReadOnlySpan<ushort> MutableRows => Tiles;
    /// <summary>Previous item selection used to update both icon palettes and sound edges.</summary>
    internal HudItem PreviousSelectedItem { get; set; }
    /// <summary>Current artwork binding, omitted from serialized gameplay state.</summary>
    [NonSerialized] private GameplayHudPresentation? presentation;

    /// <summary>Native $05F7: suppress minimap updates after a boss initializer.</summary>
    public bool MinimapDisabled { get; internal set; }

    /// <summary>$90:A7E2 blanks the fifteen minimap cells and disables updates.</summary>
    internal void DisableMinimapForBoss()
    {
        MinimapDisabled = true;
        for (int y = 0; y < 3; y++)
        for (int x = 0; x < 5; x++)
        {
            var cell = new Position16<TilemapCells>((ushort)x, (ushort)y);
            Tiles[presentation?.MinimapCellIndex(cell) ?? StockMinimapCellIndex(cell)] = (ushort)MapTileWords.HudBlank;
        }
    }

    /// <summary>$82:E1B7 restores minimap updates when leaving the room.</summary>
    internal void EnableMinimapAfterDoorEntry() => MinimapDisabled = false;

    /// <summary>Whether a ROM template has been copied into this state.</summary>
    public bool IsInitialized { get; private set; }

    /// <summary>
    /// One-frame publication of <c>QueueSfx1_Max6($39)</c> from <c>$80:9B44</c>.
    /// The frontend consumes it after the runtime frame has completed.
    /// </summary>
    public bool SelectionSoundRequestedThisFrame { get; private set; }
    /// <summary>Transfers the pending HUD sound once, including across frontend frame boundaries.</summary>
    public bool ConsumeSelectionSoundRequest()
    {
        bool requested = SelectionSoundRequestedThisFrame;
        SelectionSoundRequestedThisFrame = false;
        return requested;
    }

    /// <summary>
    /// Native queue suppression captured when the HUD requests its sound, rather than
    /// when the frontend eventually publishes it. Later explosion state changes must
    /// not retroactively admit or reject an earlier request.
    /// </summary>
    public bool SelectionSoundSuppressedThisFrame { get; private set; }

    /// <summary>Absolute area-map tile (X 0-63, Y 0-31) selected by the latest minimap update.</summary>
    public Position16<MapTiles> MinimapCenter { get; private set; }

    /// <summary>
    /// Binds current host-owned HUD presentation. Rebinding changed content rebuilds only
    /// presentation-owned cells and carries the logical 5x3 minimap to its new anchor.
    /// </summary>
    public void BindPresentation(GameplayHudPresentation? value, SamusState? samus = null)
    {
        GameplayHudPresentation? previous = presentation;
        presentation = value;
        if (!IsInitialized || value is null || samus is null ||
            string.Equals(previous?.ContentIdentity, value.ContentIdentity, StringComparison.Ordinal))
            return;

        var minimap = new ushort[15];
        for (int y = 0; y < 3; y++)
        for (int x = 0; x < 5; x++)
        {
            var cell = new Position16<TilemapCells>((ushort)x, (ushort)y);
            minimap[y * 5 + x] = Tiles[previous?.MinimapCellIndex(cell) ?? StockMinimapCellIndex(cell)];
        }

        value.ApplyTemplate(Tiles);
        ApplyCurrentPresentationState(samus);
        for (int y = 0; y < 3; y++)
        for (int x = 0; x < 5; x++)
            Tiles[value.MinimapCellIndex(new(unchecked((ushort)x), unchecked((ushort)y)))] = minimap[y * 5 + x];
    }

    /// <summary>
    /// Ports the visible data work of <c>$80:9A79</c> and its first
    /// <c>$80:9B44</c> update for an explicit inventory snapshot.
    /// </summary>
    public void Initialize(HudSnapshot snapshot)
    {

        (presentation ?? throw new InvalidOperationException(
            "HUD initialization requires installed presentation assets.")).ApplyTemplate(Tiles);

        if (snapshot.EquippedItems.HasAny(SamusEquipmentFlags.XrayScope))
            AddTwoByTwoIcon(itemIndex: 4);
        if (snapshot.EquippedItems.HasAny(SamusEquipmentFlags.GrappleBeam))
            AddTwoByTwoIcon(itemIndex: 3);
        if (snapshot.MaxMissiles != 0)
            AddMissileIcon();
        if (snapshot.MaxSuperMissiles != 0)
            AddTwoByTwoIcon(itemIndex: 1);
        if (snapshot.MaxPowerBombs != 0)
            AddTwoByTwoIcon(itemIndex: 2);

        if (snapshot.ReserveMode == ReserveTankMode.Auto)
            DrawAutoReserve(snapshot.ReserveHealth != 0);

        DrawHealth(snapshot.Health, snapshot.MaxHealth);
        if (snapshot.MaxMissiles != 0)
            DrawAmmo(itemIndex: 0, snapshot.Missiles);
        if (snapshot.MaxSuperMissiles != 0)
            DrawAmmo(itemIndex: 1, snapshot.SuperMissiles);
        if (snapshot.MaxPowerBombs != 0)
            DrawAmmo(itemIndex: 2, snapshot.PowerBombs);

        ToggleItemHighlight(snapshot.SelectedItem,
            presentation?.SelectedPalette ?? GameplayHudDefinitions.SelectedPalette);
        // `$80:9AC9` initializes samus_prev_hud_item_index to zero. HandleHudTilemap
        // performs the first live comparison on the next gameplay pass.
        PreviousSelectedItem = HudItem.Beam;
        SelectionSoundRequestedThisFrame = false;
        IsInitialized = true;
    }

    /// <summary>
    /// Replays the mutable counter portion of <c>$80:9B44</c> from live Samus state. The
    /// three rows are rewritten every gameplay frame and the next accepted frame shows them;
    /// initialization is not the sole owner of the energy digits. Updating them on every
    /// gameplay pass prevents enemy damage from changing physics state while the visible
    /// HUD remains frozen at 99.
    /// </summary>
    [IndependentFlag("Time freeze and sound suppression are separate runtime states supplied by the frame.")]
    public void UpdateGameplayCounters(
        SamusState samus,
        bool timeIsFrozen = false,
        bool soundSuppressed = false)
    {
        Ensure.NotNull(samus);
        if (!IsInitialized)
            throw new InvalidOperationException("Initialize the HUD before updating gameplay counters.");

        SelectionSoundRequestedThisFrame = false;

        SelectionSoundSuppressedThisFrame = soundSuppressed;

        // Permanent pickups can introduce an inventory family after HUD initialization.
        // The cartridge's item routines patch those blank icon cells immediately; replay
        // the same guarded writers here before updating their counters. Each writer is
        // idempotent and preserves an already-installed icon and live minimap state.
        if ((samus.EquippedItems & SamusEquipmentFlags.XrayScope) != SamusEquipmentFlags.None)
            AddTwoByTwoIcon(itemIndex: 4);
        if ((samus.EquippedItems & SamusEquipmentFlags.GrappleBeam) != SamusEquipmentFlags.None)
            AddTwoByTwoIcon(itemIndex: 3);
        if (samus.MaxMissiles != 0)
            AddMissileIcon();
        if (samus.MaxSuperMissiles != 0)
            AddTwoByTwoIcon(itemIndex: 1);
        if (samus.MaxPowerBombs != 0)
            AddTwoByTwoIcon(itemIndex: 2);

        DrawHealth(samus.Health, samus.MaxHealth);
        if (samus.MaxMissiles != 0)
            DrawAmmo(itemIndex: 0, samus.Missiles);
        if (samus.MaxSuperMissiles != 0)
            DrawAmmo(itemIndex: 1, samus.SuperMissiles);
        if (samus.MaxPowerBombs != 0)
            DrawAmmo(itemIndex: 2, samus.PowerBombs);
        if (samus.ReserveTankMode == ReserveTankMode.Auto)
            DrawAutoReserve(samus.ReserveEnergy != 0);

        // `$80:9BD3-$9C20` changes the newly selected icon to palette four, restores the
        // old icon to palette five, then publishes sound $39. This belongs in the live HUD
        // update rather than the input handler: native suppresses the sound (but not the
        // visual change) while spinning, wall-jumping, grappling, or time is frozen.
        if (samus.SelectedHudItem != PreviousSelectedItem)
        {
            ToggleItemHighlight(samus.SelectedHudItem,
                presentation?.SelectedPalette ?? GameplayHudDefinitions.SelectedPalette);
            ToggleItemHighlight(PreviousSelectedItem,
                presentation?.DeselectedPalette ?? GameplayHudDefinitions.DeselectedPalette);
            PreviousSelectedItem = samus.SelectedHudItem;

            SamusMovementType movementType = SamusPoseMetadata.ReadMovementType(samus);
            SelectionSoundRequestedThisFrame =
                movementType is not (SamusMovementType.SpinJumping or SamusMovementType.WallJumping) &&
                samus.Grapple.Phase == GrapplePhase.Inactive &&
                !timeIsFrozen;
        }
    }

    /// <summary>
    /// Ports the visible 5x3 result of <c>UpdateMinimap</c> at <c>$90:A91B</c> for a
    /// supplied room/map position, including exploration bits and the blinking center tile.
    /// </summary>
    public void UpdateMinimap(SharedGameState system, MinimapFrame frame)
    {
        Ensure.NotNull(system);
        if (!IsInitialized)
            throw new InvalidOperationException("Initialize the HUD before updating its minimap.");
        if (frame.PresentationMap is null)
            throw new InvalidOperationException("Minimap updates require an installed area-map definition.");
        if (frame.PresentationMap.Area != frame.AreaIndex)
            throw new ArgumentException("Map presentation belongs to a different area.", nameof(frame.PresentationMap));
        if (frame.RoomSize.X == 0 || frame.RoomSize.Y == 0)
            throw new ArgumentOutOfRangeException(nameof(frame.RoomSize));
        // The room header stores its map coordinates as bytes.
        if (frame.RoomMap.X > byte.MaxValue || frame.RoomMap.Y > byte.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(frame.RoomMap));

        // $90:A91B returns rather than indexing unrelated map memory if Samus is outside
        // the active room. The 16-pixel comparison consumes the same positions as collision.
        if (MinimapDisabled || !frame.RoomSize.Contains(frame.Samus.ToRoomBlock()))
            return;

        // One area-map tile represents one 256x256 room screen. The native Y formula has
        // an intentional +1 because the HUD map convention places the room header's Y one
        // row above the screen containing Samus.
        int centerX = (frame.RoomMap.X + RoomGridConversions.PixelToScreen(frame.Samus.X)) & 63;
        int centerY = frame.RoomMap.Y + RoomGridConversions.PixelToScreen(frame.Samus.Y) + 1;
        if ((uint)centerY >= 32)
            return;
        MinimapCenter = new((ushort)centerX, (ushort)centerY);

        // Exploration is persistent game state, not HUD-local rendering state. Earlier
        // builds kept this bit in HudState, which made the minimap look correct until a
        // save/load or HUD reconstruction silently erased the visited route.
        system.MarkExploredMapTile(frame.AreaIndex, new Position16<MapTiles>(unchecked((ushort)centerX), unchecked((ushort)centerY)));
        bool hasAreaMap = system.HasAreaMap(frame.AreaIndex);

        for (int outputY = 0; outputY < 3; outputY++)
        {
            int mapY = centerY + outputY - 1;
            for (int outputX = 0; outputX < 5; outputX++)
            {
                var output = new Position16<TilemapCells>((ushort)outputX, (ushort)outputY);
                int destination = presentation?.MinimapCellIndex(output) ?? StockMinimapCellIndex(output);
                int mapX = (centerX + outputX - 2) & 63;
                if ((uint)mapY >= 32)
                {
                    Tiles[destination] = (ushort)MapTileWords.HudBlank;
                    continue;
                }

                var cell = new Position16<MapTiles>(unchecked((ushort)mapX), unchecked((ushort)mapY));
                bool explored = system.IsMapTileExplored(frame.AreaIndex, cell);
                bool stationVisible = frame.PresentationMap.IsRevealedByMapStation(cell);

                // A 64x32 SNES map is two adjacent 32x32 screens in VRAM order, not one
                // linear 64-word row. Preserve that page split when reading bank-$B5 data.
                MapTileWord mapTile = frame.PresentationMap.GetTile(cell);
                if (!AreaMapVisibility.IsVisible(
                        explored,
                        hasAreaMap,
                        stationVisible,
                        frame.PresentationMap.IsDiscoverable(cell),
                        frame.MapRevealMode))
                {
                    Tiles[destination] = (ushort)MapTileWords.HudBlank;
                    continue;
                }

                Tiles[destination] = (ushort)mapTile.ForHud(explored);
                // The center slope also explores the corner above it. The native top
                // row has already been rendered and its explored bits latched, so that
                // cell acquires the explored palette on the next minimap update.
                if (outputX == 2 && outputY == 1 && explored && centerY > 0 &&
                    frame.PresentationMap.RevealsCellAbove(cell))
                    system.MarkExploredMapTile(frame.AreaIndex, new Position16<MapTiles>(unchecked((ushort)centerX), unchecked((ushort)(centerY - 1))));
            }
        }

        // Every eight NMI frames the native code ORs palette bits $1C00 into the center
        // tile. The other half-cycle leaves its explored palette intact, producing the
        // familiar blinking Samus location without a separate sprite.
        if ((frame.FrameCounter & 8) == 0)
        {
            var centerCell = new Position16<TilemapCells>(2, 1);
            int center = presentation?.MinimapCellIndex(centerCell) ?? StockMinimapCellIndex(centerCell);
            Tiles[center] = (ushort)new MapTileWord(Tiles[center]).WithLocationBlink();
        }
    }

    /// <summary>Updates energy digits and tanks through the installed HUD artwork.</summary>
    private void DrawHealth(int health, int maxHealth)
    {
        (presentation ?? throw new InvalidOperationException(
            "HUD energy requires installed presentation assets.")).ApplyEnergy(Tiles, health, maxHealth);
    }

    /// <summary>Draws the AUTO reserve indicator when its selected tank contains energy.</summary>
    private void DrawAutoReserve(bool containsEnergy)
    {
        (presentation ?? throw new InvalidOperationException(
            "HUD reserve indicator requires installed presentation assets.")).ApplyAutoReserve(Tiles, containsEnergy);
    }

    /// <summary>Ports $82:AF33 without reinitializing icons, counters or the minimap.</summary>
    public void ClearAutoReserveIndicator()
    {
        if (!IsInitialized) throw new InvalidOperationException("Initialize the HUD before clearing AUTO.");
        (presentation ?? throw new InvalidOperationException(
            "HUD reserve indicator requires installed presentation assets.")).ClearAutoReserve(Tiles);
    }

    /// <summary>Installs the missile icon if the current HUD template has not drawn it yet.</summary>
    private void AddMissileIcon()
    {
        (presentation ?? throw new InvalidOperationException(
            "HUD icons require installed presentation assets.")).TryApplyIcon(Tiles, itemIndex: 0);
    }

    /// <summary>Installs a two-by-two item icon using the selected presentation slot.</summary>
    private void AddTwoByTwoIcon(int itemIndex)
    {
        (presentation ?? throw new InvalidOperationException(
            "HUD icons require installed presentation assets.")).TryApplyIcon(Tiles, itemIndex);
    }

    /// <summary>Changes the selected item's palette without rebuilding its tile words.</summary>
    internal void ToggleItemHighlight(HudItem selectedItem, int paletteIndex)
    {
        (presentation ?? throw new InvalidOperationException(
            "HUD selection requires installed presentation assets.")).ToggleItemHighlight(Tiles, selectedItem, paletteIndex);
    }

    /// <summary>Writes the ammunition digits for one enabled HUD item.</summary>
    private void DrawAmmo(int itemIndex, int value)
    {
        (presentation ?? throw new InvalidOperationException(
            "HUD ammunition requires installed presentation assets.")).ApplyAmmo(Tiles, itemIndex, value);
    }

    /// <summary>Restores visible icons, counters, reserve mode, and selection after artwork rebind.</summary>
    private void ApplyCurrentPresentationState(SamusState samus)
    {
        if (presentation is null) return;
        if ((samus.EquippedItems & SamusEquipmentFlags.XrayScope) != SamusEquipmentFlags.None)
            presentation.TryApplyIcon(Tiles, 4);
        if ((samus.EquippedItems & SamusEquipmentFlags.GrappleBeam) != SamusEquipmentFlags.None)
            presentation.TryApplyIcon(Tiles, 3);
        if (samus.MaxMissiles != 0) presentation.TryApplyIcon(Tiles, 0);
        if (samus.MaxSuperMissiles != 0) presentation.TryApplyIcon(Tiles, 1);
        if (samus.MaxPowerBombs != 0) presentation.TryApplyIcon(Tiles, 2);
        presentation.ApplyEnergy(Tiles, samus.Health, samus.MaxHealth);
        if (samus.MaxMissiles != 0) presentation.ApplyAmmo(Tiles, 0, samus.Missiles);
        if (samus.MaxSuperMissiles != 0) presentation.ApplyAmmo(Tiles, 1, samus.SuperMissiles);
        if (samus.MaxPowerBombs != 0) presentation.ApplyAmmo(Tiles, 2, samus.PowerBombs);
        if (samus.ReserveTankMode == ReserveTankMode.Auto) presentation.ApplyAutoReserve(Tiles, samus.ReserveEnergy != 0);
        presentation.ToggleItemHighlight(Tiles, samus.SelectedHudItem, presentation.SelectedPalette);
    }

    /// <summary>Maps a cell in the five-by-three minimap to its stock place in the HUD tile buffer.</summary>
    private static int StockMinimapCellIndex(Position16<TilemapCells> output) =>
        26 + output.Y * WidthInTiles + output.X;

}
