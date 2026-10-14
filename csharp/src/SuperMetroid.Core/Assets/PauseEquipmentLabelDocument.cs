namespace SuperMetroid.Core.Assets;

/// <summary>Editable pause equipment-page label schema, separating inventory-owned text from the backdrop, wireframe, and reserve controls.</summary>
public sealed record PauseEquipmentLabelDocument
{
    /// <summary>Schema revision; loading requires version one from <see cref="PauseEquipmentLabelDefinitions.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>BG palette selector 0..7 applied to collected but unequipped labels while preserving their character, priority, and flip bits.</summary>
    public required int DisabledPalette { get; init; }
    /// <summary>Nine nonnull cells replacing the native zero-word blank strip at $82:C01A; uncollected labels and discarded Hyper beam slots use the needed prefix.</summary>
    public required PauseBackdropCell[] Blank { get; init; }
    /// <summary>Exactly fourteen case-sensitive ordinary selector-anchor keys plus Beam.Hyper, with no reserve labels; entries specify editable tile artwork and destinations.</summary>
    public required Dictionary<string, PauseEquipmentLabel> Labels { get; init; }
}
