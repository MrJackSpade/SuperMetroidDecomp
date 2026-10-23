using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Keeps the prior display through gameplay updates, for verification hosts that inspect cartridge state rather than
/// frame pixels. No display packet is materialized for an update that only steps the simulation.
/// </summary>
internal sealed class RetainedGameplayFrames : IGameplayFramePublication
{
    /// <summary>The single retaining policy.</summary>
    public static RetainedGameplayFrames Instance { get; } = new();

    /// <summary>Use <see cref="Instance"/>.</summary>
    private RetainedGameplayFrames()
    {
    }

    /// <inheritdoc />
    public DisplayPacket Publish(SuperMetroidRuntime runtime, DisplayPacket current, Func<RenderFrameIdentity> nextIdentity) => current;
}
