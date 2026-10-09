using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable three-row gameplay-HUD tilemap, visual patches and layout anchors.</summary>
public sealed class GameplayHudPresentation
{
    private readonly Dictionary<int, ushort> templateOverrides;
    private readonly Dictionary<int, ushort> topRowOverrides;
    private readonly Dictionary<int, ushort> healthDigits;
    private readonly Dictionary<int, ushort> ammoDigits;
    private readonly Dictionary<int, ushort> autoReserveOverrides;
    private readonly Dictionary<int, int> autoAnchors;
    private readonly Dictionary<int, int> energyTankAnchors;
    private readonly CompiledIcon missile, superMissile, powerBomb, grapple, xray;

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
        Blank = CompileCell(document.Blank, "HUD blank");
        FilledEnergyTank = CompileCell(document.EnergyTanks.Filled, "filled energy tank");
        EmptyEnergyTank = CompileCell(document.EnergyTanks.Empty, "empty energy tank");
        energyTankAnchors = CompileAnchors(document.EnergyTanks.Anchors, GameplayHudDefinitions.EnergyTankCount, "energy tank",
            tank => GameplayHudDefinitions.EnergyTankByteOffset(tank) / sizeof(ushort));
        healthDigits = CompileDigitOverrides(document.Digits.Health, "health digits");
        ammoDigits = CompileDigitOverrides(document.Digits.Ammo, "ammo digits");
        HealthAnchor = ValidateAnchor(document.Digits.HealthAnchor, 2, 1, "health digits");
        MissileAmmoAnchor = ValidateAnchor(document.Digits.MissileAnchor, 3, 1, "missile digits");
        SuperMissileAmmoAnchor = ValidateAnchor(document.Digits.SuperMissileAnchor, 2, 1, "Super Missile digits");
        PowerBombAmmoAnchor = ValidateAnchor(document.Digits.PowerBombAnchor, 2, 1, "Power Bomb digits");
        MinimapAnchor = ValidateAnchor(document.MinimapAnchor, 5, 3, "minimap");
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
    /// <summary>Native-order BG3 top-row bytes supplied at the existing queued DMA boundary.</summary>
    public ReadOnlyMemory<byte> TopRowTransfer
    {
        get
        {
            // This is the queued DMA output, not a cached stock lookup table.
            byte[] transfer = new byte[GameplayHudDefinitions.TopRowByteCount];
            for (int column = 0; column < GameplayHudDefinitions.Width; column++)
            {
                ushort word = topRowOverrides.TryGetValue(column, out ushort edited)
                    ? edited : GameplayHudDefinitions.TopRowWord(column);
                transfer[column * 2] = (byte)word;
                transfer[column * 2 + 1] = (byte)(word >> 8);
            }
            return transfer;
        }
    }
    /// <summary>Gets the tilemap word used to clear mutable HUD cells.</summary>
    public ushort Blank { get; }
    /// <summary>Gets the tilemap word for a filled energy tank.</summary>
    public ushort FilledEnergyTank { get; }
    /// <summary>Gets the tilemap word for an empty energy tank.</summary>
    public ushort EmptyEnergyTank { get; }
    /// <summary>Gets the BG palette index applied to the selected item icon.</summary>
    public int SelectedPalette { get; }
    /// <summary>Gets the BG palette index applied to deselected item icons.</summary>
    public int DeselectedPalette { get; }
    /// <summary>Gets the tile coordinate of the two health digits.</summary>
    public MapLabelPoint HealthAnchor { get; }
    /// <summary>Gets the tile coordinate of the three missile-ammo digits.</summary>
    public MapLabelPoint MissileAmmoAnchor { get; }
    /// <summary>Gets the tile coordinate of the two Super Missile ammo digits.</summary>
    public MapLabelPoint SuperMissileAmmoAnchor { get; }
    /// <summary>Gets the tile coordinate of the two Power Bomb ammo digits.</summary>
    public MapLabelPoint PowerBombAmmoAnchor { get; }
    /// <summary>Gets the upper-left tile coordinate of the five-by-three minimap.</summary>
    public MapLabelPoint MinimapAnchor { get; }

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
        int first = Index(icon.Anchor.X, icon.Anchor.Y);
        if (new SnesBgTilemapWord(tiles[first]).CharacterIndex != new SnesBgTilemapWord(Blank).CharacterIndex)
            return;
        for (int y = 0; y < CompiledIcon.Height; y++)
        for (int x = 0; x < icon.Width; x++)
            tiles[Index(icon.Anchor.X + x, icon.Anchor.Y + y)] = icon.Cell(y * icon.Width + x);
    }

    /// <summary>Draws energy tanks and the two-digit current-energy remainder.</summary>
    /// <param name="tiles">The mutable 32-by-3 HUD tilemap.</param>
    /// <param name="health">Current energy, including full tank hundreds.</param>
    /// <param name="maxHealth">Maximum energy used to determine the visible tank count.</param>
    public void ApplyEnergy(Span<ushort> tiles, ushort health, ushort maxHealth)
    {
        ValidateTilemap(tiles);
        int fullTanks = health / 100;
        int tankCount = Math.Min(maxHealth / 100, GameplayHudDefinitions.EnergyTankCount);
        for (int tank = 0; tank < tankCount; tank++)
            tiles[EnergyTankCell(tank)] = tank < fullTanks ? FilledEnergyTank : EmptyEnergyTank;
        DrawDigits(tiles, healthDigits, health % 100, HealthAnchor, 2);
    }

    /// <summary>Draws the ammunition count for a projectile item.</summary>
    /// <param name="tiles">The mutable 32-by-3 HUD tilemap.</param>
    /// <param name="itemIndex">Zero for missiles, one for Super Missiles, or two for Power Bombs.</param>
    /// <param name="value">The ammunition count to display.</param>
    public void ApplyAmmo(Span<ushort> tiles, int itemIndex, ushort value)
    {
        ValidateTilemap(tiles);
        (MapLabelPoint anchor, int digits) = itemIndex switch
        {
            0 => (MissileAmmoAnchor, 3),
            1 => (SuperMissileAmmoAnchor, 2),
            2 => (PowerBombAmmoAnchor, 2),
            _ => throw new ArgumentOutOfRangeException(nameof(itemIndex)),
        };
        DrawDigits(tiles, ammoDigits, value, anchor, digits);
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
            tiles[AutoReserveCell(cell)] = Blank;
    }

    /// <summary>Applies a palette to every nonblank cell of the selected item icon.</summary>
    /// <param name="tiles">The mutable 32-by-3 HUD tilemap.</param>
    /// <param name="selectedItem">The native one-based selected-item number.</param>
    /// <param name="palette">The BG palette index to apply.</param>
    public void ToggleItemHighlight(Span<ushort> tiles, ushort selectedItem, int palette)
    {
        ValidateTilemap(tiles);
        if ((uint)palette > 7) throw new ArgumentOutOfRangeException(nameof(palette));
        int itemIndex = selectedItem - 1;
        if ((uint)itemIndex >= GameplayHudDefinitions.IconNames.Length) return;
        CompiledIcon icon = Icon(itemIndex);
        for (int y = 0; y < CompiledIcon.Height; y++)
        for (int x = 0; x < icon.Width; x++)
        {
            int destination = Index(icon.Anchor.X + x, icon.Anchor.Y + y);
            if (tiles[destination] != Blank)
                tiles[destination] = new SnesBgTilemapWord(tiles[destination]).WithPaletteIndex(palette).Raw;
        }
    }

    /// <summary>Returns the linear HUD tilemap index for a minimap output coordinate.</summary>
    /// <param name="outputX">The zero-based minimap column.</param>
    /// <param name="outputY">The zero-based minimap row.</param>
    /// <returns>The corresponding cell index in the 32-by-3 HUD tilemap.</returns>
    public int MinimapCellIndex(int outputX, int outputY)
    {
        if ((uint)outputX >= 5 || (uint)outputY >= 3) throw new ArgumentOutOfRangeException(nameof(outputX));
        return Index(MinimapAnchor.X + outputX, MinimapAnchor.Y + outputY);
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
                MapPresentationFormat.JsonOptions) ??
                throw new InvalidDataException("Gameplay HUD presentation document is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid gameplay HUD presentation JSON.", error);
        }
        if (document.Version != GameplayHudDefinitions.Version || document.Template is null ||
            document.Blank is null || document.EnergyTanks is null || document.Digits is null ||
            document.AutoReserve is null || document.MinimapAnchor is null)
            throw new InvalidDataException("Gameplay HUD presentation requires version 2 and every named visual owner.");
        return new(document, source);
    }

    /// <summary>Validates and writes a gameplay-HUD presentation document as UTF-8 JSON.</summary>
    /// <param name="output">The destination stream.</param>
    /// <param name="document">The editable presentation document to serialize.</param>
    public static void Write(Stream output, GameplayHudPresentationDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }

    private static void DrawDigits(Span<ushort> tiles, Dictionary<int, ushort> glyphs, int value, MapLabelPoint anchor, int count)
    {
        int divisor = count == 3 ? 100 : 10;
        for (int digit = 0; digit < count; digit++)
        {
            int numeral = (value / divisor) % 10;
            tiles[Index(anchor.X + digit, anchor.Y)] = glyphs.TryGetValue(numeral, out ushort edited)
                ? edited : GameplayHudDefinitions.DigitWord(numeral);
            divisor /= 10;
        }
    }

    private CompiledIcon Icon(int itemIndex) => itemIndex switch
    {
        0 => missile, 1 => superMissile, 2 => powerBomb, 3 => grapple, 4 => xray,
        _ => throw new ArgumentOutOfRangeException(nameof(itemIndex)),
    };

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
            for (int x = 0; x < icon.Width; x++) Own(Index(icon.Anchor.X + x, icon.Anchor.Y + y), GameplayHudDefinitions.IconName(item));
        }
        for (int tank = 0; tank < GameplayHudDefinitions.EnergyTankCount; tank++) Own(EnergyTankCell(tank), "energy tank");
        for (int cell = 0; cell < GameplayHudDefinitions.AutoReserveCellCount; cell++) Own(AutoReserveCell(cell), "AUTO indicator");
        for (int x = 0; x < 2; x++) Own(Index(HealthAnchor.X + x, HealthAnchor.Y), "health digits");
        for (int x = 0; x < 3; x++) Own(Index(MissileAmmoAnchor.X + x, MissileAmmoAnchor.Y), "missile digits");
        for (int x = 0; x < 2; x++) Own(Index(SuperMissileAmmoAnchor.X + x, SuperMissileAmmoAnchor.Y), "Super Missile digits");
        for (int x = 0; x < 2; x++) Own(Index(PowerBombAmmoAnchor.X + x, PowerBombAmmoAnchor.Y), "Power Bomb digits");
        for (int y = 0; y < 3; y++)
        for (int x = 0; x < 5; x++) Own(MinimapCellIndex(x, y), "minimap");
    }

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
    private static Dictionary<int, ushort> CompileDigitOverrides(GameplayHudCell[]? cells, string name)
    {
        ushort[] compiled = CompileCells(cells, 10, name);
        var overrides = new Dictionary<int, ushort>();
        for (int digit = 0; digit < compiled.Length; digit++)
            if (compiled[digit] != GameplayHudDefinitions.DigitWord(digit)) overrides.Add(digit, compiled[digit]);
        return overrides;
    }
    private static ushort[] CompileCells(GameplayHudCell[]? cells, int count, string name)
    {
        if (cells is null || cells.Length != count)
            throw new InvalidDataException($"{name} requires exactly {count} cells.");
        var result = new ushort[count];
        for (int index = 0; index < count; index++)
            result[index] = CompileCell(cells[index], $"{name} cell {index}");
        return result;
    }

    private static ushort CompileCell(GameplayHudCell? cell, string name)
    {
        if (cell is null || cell.TileColumn is < 0 or > 31 || cell.TileRow is < 0 or > 31 ||
            (uint)cell.Palette > 7)
            throw new InvalidDataException($"{name} has invalid tile or palette coordinates.");
        return SnesBgTilemapWord.Create(cell.TileRow * 32 + cell.TileColumn, cell.Palette,
            cell.Priority, (cell.FlipX ? SnesTileFlipFlags.Horizontal : 0) |
            (cell.FlipY ? SnesTileFlipFlags.Vertical : 0)).Raw;
    }

    private int EnergyTankCell(int tank) => energyTankAnchors.TryGetValue(tank, out int cell)
        ? cell : GameplayHudDefinitions.EnergyTankByteOffset(tank) / sizeof(ushort);

    private int AutoReserveCell(int index) => autoAnchors.TryGetValue(index, out int cell)
        ? cell : GameplayHudDefinitions.AutoReserveCellIndex(index);

    private static Dictionary<int, int> CompileAnchors(MapLabelPoint[]? anchors, int count,
        string name, Func<int, int> defaultCell)
    {
        if (anchors is null || anchors.Length != count)
            throw new InvalidDataException($"Gameplay HUD {name} requires exactly {count} anchors.");
        var overrides = new Dictionary<int, int>();
        for (int index = 0; index < count; index++)
        {
            int cell = Index(ValidateAnchor(anchors[index], 1, 1, $"{name} {index}"));
            if (cell != defaultCell(index)) overrides.Add(index, cell);
        }
        return overrides;
    }
    private static MapLabelPoint ValidateAnchor(MapLabelPoint? anchor, int width, int height, string name)
    {
        if (anchor is null || anchor.X < 0 || anchor.Y < 0 ||
            anchor.X + width > GameplayHudDefinitions.Width || anchor.Y + height > GameplayHudDefinitions.Height)
            throw new InvalidDataException($"Gameplay HUD {name} lies outside the 32x3 mutable tilemap.");
        return anchor;
    }

    private static int ValidatePalette(int palette, string name) => (uint)palette <= 7
        ? palette
        : throw new InvalidDataException($"Gameplay HUD {name} must be in range 0..7.");

    private static int Index(MapLabelPoint point) => Index(point.X, point.Y);
    private static int Index(int x, int y) => y * GameplayHudDefinitions.Width + x;
    private static void ValidateTilemap(Span<ushort> tiles)
    {
        if (tiles.Length != GameplayHudDefinitions.CellCount)
            throw new ArgumentException("Gameplay HUD requires exactly 96 mutable cells.", nameof(tiles));
    }

    private sealed class CompiledIcon
    {
        private readonly int item;
        private readonly Dictionary<int, ushort> edits = new();
        internal int Width => GameplayHudDefinitions.IconWidth(item);
        internal const int Height = 2;

        [AllowNull]
        internal MapLabelPoint Anchor => field ?? StockAnchor(item);
        private static MapLabelPoint StockAnchor(int item)
        {
            int index = GameplayHudDefinitions.ItemByteOffset(item) / sizeof(ushort);
            return new(index % GameplayHudDefinitions.Width, index / GameplayHudDefinitions.Width);
        }
        internal CompiledIcon(int item, GameplayHudIconDocument? document)
        {
            this.item = item;
            string name = GameplayHudDefinitions.IconName(item);
            if (document is null) throw new InvalidDataException($"Gameplay HUD icon {name} is null.");
            MapLabelPoint anchor = ValidateAnchor(document.Anchor, Width, Height, $"{name} icon");
            Anchor = anchor == StockAnchor(item) ? null : anchor;
            ushort[] cells = CompileCells(document.Cells, Width * Height, $"{name} icon");
            for (int cell = 0; cell < cells.Length; cell++)
                if (cells[cell] != GameplayHudDefinitions.IconWord(item, cell)) edits.Add(cell, cells[cell]);
        }
        internal ushort Cell(int index) => edits.TryGetValue(index, out ushort edited)
            ? edited : GameplayHudDefinitions.IconWord(item, index);
    }
}

