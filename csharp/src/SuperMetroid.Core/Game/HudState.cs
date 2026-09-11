using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The three mutable 32-tile rows of Super Metroid's gameplay HUD tilemap, corresponding
/// exactly to WRAM <c>$7E:C608-$7E:C6C7</c>.
/// </summary>
public sealed class HudState
{
    public const int WidthInTiles = 32;
    public const int MutableRowCount = 3;
    public const int MutableTileCount = WidthInTiles * MutableRowCount;
    public const int MutableByteCount = MutableTileCount * 2;
    public const int WorkRamAddress = 0x7ec608;
    public const ushort VramDestination = 0x5820;

    private const int TemplateAddress = 0x8098cb;
    private const int IconTableAddress = 0x8099a3;
    private const int HealthDigitsAddress = 0x809dbf;
    private const int AmmoDigitsAddress = 0x809dd3;
    private const ushort BlankTile = 0x2c0f;

    // Byte offsets from $7E:C608, preserved from $80:9BCF. Seven tanks occupy row two;
    // the next seven wrap to row one, matching the retail HUD's two-line layout.
    private static readonly ushort[] EnergyTankByteOffsets =
    [
        0x42, 0x44, 0x46, 0x48, 0x4a, 0x4c, 0x4e,
        0x02, 0x04, 0x06, 0x08, 0x0a, 0x0c, 0x0e,
    ];

    // Byte offsets of missiles, supers, power bombs, grapple, and X-ray. The first icon is
    // 3x2 tiles; the remaining four are 2x2, which explains the special-case extra column.
    private static readonly ushort[] ItemByteOffsets = [0x14, 0x1c, 0x22, 0x28, 0x2e];

    private readonly ushort[] _tiles = new ushort[MutableTileCount];
    private ushort _previousSelectedItem;

    /// <summary>Native SNES tilemap words for debugger inspection.</summary>
    public ReadOnlySpan<ushort> Tiles => _tiles;

    /// <summary>Whether a ROM template has been copied into this state.</summary>
    public bool IsInitialized { get; private set; }

    /// <summary>
    /// One-frame publication of <c>QueueSfx1_Max6($39)</c> from <c>$80:9B44</c>.
    /// The frontend consumes it after the runtime frame has completed.
    /// </summary>
    public bool SelectionSoundRequestedThisFrame { get; private set; }

    /// <summary>Absolute 0-63 area-map X tile selected by the latest minimap update.</summary>
    public byte MinimapCenterX { get; private set; }

    /// <summary>Absolute 0-31 area-map Y tile selected by the latest minimap update.</summary>
    public byte MinimapCenterY { get; private set; }

    /// <summary>
    /// Ports the visible data work of <c>$80:9A79</c> and its first
    /// <c>$80:9B44</c> update for an explicit inventory snapshot.
    /// </summary>
    public void Initialize(ISnesAddressSpace bus, HudSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(bus);

        // $80:9AA3 copies exactly $C0 bytes (three 32-word rows) from the ROM template.
        for (int tile = 0; tile < MutableTileCount; tile++)
            _tiles[tile] = ReadRomWord(bus, TemplateAddress + tile * 2);

        if (snapshot.EquippedItems.HasAny(SamusEquipmentFlags.XrayScope))
            AddTwoByTwoIcon(bus, itemIndex: 4, IconTableAddress + 36);
        if (snapshot.EquippedItems.HasAny(SamusEquipmentFlags.GrappleBeam))
            AddTwoByTwoIcon(bus, itemIndex: 3, IconTableAddress + 28);
        if (snapshot.MaxMissiles != 0)
            AddMissileIcon(bus);
        if (snapshot.MaxSuperMissiles != 0)
            AddTwoByTwoIcon(bus, itemIndex: 1, IconTableAddress + 12);
        if (snapshot.MaxPowerBombs != 0)
            AddTwoByTwoIcon(bus, itemIndex: 2, IconTableAddress + 20);

        if (snapshot.ReserveMode == 1)
            DrawAutoReserve(bus, snapshot.ReserveHealth != 0);

        DrawHealth(bus, snapshot.Health, snapshot.MaxHealth);
        if (snapshot.MaxMissiles != 0)
            DrawThreeDigits(bus, AmmoDigitsAddress, snapshot.Missiles, byteOffset: 0x94);
        if (snapshot.MaxSuperMissiles != 0)
            DrawTwoDigits(bus, AmmoDigitsAddress, snapshot.SuperMissiles, byteOffset: 0x9c);
        if (snapshot.MaxPowerBombs != 0)
            DrawTwoDigits(bus, AmmoDigitsAddress, snapshot.PowerBombs, byteOffset: 0xa2);

        ToggleItemHighlight(snapshot.SelectedItem, paletteIndex: 4);
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
        bool timeIsFrozen = false)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        if (!IsInitialized)
            throw new InvalidOperationException("Initialize the HUD before updating gameplay counters.");

