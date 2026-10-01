namespace SuperMetroid.ResourceAudit;

/// <summary>Actual display artwork availability, separate from non-resource ID projection.</summary>
internal static class EnemyDisplayArtworkClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.EnemySpritemapCatalog", "installed-simple-enemy-display-frames", ["TryGetDisplay"],
            [new("csharp/src/SuperMetroid.Core/Assets/EnemySpritemapCatalog.cs", "7B073282CE479E5B5BBA16DB8190B6B94A45C439407B622999E85D7BDD95307E"),
             new("csharp/src/SuperMetroid.Core/Assets/EnemySpritemapDefinitions.cs", "20B76AA3C9B120CB549503ACEBD219E8EF525E4FC3149EBD9C1B69BCDD1C1968")],
            "Load alone reaches the private constructor. Current input requires every compiled frame and binding; each binding selects an installed identity in the same bank. Historical inputs inherit complete private stock and replace only existing identities with validated art/bindings. Definition identities are exposed by read-only spans over private arrays; derived additions feed both loader admission and the audit's valid domain, while separate definition inventory checks their dependencies. Known unowned bank/pointer pairs stay findings. This is valid-domain resource availability, not arbitrary selector correctness, room binding, positioning, timing or pixels."),
        new("SuperMetroid.Core.Assets.EnemyExtendedFrameCatalog", "installed-extended-enemy-display-frames", ["TryGetDisplay"],
            [new("csharp/src/SuperMetroid.Core/Assets/EnemyExtendedFrameCatalog.cs", "9F4B56B662649101933BAD23F2F86DD49B14B0B0C81E00E01EB08002C7B97411"),
             new("csharp/src/SuperMetroid.Core/Assets/EnemyExtendedFrameDefinitions.cs", "F7ACCCCC80874053260F75A461BBA5BA075DD96C38E541EC8E66F5B46EEEB69B"),
             new("csharp/src/SuperMetroid.Core/Assets/EnemySpritemapCatalog.cs", "7B073282CE479E5B5BBA16DB8190B6B94A45C439407B622999E85D7BDD95307E"),
             new("csharp/src/SuperMetroid.Core/Game/CommonEnemyEmptyExtendedFrameDefinitions.cs", "9BC211358C2707E6875D1A13C1D96DCA6B24848E318097600F82C4B451F8BAF1")],
            "Load alone reaches the private constructor. Current input requires every compiled extended frame and binding; selected targets are installed members of the same frame family. Historical overrides inherit complete private stock and replace only earlier members. TryGetDisplay falls back to physical artwork, including the explicitly compiled empty extended frame. Private-array read-only definitions supply both installation and valid-domain identities; their derived dependencies remain checked separately. Known unowned bank/pointer pairs stay findings. GetDisplayPointer has its own independent non-resource projection rule. This is availability, not arbitrary slot IDs, BG2 placement, collision, timing or pixels."),
    ];
}
