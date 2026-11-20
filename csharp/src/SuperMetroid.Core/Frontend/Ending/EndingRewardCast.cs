namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The actors each ending reward's post-credits scenes spawn (#1513): the idle Samus the credits' end shows, the gesture
/// actors and the jump's head and body, in spawn order. The scenes spawn these, and the import pictures the reward
/// sheets' frames from them.
/// </summary>
internal static class EndingRewardCast
{
    /// <summary>The idle of each reward.</summary>
    private static IdentityTable<EndingReward, IReadOnlyList<EndingSpriteDefinition>> IdleByReward => field ??= new(new Dictionary<EndingReward, IReadOnlyList<EndingSpriteDefinition>>
    {
        [EndingReward.Suitless] = EndingActorDefinitions.SuitlessRewardIdle,
        [EndingReward.Helmetless] = EndingActorDefinitions.HelmetlessRewardIdle,
        [EndingReward.Armored] = EndingActorDefinitions.ArmoredRewardIdle,
    });

    /// <summary>The idle actors the credits' end spawns for <paramref name="reward"/>, in spawn order.</summary>
    internal static IReadOnlyList<EndingSpriteDefinition> Idle(EndingReward reward) => IdleByReward[reward];

    /// <summary>The gesture of each reward.</summary>
    private static IdentityTable<EndingReward, IReadOnlyList<EndingRewardActor>> GestureByReward => field ??= new(new Dictionary<EndingReward, IReadOnlyList<EndingRewardActor>>
    {
        [EndingReward.Suitless] = [EndingRewardActor.SuitlessUpper, EndingRewardActor.SuitlessLower],
        [EndingReward.Helmetless] = [EndingRewardActor.HelmetlessHead, EndingRewardActor.SuitedBody, EndingRewardActor.SuitedArm],
        [EndingReward.Armored] = [EndingRewardActor.HelmetedHead, EndingRewardActor.SuitedBody, EndingRewardActor.SuitedArm],
    });

    /// <summary>The gesture actors <paramref name="reward"/> spawns, in allocation order: the suited head first, then body and arm.</summary>
    internal static IReadOnlyList<EndingRewardActor> Gesture(EndingReward reward) => GestureByReward[reward];

    /// <summary>The jump head of each reward.</summary>
    private static IdentityTable<EndingReward, EndingRewardActor?> JumpHeadByReward => field ??= new(new Dictionary<EndingReward, EndingRewardActor?>
    {
        [EndingReward.Suitless] = null,
        [EndingReward.Helmetless] = EndingRewardActor.JumpHelmetlessHead,
        [EndingReward.Armored] = EndingRewardActor.JumpHelmetedHead,
    });

    /// <summary>The jump's head actor for <paramref name="reward"/>: none for the suitless reward, whose body carries its head.</summary>
    internal static EndingRewardActor? JumpHead(EndingReward reward) => JumpHeadByReward[reward];

    /// <summary>The jump body of each reward.</summary>
    private static IdentityTable<EndingReward, EndingRewardActor> JumpBodyByReward => field ??= new(new Dictionary<EndingReward, EndingRewardActor>
    {
        [EndingReward.Suitless] = EndingRewardActor.JumpSuitlessBody,
        [EndingReward.Helmetless] = EndingRewardActor.JumpSuitedBody,
        [EndingReward.Armored] = EndingRewardActor.JumpSuitedBody,
    });

    /// <summary>The jump's body actor for <paramref name="reward"/>.</summary>
    internal static EndingRewardActor JumpBody(EndingReward reward) => JumpBodyByReward[reward];
}