        SelectionSoundRequestedThisFrame = false;

        // Permanent pickups can introduce an inventory family after HUD initialization.
        // The cartridge's item routines patch those blank icon cells immediately; replay
        // the same guarded writers here before updating their counters. Each writer is
        // idempotent and preserves an already-installed icon and live minimap state.
        if ((samus.EquippedItems & (ushort)SamusEquipmentFlags.XrayScope) != 0)
            AddTwoByTwoIcon(bus, itemIndex: 4, IconTableAddress + 36);
        if ((samus.EquippedItems & (ushort)SamusEquipmentFlags.GrappleBeam) != 0)
            AddTwoByTwoIcon(bus, itemIndex: 3, IconTableAddress + 28);
        if (samus.MaxMissiles != 0)
            AddMissileIcon(bus);
        if (samus.MaxSuperMissiles != 0)
            AddTwoByTwoIcon(bus, itemIndex: 1, IconTableAddress + 12);
        if (samus.MaxPowerBombs != 0)
            AddTwoByTwoIcon(bus, itemIndex: 2, IconTableAddress + 20);

        DrawHealth(bus, samus.Health, samus.MaxHealth);
        if (samus.MaxMissiles != 0)
            DrawThreeDigits(bus, AmmoDigitsAddress, samus.Missiles, byteOffset: 0x94);
        if (samus.MaxSuperMissiles != 0)
            DrawTwoDigits(bus, AmmoDigitsAddress, samus.SuperMissiles, byteOffset: 0x9c);
        if (samus.MaxPowerBombs != 0)
            DrawTwoDigits(bus, AmmoDigitsAddress, samus.PowerBombs, byteOffset: 0xa2);
        if (samus.ReserveTankMode == 1)
            DrawAutoReserve(bus, samus.ReserveEnergy != 0);

