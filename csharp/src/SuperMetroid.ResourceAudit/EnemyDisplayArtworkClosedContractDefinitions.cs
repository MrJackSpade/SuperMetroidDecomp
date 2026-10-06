namespace SuperMetroid.ResourceAudit;

/// <summary>Actual display artwork availability, separate from non-resource ID projection.</summary>
internal static class EnemyDisplayArtworkClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.EnemySpritemapCatalog", "installed-simple-enemy-display-frames", ["TryGetDisplay"],
            [new("csharp/src/SuperMetroid.Core/Assets/EnemySpritemapCatalog.cs", "78BEE15C6B3D2B0F6605244968C2B30A6D2326BFE20ABD2E4054AB25AD219D2C"),
             new("csharp/src/SuperMetroid.Core/Assets/ChozoStrideGeometryDefinitions.cs", "D186B43BBC1233DF6F089B9F7E80BE140D878EC22B4B5FEF7CCA37F4075A1D31"),
             new("csharp/src/SuperMetroid.Core/Assets/BabyMetroidSpriteParts.cs", "4D1E9A44D8DA9FDA65806635F113BCE341F5CB06FAA95DBC4A19F624F8E362CC"),
             new("csharp/src/SuperMetroid.Core/Assets/BabyMetroidCompositionDefinitions.cs", "9286127000F8BC17979896282DCA39CF8A7C4BB935060F001F5D0FA498C134B3"),
             new("csharp/src/SuperMetroid.Core/Hardware/EnemySpritemapParts.cs", "A4314E73E8BB50CE1CE52EA245F79EAF39C90489C7677FEE5A774B8204193502"),
             new("csharp/src/SuperMetroid.Core/Assets/EnemySpritemapDefinitions.cs", "2021CB5537EFD9A98077D0C3350A015C0B59CFF730AC265C931D96E419736A8F")],
            "Load alone reaches the private constructor. Current input requires every compiled frame and binding; each binding selects an installed identity in the same bank. Historical inputs inherit complete private stock and replace only existing identities with validated art/bindings. Definition identities use a read-only calculated registry; ordinary compositions use immutable indexed views that preserve independent supplied parts and drawing order; derived additions feed both loader admission and the audit's valid domain, while separate definition inventory checks their dependencies. Known unowned bank/pointer pairs stay findings. This is valid-domain resource availability, not arbitrary selector correctness, room binding, positioning, timing or pixels."),
        new("SuperMetroid.Core.Assets.EnemyExtendedFrameCatalog", "installed-extended-enemy-display-frames", ["TryGetDisplay"],
            [new("csharp/src/SuperMetroid.Core/Assets/EnemyExtendedFrameCatalog.cs", "D658FAC93CBEA1A4697CEB0D0C5C02E935272372673A2C2290DEE18B3679B25B"),
             new("csharp/src/SuperMetroid.Core/Assets/GoldenTorizoStrideGeometryDefinitions.cs", "B82370AED8024E53A741A8C09D0E7E700C0FB4FBB0B1061CC2EAAEBA9CB5BA36"),
             new("csharp/src/SuperMetroid.Core/Assets/EnemyExtendedFrameDefinitions.cs", "FAC0D729EB8EBC0ADE643DEDBAA110F48E597D606A58985A5E0BF9B65FD60391"),
             new("csharp/src/SuperMetroid.Core/Assets/EnemyExtendedFrameSequence.cs", "4FEF795EE481DA31C310082D30D7425BF127AEFD74CA1BC09C652F00BC4E94FB"),
             new("csharp/src/SuperMetroid.Core/Assets/BossOamFrameDefinitions.cs", "E406FF3323E0D9BD239376294BC30C5EFD13AECFC4DDCFF20C8E63D567660DC5"),
             new("csharp/src/SuperMetroid.Core/Assets/PirateArtworkNameDefinitions.cs", "83D8AAFF4A60C7C24263F7705BF52169EA3369B01F54121B320F3A110667F4F4"),
             new("csharp/src/SuperMetroid.Core/Assets/EnemySpritemapCatalog.cs", "78BEE15C6B3D2B0F6605244968C2B30A6D2326BFE20ABD2E4054AB25AD219D2C"),
             new("csharp/src/SuperMetroid.Core/Assets/ChozoStrideGeometryDefinitions.cs", "D186B43BBC1233DF6F089B9F7E80BE140D878EC22B4B5FEF7CCA37F4075A1D31"),
             new("csharp/src/SuperMetroid.Core/Assets/BabyMetroidSpriteParts.cs", "4D1E9A44D8DA9FDA65806635F113BCE341F5CB06FAA95DBC4A19F624F8E362CC"),
             new("csharp/src/SuperMetroid.Core/Assets/BabyMetroidCompositionDefinitions.cs", "9286127000F8BC17979896282DCA39CF8A7C4BB935060F001F5D0FA498C134B3"),
             new("csharp/src/SuperMetroid.Core/Hardware/EnemySpritemapParts.cs", "A4314E73E8BB50CE1CE52EA245F79EAF39C90489C7677FEE5A774B8204193502"),
             new("csharp/src/SuperMetroid.Core/Game/CommonEnemyEmptyExtendedFrameDefinitions.cs", "FDA923F284D3E91EA136F923E9C66D4B1C264306F149450AFAABA76E5DA3ACC0")],
            "Load alone reaches the private constructor. Current input requires every compiled extended frame and binding; selected targets are installed members of the same frame family. Historical overrides inherit complete private stock and replace only earlier members. TryGetDisplay falls back to physical artwork, including the explicitly compiled empty extended frame. Ordered on-demand definitions supply both installation and valid-domain identities; the sequence, boss-root and Pirate-name generators are pinned here, while other family dependencies remain checked separately. Known unowned bank/pointer pairs stay findings. GetDisplayPointer has its own independent non-resource projection rule. This is availability, not arbitrary slot IDs, BG2 placement, collision, timing or pixels."),
    ];
}
