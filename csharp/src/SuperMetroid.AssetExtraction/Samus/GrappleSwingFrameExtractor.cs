using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Extracts displayed orientation, never the physical body placement associated with it.</summary>
public static class GrappleSwingFrameExtractor
{
    /// <summary>Asset filename for the editable swing-direction-to-displayed-orientation mapping.</summary>
    public const string FileName = "grapple-swing-frames.json";

    /// <summary>
    /// Exports the displayed Samus frame of each grapple swing direction from the cartridge's 256-entry selection by angle
    /// at $9B:C1C2–C2C1. Every angle selects the frame of the swing direction nearest it, so each direction's frame is
    /// read once and every other angle of the direction must select the same frame.
    /// </summary>
    /// <param name="bus">Non-null cartridge source supplying one visual frame selector for each high byte of the native swing angle.</param>
    /// <returns>A new UTF-8 JSON buffer with versioned frame indices 0..31 in swing-direction order.</returns>
    /// <exception cref="InvalidDataException">Two angles of one swing direction select different frames.</exception>
    /// <remarks>This is an orientation-art selection, not a physical body-offset table; extraction does not modify swing geometry or facing.</remarks>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        var frames = new int?[GrappleSwingDirection.Count];
        for (int angle = 0; angle < CartridgeAngleCount; angle++)
        {
            int frame = bus.ReadCartridgeByte(SamusSourceDefinitions.GrappleSwingFrameByAngle.Value + angle);
            int direction = GrappleSwingDirection.Nearest(new TrigonometryAngle(angle)).Number;
            if (frames[direction] is { } chosen && chosen != frame)
                throw new InvalidDataException(
                    $"Grapple swing angle {angle} selects frame {frame}, but its swing direction {direction} already selects frame {chosen}.");
            frames[direction] = frame;
        }

        return GrappleSwingFrameFile.Write(new()
        {
            Version = GrappleSwingFrameFile.Version,
            Frames = [.. frames.Select(frame => frame ?? throw new InvalidDataException("A grapple swing direction holds no angle."))],
        });
    }

    /// <summary>The cartridge's frame selection holds one entry for each high byte of the swing angle.</summary>
    private const int CartridgeAngleCount = 256;
}
