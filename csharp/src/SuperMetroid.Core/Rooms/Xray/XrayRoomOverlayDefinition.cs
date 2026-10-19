namespace SuperMetroid.Core.Rooms.Xray;

/// <summary>A room-specific X-ray overlay: blocks a room state reveals in addition to its items, drawn from installed tiles.</summary>
/// <param name="name">The overlay's name, the disassembly's label for it without its prefix, or a fixture's description.</param>
public sealed class XrayRoomOverlayDefinition(string name)
{
    /// <summary>The overlay's name.</summary>
    public string Name { get; } = Ensure.NotNull(name);
}
