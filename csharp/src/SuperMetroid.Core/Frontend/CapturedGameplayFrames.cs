using SuperMetroid.Core.World;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Frontend;

/// <summary>Captures every gameplay update's display frame, as every production host does.</summary>
internal sealed partial class CapturedGameplayFrames : IGameplayFramePublication
{
    /// <summary>The single capturing policy.</summary>
    public static CapturedGameplayFrames Instance { get; } = new();

    /// <summary>Use <see cref="Instance"/>.</summary>
    private CapturedGameplayFrames()
    {
    }

    /// <inheritdoc />
    public DisplayPacket Publish(SuperMetroidRuntime runtime, RoomScope loaded, DisplayPacket current, int frameNumber)
    {
        Ensure.NotNull(runtime);
        GameplayDisplayFrame frame = runtime.PublishDisplayFrame(loaded);
        return new(frameNumber, GameplayDisplayCapture.Capture(frame));
    }
}
