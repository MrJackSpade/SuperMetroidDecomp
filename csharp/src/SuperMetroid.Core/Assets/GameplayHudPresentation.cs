using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable three-row gameplay-HUD tilemap, visual patches and layout anchors.</summary>
public sealed class GameplayHudPresentation
{
    private readonly ushort[] template;
    private readonly byte[] topRowTransfer;
    private readonly Dictionary<int, ushort> healthDigits;
    private readonly Dictionary<int, ushort> ammoDigits;
    private readonly ushort[] autoReserveBasis;
    private readonly Dictionary<int, ushort> autoReserveOverrides;
    private readonly Dictionary<int, int> autoAnchors;
    private readonly Dictionary<int, int> energyTankAnchors;
    private readonly Dictionary<string, CompiledIcon> icons;

    private GameplayHudPresentation(GameplayHudPresentationDocument document, byte[] source)
    {
        ushort[] topRow = CompileCells(document.TopRow, GameplayHudDefinitions.TopRowCellCount,
            "HUD immutable top row");
        topRowTransfer = new byte[GameplayHudDefinitions.TopRowByteCount];
        for (int index = 0; index < topRow.Length; index++)
        {
            topRowTransfer[index * 2] = (byte)topRow[index];
            topRowTransfer[index * 2 + 1] = (byte)(topRow[index] >> 8);
        }
        template = CompileCells(document.Template, GameplayHudDefinitions.CellCount, "HUD template");
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
        (autoReserveBasis, autoReserveOverrides) = CompileAutoReserve(document.AutoReserve);
        autoAnchors = CompileAnchors(document.AutoReserve.Anchors, GameplayHudDefinitions.AutoReserveCellCount, "AUTO indicator",
            GameplayHudDefinitions.AutoReserveCellIndex);
        SelectedPalette = ValidatePalette(document.SelectedPalette, nameof(document.SelectedPalette));
        DeselectedPalette = ValidatePalette(document.DeselectedPalette, nameof(document.DeselectedPalette));

        if (document.Icons is null || document.Icons.Count != GameplayHudDefinitions.IconNames.Length ||
            GameplayHudDefinitions.IconNames.Any(name => !document.Icons.ContainsKey(name)))
            throw new InvalidDataException("Gameplay HUD requires exactly the five named item icons.");
        icons = new(StringComparer.Ordinal);
        for (int item = 0; item < GameplayHudDefinitions.IconNames.Length; item++)
        {
            string name = GameplayHudDefinitions.IconName(item);
            GameplayHudIconDocument icon = document.Icons[name] ??
                throw new InvalidDataException($"Gameplay HUD icon {name} is null.");
            int width = item == 0 ? 3 : 2;
            int height = 2;
            icons.Add(name, new(ValidateAnchor(icon.Anchor, width, height, $"{name} icon"),
                width, height, CompileCells(icon.Cells, width * height, $"{name} icon")));
        }

        ValidateDistinctDynamicCells();
        ContentIdentity = Convert.ToHexString(SHA256.HashData(source));
    }

    public string ContentIdentity { get; }
    /// <summary>Native-order BG3 top-row bytes supplied at the existing queued DMA boundary.</summary>
    public ReadOnlyMemory<byte> TopRowTransfer => topRowTransfer;
    public ushort Blank { get; }
    public ushort FilledEnergyTank { get; }
    public ushort EmptyEnergyTank { get; }
    public int SelectedPalette { get; }
    public int DeselectedPalette { get; }
    public MapLabelPoint HealthAnchor { get; }
    public MapLabelPoint MissileAmmoAnchor { get; }
    public MapLabelPoint SuperMissileAmmoAnchor { get; }
    public MapLabelPoint PowerBombAmmoAnchor { get; }
    public MapLabelPoint MinimapAnchor { get; }

    public void ApplyTemplate(Span<ushort> tiles)
    {
        ValidateTilemap(tiles);
        template.CopyTo(tiles);
    }

