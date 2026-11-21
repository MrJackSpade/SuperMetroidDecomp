namespace SuperMetroid.AssetExtraction.World.Artwork;

/// <summary>A run of consecutive frames of the extended frames document that belong to one enemy family (#1623).</summary>
/// <param name="Family">The family the run's frames belong to.</param>
/// <param name="Count">How many frames the run holds.</param>
/// <param name="Layers">The layers the run's frames' components draw on.</param>
/// <param name="MaximumComponents">The most components one of the run's frames lists in its header.</param>
/// <param name="HeaderHighByte">
/// The byte after the component count in the run's frames' headers, cartridge drawing metadata the walkers keep.
/// </param>
internal readonly record struct EnemyExtendedFrameRun(EnemyExtendedFrameFamily Family, int Count,
    EnemyExtendedFrameLayers Layers = EnemyExtendedFrameLayers.Sprites,
    int MaximumComponents = EnemyExtendedFrameDefinitions.MaximumComponents,
    int HeaderHighByte = 0);