        // `$80:9BD3-$9C20` changes the newly selected icon to palette four, restores the
        // old icon to palette five, then publishes sound $39. This belongs in the live HUD
        // update rather than the input handler: native suppresses the sound (but not the
        // visual change) while spinning, wall-jumping, grappling, or time is frozen.
        if (samus.SelectedHudItem != _previousSelectedItem)
        {
            ToggleItemHighlight(samus.SelectedHudItem, paletteIndex: 4);
            ToggleItemHighlight(_previousSelectedItem, paletteIndex: 5);
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
        int areaTableIndex = AreaIds.ToIndex(areaIndex);
        if (presentationMap is not null && presentationMap.Area != areaIndex)
            throw new ArgumentException("Map presentation belongs to a different area.", nameof(presentationMap));
        if (roomWidthInBlocks <= 0 || roomHeightInBlocks <= 0)
            throw new ArgumentOutOfRangeException(nameof(roomWidthInBlocks));

        // $90:A91B returns rather than indexing unrelated map memory if Samus is outside
        // the active room. The 16-pixel comparison consumes the same positions as collision.
        if ((samusX >> 4) >= roomWidthInBlocks || (samusY >> 4) >= roomHeightInBlocks)
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

        int areaMapPointerAddress = AreaMapRomData.TilemapPointerTable + areaTableIndex * 3;
        int areaMapAddress = presentationMap is null ?
            bus.ReadByte(areaMapPointerAddress) |
            (bus.ReadByte(areaMapPointerAddress + 1) << 8) |
            (bus.ReadByte(areaMapPointerAddress + 2) << 16) : 0;
        ushort mapDataPointer = presentationMap is null ? ReadRomWord(
            bus,
            AreaMapRomData.StationRevealMaskPointerTable + areaTableIndex * 2) : (ushort)0;
        int mapDataAddress = AreaMapRomData.StationRevealMaskBank | mapDataPointer;

        for (int outputY = 0; outputY < 3; outputY++)
        {
            int mapY = centerY + outputY - 1;
            for (int outputX = 0; outputX < 5; outputX++)
            {
                int destination = 26 + outputY * WidthInTiles + outputX;
                int mapX = (centerX + outputX - 2) & 0x3f;
                if ((uint)mapY >= 32)
                {
                    _tiles[destination] = (ushort)MapTileWords.HudBlank;
                    continue;
                }

                bool explored = system.IsMapTileExplored(areaIndex, mapX, mapY);
                bool stationVisible = presentationMap?.IsRevealedByMapStation(mapX, mapY) ??
                    ReadMapBit(bus, mapDataAddress, mapX, mapY);

                // A 64x32 SNES map is two adjacent 32x32 screens in VRAM order, not one
                // linear 64-word row. Preserve that page split when reading bank-$B5 data.
                int tilemapIndex = AreaMapLayout.GetTilemapWordIndex(mapX, mapY);
                MapTileWord mapTile = presentationMap?.GetTile(mapX, mapY) ??
                    (MapTileWord)ReadRomWord(bus, areaMapAddress + tilemapIndex * 2);
                if (!AreaMapVisibility.IsVisible(
                        explored,
                        hasAreaMap,
                        stationVisible,
                        presentationMap?.IsDiscoverable(mapX, mapY) ?? !mapTile.IsBlank,
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
                    (presentationMap?.RevealsCellAbove(mapX, mapY) ??
                     ((mapTile.Raw & MapTileWords.SlopedHallwayIdentityMask) == MapTileWords.SlopedHallwayCharacter)))
                    system.MarkExploredMapTile(areaIndex, centerX, centerY - 1);
            }
        }

        // Every eight NMI frames the native code ORs palette bits $1C00 into the center
        // tile. The other half-cycle leaves its explored palette intact, producing the
        // familiar blinking Samus location without a separate sprite.
        if ((nmiFrameCounter & 8) == 0)
            _tiles[60] = (ushort)new MapTileWord(_tiles[60]).WithLocationBlink();
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

    private void DrawHealth(ISnesAddressSpace bus, ushort health, ushort maxHealth)
    {
        int fullTanks = health / 100;
        int tankCount = Math.Min(maxHealth / 100, EnergyTankByteOffsets.Length);
        for (int tank = 0; tank < tankCount; tank++)
        {
            // $2831 is a filled E-tank and $3430 is empty. These complete tilemap words
            // include their different palette selection, not merely a character number.
            _tiles[EnergyTankByteOffsets[tank] / 2] = tank < fullTanks ? (ushort)0x2831 : (ushort)0x3430;
        }

        DrawTwoDigits(bus, HealthDigitsAddress, (ushort)(health % 100), byteOffset: 0x8c);
    }

    private void DrawAutoReserve(ISnesAddressSpace bus, bool containsEnergy)
    {
        ReadOnlySpan<int> destinations = HudReserveLayout.TileIndices;
        int source = HudReserveLayout.AutoTable + (containsEnergy ? 0 : destinations.Length * 2);
        for (int tile = 0; tile < destinations.Length; tile++)
            _tiles[destinations[tile]] = ReadRomWord(bus, source + tile * 2);
    }

    /// <summary>Ports $82:AF33 without reinitializing icons, counters or the minimap.</summary>
    public void ClearAutoReserveIndicator()
    {
        if (!IsInitialized) throw new InvalidOperationException("Initialize the HUD before clearing AUTO.");
        foreach (int index in HudReserveLayout.TileIndices)
            _tiles[index] = HudReserveLayout.Blank;
    }

    private void AddMissileIcon(ISnesAddressSpace bus)
    {
        // Unlike the other equipment icons, missiles are 3x2 tiles and occupy table words
        // 0-5 at $80:99A3. Only replace a blank slot, exactly like $80:99CF's guard.
        int destination = 0x14 / 2;
        if (new SnesBgTilemapWord(_tiles[destination]).CharacterIndex !=
            new SnesBgTilemapWord(BlankTile).CharacterIndex)
            return;

        _tiles[destination] = ReadRomWord(bus, IconTableAddress);
        _tiles[destination + 1] = ReadRomWord(bus, IconTableAddress + 2);
        _tiles[destination + 2] = ReadRomWord(bus, IconTableAddress + 4);
        _tiles[destination + WidthInTiles] = ReadRomWord(bus, IconTableAddress + 6);
        _tiles[destination + WidthInTiles + 1] = ReadRomWord(bus, IconTableAddress + 8);
        _tiles[destination + WidthInTiles + 2] = ReadRomWord(bus, IconTableAddress + 10);
    }

    private void AddTwoByTwoIcon(ISnesAddressSpace bus, int itemIndex, int source)
    {
        int destination = ItemByteOffsets[itemIndex] / 2;
        if (new SnesBgTilemapWord(_tiles[destination]).CharacterIndex !=
            new SnesBgTilemapWord(BlankTile).CharacterIndex)
            return;

        _tiles[destination] = ReadRomWord(bus, source);
        _tiles[destination + 1] = ReadRomWord(bus, source + 2);
        _tiles[destination + WidthInTiles] = ReadRomWord(bus, source + 4);
        _tiles[destination + WidthInTiles + 1] = ReadRomWord(bus, source + 6);
    }

    private void ToggleItemHighlight(ushort selectedItem, int paletteIndex)
    {
        int itemIndex = selectedItem - 1;
        if ((uint)itemIndex >= ItemByteOffsets.Length)
            return;

        int destination = ItemByteOffsets[itemIndex] / 2;
        ApplyPaletteUnlessBlank(destination, paletteIndex);
        ApplyPaletteUnlessBlank(destination + 1, paletteIndex);
        ApplyPaletteUnlessBlank(destination + WidthInTiles, paletteIndex);
        ApplyPaletteUnlessBlank(destination + WidthInTiles + 1, paletteIndex);

        if (itemIndex == 0)
        {
            // The missile selector spans the icon's third column.
            ApplyPaletteUnlessBlank(destination + 2, paletteIndex);
            ApplyPaletteUnlessBlank(destination + WidthInTiles + 2, paletteIndex);
        }
    }

    private void ApplyPaletteUnlessBlank(int tileIndex, int paletteIndex)
    {
        if (_tiles[tileIndex] != BlankTile)
        {
            _tiles[tileIndex] = new SnesBgTilemapWord(_tiles[tileIndex])
                .WithPaletteIndex(paletteIndex);
        }
    }

    private void DrawThreeDigits(ISnesAddressSpace bus, int digitTable, ushort value, int byteOffset)
    {
        _tiles[byteOffset / 2] = ReadRomWord(bus, digitTable + value / 100 * 2);
        DrawTwoDigits(bus, digitTable, (ushort)(value % 100), byteOffset + 2);
    }

    private void DrawTwoDigits(ISnesAddressSpace bus, int digitTable, ushort value, int byteOffset)
    {
        int destination = byteOffset / 2;
        _tiles[destination] = ReadRomWord(bus, digitTable + value / 10 * 2);
        _tiles[destination + 1] = ReadRomWord(bus, digitTable + value % 10 * 2);
    }

    private static ushort ReadRomWord(ISnesAddressSpace bus, int address)
    {
        // Every table used here remains within bank $80, but wrapping the offset documents
        // the 65C816 absolute/long access behavior and avoids accidental linear-bank reads.
        SnesAddress source = SnesAddress.FromBusAddress(address);
        return (ushort)(
            bus.ReadByte((int)source) |
            (bus.ReadByte((int)source.AddWithinBank(1)) << 8));
    }

    private static bool ReadMapBit(ISnesAddressSpace bus, int mapDataAddress, int mapX, int mapY)
    {
        int byteIndex = AreaMapLayout.GetBitByteIndex(mapX, mapY);
        return (bus.ReadByte(mapDataAddress + byteIndex) & AreaMapLayout.GetBitMask(mapX)) != 0;
    }
}

/// <summary>Explicit inputs consumed while constructing one HUD state.</summary>
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