    public void TryApplyIcon(Span<ushort> tiles, int itemIndex)
    {
        ValidateTilemap(tiles);
        CompiledIcon icon = Icon(itemIndex);
        int first = Index(icon.Anchor.X, icon.Anchor.Y);
        if (new SnesBgTilemapWord(tiles[first]).CharacterIndex != new SnesBgTilemapWord(Blank).CharacterIndex)
            return;
        for (int y = 0; y < icon.Height; y++)
        for (int x = 0; x < icon.Width; x++)
            tiles[Index(icon.Anchor.X + x, icon.Anchor.Y + y)] = icon.Cells[y * icon.Width + x];
    }

    public void ApplyEnergy(Span<ushort> tiles, ushort health, ushort maxHealth)
    {
        ValidateTilemap(tiles);
        int fullTanks = health / 100;
        int tankCount = Math.Min(maxHealth / 100, GameplayHudDefinitions.EnergyTankCount);
        for (int tank = 0; tank < tankCount; tank++)
            tiles[EnergyTankCell(tank)] = tank < fullTanks ? FilledEnergyTank : EmptyEnergyTank;
        DrawDigits(tiles, healthDigits, health % 100, HealthAnchor, 2);
    }

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

    public void ApplyAutoReserve(Span<ushort> tiles, bool containsEnergy)
    {
        ValidateTilemap(tiles);
        for (int index = 0; index < GameplayHudDefinitions.AutoReserveCellCount; index++)
            tiles[AutoReserveCell(index)] = autoReserveOverrides.TryGetValue(index + (containsEnergy ? 0 : 6), out ushort edited)
                ? edited : GameplayHudDefinitions.AutoReserveWord(autoReserveBasis, index, containsEnergy);
    }

    public void ClearAutoReserve(Span<ushort> tiles)
    {
        ValidateTilemap(tiles);
        for (int cell = 0; cell < GameplayHudDefinitions.AutoReserveCellCount; cell++)
            tiles[AutoReserveCell(cell)] = Blank;
    }

    public void ToggleItemHighlight(Span<ushort> tiles, ushort selectedItem, int palette)
    {
        ValidateTilemap(tiles);
        if ((uint)palette > 7) throw new ArgumentOutOfRangeException(nameof(palette));
        int itemIndex = selectedItem - 1;
        if ((uint)itemIndex >= GameplayHudDefinitions.IconNames.Length) return;
        CompiledIcon icon = Icon(itemIndex);
        for (int y = 0; y < icon.Height; y++)
        for (int x = 0; x < icon.Width; x++)
        {
            int destination = Index(icon.Anchor.X + x, icon.Anchor.Y + y);
            if (tiles[destination] != Blank)
                tiles[destination] = new SnesBgTilemapWord(tiles[destination]).WithPaletteIndex(palette).Raw;
        }
    }

    public int MinimapCellIndex(int outputX, int outputY)
    {
        if ((uint)outputX >= 5 || (uint)outputY >= 3) throw new ArgumentOutOfRangeException(nameof(outputX));
        return Index(MinimapAnchor.X + outputX, MinimapAnchor.Y + outputY);
    }

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

    private CompiledIcon Icon(int itemIndex) => icons[GameplayHudDefinitions.IconName(itemIndex)];

