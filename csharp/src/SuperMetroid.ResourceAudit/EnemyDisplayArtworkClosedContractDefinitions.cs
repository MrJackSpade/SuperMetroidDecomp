namespace SuperMetroid.ResourceAudit;

/// <summary>Actual display artwork availability, separate from non-resource ID projection.</summary>
internal static class EnemyDisplayArtworkClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.EnemySpritemapCatalog", "installed-simple-enemy-display-frames", ["TryGetDisplay"],
            [new("csharp/src/SuperMetroid.Core/Assets/EnemySpritemapCatalog.cs", "7B073282CE479E5B5BBA16DB8190B6B94A45C439407B622999E85D7BDD95307E"),
             new("csharp/src/SuperMetroid.Core/Assets/EnemySpritemapDefinitions.cs", "1742A2E510A4204BF06F8C659BA34E55978803AE161C764DBD5BD8BC61BB33A8")],
            "Load alone reaches the private constructor. Current input requires every compiled frame and binding; each binding selects an installed identity in the same bank. Historical inputs inherit complete private stock and replace only existing identities with validated art/bindings. Definition identities are exposed by read-only spans over private arrays; derived additions feed both loader admission and the audit's valid domain, while separate definition inventory checks their dependencies. Known unowned bank/pointer pairs stay findings. This is valid-domain resource availability, not arbitrary selector correctness, room binding, positioning, timing or pixels."),
        new("SuperMetroid.Core.Assets.EnemyExtendedFrameCatalog", "installed-extended-enemy-display-frames", ["TryGetDisplay"],
            [new("csharp/src/SuperMetroid.Core/Assets/EnemyExtendedFrameCatalog.cs", "2A8DCB47F4C1E1B16847B2B64259F04C85FC4C57A54DBF9608023B6E335F379D"),
             new("csharp/src/SuperMetroid.Core/Assets/EnemyExtendedFrameDefinitions.cs", "FAC0D729EB8EBC0ADE643DEDBAA110F48E597D606A58985A5E0BF9B65FD60391"),
             new("csharp/src/SuperMetroid.Core/Assets/EnemyExtendedFrameSequence.cs", "4FEF795EE481DA31C310082D30D7425BF127AEFD74CA1BC09C652F00BC4E94FB"),
             new("csharp/src/SuperMetroid.Core/Assets/BossOamFrameDefinitions.cs", "E406FF3323E0D9BD239376294BC30C5EFD13AECFC4DDCFF20C8E63D567660DC5"),
             new("csharp/src/SuperMetroid.Core/Assets/PirateArtworkNameDefinitions.cs", "83D8AAFF4A60C7C24263F7705BF52169EA3369B01F54121B320F3A110667F4F4"),
             new("csharp/src/SuperMetroid.Core/Assets/EnemySpritemapCatalog.cs", "7B073282CE479E5B5BBA16DB8190B6B94A45C439407B622999E85D7BDD95307E"),
             new("csharp/src/SuperMetroid.Core/Game/CommonEnemyEmptyExtendedFrameDefinitions.cs", "9BC211358C2707E6875D1A13C1D96DCA6B24848E318097600F82C4B451F8BAF1")],
            "Load alone reaches the private constructor. Current input requires every compiled extended frame and binding; selected targets are installed members of the same frame family. Historical overrides inherit complete private stock and replace only earlier members. TryGetDisplay falls back to physical artwork, including the explicitly compiled empty extended frame. Ordered on-demand definitions supply both installation and valid-domain identities; the sequence, boss-root and Pirate-name generators are pinned here, while other family dependencies remain checked separately. Known unowned bank/pointer pairs stay findings. GetDisplayPointer has its own independent non-resource projection rule. This is availability, not arbitrary slot IDs, BG2 placement, collision, timing or pixels."),
    ];
}
