using SuperMetroid.Core.Assets;

namespace SuperMetroid.ResourceAudit;

/// <summary>Reviewed source-owned interface dispatch sets; external injected implementations are not certified.</summary>
internal static class InterfacePresentationContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.IPaletteFxColorSource", "palette-interface-total-membership-query", ["TryReadColor"],
            [Source("IPaletteFxColorSource", "DE4F1FA88F356190D7E6689581A21DFC5A7B578D2FEE2BAB5EB3DDCDEED65839"),
             Source("RoomPaletteFxPresentation", "252D07266853CCEC540D3B005FEE73556D093EE4A4A3B650A35F2C38D6EAB513"),
             Source("HeatPaletteColorDefinitions", "D0FA578A9F24EED61162E61CEE8F128082C3D230DB1FAD07ABBD4C9139C5DFF1"),
             Source("PaletteFxHeatInstructionListDefinitions", "145331301C26464315BA0D09B133766757220E35C717862574CC791DD7EC6411"),
             Source("TitlePalettePresentation", "C1ECCD51302467FDF4B1E32F4936D8FD7724776A598884B3590E4EEA9B07C45A")],
            "The only two source-owned implementations query private installed colors, including calculated heat-row aliases, and return false for unowned addresses; neither demands artwork or throws for a missing key. The separate palette definition inventory checks every recognized bounded program color against actual importer/loader keys. This query proof does not certify the downstream caller's selected program, correct room/title provider binding, installation, clocks, or arbitrary externally injected implementations. New Core implementations or altered reviewed sources revoke it."),
        new("SuperMetroid.Core.Assets.IIntroCinematicSpritePresentation", "cinematic-interface-complete-owned-frame-domains", ["Draw"],
            [Source("IIntroCinematicSpritePresentation", "B53C0DC30B4B57E3F0CEF08518C7F44BCD37D3F7E5280E54A844D2D3C1080774"),
             Source("IntroDiscoveryActorSpritePresentation", "1C8DC743598FBB33AB793E2F6EE333ABBC4110DA0D5580C57DAEF3A1010B654D"),
             Source("IntroDiscoveryActorSpriteDefinitions", "9380CFA1C6FC77E4F2FC5C191FD86B54FF291299CA8C4C9F1F06096D18ED7F66"),
             Source("IntroEggEffectSpritePresentation", "9B35344B60270279A5276985D8A28AA22E78B97AB741C231759456FDAE284400"),
             Source("IntroEggEffectSpriteDefinitions", "42272EC863B176CB7B93DDF49E2853DD7A03927F2BFF36907197B100712BE237"),
             Source("IntroRinkaSpritePresentation", "7D463CF1AF7EFF18687A2EDBF01DE1D1ADEF00359ABA35F34A4CE765E0915AB7"),
             Source("IntroRinkaSpriteDefinitions", "00F443062BB38D20AE18F64774ABB09C6D11D7EF2717D85B542875551D37212D"),
             Source("IntroScientistSpritePresentation", "3509FA7CC38EF8400EEDEBB75FFEE3B0861B36B5109DE4A5C715F74E2C03A63F"),
             Source("IntroScientistSpriteDefinitions", "8202070A80637FBD7C91019543819B6F662ECB60DF686E9C4D9BB89CF1B17B8F"),
             Source("CeresFlightSpritePresentation", "36A9EB3B38330311611C923888F3F19DB9A5EEE96F57F85D7E661C3254520840"),
             Source("CeresDestructionSpritePresentation", "7211E9430DE44ED953097DB276AA0B23CD19385E367E2DFE8B1C1282F783C026"),
             Source("EndingCloudSpritePresentation", "61D8F94F1842CBEE7DD67C3014BCF5D0B5191A41092D18F6335415C6799AA8F9"),
             Source("EndingCompletionTextSpritePresentation", "4BC7CA595F5ACE98BF5603525CFC99555FCFE4AF22FBE6E2DD79BFCEF93CD6EA"),
             Source("EndingExplosionSpritePresentation", "F4544E60FA882E3D65ACF6C1C5BFAE945D112BA9AA024904E1194A9E820EDE11"),
             Source("EndingLogoSpritePresentation", "50B428083D15432C6F4B5C6C09A57891FAB0E957A6EBB311253AC2576758F8E3"),
             Source("EndingRewardSpritePresentation", "BD8B9C7D5ECB3A10F18F4D99C411773C00A96B30AA8F38766EEBDFBF18C1D83D"),
             Source("IntroCinematicSpriteCompiler", "942CADA4FA409A846B28F792D17C631ADD6F7851B49DD1A22E19BC19B92106A1")],
            "Each of the eleven source-owned concrete catalogs has private construction reached only through a loader that requires every named compiled frame and compiles independent nonnull compositions. The twelfth implementation, CeresSceneSpritePresentation, routes only to the complete destruction catalog when it owns the key, otherwise to complete flight art. This proves availability within the actual receiver's owned domain, not that a union key belongs to every receiver or that an arbitrary actor selected its correct provider. Known pointers outside the combined domain remain findings. New implementations, metadata declarations, or changed loader/compiler/router sources revoke closure. Caller binding, actor lists, positions, OAM capacity, timing and pixels are not certified."),
    ];

    private static ReviewedSource Source(string name, string hash) => new("csharp/src/SuperMetroid.Core/Assets/" + name + ".cs", hash);

    internal static string[]? Implementations(string type) => type switch
    {
        "SuperMetroid.Core.Assets.IPaletteFxColorSource" => [typeof(RoomPaletteFxPresentation).FullName!, typeof(TitlePalettePresentation).FullName!],
        "SuperMetroid.Core.Assets.IIntroCinematicSpritePresentation" =>
            [typeof(IntroDiscoveryActorSpritePresentation).FullName!, typeof(IntroEggEffectSpritePresentation).FullName!,
             typeof(IntroRinkaSpritePresentation).FullName!, typeof(IntroScientistSpritePresentation).FullName!,
             typeof(CeresFlightSpritePresentation).FullName!, typeof(CeresDestructionSpritePresentation).FullName!,
             typeof(CeresSceneSpritePresentation).FullName!, typeof(EndingCloudSpritePresentation).FullName!,
             typeof(EndingCompletionTextSpritePresentation).FullName!, typeof(EndingExplosionSpritePresentation).FullName!,
             typeof(EndingLogoSpritePresentation).FullName!, typeof(EndingRewardSpritePresentation).FullName!],
        _ => null,
    };
}
