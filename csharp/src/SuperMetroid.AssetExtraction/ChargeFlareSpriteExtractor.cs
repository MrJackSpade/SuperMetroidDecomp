using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts only charge-flare sprite parts, not animation or gameplay data.</summary>
public static class ChargeFlareSpriteExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        byte[] json = ProjectileSpriteExtractor.ExtractFrames(bus, ChargeFlareSpriteDefinitions.NativePointers);
        _ = ChargeFlareSpriteCatalog.Load(new MemoryStream(json));
        return json;
    }
}
