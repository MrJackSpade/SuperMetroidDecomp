namespace SuperMetroid.ResourceAudit;

/// <summary>Validated direct/program projectile compositions, including complete legacy inheritance.</summary>
internal static class EnemyProjectileArtworkClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.EnemyProjectileSpritemapCatalog", "installed-enemy-projectile-direct-and-program-frames",
            ["Get", "GetProgramFrame"],
            [new("csharp/src/SuperMetroid.Core/Assets/EnemyProjectileSpritemapCatalog.cs", "F8DD2BBD730B2AC1AB8CD46BB24C374591F62E210E3EE53ADF43D5A2F681ABC1"),
             new("csharp/src/SuperMetroid.Core/Game/EnemyProjectilePresentationFrameDefinitions.cs", "EFED0B48AC4007EDA5128A805A7E2987861E25E2B04A676111859E0B88985923"),
             new("csharp/src/SuperMetroid.Core/Game/EnemyProjectileInstructionMechanicsDefinitions.cs", "2E81F5ABDE5EEA0E8ED8EC67DD167A3FAA0AAF3D9FB3E986202C0E73A97FE3EB"),
             new("csharp/src/SuperMetroid.Core/Assets/SkreeMetareeParticleVisualDefinitions.cs", "1D2E4DB442FED52CAA82DEEB84E6067FBC63DDB77A7A9081A2628FAA09629B81")],
            "The sole private constructor is reached through Load. Current input requires every direct frame and every current compiled program-operand frame, each nonnull and compiled. Legacy overrides begin with complete private stock dictionaries and replace only historical members; old stock cannot load independently. Get's native blank ID is an empty composition, not an installed resource. Known unowned direct/program IDs stay findings. External Core references to the mutable direct-definition array or new reviewed metadata declarations revoke the rule. Compiled program-definition changes flow into both required installation and audit identities; separate definition inventory verifies their presentation dependencies. This certifies valid resource availability, not arbitrary slot IDs, selection, motion, timing or pixels."),
    ];
}