/// <summary>Serializable versioned document describing the editable gameplay HUD.</summary>
public sealed record GameplayHudPresentationDocument
{
    /// <summary>Gets the gameplay-HUD document schema version.</summary>
    public required int Version { get; init; }
    /// <summary>Gets the immutable 32-cell top row transferred at the HUD DMA boundary.</summary>
    public required GameplayHudCell[] TopRow { get; init; }
    /// <summary>Gets the complete 32-by-3 mutable HUD tilemap template.</summary>
    public required GameplayHudCell[] Template { get; init; }
    /// <summary>Gets the cell used to clear dynamic HUD regions.</summary>
    public required GameplayHudCell Blank { get; init; }
    /// <summary>Gets the palette index used for a selected item.</summary>
    public required int SelectedPalette { get; init; }
    /// <summary>Gets the palette index used for a deselected item.</summary>
    public required int DeselectedPalette { get; init; }
    /// <summary>Gets the upper-left coordinate of the minimap output region.</summary>
    public required MapLabelPoint MinimapAnchor { get; init; }
    /// <summary>Gets the energy-tank visuals and anchors.</summary>
    public required GameplayHudEnergyTankDocument EnergyTanks { get; init; }
    /// <summary>Gets the health and ammunition digit visuals and anchors.</summary>
    public required GameplayHudDigitDocument Digits { get; init; }
    /// <summary>Gets the AUTO reserve-indicator visuals and anchors.</summary>
    public required GameplayHudAutoReserveDocument AutoReserve { get; init; }
    /// <summary>Gets item-icon documents keyed by their canonical names.</summary>
    public required Dictionary<string, GameplayHudIconDocument> Icons { get; init; }
}

