using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>Editable three-row gameplay-HUD tilemap, visual patches and layout anchors.</summary>
public sealed class GameplayHudPresentation
{
    /// <summary>Edited template cells keyed by their 32-by-3 tilemap index.</summary>
    internal readonly Dictionary<int, ushort> templateOverrides;
    /// <summary>Edited cells in the separately transferred immutable top row.</summary>
    internal readonly Dictionary<int, ushort> topRowOverrides;
    /// <summary>Edited decimal glyph words used by the health field.</summary>
    internal readonly Dictionary<int, ushort> healthDigits;
    /// <summary>Edited decimal glyph words shared by ammunition fields.</summary>
    internal readonly Dictionary<int, ushort> ammoDigits;
    /// <summary>Edited AUTO reserve cells for filled and empty states.</summary>
    internal readonly Dictionary<int, ushort> autoReserveOverrides;
    /// <summary>Non-stock tilemap positions for the six AUTO indicator cells.</summary>
    internal readonly Dictionary<int, int> autoAnchors;
    /// <summary>Non-stock tilemap positions for energy tank cells.</summary>
    internal readonly Dictionary<int, int> energyTankAnchors;
    /// <summary>Compiled item icons in the cartridge's HUD selection order.</summary>
    internal readonly CompiledIcon missile, superMissile, powerBomb, grapple, xray;

    /// <summary>Validates layout and stores only cells differing from compiled stock art.</summary>
    private GameplayHudPresentation(GameplayHudPresentationDocument document, byte[] source)
    {
        ushort[] topRow = CompileCells(document.TopRow, GameplayHudDefinitions.TopRowCellCount,
            "HUD immutable top row");
        topRowOverrides = new();
        for (int index = 0; index < topRow.Length; index++)
            if (topRow[index] != GameplayHudDefinitions.TopRowWord(index)) topRowOverrides.Add(index, topRow[index]);
        ushort[] template = CompileCells(document.Template, GameplayHudDefinitions.CellCount, "HUD template");
        templateOverrides = new();
        for (int index = 0; index < template.Length; index++)
            if (template[index] != GameplayHudDefinitions.TemplateWord(index)) templateOverrides.Add(index, template[index]);
        Blank = new SnesBgTilemapWord(CompileCell(document.Blank, "HUD blank"));
        FilledEnergyTank = new SnesBgTilemapWord(CompileCell(document.EnergyTanks.Filled, "filled energy tank"));
        EmptyEnergyTank = new SnesBgTilemapWord(CompileCell(document.EnergyTanks.Empty, "empty energy tank"));
        energyTankAnchors = CompileAnchors(document.EnergyTanks.Anchors, GameplayHudDefinitions.EnergyTankCount, "energy tank",
            tank => GameplayHudDefinitions.EnergyTankCell(tank));
        healthDigits = CompileDigitOverrides(document.Digits.Health, "health digits");
        ammoDigits = CompileDigitOverrides(document.Digits.Ammo, "ammo digits");
        HealthAnchor = ValidateAnchor(document.Digits.HealthAnchor, new(2, 1), "health digits");
        MissileAmmoAnchor = ValidateAnchor(document.Digits.MissileAnchor, new(3, 1), "missile digits");
        SuperMissileAmmoAnchor = ValidateAnchor(document.Digits.SuperMissileAnchor, new(2, 1), "Super Missile digits");
        PowerBombAmmoAnchor = ValidateAnchor(document.Digits.PowerBombAnchor, new(2, 1), "Power Bomb digits");
        MinimapAnchor = ValidateAnchor(document.MinimapAnchor, MinimapSize, "minimap");
        autoReserveOverrides = CompileAutoReserve(document.AutoReserve);
        autoAnchors = CompileAnchors(document.AutoReserve.Anchors, GameplayHudDefinitions.AutoReserveCellCount, "AUTO indicator",
            GameplayHudDefinitions.AutoReserveCellIndex);
        SelectedPalette = ValidatePalette(document.SelectedPalette, nameof(document.SelectedPalette));
        DeselectedPalette = ValidatePalette(document.DeselectedPalette, nameof(document.DeselectedPalette));

        if (document.Icons is null || document.Icons.Count != GameplayHudDefinitions.IconNames.Length ||
            GameplayHudDefinitions.IconNames.Any(name => !document.Icons.ContainsKey(name)))
            throw new InvalidDataException("Gameplay HUD requires exactly the five named item icons.");
        missile = new(0, document.Icons["Missile"]);
        superMissile = new(1, document.Icons["SuperMissile"]);
        powerBomb = new(2, document.Icons["PowerBomb"]);
        grapple = new(3, document.Icons["Grapple"]);
        xray = new(4, document.Icons["XRay"]);
        ValidateDistinctDynamicCells();
        ContentIdentity = Convert.ToHexString(SHA256.HashData(source));
    }

