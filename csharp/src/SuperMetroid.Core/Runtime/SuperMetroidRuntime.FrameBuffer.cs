using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    // Final gameplay frame, reused by every software render of this runtime. A rendered
    // frame is valid until the next render; hosts and tests that keep one must copy it.
    [field: NonSerialized]
    internal Rgba32[] GameplayFrameBuffer =>
        field ??= new Rgba32[SnesPpuLayout.ScreenWidthPixels * SnesPpuLayout.ScreenHeightPixels];
}
