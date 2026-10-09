using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts only charge-flare sprite parts, not animation or gameplay data.</summary>
public static class ChargeFlareSpriteExtractor
{
    /// <summary>Extracts the cataloged native charge-flare spritemap parts and validates the serialized document by loading it through the production catalog.</summary>
    /// <param name="bus">Supported-cartridge address space containing the charge-flare spritemaps.</param>
    /// <returns>UTF-8 JSON bytes containing charge-flare sprite parts only.</returns>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        byte[] json = ProjectileSpriteExtractor.ExtractFrames(bus, ChargeFlareSpriteDefinitions.NativePointers.ToArray());
        _ = ChargeFlareSpriteCatalog.Load(new MemoryStream(json));
        return json;
    }
}
