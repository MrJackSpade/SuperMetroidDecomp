namespace SuperMetroid.Core.Assets;

/// <summary>Editable JSON schema for the pause equipment page's static base tilemap.</summary>
public sealed record PauseEquipmentBaseDocument
{
    /// <summary>Must match the installed equipment-base format before any cell is accepted.</summary>
    public required int Version { get; init; }

    /// <summary>Gets all 1024 BG tile references in 32-column row-major order.</summary>
    public required PauseBackdropCell[] Cells { get; init; }
}
