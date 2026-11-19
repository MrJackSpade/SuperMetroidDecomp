namespace SuperMetroid.Core.Rooms.Xray;

/// <summary>A room-specific X-ray overlay: blocks a room state reveals in addition to its items, drawn from installed pictures.</summary>
/// <param name="name">The overlay's name.</param>
public sealed class XrayRoomOverlayDefinition([InstalledKey] string name)
{
    /// <summary>The overlay's name.</summary>
    public string Name { get; } = Ensure.NotNull(name);
}
