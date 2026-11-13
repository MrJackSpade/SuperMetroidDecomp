using System.Collections.Immutable;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.Core;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>
/// The codec of the editable gameplay-HUD document (#1466): each cell's picture coordinates, palette, priority and
/// mirroring decoded once into the tiles the HUD's artwork draws, its anchors validated against the HUD's rows and each
/// other, and the minimap's shades named from the cartridge's HUD attributes.
/// </summary>
public static class GameplayHudPresentationFile
{
    /// <summary>Characters across the HUD's picture page: a cell's column and row name its picture.</summary>
    private const int PictureColumns = 32;

    /// <summary>Loads and validates a gameplay-HUD presentation document.</summary>
    /// <param name="json">The UTF-8 JSON document stream.</param>
    /// <returns>The decoded gameplay-HUD artwork.</returns>
    /// <exception cref="InvalidDataException">The document is malformed, incomplete, or places elements outside the HUD or over each other.</exception>
    public static GameplayHudArtwork Read(Stream json)
    {
        Ensure.NotNull(json);
        byte[] source;
        using (var buffer = new MemoryStream())
        {
            json.CopyTo(buffer);
            source = buffer.ToArray();
        }
        GameplayHudPresentationDocument document;
        try
        {
            document = JsonAssetDocument.Read<GameplayHudPresentationDocument>(source, ArtworkJson.Options) ??
                throw new InvalidDataException("Gameplay HUD presentation document is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid gameplay HUD presentation JSON.", error);
        }
        if (document.Version != GameplayHudStockDefinitions.Version || document.Template is null ||
            document.Blank is null || document.EnergyTanks is null || document.Digits is null ||
            document.AutoReserve is null)
            throw new InvalidDataException("Gameplay HUD presentation requires version 2 and every named visual owner.");
        return new GameplayHudArtwork(Decode(document));
    }

    /// <summary>Writes a gameplay-HUD presentation document as UTF-8 JSON.</summary>
    /// <param name="output">The destination stream.</param>
    /// <param name="document">The editable presentation document to serialize.</param>
    public static void Write(Stream output, GameplayHudPresentationDocument document)
    {
        Ensure.NotNull(output);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        output.Write(bytes);
    }

    /// <summary>Decodes every cell and anchor and rejects any element outside the HUD or over another.</summary>
    private static GameplayHudArtworkParts Decode(GameplayHudPresentationDocument document)
    {
        if (document.Icons is null || document.Icons.Count != GameplayHudDefinitions.IconNames.Length ||
            GameplayHudDefinitions.IconNames.Any(name => !document.Icons.ContainsKey(name)))
            throw new InvalidDataException("Gameplay HUD requires exactly the five named item icons.");
        var icons = ImmutableArray.CreateBuilder<GameplayHudIcon>(GameplayHudDefinitions.ItemCount);
        for (int item = 0; item < GameplayHudDefinitions.ItemCount; item++)
        {
            string name = GameplayHudDefinitions.IconNames[item];
            GameplayHudIconDocument icon = document.Icons[name] ?? throw new InvalidDataException($"Gameplay HUD icon {name} is null.");
            int width = GameplayHudStockDefinitions.IconWidths[item];
            Position16<CharacterCells> anchor = ValidateAnchor(icon.Anchor, new(checked((ushort)width), GameplayHudIcon.Height), $"{name} icon");
            icons.Add(new GameplayHudIcon(anchor, width, CompileCells(icon.Cells, width * GameplayHudIcon.Height, $"{name} icon")));
        }
        var minimapHidden = MapTileWords.HudBlank;
        var parts = new GameplayHudArtworkParts
        {
            TopRow = CompileCells(document.TopRow, GameplayHudDocumentDefinitions.TopRowCellCount, "HUD immutable top row"),
            Template = CompileCells(document.Template, GameplayHudDocumentDefinitions.CellCount, "HUD template"),
            Blank = CompileCell(document.Blank, "HUD blank"),
            FilledEnergyTank = CompileCell(document.EnergyTanks.Filled, "filled energy tank"),
            EmptyEnergyTank = CompileCell(document.EnergyTanks.Empty, "empty energy tank"),
            EnergyTankCells = CompileAnchors(document.EnergyTanks.Anchors, GameplayHudDefinitions.EnergyTankCount, "energy tank"),
            HealthDigits = CompileCells(document.Digits.Health, 10, "health digits"),
            AmmoDigits = CompileCells(document.Digits.Ammo, 10, "ammo digits"),
            HealthAnchor = ValidateAnchor(document.Digits.HealthAnchor, new(2, 1), "health digits"),
            MissileAmmoAnchor = ValidateAnchor(document.Digits.MissileAnchor, new(3, 1), "missile digits"),
            SuperMissileAmmoAnchor = ValidateAnchor(document.Digits.SuperMissileAnchor, new(2, 1), "Super Missile digits"),
            PowerBombAmmoAnchor = ValidateAnchor(document.Digits.PowerBombAnchor, new(2, 1), "Power Bomb digits"),
            AutoReserveFilled = CompileCells(document.AutoReserve.ContainsEnergy, GameplayHudDefinitions.AutoReserveCellCount, "filled AUTO indicator"),
            AutoReserveEmpty = CompileCells(document.AutoReserve.Empty, GameplayHudDefinitions.AutoReserveCellCount, "empty AUTO indicator"),
            AutoReserveCells = CompileAnchors(document.AutoReserve.Anchors, GameplayHudDefinitions.AutoReserveCellCount, "AUTO indicator"),
            SelectedPalette = ValidatePalette(document.SelectedPalette, nameof(document.SelectedPalette)),
            DeselectedPalette = ValidatePalette(document.DeselectedPalette, nameof(document.DeselectedPalette)),
            Icons = icons.MoveToImmutable(),
            MinimapAnchor = ValidateAnchor(document.MinimapAnchor, MinimapCells, "minimap"),
            MinimapHidden = minimapHidden.ToTile(),
            MinimapUnexploredColourVariant = minimapHidden.ForHud(explored: false).PaletteIndex,
            MinimapExploredColourVariant = minimapHidden.ForHud(explored: true).PaletteIndex,
            MinimapLocationColourVariant = minimapHidden.ForHud(explored: true).WithLocationBlink().PaletteIndex,
        };
        ValidateDistinctDynamicCells(parts);
        return parts;
    }

