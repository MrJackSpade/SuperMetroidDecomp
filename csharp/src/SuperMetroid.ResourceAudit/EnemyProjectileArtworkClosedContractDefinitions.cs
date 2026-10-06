namespace SuperMetroid.ResourceAudit;

/// <summary>Validated direct/program projectile compositions, including complete legacy inheritance.</summary>
internal static class EnemyProjectileArtworkClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.EnemyProjectileSpritemapCatalog", "installed-enemy-projectile-direct-and-program-frames",
            ["Get", "GetProgramFrame"],
            [new("csharp/src/SuperMetroid.Core/Assets/EnemyProjectileSpritemapCatalog.cs", "6D196AA19D6C4437F62AAE5729601A586516D153A33328324F2F9EF35A89191D"),
             new("csharp/src/SuperMetroid.Core/Game/EnemyProjectilePresentationFrameDefinitions.cs", "EFED0B48AC4007EDA5128A805A7E2987861E25E2B04A676111859E0B88985923"),
             new("csharp/src/SuperMetroid.Core/Game/EnemyProjectileInstructionMechanicsDefinitions.cs", "DCB8206CC3BEF45B14B6369D10EFF25BDE0E82B980507BC03499F6154A361287"),
             new("csharp/src/SuperMetroid.Core/Game/SmallExplosionAnimationDefinitions.cs", "64CBF66FDA0E75FDF85665C3AD832FD96C7CF20B368FF2D5595DB6B1DCCE62C4"),
             new("csharp/src/SuperMetroid.Core/Game/EnemyDeathInstructionProgramDefinitions.cs", "5E0D21D99117FFFE15492CDABAD5C6A2A0E9022E43F91514E323075EAD4F8B52"),
             new("csharp/src/SuperMetroid.Core/Assets/SkreeMetareeParticleVisualDefinitions.cs", "1D2E4DB442FED52CAA82DEEB84E6067FBC63DDB77A7A9081A2628FAA09629B81")],
            "The sole private constructor is reached through Load. Current input requires every direct frame and every current compiled program-operand frame, each nonnull and compiled. Legacy overrides begin with complete private stock dictionaries and replace only historical members; old stock cannot load independently. Get's native blank ID is an empty composition, not an installed resource. Known unowned direct/program IDs stay findings. External Core references to the mutable direct-definition array or new reviewed metadata declarations revoke the rule. Compiled program-definition changes flow into both required installation and audit identities; separate definition inventory verifies their presentation dependencies. This certifies valid resource availability, not arbitrary slot IDs, selection, motion, timing or pixels."),
    ];
}
