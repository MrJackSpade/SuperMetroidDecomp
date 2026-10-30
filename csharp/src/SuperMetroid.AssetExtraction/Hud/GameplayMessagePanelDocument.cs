using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>Editable large item-instruction panel schema; border arrays, panel dictionary and nested templates remain caller-mutable until compilation.</summary>
public sealed record GameplayMessagePanelDocument
{
    /// <summary>Schema revision; loading requires <see cref="GameplayMessagePanelDefinitions.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Thirty-two tiles copied unchanged to both outer rows.</summary>
    public required BackgroundTile[] Border { get; init; }
    /// <summary>Exactly MissileTank, SuperMissileTank, PowerBombTank, GrappleBeam, XrayScope, SpeedBooster and Bombs, using their case-sensitive message names as keys.</summary>
    public required Dictionary<string, GameplayMessagePanel> Panels { get; init; }
}
