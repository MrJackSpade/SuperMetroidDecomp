using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The three mutable 32-tile rows of Super Metroid's gameplay HUD tilemap, corresponding
/// exactly to WRAM <c>$7E:C608-$7E:C6C7</c>.
/// </summary>
public sealed class HudState
{
    /// <summary>Number of eight-pixel BG3 cells per HUD row, matching the native 32-word tilemap stride.</summary>
    public const int WidthInTiles = 32;
    /// <summary>Number of mutable HUD rows uploaded each gameplay update, excluding the preceding static row.</summary>
    public const int MutableRowCount = 3;
    /// <summary>Ninety-six mutable BG3 tile words, stored row-major across the three 32-cell HUD rows.</summary>
    public const int MutableTileCount = WidthInTiles * MutableRowCount;
    /// <summary>$00C0 bytes in the little-endian mutable HUD upload, two bytes for each of the ninety-six tile words.</summary>
    public const int MutableByteCount = MutableTileCount * 2;
    /// <summary>$7E:C608, native mutable HUD tilemap buffer written before the $80:9CC3-$9CE9 VRAM queue entry is appended.</summary>
    public const int WorkRamAddress = 0x7ec608;
    /// <summary>VRAM word $5820, one 32-cell row beyond the BG3 tilemap base $5800; direct VRAM byte access uses twice this address.</summary>
    public const ushort VramDestination = 0x5820;

    /// <summary>Row-major words for the three mutable HUD tilemap rows uploaded to WRAM.</summary>
    private readonly ushort[] _tiles = new ushort[MutableTileCount];
    /// <summary>Prior selected-item index used to detect inventory selection changes.</summary>
    private ushort _previousSelectedItem;
    /// <summary>Optional installed artwork and layout used to render host-owned HUD presentation.</summary>
    [NonSerialized] private GameplayHudPresentation? presentation;

    /// <summary>Native $05F7: suppress minimap updates after a boss initializer.</summary>
    public bool MinimapDisabled { get; private set; }

    /// <summary>$90:A7E2 blanks the fifteen minimap cells and disables updates.</summary>
    internal void DisableMinimapForBoss()
    {
        MinimapDisabled = true;
        for (int y = 0; y < 3; y++)
        for (int x = 0; x < 5; x++)
            _tiles[presentation?.MinimapCellIndex(x, y) ?? NativeMinimapCellIndex(x, y)] =
                (ushort)MapTileWords.HudBlank;
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

    /// <summary>Absolute 0-63 area-map X tile selected by the latest minimap update.</summary>
    public byte MinimapCenterX { get; private set; }

    /// <summary>Absolute 0-31 area-map Y tile selected by the latest minimap update.</summary>
    public byte MinimapCenterY { get; private set; }

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
            minimap[y * 5 + x] = _tiles[previous?.MinimapCellIndex(x, y) ?? NativeMinimapCellIndex(x, y)];

        value.ApplyTemplate(_tiles);
        ApplyCurrentPresentationState(samus);
        for (int y = 0; y < 3; y++)
        for (int x = 0; x < 5; x++)
            _tiles[value.MinimapCellIndex(x, y)] = minimap[y * 5 + x];
    }

