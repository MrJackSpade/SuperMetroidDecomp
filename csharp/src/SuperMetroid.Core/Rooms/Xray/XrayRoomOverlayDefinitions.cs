namespace SuperMetroid.Core.Rooms.Xray;

/// <summary>The room-specific X-ray overlays.</summary>
public static partial class XrayRoomOverlayDefinitions
{
    /// <summary>The blocks Bomb Torizo's escape-state room reveals where the animals break out.</summary>
    public static XrayRoomOverlayDefinition BombTorizoAnimalEscapeBlocks { get; } = new("BombTorizoAnimalEscapeBlocks");

    /// <summary>Every overlay; an installation must supply pictures for each.</summary>
    public static IReadOnlyList<XrayRoomOverlayDefinition> All { get; } = [BombTorizoAnimalEscapeBlocks];
}
