using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts displayed orientation, never the physical body placement associated with it.</summary>
public static class GrappleSwingFrameExtractor
{
    /// <summary>Exports the 256-entry Grapple angle-to-displayed-Samus-frame mapping from $9B:C1C2–C2C1.</summary>
    /// <param name="bus">Non-null cartridge source supplying one visual frame selector for each high byte of the native swing angle.</param>
    /// <returns>A new UTF-8 JSON buffer with versioned frame indices 0..31 in unsigned angle-byte order.</returns>
    /// <remarks>This is an orientation-art selection, not a physical body-offset table; extraction does not modify swing geometry or facing.</remarks>
    public static byte[] Extract(ISnesAddressSpace bus)
        => GrappleSwingFrameCatalog.Write(new()
        {
            Version = GrappleSwingFrameDefinitions.Version,
            Frames = Enumerable.Range(0, GrappleSwingFrameDefinitions.AngleCount)
                .Select(angle => (int)bus.ReadCartridgeByte(SamusGrappleRomData.Rendering.SwingFrameByAngle + angle)).ToArray(),
        });
}