    /// <summary>Gets the SHA-256 identity of the source presentation document.</summary>
    public string ContentIdentity { get; }
    /// <summary>The HUD's static top row, with any installed edits (#1348).</summary>
    public ushort[] TopRow()
    {
        var row = new ushort[GameplayHudDefinitions.Width];
        for (int column = 0; column < row.Length; column++)
            row[column] = topRowOverrides.TryGetValue(column, out ushort edited) ? edited : GameplayHudDefinitions.TopRowWord(column);
        return row;
    }

    /// <summary>Native-order BG3 top-row bytes supplied at the existing queued DMA boundary.</summary>
    public ReadOnlyMemory<byte> TopRowTransfer
    {
        get
        {
            ushort[] row = TopRow();
            byte[] transfer = new byte[GameplayHudDefinitions.TopRowByteCount];
            for (int column = 0; column < row.Length; column++)
            {
                transfer[column * 2] = (byte)row[column];
                transfer[column * 2 + 1] = SnesLittleEndian.HighByte(row[column]);
            }
            return transfer;
        }
    }
    /// <summary>Gets the tilemap word used to clear mutable HUD cells.</summary>
    public SnesBgTilemapWord Blank { get; }
    /// <summary>Gets the tilemap word for a filled energy tank.</summary>
    public SnesBgTilemapWord FilledEnergyTank { get; }
    /// <summary>Gets the tilemap word for an empty energy tank.</summary>
    public SnesBgTilemapWord EmptyEnergyTank { get; }
    /// <summary>Gets the BG palette index applied to the selected item icon.</summary>
    public int SelectedPalette { get; }
    /// <summary>Gets the BG palette index applied to deselected item icons.</summary>
    public int DeselectedPalette { get; }
    /// <summary>Gets the tile coordinate of the two health digits.</summary>
    public Position16<TilemapCells> HealthAnchor { get; }
    /// <summary>Gets the tile coordinate of the three missile-ammo digits.</summary>
    public Position16<TilemapCells> MissileAmmoAnchor { get; }
    /// <summary>Gets the tile coordinate of the two Super Missile ammo digits.</summary>
    public Position16<TilemapCells> SuperMissileAmmoAnchor { get; }
    /// <summary>Gets the tile coordinate of the two Power Bomb ammo digits.</summary>
    public Position16<TilemapCells> PowerBombAmmoAnchor { get; }
    /// <summary>Gets the upper-left tile coordinate of the five-by-three minimap.</summary>
    public Position16<TilemapCells> MinimapAnchor { get; }

    /// <summary>Writes the complete editable HUD template into a mutable tilemap.</summary>
    /// <param name="tiles">The 32-by-3 HUD tilemap to populate.</param>
    public void ApplyTemplate(Span<ushort> tiles)
    {
        ValidateTilemap(tiles);
        for (int index = 0; index < GameplayHudDefinitions.CellCount; index++)
            tiles[index] = templateOverrides.TryGetValue(index, out ushort edited)
                ? edited : GameplayHudDefinitions.TemplateWord(index);
    }

    /// <summary>Draws an item icon when its anchor cell is currently blank.</summary>
    /// <param name="tiles">The mutable 32-by-3 HUD tilemap.</param>
    /// <param name="itemIndex">The zero-based item icon index.</param>
    public void TryApplyIcon(Span<ushort> tiles, int itemIndex)
    {
        ValidateTilemap(tiles);
        CompiledIcon icon = Icon(itemIndex);
        int first = Index(icon.Anchor);
        if (new SnesBgTilemapWord(tiles[first]).CharacterIndex != Blank.CharacterIndex)
            return;
        for (int y = 0; y < CompiledIcon.Height; y++)
        for (int x = 0; x < icon.Width; x++)
            tiles[Index(icon.Anchor + new Offset16<TilemapCells>(checked((short)x), checked((short)y)))] = icon.Cell(y * icon.Width + x);
    }

