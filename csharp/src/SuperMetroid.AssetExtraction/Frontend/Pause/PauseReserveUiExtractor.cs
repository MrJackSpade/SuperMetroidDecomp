using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Pause;

/// <summary>Resolves reserve-screen presentation once without exposing its mode or energy mechanics.</summary>
public static class PauseReserveUiExtractor
{
    /// <summary>Resolves reserve labels, decimal digit cells, arrow positions, palettes, and animation colors into a versioned presentation document.</summary>
    /// <param name="bus">Supported-cartridge address space containing reserve-screen label pointers, tilemaps, and arrow color tables.</param>
    /// <returns>UTF-8 JSON bytes for reserve-screen visuals without mode or energy behavior.</returns>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        var labels = new Dictionary<string, PauseReserveLabelVisual>
        {
            [PauseReserveUiTiles.Name(PauseReserveLabel.Mode)] = Label(0, PauseReserveUiDefinitions.LabelWords),
            [PauseReserveUiTiles.Name(PauseReserveLabel.ReserveTank)] = Label(1, PauseReserveUiDefinitions.LabelWords),
            [PauseReserveUiTiles.Name(PauseReserveLabel.Manual)] = DirectLabel(PauseMenuSourceDefinitions.ReserveManualText, PauseReserveUiDefinitions.ModeCell, PauseReserveUiDefinitions.ModeWords),
            [PauseReserveUiTiles.Name(PauseReserveLabel.Auto)] = DirectLabel(PauseMenuSourceDefinitions.ReserveAutoText, PauseReserveUiDefinitions.ModeCell, PauseReserveUiDefinitions.ModeWords),
        };
        var digits = new PauseReserveDigitVisual
        {
            Anchor = Point(PauseReserveUiDefinitions.DigitCell),
            Cells = Enumerable.Range(0, PauseReserveUiDefinitions.DigitCount)
                .Select(value => PauseTileGrid.FromWord(BackgroundTileWords.ToWord(PauseReserveUiTiles.DigitTile(value)), $"Reserve.Digit{value}"))
                .ToArray(),
        };
        PauseGridPoint[] arrowCells =
        [
            .. Enumerable.Range(0, PauseReserveUiDefinitions.VerticalCount)
                .Select(index => Point(PauseReserveUiDefinitions.VerticalStartCell + index * PauseReserveUiDefinitions.RowStrideCells)),
            .. Enumerable.Range(0, PauseReserveUiDefinitions.HorizontalCount)
                .Select(index => Point(PauseReserveUiDefinitions.HorizontalStartCell + index)),
        ];
        var frames = new PauseReserveArrowFrame[PauseReserveUiDefinitions.ArrowFrames];
        for (int frame = 0; frame < frames.Length; frame++)
            frames[frame] = new()
            {
                Color6 = Rgb(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), PauseMenuSourceDefinitions.ReserveArrowFirstColours + frame * 2)),
                Color11 = Rgb(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), PauseMenuSourceDefinitions.ReserveArrowSecondColours + frame * 2)),
            };
        using var output = new MemoryStream();
        PauseReserveUiFile.Write(output, new()
        {
            Version = PauseReserveUiDefinitions.Version,
            Labels = labels,
            Digits = digits,
            Arrow = new()
            {
                Cells = arrowCells,
                EnabledPalette = PauseReserveUiDefinitions.EnabledPalette,
                DisabledPalette = PauseReserveUiDefinitions.DisabledPalette,
                SolidColor6 = Rgb(PauseReserveUiDefinitions.SolidColor6),
                SolidColor11 = Rgb(PauseReserveUiDefinitions.SolidColor11),
                Frames = frames,
            },
        });
        return output.ToArray();

        PauseReserveLabelVisual Label(int index, int words)
        {
            int destination = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), PauseMenuSourceDefinitions.ReserveLabelPlacements.AddWithinBank(index * 2)) -
                PauseMenuSourceDefinitions.EquipmentTilemapBase;
            int pointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), PauseMenuSourceDefinitions.ReserveLabelSources.AddWithinBank(index * 2));
            return DirectLabel(CartridgeBankWindow.At(PauseMenuSourceDefinitions.MenuBank, pointer), destination / 2, words);
        }
        PauseReserveLabelVisual DirectLabel(int address, int cell, int words) => new()
        {
            Anchor = Point(cell),
            Cells = Enumerable.Range(0, words).Select(index => PauseTileGrid.FromWord(
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address + index * 2), $"Reserve label {address:X6}.{index}")).ToArray(),
        };
        static PauseGridPoint Point(int cell) => new()
        {
            Column = cell % PauseReserveUiDefinitions.TilemapColumns,
            Row = cell / PauseReserveUiDefinitions.TilemapColumns,
        };
        static PaletteRgb5 Rgb(int word) => new() { Red = word & 31, Green = word >> 5 & 31, Blue = word >> 10 & 31 };
    }
}
