using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Frontend;

/// <summary>Captures every gameplay update's display frame, as every production host does.</summary>
internal sealed class CapturedGameplayFrames : IGameplayFramePublication
{
    /// <summary>The single capturing policy.</summary>
    public static CapturedGameplayFrames Instance { get; } = new();

    /// <summary>Use <see cref="Instance"/>.</summary>
    private CapturedGameplayFrames()
    {
    }

    /// <inheritdoc />
    public DisplayPacket Publish(SuperMetroidRuntime runtime, DisplayPacket current, Func<RenderFrameIdentity> nextIdentity)
    {
        Ensure.NotNull(runtime);
        Ensure.NotNull(nextIdentity);
        GameplayDisplayFrame frame = runtime.PublishDisplayFrame();
        return new(nextIdentity(), GameplayDisplayCapture.Capture(frame));
    }
}
