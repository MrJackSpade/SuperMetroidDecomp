using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Expands title HDMA streams into editable RGB5/control scanline records.</summary>
internal static class TitleGradientExtractor
{
    /// <summary>Expands all sixteen native title zoom variants into a versioned scanline-color and color-math JSON document.</summary>
    /// <param name="bus">Import address space containing the fixed-color and control HDMA tables.</param>
    /// <returns>The UTF-8 title-gradient document accepted by <see cref="TitleGradientPresentation"/>.</returns>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var variants = new TitleGradientVariant[TitleGradientFormat.VariantCount];
        for (int variant = 0; variant < variants.Length; variant++)
        {
            TitleGradientLine[] decoded = TitleGradientDecoder.Decode(
                CartridgeImportSource.Require(bus), checked((ushort)(variant << 4)));
            variants[variant] = new TitleGradientVariant
            {
                ZoomHighNibble = variant,
                Lines = decoded.Select(line => new TitleGradientScanline
                {
                    Red = line.Red,
                    Green = line.Green,
                    Blue = line.Blue,
                    ColorMathControl = line.Control,
                }).ToArray(),
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