    /// <summary>Draws energy tanks and the two-digit current-energy remainder.</summary>
    /// <param name="tiles">The mutable 32-by-3 HUD tilemap.</param>
    /// <param name="health">Current energy, including full tank hundreds.</param>
    /// <param name="maxHealth">Maximum energy used to determine the visible tank count.</param>
    public void ApplyEnergy(Span<ushort> tiles, int health, int maxHealth)
    {
        ValidateTilemap(tiles);
        int fullTanks = health / 100;
        int tankCount = Math.Min(maxHealth / 100, GameplayHudDefinitions.EnergyTankCount);
        for (int tank = 0; tank < tankCount; tank++)
            tiles[EnergyTankCell(tank)] = (tank < fullTanks ? FilledEnergyTank : EmptyEnergyTank).Raw;
        DrawDigits(tiles, healthDigits, health % 100, HealthAnchor, 2);
    }

    /// <summary>Draws the ammunition count for a projectile item.</summary>
    /// <param name="tiles">The mutable 32-by-3 HUD tilemap.</param>
    /// <param name="itemIndex">Zero for missiles, one for Super Missiles, or two for Power Bombs.</param>
    /// <param name="value">The ammunition count to display.</param>
    public void ApplyAmmo(Span<ushort> tiles, int itemIndex, int value)
    {
        ValidateTilemap(tiles);
        (Position16<TilemapCells> anchor, int digits) = itemIndex switch
        {
            0 => new HudAmmoField(MissileAmmoAnchor, 3),
            1 => new HudAmmoField(SuperMissileAmmoAnchor, 2),
            2 => new HudAmmoField(PowerBombAmmoAnchor, 2),
            _ => throw new ArgumentOutOfRangeException(nameof(itemIndex)),
        };
        // The digits divide the stored word, so a count the cartridge has drained below zero draws as its word.
        DrawDigits(tiles, ammoDigits, SamusQuantity.Encode(value), anchor, digits);
    }

    /// <summary>Draws the AUTO reserve indicator for the requested reserve-energy state.</summary>
    /// <param name="tiles">The mutable 32-by-3 HUD tilemap.</param>
    /// <param name="containsEnergy">Whether the reserve tank currently contains energy.</param>
    public void ApplyAutoReserve(Span<ushort> tiles, bool containsEnergy)
    {
        ValidateTilemap(tiles);
        for (int index = 0; index < GameplayHudDefinitions.AutoReserveCellCount; index++)
            tiles[AutoReserveCell(index)] = autoReserveOverrides.TryGetValue(index + (containsEnergy ? 0 : 6), out ushort edited)
                ? edited : GameplayHudDefinitions.AutoReserveWord(index, containsEnergy);
    }

    /// <summary>Replaces every AUTO reserve-indicator cell with the configured blank tile.</summary>
    /// <param name="tiles">The mutable 32-by-3 HUD tilemap.</param>
    public void ClearAutoReserve(Span<ushort> tiles)
    {
        ValidateTilemap(tiles);
        for (int cell = 0; cell < GameplayHudDefinitions.AutoReserveCellCount; cell++)
            tiles[AutoReserveCell(cell)] = Blank.Raw;
    }

    /// <summary>Applies a palette to every nonblank cell of the selected item icon.</summary>
    /// <param name="tiles">The mutable 32-by-3 HUD tilemap.</param>
    /// <param name="selectedItem">The native one-based selected-item number.</param>
    /// <param name="palette">The BG palette index to apply.</param>
    public void ToggleItemHighlight(Span<ushort> tiles, SuperMetroid.Core.Samus.HudItem selectedItem, int palette)
    {
        ValidateTilemap(tiles);
        if ((uint)palette > 7) throw new ArgumentOutOfRangeException(nameof(palette));
        int itemIndex = (int)selectedItem - 1;
        if ((uint)itemIndex >= GameplayHudDefinitions.IconNames.Length) return;
        CompiledIcon icon = Icon(itemIndex);
        for (int y = 0; y < CompiledIcon.Height; y++)
        for (int x = 0; x < icon.Width; x++)
        {
            int destination = Index(icon.Anchor + new Offset16<TilemapCells>(checked((short)x), checked((short)y)));
            if (tiles[destination] != Blank.Raw)
                tiles[destination] = new SnesBgTilemapWord(tiles[destination]).WithPaletteIndex(palette).Raw;
        }
    }

