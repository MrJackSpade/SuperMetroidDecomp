using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Imports gameplay-HUD artwork references and positions while excluding counter mechanics.</summary>
public static class GameplayHudPresentationExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var icons = new Dictionary<string, GameplayHudIconDocument>(StringComparer.Ordinal);
        int sourceWord = 0;
        for (int item = 0; item < GameplayHudDefinitions.IconNames.Length; item++)
        {
            int width = item == 0 ? 3 : 2;
            int count = width * 2;
            icons.Add(GameplayHudDefinitions.IconName(item), new()
            {
                Anchor = Point(GameplayHudDefinitions.ItemByteOffsets[item] / sizeof(ushort)),
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
                Anchors = GameplayHudDefinitions.EnergyTankByteOffsets.ToArray()
                    .Select(offset => Point(offset / sizeof(ushort))).ToArray(),
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
                Anchors = GameplayHudDefinitions.AutoReserveCellIndices.ToArray().Select(Point).ToArray(),
                ContainsEnergy = ReadCells(bus, GameplayHudDefinitions.AutoReserveTableAddress, 6),
                Empty = ReadCells(bus, GameplayHudDefinitions.AutoReserveTableAddress + 12, 6),
            },
            Icons = icons,
        });
        return output.ToArray();
    }

    private static GameplayHudCell[] ReadCells(ISnesAddressSpace bus, int address, int count) =>
        Enumerable.Range(0, count)
            .Select(index => Cell(RomDataReader.ReadWordFixedBank(bus, address + index * sizeof(ushort))))
            .ToArray();

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

    private static MapLabelPoint Point(int index) =>
        new(index % GameplayHudDefinitions.Width, index / GameplayHudDefinitions.Width);
}
