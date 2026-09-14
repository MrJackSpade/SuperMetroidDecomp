using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts displayed orientation, never the physical body placement associated with it.</summary>
public static class GrappleSwingFrameExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
        => GrappleSwingFrameCatalog.Write(new()
        {
            Version = GrappleSwingFrameDefinitions.Version,
            Frames = Enumerable.Range(0, GrappleSwingFrameDefinitions.AngleCount)
                .Select(angle => (int)bus.ReadByte(SamusGrappleRomData.Rendering.SwingFrameByAngle + angle)).ToArray(),
        });
}
