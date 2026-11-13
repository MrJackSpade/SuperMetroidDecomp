using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The codec from a pose's arm-cannon drawing-mode byte to the cover's <see cref="SamusArmCannonDrawOrder"/> (#1630). The
/// drawing handler reads only the byte's low nibble: zero hides the cover, two draws it after the body and every other value
/// draws it before the body. The game holds the draw order and never sees the byte.
/// </summary>
[EncodingOwner("arm-cannon drawing mode")]
internal static class SamusArmCannonDrawOrderCodec
{
    /// <summary>The draw order the drawing-mode byte <paramref name="drawingMode"/> selects.</summary>
    /// <param name="drawingMode">The pose's drawing-mode byte.</param>
    internal static SamusArmCannonDrawOrder FromDrawingMode(int drawingMode) => (drawingMode & 0x0f) switch
    {
        0 => SamusArmCannonDrawOrder.Hidden,
        2 => SamusArmCannonDrawOrder.AfterBody,
        _ => SamusArmCannonDrawOrder.BeforeBody,
    };
}