    /// <summary>Rejects overlap among icons, tanks, digits, AUTO cells, and minimap cells.</summary>
    private static void ValidateDistinctDynamicCells(GameplayHudArtworkParts parts)
    {
        var owners = new Dictionary<Position16<CharacterCells>, string>();
        void Own(Position16<CharacterCells> cell, string owner)
        {
            if (!owners.TryAdd(cell, owner))
                throw new InvalidDataException($"Gameplay HUD {owner} overlaps {owners[cell]} at cell {GameplayHudDocumentDefinitions.Size.RowMajorIndex(cell)}.");
        }
        void OwnRun(Position16<CharacterCells> anchor, int width, int height, string owner)
        {
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                Own(anchor + new Offset16<CharacterCells>(checked((short)x), checked((short)y)), owner);
        }
        for (int item = 0; item < GameplayHudDefinitions.ItemCount; item++)
            OwnRun(parts.Icons[item].Anchor, parts.Icons[item].Width, GameplayHudIcon.Height, GameplayHudDefinitions.IconNames[item]);
        foreach (Position16<CharacterCells> tank in parts.EnergyTankCells) Own(tank, "energy tank");
        foreach (Position16<CharacterCells> cell in parts.AutoReserveCells) Own(cell, "AUTO indicator");
        OwnRun(parts.HealthAnchor, 2, 1, "health digits");
        OwnRun(parts.MissileAmmoAnchor, 3, 1, "missile digits");
        OwnRun(parts.SuperMissileAmmoAnchor, 2, 1, "Super Missile digits");
        OwnRun(parts.PowerBombAmmoAnchor, 2, 1, "Power Bomb digits");
        OwnRun(parts.MinimapAnchor, MinimapCells.X, MinimapCells.Y, "minimap");
    }

    /// <summary>Requires the expected cell count and decodes each authored cell's tile.</summary>
    private static ImmutableArray<BackgroundTile> CompileCells(GameplayHudCell[]? cells, int count, string name)
    {
        if (cells is null || cells.Length != count)
            throw new InvalidDataException($"{name} requires exactly {count} cells.");
        var result = ImmutableArray.CreateBuilder<BackgroundTile>(count);
        for (int index = 0; index < count; index++)
            result.Add(CompileCell(cells[index], $"{name} cell {index}"));
        return result.MoveToImmutable();
    }

    /// <summary>Validates a character coordinate and palette, then places the cell's tile with its mirroring and priority.</summary>
    private static BackgroundTile CompileCell(GameplayHudCell? cell, string name)
    {
        if (cell is null || cell.TileColumn is < 0 or >= PictureColumns || cell.TileRow is < 0 or >= PictureColumns ||
            (uint)cell.Palette >= BackgroundTile.ColourVariants)
            throw new InvalidDataException($"{name} has invalid tile or palette coordinates.");
        return new BackgroundTile(cell.TileRow * PictureColumns + cell.TileColumn, cell.Palette, cell.FlipX, cell.FlipY, cell.Priority);
    }

    /// <summary>Validates the complete anchor array of one-cell elements.</summary>
    private static ImmutableArray<Position16<CharacterCells>> CompileAnchors(Position16<CharacterCells>[]? anchors, int count, string name)
    {
        if (anchors is null || anchors.Length != count)
            throw new InvalidDataException($"Gameplay HUD {name} requires exactly {count} anchors.");
        var result = ImmutableArray.CreateBuilder<Position16<CharacterCells>>(count);
        for (int index = 0; index < count; index++)
            result.Add(ValidateAnchor(anchors[index], new(1, 1), $"{name} {index}"));
        return result.MoveToImmutable();
    }

    /// <summary>The cells the minimap covers: one for each of its area-map squares.</summary>
    private static Extent16<CharacterCells> MinimapCells => new(GameplayHudPresentation.MinimapSize.X, GameplayHudPresentation.MinimapSize.Y);

    /// <summary>Ensures an anchored rectangle fits within the mutable 32-by-3 HUD tilemap.</summary>
    private static Position16<CharacterCells> ValidateAnchor(Position16<CharacterCells> anchor, Extent16<CharacterCells> size, string name)
    {
        if (anchor.X < 0 || anchor.Y < 0 ||
            anchor.X + size.X > GameplayHudDocumentDefinitions.Columns || anchor.Y + size.Y > GameplayHudDocumentDefinitions.DrawnRows)
            throw new InvalidDataException($"Gameplay HUD {name} lies outside the 32x3 mutable tilemap.");
        return anchor;
    }

    /// <summary>Restricts a colour variant to its three-bit range.</summary>
    private static int ValidatePalette(int palette, string name) => (uint)palette < BackgroundTile.ColourVariants
        ? palette
        : throw new InvalidDataException($"Gameplay HUD {name} must be in range 0..7.");
}
