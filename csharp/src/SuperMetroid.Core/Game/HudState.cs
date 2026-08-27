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
    private const int AutoReserveTableAddress = 0x80998b;
    private const int HealthDigitsAddress = 0x809dbf;
    private const int AmmoDigitsAddress = 0x809dd3;
    private const int AreaMapPointerTable = 0x82964a;
    private const int MapDataPointerTable = 0x829717;
    private const ushort BlankTile = 0x2c0f;
    private const ushort BlankMapTile = 0x2c1f;

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
    private readonly byte[] _exploredMapTiles = new byte[0x100];

    /// <summary>Native SNES tilemap words for debugger inspection.</summary>
    public ReadOnlySpan<ushort> Tiles => _tiles;

    /// <summary>Whether a ROM template has been copied into this state.</summary>
    public bool IsInitialized { get; private set; }

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

        ToggleItemHighlight(snapshot.SelectedItem, paletteBits: 0x1000);
        IsInitialized = true;
    }

    /// <summary>
    /// Ports the visible 5x3 result of <c>UpdateMinimap</c> at <c>$90:A91B</c> for a
    /// supplied room/map position, including exploration bits and the blinking center tile.
    /// </summary>
    public void UpdateMinimap(
        ISnesAddressSpace bus,
        byte areaIndex,
        byte roomMapX,
        byte roomMapY,
        int roomWidthInBlocks,
        int roomHeightInBlocks,
        ushort samusX,
        ushort samusY,
        byte nmiFrameCounter,
        bool hasAreaMap = false)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (!IsInitialized)
            throw new InvalidOperationException("Initialize the HUD before updating its minimap.");
        if (areaIndex >= 7)
            throw new ArgumentOutOfRangeException(nameof(areaIndex));
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

        MarkExplored(centerX, centerY);

        int areaMapPointerAddress = AreaMapPointerTable + areaIndex * 3;
        int areaMapAddress =
            bus.ReadByte(areaMapPointerAddress) |
            (bus.ReadByte(areaMapPointerAddress + 1) << 8) |
            (bus.ReadByte(areaMapPointerAddress + 2) << 16);
        ushort mapDataPointer = ReadRomWord(bus, MapDataPointerTable + areaIndex * 2);
        int mapDataAddress = 0x820000 | mapDataPointer;

        for (int outputY = 0; outputY < 3; outputY++)
        {
            int mapY = centerY + outputY - 1;
            for (int outputX = 0; outputX < 5; outputX++)
            {
                int destination = 26 + outputY * WidthInTiles + outputX;
                int mapX = (centerX + outputX - 2) & 0x3f;
                if ((uint)mapY >= 32)
                {
                    _tiles[destination] = BlankMapTile;
                    continue;
                }

                bool exists = ReadMapBit(bus, mapDataAddress, mapX, mapY);
                bool explored = ReadExplored(mapX, mapY);
                if (!explored && (!exists || !hasAreaMap))
                {
                    _tiles[destination] = BlankMapTile;
                    continue;
                }

                // A 64x32 SNES map is two adjacent 32x32 screens in VRAM order, not one
                // linear 64-word row. Preserve that page split when reading bank-$B5 data.
                int tilemapIndex =
                    (mapX & 31) + mapY * 32 + (mapX >= 32 ? 0x400 : 0);
                ushort mapTile = ReadRomWord(bus, areaMapAddress + tilemapIndex * 2);
                ushort palette = explored ? (ushort)0x2800 : (ushort)0x2c00;
                _tiles[destination] = (ushort)((mapTile & 0xc3ff) | palette);
            }
        }

        // Every eight NMI frames the native code ORs palette bits $1C00 into the center
        // tile. The other half-cycle leaves its explored palette intact, producing the
        // familiar blinking Samus location without a separate sprite.
        if ((nmiFrameCounter & 8) == 0)
            _tiles[60] |= 0x1c00;
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
        int source = AutoReserveTableAddress + (containsEnergy ? 0 : 12);
        int[] destinations = [8, 9, 40, 41, 72, 73];
        for (int tile = 0; tile < destinations.Length; tile++)
            _tiles[destinations[tile]] = ReadRomWord(bus, source + tile * 2);
    }

    private void AddMissileIcon(ISnesAddressSpace bus)
    {
        // Unlike the other equipment icons, missiles are 3x2 tiles and occupy table words
        // 0-5 at $80:99A3. Only replace a blank slot, exactly like $80:99CF's guard.
        int destination = 0x14 / 2;
        if ((_tiles[destination] & 0x03ff) != (BlankTile & 0x03ff))
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
        if ((_tiles[destination] & 0x03ff) != (BlankTile & 0x03ff))
            return;

        _tiles[destination] = ReadRomWord(bus, source);
        _tiles[destination + 1] = ReadRomWord(bus, source + 2);
        _tiles[destination + WidthInTiles] = ReadRomWord(bus, source + 4);
        _tiles[destination + WidthInTiles + 1] = ReadRomWord(bus, source + 6);
    }

    private void ToggleItemHighlight(ushort selectedItem, ushort paletteBits)
    {
        int itemIndex = selectedItem - 1;
        if ((uint)itemIndex >= ItemByteOffsets.Length)
            return;

        int destination = ItemByteOffsets[itemIndex] / 2;
        ApplyPaletteBitsUnlessBlank(destination, paletteBits);
        ApplyPaletteBitsUnlessBlank(destination + 1, paletteBits);
        ApplyPaletteBitsUnlessBlank(destination + WidthInTiles, paletteBits);
        ApplyPaletteBitsUnlessBlank(destination + WidthInTiles + 1, paletteBits);

        if (itemIndex == 0)
        {
            // The missile selector spans the icon's third column.
            ApplyPaletteBitsUnlessBlank(destination + 2, paletteBits);
            ApplyPaletteBitsUnlessBlank(destination + WidthInTiles + 2, paletteBits);
        }
    }

    private void ApplyPaletteBitsUnlessBlank(int tileIndex, ushort paletteBits)
    {
        if (_tiles[tileIndex] != BlankTile)
        {
            // Mask $E3FF clears palette bits 10-12 and retains character, priority, and
            // flips. The caller supplies the desired already-shifted palette selection.
            _tiles[tileIndex] = (ushort)((_tiles[tileIndex] & 0xe3ff) | paletteBits);
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
        int bank = address & 0xff0000;
        int offset = address & 0xffff;
        return (ushort)(bus.ReadByte(bank | offset) | (bus.ReadByte(bank | ((offset + 1) & 0xffff)) << 8));
    }

    private void MarkExplored(int mapX, int mapY)
    {
        int byteIndex = (mapX >> 3) + 4 * ((mapX & 0x20) + mapY);
        _exploredMapTiles[byteIndex] |= (byte)(0x80 >> (mapX & 7));
    }

    private bool ReadExplored(int mapX, int mapY)
    {
        int byteIndex = (mapX >> 3) + 4 * ((mapX & 0x20) + mapY);
        return (_exploredMapTiles[byteIndex] & (0x80 >> (mapX & 7))) != 0;
    }

    private static bool ReadMapBit(ISnesAddressSpace bus, int mapDataAddress, int mapX, int mapY)
    {
        int byteIndex = (mapX >> 3) + 4 * ((mapX & 0x20) + mapY);
        return (bus.ReadByte(mapDataAddress + byteIndex) & (0x80 >> (mapX & 7))) != 0;
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
