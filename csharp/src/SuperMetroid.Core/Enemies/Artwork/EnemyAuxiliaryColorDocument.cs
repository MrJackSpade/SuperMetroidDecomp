using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Editable JSON schema for four auxiliary enemy palette families; runtime AI retains palette destination, update cadence, drain-stage progression, and health-band selection.</summary>
public sealed record EnemyAuxiliaryColorDocument
{
    /// <summary>Schema revision, which must equal <see cref="EnemyAuxiliaryColorFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>All four enum-named RGB5 families: FaceBlock has eight four-color rows, DeadSidehopper seven fifteen-color rows, and each Golden Torizo family eight sixteen-color health bands.</summary>
    public required Dictionary<EnemyAuxiliaryPalette, PaletteRgb5[][]> Palettes { get; init; }
}
