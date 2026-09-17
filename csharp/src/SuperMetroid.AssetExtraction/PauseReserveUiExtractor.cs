using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Resolves reserve-screen presentation once without exposing its mode or energy mechanics.</summary>
public static class PauseReserveUiExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        var labels = new Dictionary<string, PauseReserveLabelVisual>
        {
            ["Mode"] = Label(0, PauseReserveUiDefinitions.LabelWords),
            ["ReserveTank"] = Label(1, PauseReserveUiDefinitions.LabelWords),
            ["Manual"] = DirectLabel(PauseReserveUiDefinitions.ManualSource, PauseReserveUiDefinitions.ModeCell, PauseReserveUiDefinitions.ModeWords),
            ["Auto"] = DirectLabel(PauseReserveUiDefinitions.AutoSource, PauseReserveUiDefinitions.ModeCell, PauseReserveUiDefinitions.ModeWords),
        };
        var digits = new PauseReserveDigitVisual
        {
            Anchor = Point(PauseReserveUiDefinitions.DigitCell),
            Cells = Enumerable.Range(0, PauseReserveUiDefinitions.DigitCount)
                .Select(value => PauseTileGrid.FromWord((ushort)(PauseReserveUiDefinitions.DigitZeroWord + value), $"Reserve.Digit{value}"))
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
                Color6 = Rgb(RomDataReader.ReadWordFixedBank(bus, PauseReserveUiDefinitions.ArrowColor6Source + frame * 2)),
                Color11 = Rgb(RomDataReader.ReadWordFixedBank(bus, PauseReserveUiDefinitions.ArrowColor11Source + frame * 2)),
            };
        using var output = new MemoryStream();
        PauseReserveUiPresentation.Write(output, new()
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
            int destination = RomDataReader.ReadWordFixedBank(bus, PauseReserveUiDefinitions.LabelDestinations + index * 2) -
                PauseReserveUiDefinitions.TilemapWramBase;
            ushort pointer = RomDataReader.ReadWordFixedBank(bus, PauseReserveUiDefinitions.LabelSources + index * 2);
            return DirectLabel(0x820000 | pointer, destination / 2, words);
        }
        PauseReserveLabelVisual DirectLabel(int address, int cell, int words) => new()
        {
            Anchor = Point(cell),
            Cells = Enumerable.Range(0, words).Select(index => PauseTileGrid.FromWord(
                RomDataReader.ReadWordFixedBank(bus, address + index * 2), $"Reserve label {address:X6}.{index}")).ToArray(),
        };
        static PauseGridPoint Point(int cell) => new()
        {
            Column = cell % PauseReserveUiDefinitions.TilemapColumns,
            Row = cell / PauseReserveUiDefinitions.TilemapColumns,
        };
        static PaletteRgb5 Rgb(ushort word) => new() { Red = word & 31, Green = word >> 5 & 31, Blue = word >> 10 & 31 };
    }
}