    /// <summary>
    /// Ports the visible data work of <c>$80:9A79</c> and its first
    /// <c>$80:9B44</c> update for an explicit inventory snapshot.
    /// </summary>
    public void Initialize(ISnesAddressSpace bus, HudSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(bus);

        (presentation ?? throw new InvalidOperationException(
            "HUD initialization requires installed presentation assets.")).ApplyTemplate(_tiles);

        if (snapshot.EquippedItems.HasAny(SamusEquipmentFlags.XrayScope))
            AddTwoByTwoIcon(bus, itemIndex: 4, GameplayHudDefinitions.IconTableAddress + 36);
        if (snapshot.EquippedItems.HasAny(SamusEquipmentFlags.GrappleBeam))
            AddTwoByTwoIcon(bus, itemIndex: 3, GameplayHudDefinitions.IconTableAddress + 28);
        if (snapshot.MaxMissiles != 0)
            AddMissileIcon(bus);
        if (snapshot.MaxSuperMissiles != 0)
            AddTwoByTwoIcon(bus, itemIndex: 1, GameplayHudDefinitions.IconTableAddress + 12);
        if (snapshot.MaxPowerBombs != 0)
            AddTwoByTwoIcon(bus, itemIndex: 2, GameplayHudDefinitions.IconTableAddress + 20);

        if (snapshot.ReserveMode == 1)
            DrawAutoReserve(bus, snapshot.ReserveHealth != 0);

        DrawHealth(bus, snapshot.Health, snapshot.MaxHealth);
        if (snapshot.MaxMissiles != 0)
            DrawAmmo(bus, itemIndex: 0, snapshot.Missiles, byteOffset: 0x94);
        if (snapshot.MaxSuperMissiles != 0)
            DrawAmmo(bus, itemIndex: 1, snapshot.SuperMissiles, byteOffset: 0x9c);
        if (snapshot.MaxPowerBombs != 0)
            DrawAmmo(bus, itemIndex: 2, snapshot.PowerBombs, byteOffset: 0xa2);

        ToggleItemHighlight(snapshot.SelectedItem,
            presentation?.SelectedPalette ?? GameplayHudDefinitions.SelectedPalette);
        // `$80:9AC9` initializes samus_prev_hud_item_index to zero. HandleHudTilemap
        // performs the first live comparison on the next gameplay pass.
        _previousSelectedItem = 0;
        SelectionSoundRequestedThisFrame = false;
        IsInitialized = true;
    }

    /// <summary>
    /// Replays the mutable counter portion of <c>$80:9B44</c> from live Samus state. The
    /// three-row HUD upload runs every gameplay frame; initialization is not the sole owner
    /// of the energy digits. Keeping this update beside <see cref="QueueUpload"/> prevents
    /// enemy damage from changing physics state while the visible HUD remains frozen at 99.
    /// </summary>
    public void UpdateGameplayCounters(
        ISnesAddressSpace bus,
        SamusState samus,
        bool timeIsFrozen = false,
        bool soundSuppressed = false)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        if (!IsInitialized)
            throw new InvalidOperationException("Initialize the HUD before updating gameplay counters.");

        SelectionSoundRequestedThisFrame = false;

        SelectionSoundSuppressedThisFrame = soundSuppressed;

        // Permanent pickups can introduce an inventory family after HUD initialization.
        // The cartridge's item routines patch those blank icon cells immediately; replay
        // the same guarded writers here before updating their counters. Each writer is
        // idempotent and preserves an already-installed icon and live minimap state.
        if ((samus.EquippedItems & (ushort)SamusEquipmentFlags.XrayScope) != 0)
            AddTwoByTwoIcon(bus, itemIndex: 4, GameplayHudDefinitions.IconTableAddress + 36);
        if ((samus.EquippedItems & (ushort)SamusEquipmentFlags.GrappleBeam) != 0)
            AddTwoByTwoIcon(bus, itemIndex: 3, GameplayHudDefinitions.IconTableAddress + 28);
        if (samus.MaxMissiles != 0)
            AddMissileIcon(bus);
        if (samus.MaxSuperMissiles != 0)
            AddTwoByTwoIcon(bus, itemIndex: 1, GameplayHudDefinitions.IconTableAddress + 12);
        if (samus.MaxPowerBombs != 0)
            AddTwoByTwoIcon(bus, itemIndex: 2, GameplayHudDefinitions.IconTableAddress + 20);

        DrawHealth(bus, samus.Health, samus.MaxHealth);
        if (samus.MaxMissiles != 0)
            DrawAmmo(bus, itemIndex: 0, samus.Missiles, byteOffset: 0x94);
        if (samus.MaxSuperMissiles != 0)
            DrawAmmo(bus, itemIndex: 1, samus.SuperMissiles, byteOffset: 0x9c);
        if (samus.MaxPowerBombs != 0)
            DrawAmmo(bus, itemIndex: 2, samus.PowerBombs, byteOffset: 0xa2);
        if (samus.ReserveTankMode == 1)
            DrawAutoReserve(bus, samus.ReserveEnergy != 0);

