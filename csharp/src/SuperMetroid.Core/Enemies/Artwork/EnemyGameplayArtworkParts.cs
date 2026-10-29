namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>The installed enemy palettes, extended frames and colour catalogs an enemy artwork catalog holds for gameplay (#1463).</summary>
[ArchitectureReview("context-bag", "A record of installed artwork catalogs; its members are data, not services.")]
public sealed record EnemyGameplayArtworkParts
{
    /// <summary>Each enemy's palette.</summary>
    public required IReadOnlyDictionary<EnemyDefinitionId, EnemyPaletteSheet> Palettes { get; init; }

    /// <summary>Extended frames.</summary>
    public EnemyExtendedFrameCatalog? ExtendedFrames { get; init; } = null;

    /// <summary>Magdollite palette cycle.</summary>
    public MagdollitePaletteCycle? MagdollitePaletteCycle { get; init; } = null;

    /// <summary>Work robot palette cycle.</summary>
    public WorkRobotPaletteCycle? WorkRobotPaletteCycle { get; init; } = null;

    /// <summary>Chozo and tube colors.</summary>
    public ChozoAndTubeColorCatalog? ChozoAndTubeColors { get; init; } = null;

    /// <summary>Dachora colors.</summary>
    public DachoraColorCatalog? DachoraColors { get; init; } = null;

    /// <summary>Auxiliary colors.</summary>
    public EnemyAuxiliaryColorCatalog? AuxiliaryColors { get; init; } = null;
}
