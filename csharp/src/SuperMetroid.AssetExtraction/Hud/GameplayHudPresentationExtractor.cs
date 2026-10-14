using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>Imports gameplay-HUD artwork references and positions while excluding counter mechanics.</summary>
[CpuBusBoundary("codec: ROM gameplay HUD tilemap and artwork references → HUD presentation")]
public static class GameplayHudPresentationExtractor
{
    /// <summary>Imports HUD tilemap templates, item icons, digit cells, energy-tank artwork, and automatic-reserve indicators.</summary>
    /// <param name="bus">Non-null cartridge import address space supplying native HUD BG-cell tables.</param>
    /// <returns>New UTF-8 JSON bytes containing tile references, palette and flip attributes, and anchors measured in HUD tile cells.</returns>
    /// <remarks>Includes selected and deselected visual palettes and the minimap anchor, but not counters, inventory state, or minimap-generation mechanics.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var icons = new Dictionary<string, GameplayHudIconDocument>(StringComparer.Ordinal);
        int sourceWord = 0;
        for (int item = 0; item < GameplayHudDefinitions.IconNames.Length; item++)
        {
            int width = item == 0 ? 3 : 2;
            int count = width * 2;
            icons.Add(GameplayHudDefinitions.IconName(item), new()
            {
                Anchor = Point(GameplayHudDefinitions.ItemByteOffset(item) / sizeof(ushort)),
                Cells = ReadCells(bus, GameplayHudDefinitions.IconTableAddress + sourceWord * sizeof(ushort), count),
            });
            sourceWord += count;
        }

        using var output = new MemoryStream();
        GameplayHudPresentation.Write(output, new()
        {
            Version = GameplayHudDefinitions.Version,
            TopRow = ReadCells(bus, GameplayHudDefinitions.TopRowAddress,
                GameplayHudDefinitions.TopRowCellCount),
            Template = ReadCells(bus, GameplayHudDefinitions.TemplateAddress, GameplayHudDefinitions.CellCount),
            Blank = Cell(GameplayHudDefinitions.BlankWord),
            SelectedPalette = GameplayHudDefinitions.SelectedPalette,
            DeselectedPalette = GameplayHudDefinitions.DeselectedPalette,
            MinimapAnchor = new(26, 0),
            EnergyTanks = new()
            {
                Anchors = Enumerable.Range(0, GameplayHudDefinitions.EnergyTankCount)
                    .Select(tank => Point(GameplayHudDefinitions.EnergyTankByteOffset(tank) / sizeof(ushort))).ToArray(),
                Filled = Cell(GameplayHudDefinitions.FilledEnergyTankWord),
                Empty = Cell(GameplayHudDefinitions.EmptyEnergyTankWord),
            },
            Digits = new()
            {
                Health = ReadCells(bus, GameplayHudDefinitions.HealthDigitsAddress, 10),
                Ammo = ReadCells(bus, GameplayHudDefinitions.AmmoDigitsAddress, 10),
                HealthAnchor = Point(0x8c / sizeof(ushort)),
                MissileAnchor = Point(0x94 / sizeof(ushort)),
                SuperMissileAnchor = Point(0x9c / sizeof(ushort)),
                PowerBombAnchor = Point(0xa2 / sizeof(ushort)),
            },
            AutoReserve = new()
            {
                Anchors = Enumerable.Range(0, GameplayHudDefinitions.AutoReserveCellCount)
                    .Select(cell => Point(GameplayHudDefinitions.AutoReserveCellIndex(cell))).ToArray(),
                ContainsEnergy = ReadCells(bus, GameplayHudDefinitions.AutoReserveTableAddress, 6),
                Empty = ReadCells(bus, GameplayHudDefinitions.AutoReserveTableAddress + 12, 6),
            },
            Icons = icons,
        });
        return output.ToArray();
    }

    /// <summary>Reads consecutive fixed-bank HUD tilemap words and decodes their presentation attributes.</summary>
    private static GameplayHudCell[] ReadCells(ISnesAddressSpace bus, int address, int count) =>
        Enumerable.Range(0, count)
            .Select(index => Cell(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address + index * sizeof(ushort))))
            .ToArray();

    /// <summary>Splits a native BG tilemap word into atlas coordinates, palette, priority, and flips.</summary>
    private static GameplayHudCell Cell(ushort raw)
    {
        var word = new SnesBgTilemapWord(raw);
        return new()
        {
            TileColumn = word.CharacterIndex % 32,
            TileRow = word.CharacterIndex / 32,
            Palette = word.PaletteIndex,
            Priority = word.HasPriority,
            FlipX = word.FlipHorizontally,
            FlipY = word.FlipVertically,
        };
    }

    /// <summary>Converts a linear HUD tilemap index into its two-dimensional cell anchor.</summary>
    private static Position16<TilemapCells> Point(int index) =>
        new(checked((ushort)(index % GameplayHudDefinitions.Width)), checked((ushort)(index / GameplayHudDefinitions.Width)));
}