        // `$80:9BD3-$9C20` changes the newly selected icon to palette four, restores the
        // old icon to palette five, then publishes sound $39. This belongs in the live HUD
        // update rather than the input handler: native suppresses the sound (but not the
        // visual change) while spinning, wall-jumping, grappling, or time is frozen.
        if (samus.SelectedHudItem != _previousSelectedItem)
        {
            ToggleItemHighlight(samus.SelectedHudItem,
                presentation?.SelectedPalette ?? GameplayHudDefinitions.SelectedPalette);
            ToggleItemHighlight(_previousSelectedItem,
                presentation?.DeselectedPalette ?? GameplayHudDefinitions.DeselectedPalette);
            _previousSelectedItem = samus.SelectedHudItem;

            SamusMovementType movementType = samus.ReadMovementType(bus);
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
    public void UpdateMinimap(
        ISnesAddressSpace bus,
        Bank80SystemState system,
        AreaId areaIndex,
        byte roomMapX,
        byte roomMapY,
        int roomWidthInBlocks,
        int roomHeightInBlocks,
        ushort samusX,
        ushort samusY,
        byte nmiFrameCounter,
        MapRevealMode mapRevealMode = MapRevealMode.None,
        IAreaMapView? presentationMap = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(system);
        if (!IsInitialized)
            throw new InvalidOperationException("Initialize the HUD before updating its minimap.");
        if (presentationMap is null)
            throw new InvalidOperationException("Minimap updates require an installed area-map definition.");
        if (presentationMap.Area != areaIndex)
            throw new ArgumentException("Map presentation belongs to a different area.", nameof(presentationMap));
        if (roomWidthInBlocks <= 0 || roomHeightInBlocks <= 0)
            throw new ArgumentOutOfRangeException(nameof(roomWidthInBlocks));

        // $90:A91B returns rather than indexing unrelated map memory if Samus is outside
        // the active room. The 16-pixel comparison consumes the same positions as collision.
        if (MinimapDisabled || (samusX >> 4) >= roomWidthInBlocks || (samusY >> 4) >= roomHeightInBlocks)
            return;

        // One area-map tile represents one 256x256 room screen. The native Y formula has
        // an intentional +1 because the HUD map convention places the room header's Y one
        // row above the screen containing Samus.
        int centerX = (roomMapX + (samusX >> 8)) & 0x3f;
        int centerY = roomMapY + (samusY >> 8) + 1;
        if ((uint)centerY >= 32)
            return;
        MinimapCenterX = (byte)centerX;
        MinimapCenterY = (byte)centerY;

        // Exploration is persistent game state, not HUD-local rendering state. Earlier
        // builds kept this bit in HudState, which made the minimap look correct until a
        // save/load or HUD reconstruction silently erased the visited route.
        system.MarkExploredMapTile(areaIndex, centerX, centerY);
        bool hasAreaMap = system.HasAreaMap(areaIndex);

        for (int outputY = 0; outputY < 3; outputY++)
        {
            int mapY = centerY + outputY - 1;
            for (int outputX = 0; outputX < 5; outputX++)
            {
                int destination = presentation?.MinimapCellIndex(outputX, outputY) ??
                    NativeMinimapCellIndex(outputX, outputY);
                int mapX = (centerX + outputX - 2) & 0x3f;
                if ((uint)mapY >= 32)
                {
                    _tiles[destination] = (ushort)MapTileWords.HudBlank;
                    continue;
                }

                bool explored = system.IsMapTileExplored(areaIndex, mapX, mapY);
                bool stationVisible = presentationMap.IsRevealedByMapStation(mapX, mapY);

                // A 64x32 SNES map is two adjacent 32x32 screens in VRAM order, not one
                // linear 64-word row. Preserve that page split when reading bank-$B5 data.
                MapTileWord mapTile = presentationMap.GetTile(mapX, mapY);
                if (!AreaMapVisibility.IsVisible(
                        explored,
                        hasAreaMap,
                        stationVisible,
                        presentationMap.IsDiscoverable(mapX, mapY),
                        mapRevealMode))
                {
                    _tiles[destination] = (ushort)MapTileWords.HudBlank;
                    continue;
                }

                _tiles[destination] = (ushort)mapTile.ForHud(explored);
                // The center slope also explores the corner above it. The native top
                // row has already been rendered and its explored bits latched, so that
                // cell acquires the explored palette on the next minimap update.
                if (outputX == 2 && outputY == 1 && explored && centerY > 0 &&
                    presentationMap.RevealsCellAbove(mapX, mapY))
                    system.MarkExploredMapTile(areaIndex, centerX, centerY - 1);
            }
        }

        // Every eight NMI frames the native code ORs palette bits $1C00 into the center
        // tile. The other half-cycle leaves its explored palette intact, producing the
        // familiar blinking Samus location without a separate sprite.
        if ((nmiFrameCounter & 8) == 0)
        {
            int center = presentation?.MinimapCellIndex(2, 1) ?? NativeMinimapCellIndex(2, 1);
            _tiles[center] = (ushort)new MapTileWord(_tiles[center]).WithLocationBlink();
        }
    }

    /// <summary>
    /// Writes the typed words into their authentic WRAM buffer and appends the seven-byte
    /// VRAM queue entry constructed at <c>$80:9CC3-$80:9CE9</c>.
    /// </summary>
    public void QueueUpload(ISnesAddressSpace bus, VramWriteQueue queue)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(queue);
        if (!IsInitialized)
            throw new InvalidOperationException("Initialize the HUD before queueing its tilemap.");

        for (int tile = 0; tile < MutableTileCount; tile++)
        {
            ushort value = _tiles[tile];
            bus.WriteByte(WorkRamAddress + tile * 2, (byte)value);
            bus.WriteByte(WorkRamAddress + tile * 2 + 1, (byte)(value >> 8));
        }

        queue.Enqueue(MutableByteCount, WorkRamAddress, VramDestination);
    }

