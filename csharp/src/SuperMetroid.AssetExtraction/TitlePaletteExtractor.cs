using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Converts the title's complete cartridge CGRAM image to editable RGB5 JSON.</summary>
internal static class TitlePaletteExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var source = new SnesCgram();
        source.LoadFromBus(bus, TitleSequenceRomData.Assets.PaletteAddress);
        PaletteRgb5[] colors = source.Colors.ToArray().Select(color => new PaletteRgb5
        {
            Red = color & 31,
            Green = color >> 5 & 31,
            Blue = color >> 10 & 31,
        }).ToArray();
        using var json = new MemoryStream();
        TitlePalettePresentation.Write(json, new TitlePaletteDocument
        {
            Version = TitlePaletteFormat.Version,
            Colors = colors,
        });
        return json.ToArray();
    }
}
