using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    // Final gameplay frame, reused by every software render of this runtime. A rendered
    // frame is valid until the next render; hosts and tests that keep one must copy it.
    /// <summary>Reusable final software-render target; valid only until the runtime renders again.</summary>
    [NonSerialized]
    private Rgba32[]? gameplayFrameBuffer;

    /// <summary>Returns the runtime-owned 256-by-224 pixel buffer, allocating it on first use.</summary>
    internal Rgba32[] GameplayFrameBuffer =>
        gameplayFrameBuffer ??= new Rgba32[SnesPpuLayout.ScreenWidthPixels * SnesPpuLayout.ScreenHeightPixels];
}