    /// <summary>The five-by-three minimap's size in HUD tilemap cells.</summary>
    public static Extent16<TilemapCells> MinimapSize => new(5, 3);

    /// <summary>Returns the linear HUD tilemap index for a minimap output coordinate.</summary>
    /// <param name="output">The zero-based minimap column and row.</param>
    /// <returns>The corresponding cell index in the 32-by-3 HUD tilemap.</returns>
    public int MinimapCellIndex(Position16<TilemapCells> output)
    {
        if (!MinimapSize.Contains(output)) throw new ArgumentOutOfRangeException(nameof(output));
        return Index(MinimapAnchor + (output - Position16<TilemapCells>.Zero));
    }

    /// <summary>Loads and validates a gameplay-HUD presentation document.</summary>
    /// <param name="json">The UTF-8 JSON document stream.</param>
    /// <returns>The compiled gameplay-HUD presentation.</returns>
    public static GameplayHudPresentation Load(Stream json)
    {
        byte[] source;
        using (var buffer = new MemoryStream())
        {
            json.CopyTo(buffer);
            source = buffer.ToArray();
        }
        GameplayHudPresentationDocument document;
        try
        {
            document = JsonAssetDocument.Read<GameplayHudPresentationDocument>(source,
                ArtworkJson.Options) ??
                throw new InvalidDataException("Gameplay HUD presentation document is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid gameplay HUD presentation JSON.", error);
        }
        if (document.Version != GameplayHudDefinitions.Version || document.Template is null ||
            document.Blank is null || document.EnergyTanks is null || document.Digits is null ||
            document.AutoReserve is null)
            throw new InvalidDataException("Gameplay HUD presentation requires version 2 and every named visual owner.");
        return new(document, source);
    }

    /// <summary>Validates and writes a gameplay-HUD presentation document as UTF-8 JSON.</summary>
    /// <param name="output">The destination stream.</param>
    /// <param name="document">The editable presentation document to serialize.</param>
    public static void Write(Stream output, GameplayHudPresentationDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }

    /// <summary>Writes two or three decimal digits, using edited glyphs where supplied.</summary>
    private static void DrawDigits(Span<ushort> tiles, Dictionary<int, ushort> glyphs, int value, Position16<TilemapCells> anchor, int count)
    {
        int divisor = count == 3 ? 100 : 10;
        for (int digit = 0; digit < count; digit++)
        {
            int numeral = (value / divisor) % 10;
            tiles[Index(anchor + new Offset16<TilemapCells>(checked((short)digit), 0))] = glyphs.TryGetValue(numeral, out ushort edited)
                ? edited : GameplayHudDefinitions.DigitWord(numeral);
            divisor /= 10;
        }
    }

    /// <summary>Resolves a native item selector to its compiled HUD icon.</summary>
    private CompiledIcon Icon(int itemIndex) => itemIndex switch
    {
        0 => missile, 1 => superMissile, 2 => powerBomb, 3 => grapple, 4 => xray,
        _ => throw new ArgumentOutOfRangeException(nameof(itemIndex)),
    };

    /// <summary>Rejects overlap among icons, tanks, digits, AUTO cells, and minimap cells.</summary>
    private void ValidateDistinctDynamicCells()
    {
        var owners = new Dictionary<int, string>();
        void Own(int cell, string owner)
        {
            if (!owners.TryAdd(cell, owner))
                throw new InvalidDataException($"Gameplay HUD {owner} overlaps {owners[cell]} at cell {cell}.");
        }
        for (int item = 0; item < GameplayHudDefinitions.ItemCount; item++)
        {
            CompiledIcon icon = Icon(item);
            for (int y = 0; y < CompiledIcon.Height; y++)
            for (int x = 0; x < icon.Width; x++) Own(Index(icon.Anchor + new Offset16<TilemapCells>(checked((short)x), checked((short)y))), GameplayHudDefinitions.IconName(item));
        }
        for (int tank = 0; tank < GameplayHudDefinitions.EnergyTankCount; tank++) Own(EnergyTankCell(tank), "energy tank");
        for (int cell = 0; cell < GameplayHudDefinitions.AutoReserveCellCount; cell++) Own(AutoReserveCell(cell), "AUTO indicator");
        for (int x = 0; x < 2; x++) Own(Index(HealthAnchor + new Offset16<TilemapCells>(checked((short)x), 0)), "health digits");
        for (int x = 0; x < 3; x++) Own(Index(MissileAmmoAnchor + new Offset16<TilemapCells>(checked((short)x), 0)), "missile digits");
        for (int x = 0; x < 2; x++) Own(Index(SuperMissileAmmoAnchor + new Offset16<TilemapCells>(checked((short)x), 0)), "Super Missile digits");
        for (int x = 0; x < 2; x++) Own(Index(PowerBombAmmoAnchor + new Offset16<TilemapCells>(checked((short)x), 0)), "Power Bomb digits");
        for (int y = 0; y < 3; y++)
        for (int x = 0; x < 5; x++) Own(MinimapCellIndex(new(checked((ushort)x), checked((ushort)y))), "minimap");
    }