/// <summary>Serializable energy-tank cell definitions and HUD anchors.</summary>
public sealed record GameplayHudEnergyTankDocument
{
    /// <summary>Gets the ordered energy-tank cell coordinates.</summary>
    public required MapLabelPoint[] Anchors { get; init; }
    /// <summary>Gets the cell drawn for a filled energy tank.</summary>
    public required GameplayHudCell Filled { get; init; }
    /// <summary>Gets the cell drawn for an empty energy tank.</summary>
    public required GameplayHudCell Empty { get; init; }
}

/// <summary>Serializable numeric glyphs and their gameplay-HUD anchors.</summary>
public sealed record GameplayHudDigitDocument
{
    /// <summary>Gets the ten health-digit cells in numeral order.</summary>
    public required GameplayHudCell[] Health { get; init; }
    /// <summary>Gets the ten ammunition-digit cells in numeral order.</summary>
    public required GameplayHudCell[] Ammo { get; init; }
    /// <summary>Gets the anchor of the health digits.</summary>
    public required MapLabelPoint HealthAnchor { get; init; }
    /// <summary>Gets the anchor of the missile digits.</summary>
    public required MapLabelPoint MissileAnchor { get; init; }
    /// <summary>Gets the anchor of the Super Missile digits.</summary>
    public required MapLabelPoint SuperMissileAnchor { get; init; }
    /// <summary>Gets the anchor of the Power Bomb digits.</summary>
    public required MapLabelPoint PowerBombAnchor { get; init; }
}

