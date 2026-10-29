namespace SuperMetroid.ResourceAudit.ClosedContracts.Enemies;

/// <summary>Display-ID projection without an artwork request; not a frame-availability proof.</summary>
internal static class NativeDisplaySelectorClosedContractDefinitions
{
    /// <summary>Requires display-pointer selection to stay a closed identity projection without requesting artwork.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Enemies.Artwork.EnemyExtendedFramePlacementCatalog", "extended-display-id-projection-no-resource-read", ["GetDisplayFrame"],
            [new("csharp/src/SuperMetroid.Core/Enemies/Artwork/EnemyExtendedFramePlacementCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Artwork/EnemyExtendedFramePlacementSelection.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Artwork/EnemyExtendedFramePlacements.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Artwork/EnemyExtendedComponentPlacement.cs")]),
    ];
}
