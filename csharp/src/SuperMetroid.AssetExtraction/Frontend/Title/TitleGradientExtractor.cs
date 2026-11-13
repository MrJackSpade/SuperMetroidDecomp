using SuperMetroid.AssetExtraction.Frontend.Title.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.AssetExtraction.Frontend.Title;

/// <summary>Turns the title HDMA streams into the editable gradient stops (#1468).</summary>
internal static class TitleGradientExtractor
{
    /// <summary>Decodes the title HDMA gradient for every zoom bucket and
    /// writes each gradient's stops as presentation data.</summary>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var variants = new TitleGradientVariant[TitleGradientFormat.VariantCount];
        for (int variant = 0; variant < variants.Length; variant++)
        {
            ScreenGradient decoded = TitleGradientDecoder.Decode(
                CartridgeImportSource.Require(bus), checked((ushort)(variant << 4)));
            variants[variant] = new TitleGradientVariant
            {
                Stops = [.. decoded.Stops.Select(TitleGradientFile.Author)],
            };
        }

        using var json = new MemoryStream();
        TitleGradientFile.Write(json, new TitleGradientDocument
        {
            Version = TitleGradientFile.Version,
            Variants = variants,
        });
        return json.ToArray();
    }
}