    /// <summary>Writes the current energy digits and bar using the installed HUD presentation.</summary>
    private void DrawHealth(ISnesAddressSpace bus, ushort health, ushort maxHealth)
    {
        (presentation ?? throw new InvalidOperationException(
            "HUD energy requires installed presentation assets.")).ApplyEnergy(_tiles, health, maxHealth);
    }

    /// <summary>Updates the AUTO reserve indicator to reflect whether stored reserve energy is available.</summary>
    private void DrawAutoReserve(ISnesAddressSpace bus, bool containsEnergy)
    {
        (presentation ?? throw new InvalidOperationException(
            "HUD reserve indicator requires installed presentation assets.")).ApplyAutoReserve(_tiles, containsEnergy);
    }

    /// <summary>Ports $82:AF33 without reinitializing icons, counters or the minimap.</summary>
    public void ClearAutoReserveIndicator()
    {
        if (!IsInitialized) throw new InvalidOperationException("Initialize the HUD before clearing AUTO.");
        (presentation ?? throw new InvalidOperationException(
            "HUD reserve indicator requires installed presentation assets.")).ClearAutoReserve(_tiles);
    }

    /// <summary>Installs the missile icon in its HUD tile cells when the presentation provides it.</summary>
    private void AddMissileIcon(ISnesAddressSpace bus)
    {
        (presentation ?? throw new InvalidOperationException(
            "HUD icons require installed presentation assets.")).TryApplyIcon(_tiles, itemIndex: 0);
    }

    /// <summary>Installs the selected 2-by-2 equipment icon into the corresponding HUD cells.</summary>
    private void AddTwoByTwoIcon(ISnesAddressSpace bus, int itemIndex, int source)
    {
        (presentation ?? throw new InvalidOperationException(
            "HUD icons require installed presentation assets.")).TryApplyIcon(_tiles, itemIndex);
    }

    /// <summary>Changes the item-selection highlight palette when the selected HUD item changes.</summary>
    private void ToggleItemHighlight(ushort selectedItem, int paletteIndex)
    {
        (presentation ?? throw new InvalidOperationException(
            "HUD selection requires installed presentation assets.")).ToggleItemHighlight(_tiles, selectedItem, paletteIndex);
    }

    /// <summary>Writes the formatted ammunition count for one equipment slot.</summary>
    private void DrawAmmo(ISnesAddressSpace bus, int itemIndex, ushort value, int byteOffset)
    {
        (presentation ?? throw new InvalidOperationException(
            "HUD ammunition requires installed presentation assets.")).ApplyAmmo(_tiles, itemIndex, value);
    }