    /// <summary>Keeps only AUTO indicator cells whose filled or empty art differs from stock.</summary>
    private static Dictionary<int, ushort> CompileAutoReserve(GameplayHudAutoReserveDocument document)
    {
        ushort[] full = CompileCells(document.ContainsEnergy, 6, "filled AUTO indicator");
        ushort[] empty = CompileCells(document.Empty, 6, "empty AUTO indicator");
        var overrides = new Dictionary<int, ushort>();
        for (int state = 0; state < 2; state++)
        for (int cell = 0; cell < 6; cell++)
        {
            ushort supplied = (state == 0 ? full : empty)[cell];
            if (supplied != GameplayHudDefinitions.AutoReserveWord(cell, state == 0))
                overrides.Add(state * 6 + cell, supplied);
        }
        return overrides;
    }
    /// <summary>Compiles ten decimal glyphs and retains only words differing from stock.</summary>
    private static Dictionary<int, ushort> CompileDigitOverrides(GameplayHudCell[]? cells, string name)
    {
        ushort[] compiled = CompileCells(cells, 10, name);
        var overrides = new Dictionary<int, ushort>();
        for (int digit = 0; digit < compiled.Length; digit++)
            if (compiled[digit] != GameplayHudDefinitions.DigitWord(digit)) overrides.Add(digit, compiled[digit]);
        return overrides;
    }
    /// <summary>Requires the expected cell count and packs authored cells into native tile words.</summary>
    private static ushort[] CompileCells(GameplayHudCell[]? cells, int count, string name)
    {
        if (cells is null || cells.Length != count)
            throw new InvalidDataException($"{name} requires exactly {count} cells.");
        var result = new ushort[count];
        for (int index = 0; index < count; index++)
            result[index] = CompileCell(cells[index], $"{name} cell {index}");
        return result;
    }

    /// <summary>Validates a character coordinate and palette, then packs its flip and priority bits.</summary>
    private static ushort CompileCell(GameplayHudCell? cell, string name)
    {
        if (cell is null || cell.TileColumn is < 0 or > 31 || cell.TileRow is < 0 or > 31 ||
            (uint)cell.Palette > 7)
            throw new InvalidDataException($"{name} has invalid tile or palette coordinates.");
        return SnesBgTilemapWord.Create(cell.TileRow * 32 + cell.TileColumn, cell.Palette,
            cell.Priority, (cell.FlipX ? SnesTileFlipFlags.Horizontal : 0) |
            (cell.FlipY ? SnesTileFlipFlags.Vertical : 0)).Raw;
    }

    /// <summary>Uses an edited tank position when present, otherwise the native HUD offset.</summary>
    private int EnergyTankCell(int tank) => energyTankAnchors.TryGetValue(tank, out int cell)
        ? cell : GameplayHudDefinitions.EnergyTankCell(tank);

    /// <summary>Uses an edited AUTO position when present, otherwise its stock cell.</summary>
    private int AutoReserveCell(int index) => autoAnchors.TryGetValue(index, out int cell)
        ? cell : GameplayHudDefinitions.AutoReserveCellIndex(index);

