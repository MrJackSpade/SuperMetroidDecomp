using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.AssetExtraction.Frontend.Title;

/// <summary>Expands title HDMA streams into editable RGB5/control scanline records.</summary>
internal static class TitleGradientExtractor
{
    /// <summary>Decodes the title HDMA gradient for every zoom high nibble and
    /// writes per-scanline RGB5 and color-math controls as presentation data.</summary>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var variants = new TitleGradientVariant[TitleGradientFormat.VariantCount];
        for (int variant = 0; variant < variants.Length; variant++)
        {
            TitleGradientLine[] decoded = TitleGradientDecoder.Decode(
                CartridgeImportSource.Require(bus), checked((ushort)(variant << 4)));
            variants[variant] = new TitleGradientVariant
            {
                ZoomHighNibble = variant,
                Lines = decoded.Select(TitleGradientPresentation.Author).ToArray(),
            };
        }

        using var json = new MemoryStream();
        TitleGradientPresentation.Write(json, new TitleGradientDocument
        {
            Version = TitleGradientFormat.Version,
            Variants = variants,
        });
        return json.ToArray();
    }
}