    /// <summary>Rebuilds mutable presentation cells from current inventory, counters, reserve state, and selection.</summary>
    private void ApplyCurrentPresentationState(SamusState samus)
    {
        if (presentation is null) return;
        if ((samus.EquippedItems & (ushort)SamusEquipmentFlags.XrayScope) != 0)
            presentation.TryApplyIcon(_tiles, 4);
        if ((samus.EquippedItems & (ushort)SamusEquipmentFlags.GrappleBeam) != 0)
            presentation.TryApplyIcon(_tiles, 3);
        if (samus.MaxMissiles != 0) presentation.TryApplyIcon(_tiles, 0);
        if (samus.MaxSuperMissiles != 0) presentation.TryApplyIcon(_tiles, 1);
        if (samus.MaxPowerBombs != 0) presentation.TryApplyIcon(_tiles, 2);
        presentation.ApplyEnergy(_tiles, samus.Health, samus.MaxHealth);
        if (samus.MaxMissiles != 0) presentation.ApplyAmmo(_tiles, 0, samus.Missiles);
        if (samus.MaxSuperMissiles != 0) presentation.ApplyAmmo(_tiles, 1, samus.SuperMissiles);
        if (samus.MaxPowerBombs != 0) presentation.ApplyAmmo(_tiles, 2, samus.PowerBombs);
        if (samus.ReserveTankMode == 1) presentation.ApplyAutoReserve(_tiles, samus.ReserveEnergy != 0);
        presentation.ToggleItemHighlight(_tiles, samus.SelectedHudItem, presentation.SelectedPalette);
    }

    /// <summary>Maps a coordinate in the native 5-by-3 minimap window to the mutable HUD tile buffer.</summary>
    /// <param name="outputX">Column within the minimap window.</param>
    /// <param name="outputY">Row within the minimap window.</param>
    /// <returns>Row-major index in the 96-word mutable HUD buffer.</returns>
    private static int NativeMinimapCellIndex(int outputX, int outputY) =>
        26 + outputY * WidthInTiles + outputX;

}

/// <summary>Explicit inventory and resource values consumed when constructing or refreshing the HUD.</summary>
/// <param name="Health">Current energy units shown in the HUD.</param>
/// <param name="MaxHealth">Maximum energy capacity used to render the energy display.</param>
/// <param name="Missiles">Current missile count.</param>
/// <param name="MaxMissiles">Missile capacity; zero means the missile icon and count are absent.</param>
/// <param name="SuperMissiles">Current Super Missile count.</param>
/// <param name="MaxSuperMissiles">Super Missile capacity; zero means its icon and count are absent.</param>
/// <param name="PowerBombs">Current Power Bomb count.</param>
/// <param name="MaxPowerBombs">Power Bomb capacity; zero means its icon and count are absent.</param>
/// <param name="EquippedItems">Equipment bitset used to decide which equipment icons appear.</param>
/// <param name="SelectedItem">Native HUD item index whose selection highlight is applied.</param>
/// <param name="ReserveHealth">Stored reserve energy units.</param>
/// <param name="ReserveMode">Native reserve mode; value one enables the AUTO indicator.</param>
public readonly record struct HudSnapshot(
    ushort Health,
    ushort MaxHealth,
    ushort Missiles,
    ushort MaxMissiles,
    ushort SuperMissiles,
    ushort MaxSuperMissiles,
    ushort PowerBombs,
    ushort MaxPowerBombs,
    ushort EquippedItems,
    ushort SelectedItem,
    ushort ReserveHealth,
    ushort ReserveMode)
{
    /// <summary>A useful pre-item debugging state: 99 energy and no equipment.</summary>
    public static HudSnapshot CeresDebug => new(
        Health: 99,
        MaxHealth: 99,
        Missiles: 0,
        MaxMissiles: 0,
        SuperMissiles: 0,
        MaxSuperMissiles: 0,
        PowerBombs: 0,
        MaxPowerBombs: 0,
        EquippedItems: 0,
        SelectedItem: 0,
        ReserveHealth: 0,
        ReserveMode: 0);
}