    /// <summary>Validates the complete anchor array and retains only non-stock positions.</summary>
    private static Dictionary<int, int> CompileAnchors(Position16<TilemapCells>[]? anchors, int count,
        string name, Func<int, int> defaultCell)
    {
        if (anchors is null || anchors.Length != count)
            throw new InvalidDataException($"Gameplay HUD {name} requires exactly {count} anchors.");
        var overrides = new Dictionary<int, int>();
        for (int index = 0; index < count; index++)
        {
            int cell = Index(ValidateAnchor(anchors[index], new(1, 1), $"{name} {index}"));
            if (cell != defaultCell(index)) overrides.Add(index, cell);
        }
        return overrides;
    }
    /// <summary>Ensures an anchored rectangle fits within the mutable 32-by-3 HUD tilemap.</summary>
    private static Position16<TilemapCells> ValidateAnchor(Position16<TilemapCells> anchor, Extent16<TilemapCells> size, string name)
    {
        if (anchor.X < 0 || anchor.Y < 0 ||
            anchor.X + size.X > GameplayHudDefinitions.Width || anchor.Y + size.Y > GameplayHudDefinitions.Height)
            throw new InvalidDataException($"Gameplay HUD {name} lies outside the 32x3 mutable tilemap.");
        return anchor;
    }

    /// <summary>Restricts a BG palette selector to its three-bit range.</summary>
    private static int ValidatePalette(int palette, string name) => (uint)palette <= 7
        ? palette
        : throw new InvalidDataException($"Gameplay HUD {name} must be in range 0..7.");

    /// <summary>Converts an HUD tile coordinate to the mutable row-major cell index.</summary>
    private static int Index(Position16<TilemapCells> point) => GameplayHudDefinitions.Size.RowMajorIndex(point);
    /// <summary>Requires a destination span for exactly three 32-cell HUD rows.</summary>
    private static void ValidateTilemap(Span<ushort> tiles)
    {
        if (tiles.Length != GameplayHudDefinitions.CellCount)
            throw new ArgumentException("Gameplay HUD requires exactly 96 mutable cells.", nameof(tiles));
    }

    /// <summary>Item icon art and placement, retaining only edits to native HUD definitions.</summary>
    internal sealed class CompiledIcon
    {
        /// <summary>Native item selector used to recover stock dimensions and cells.</summary>
        private readonly int item;
        /// <summary>Edited icon cells keyed by local row-major index.</summary>
        internal readonly Dictionary<int, ushort> edits = new();
        /// <summary>Stock width for the selected item icon.</summary>
        internal int Width => GameplayHudDefinitions.IconWidth(item);
        /// <summary>All item icons occupy two HUD tile rows.</summary>
        internal const int Height = 2;

        /// <summary>Authored placement when it differs from the item's stock anchor.</summary>
        internal readonly Position16<TilemapCells>? anchorOverride;
        /// <summary>Selected authored or stock upper-left tile coordinate.</summary>
        internal Position16<TilemapCells> Anchor => anchorOverride ?? StockAnchor(item);
        /// <summary>Converts the native item offset into a 32-column tile coordinate.</summary>
        private static Position16<TilemapCells> StockAnchor(int item)
        {
            int index = GameplayHudDefinitions.ItemCell(item);
            return new(checked((ushort)(index % GameplayHudDefinitions.Width)), checked((ushort)(index / GameplayHudDefinitions.Width)));
        }
        /// <summary>Validates the icon rectangle and stores only non-stock placement or art.</summary>
        internal CompiledIcon(int item, GameplayHudIconDocument? document)
        {
            this.item = item;
            string name = GameplayHudDefinitions.IconName(item);
            if (document is null) throw new InvalidDataException($"Gameplay HUD icon {name} is null.");
            Position16<TilemapCells> anchor = ValidateAnchor(document.Anchor, new(checked((ushort)Width), Height), $"{name} icon");
            anchorOverride = anchor == StockAnchor(item) ? null : anchor;
            ushort[] cells = CompileCells(document.Cells, Width * Height, $"{name} icon");
            for (int cell = 0; cell < cells.Length; cell++)
                if (cells[cell] != GameplayHudDefinitions.IconWord(item, cell)) edits.Add(cell, cells[cell]);
        }
        /// <summary>Reads an edited icon cell or its compiled stock tile word.</summary>
        internal ushort Cell(int index) => edits.TryGetValue(index, out ushort edited)
            ? edited : GameplayHudDefinitions.IconWord(item, index);
    }
}
