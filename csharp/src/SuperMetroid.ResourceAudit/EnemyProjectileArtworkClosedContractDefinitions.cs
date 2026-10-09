namespace SuperMetroid.ResourceAudit;

/// <summary>Validated direct/program projectile compositions, including complete legacy inheritance.</summary>
internal static class EnemyProjectileArtworkClosedContractDefinitions
{
    /// <summary>Fingerprints that close direct and instruction-program enemy projectile compositions.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.EnemyProjectileSpritemapCatalog", "installed-enemy-projectile-direct-and-program-frames",
            ["Get", "GetProgramFrame"],
            [new("csharp/src/SuperMetroid.Core/Assets/EnemyProjectileSpritemapCatalog.cs", "3836079A8EAAF3C71752B766807BA5E4EFEFEE2F0384278123ADABE7AF7F6F8D"),
             new("csharp/src/SuperMetroid.Core/Game/EnemyProjectilePresentationFrameDefinitions.cs", "A56656C6C5E6D3F907D86949D59CF70EC5704873DEEC55F15CBD7FC4F38A5AC6"),
             new("csharp/src/SuperMetroid.Core/Game/EnemyProjectileInstructionMechanicsDefinitions.cs", "41024DA1E04784F6A7A50F3142384BDD15988EFF363B8056CC04DA51FEC8EF1F"),
             new("csharp/src/SuperMetroid.Core/Game/SmallExplosionAnimationDefinitions.cs", "1ECA8C8290466E0582186CF82F9E104A32B0101E030E482364F3DDE7B1DB81C3"),
             new("csharp/src/SuperMetroid.Core/Game/EnemyDeathInstructionProgramDefinitions.cs", "C42A00F643F7A4ECAAC4B60F85966476E29DB4BB19B44FC510093E8322039896"),
             new("csharp/src/SuperMetroid.Core/Assets/SkreeMetareeParticleVisualDefinitions.cs", "F6A75E543F74E7390731BB6B1FFF3D148BE71A70AA79CD41818F1A5AEC8C0C6E")]),
    ];
}