    private void ValidateDistinctDynamicCells()
    {
        var owners = new Dictionary<int, string>();
        void Own(int cell, string owner)
        {
            if (!owners.TryAdd(cell, owner))
                throw new InvalidDataException($"Gameplay HUD {owner} overlaps {owners[cell]} at cell {cell}.");
        }
        foreach ((string name, CompiledIcon icon) in icons)
        for (int y = 0; y < icon.Height; y++)
        for (int x = 0; x < icon.Width; x++) Own(Index(icon.Anchor.X + x, icon.Anchor.Y + y), name);
        for (int tank = 0; tank < GameplayHudDefinitions.EnergyTankCount; tank++) Own(EnergyTankCell(tank), "energy tank");
        for (int cell = 0; cell < GameplayHudDefinitions.AutoReserveCellCount; cell++) Own(AutoReserveCell(cell), "AUTO indicator");
        for (int x = 0; x < 2; x++) Own(Index(HealthAnchor.X + x, HealthAnchor.Y), "health digits");
        for (int x = 0; x < 3; x++) Own(Index(MissileAmmoAnchor.X + x, MissileAmmoAnchor.Y), "missile digits");
        for (int x = 0; x < 2; x++) Own(Index(SuperMissileAmmoAnchor.X + x, SuperMissileAmmoAnchor.Y), "Super Missile digits");
        for (int x = 0; x < 2; x++) Own(Index(PowerBombAmmoAnchor.X + x, PowerBombAmmoAnchor.Y), "Power Bomb digits");
        for (int y = 0; y < 3; y++)
        for (int x = 0; x < 5; x++) Own(MinimapCellIndex(x, y), "minimap");
    }

    private static (ushort[] Basis, Dictionary<int, ushort> Overrides) CompileAutoReserve(GameplayHudAutoReserveDocument document)
    {
        ushort[] full = CompileCells(document.ContainsEnergy, 6, "filled AUTO indicator");
        ushort[] empty = CompileCells(document.Empty, 6, "empty AUTO indicator");
        ushort[] basis = full[..4];
        var overrides = new Dictionary<int, ushort>();
        for (int state = 0; state < 2; state++)
        for (int cell = 0; cell < 6; cell++)
        {
            ushort supplied = (state == 0 ? full : empty)[cell];
            if (supplied != GameplayHudDefinitions.AutoReserveWord(basis, cell, state == 0))
                overrides.Add(state * 6 + cell, supplied);
        }
        return (basis, overrides);
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

    private sealed record CompiledIcon(MapLabelPoint Anchor, int Width, int Height, ushort[] Cells);
}

public sealed record GameplayHudPresentationDocument
{
    public required int Version { get; init; }
    public required GameplayHudCell[] TopRow { get; init; }
    public required GameplayHudCell[] Template { get; init; }
    public required GameplayHudCell Blank { get; init; }
    public required int SelectedPalette { get; init; }
    public required int DeselectedPalette { get; init; }
    public required MapLabelPoint MinimapAnchor { get; init; }
    public required GameplayHudEnergyTankDocument EnergyTanks { get; init; }
    public required GameplayHudDigitDocument Digits { get; init; }
    public required GameplayHudAutoReserveDocument AutoReserve { get; init; }
    public required Dictionary<string, GameplayHudIconDocument> Icons { get; init; }
}

public sealed record GameplayHudEnergyTankDocument
{
    public required MapLabelPoint[] Anchors { get; init; }
    public required GameplayHudCell Filled { get; init; }
    public required GameplayHudCell Empty { get; init; }
}

public sealed record GameplayHudDigitDocument
{
    public required GameplayHudCell[] Health { get; init; }
    public required GameplayHudCell[] Ammo { get; init; }
    public required MapLabelPoint HealthAnchor { get; init; }
    public required MapLabelPoint MissileAnchor { get; init; }
    public required MapLabelPoint SuperMissileAnchor { get; init; }
    public required MapLabelPoint PowerBombAnchor { get; init; }
}

public sealed record GameplayHudAutoReserveDocument
{
    public required MapLabelPoint[] Anchors { get; init; }
    public required GameplayHudCell[] ContainsEnergy { get; init; }
    public required GameplayHudCell[] Empty { get; init; }
}

public sealed record GameplayHudIconDocument
{
    public required MapLabelPoint Anchor { get; init; }
    public required GameplayHudCell[] Cells { get; init; }
}

public sealed record GameplayHudCell
{
    public required int TileColumn { get; init; }
    public required int TileRow { get; init; }
    public required int Palette { get; init; }
    public required bool Priority { get; init; }
    public required bool FlipX { get; init; }
    public required bool FlipY { get; init; }
}
