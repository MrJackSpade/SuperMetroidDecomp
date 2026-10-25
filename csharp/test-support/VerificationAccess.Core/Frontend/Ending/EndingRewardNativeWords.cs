using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// Verification's native view of the ending reward identities (#1416): the bank-$8B words naming each member. Core's
/// values are arbitrary; the ROM oracle translates through these tables.
/// </summary>
internal static class EndingRewardNativeWords
{
    /// <summary>The cinematic-object definition word naming each <see cref="EndingRewardActor"/>.</summary>
    internal static readonly NativeWordIdentities<EndingRewardActor> Actors = new(new Dictionary<EndingRewardActor, ushort>
    {
        [EndingRewardActor.SuitlessUpper] = 0xef33,
        [EndingRewardActor.SuitlessLower] = 0xef39,
        [EndingRewardActor.JumpSuitlessBody] = 0xef3f,
        [EndingRewardActor.SuitedBody] = 0xef57,
        [EndingRewardActor.SuitedArm] = 0xef5d,
        [EndingRewardActor.HelmetedHead] = 0xef63,
        [EndingRewardActor.HelmetlessHead] = 0xef69,
        [EndingRewardActor.JumpSuitedBody] = 0xef6f,
        [EndingRewardActor.JumpHelmetedHead] = 0xef75,
        [EndingRewardActor.JumpHelmetlessHead] = 0xef7b,
    });

    /// <summary>The initialization routine word naming each <see cref="EndingRewardInitialization"/>.</summary>
    internal static readonly NativeWordIdentities<EndingRewardInitialization> Initializations = new(new Dictionary<EndingRewardInitialization, ushort>
    {
        [EndingRewardInitialization.Suitless] = 0xf143,
        [EndingRewardInitialization.SuitedBody] = 0xf156,
        [EndingRewardInitialization.HelmetedHead] = 0xf169,
        [EndingRewardInitialization.HelmetlessHead] = 0xf17c,
    });
}
