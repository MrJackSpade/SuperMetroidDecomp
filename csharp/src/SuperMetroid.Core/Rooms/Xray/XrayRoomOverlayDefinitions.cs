namespace SuperMetroid.Core.Rooms.Xray;

/// <summary>The retail room-specific X-ray overlays.</summary>
public static class XrayRoomOverlayDefinitions
{
    /// <summary>The blocks Bomb Torizo's escape-state room reveals where the animals break out.</summary>
    public static XrayRoomOverlayDefinition BombTorizoAnimalEscapeBlocks { get; } = new(nameof(BombTorizoAnimalEscapeBlocks));

    /// <summary>Every retail overlay; an installation must supply tiles for each.</summary>
    public static IReadOnlyList<XrayRoomOverlayDefinition> All { get; } = [BombTorizoAnimalEscapeBlocks];
}