/// <summary>Serializable AUTO reserve-indicator cells and their HUD anchors.</summary>
public sealed record GameplayHudAutoReserveDocument
{
    /// <summary>Gets the ordered AUTO indicator cell coordinates.</summary>
    public required MapLabelPoint[] Anchors { get; init; }
    /// <summary>Gets the six cells drawn when reserve energy is available.</summary>
    public required GameplayHudCell[] ContainsEnergy { get; init; }
    /// <summary>Gets the six cells drawn when the reserve is empty.</summary>
    public required GameplayHudCell[] Empty { get; init; }
}

/// <summary>Serializable item-icon anchor and tile cells.</summary>
public sealed record GameplayHudIconDocument
{
    /// <summary>Gets the upper-left coordinate of the icon.</summary>
    public required MapLabelPoint Anchor { get; init; }
    /// <summary>Gets the icon cells in row-major order.</summary>
    public required GameplayHudCell[] Cells { get; init; }
}

/// <summary>Serializable components of one SNES BG tilemap word.</summary>
public sealed record GameplayHudCell
{
    /// <summary>Gets the zero-based character column in the 32-by-32 character page.</summary>
    public required int TileColumn { get; init; }
    /// <summary>Gets the zero-based character row in the 32-by-32 character page.</summary>
    public required int TileRow { get; init; }
    /// <summary>Gets the three-bit BG palette index.</summary>
    public required int Palette { get; init; }
    /// <summary>Gets whether the BG tile uses high priority.</summary>
    public required bool Priority { get; init; }
    /// <summary>Gets whether the character is flipped horizontally.</summary>
    public required bool FlipX { get; init; }
    /// <summary>Gets whether the character is flipped vertically.</summary>
    public required bool FlipY { get; init; }
}
