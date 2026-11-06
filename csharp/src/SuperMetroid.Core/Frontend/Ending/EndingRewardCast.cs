namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The actors each ending reward's post-credits scenes spawn (#1513): the idle Samus the credits' end shows, the gesture
/// actors and the jump's head and body, in spawn order. The scenes spawn these, and the import pictures the reward
/// sheets' frames from them.
/// </summary>
internal static class EndingRewardCast
{
    /// <summary>The idle actors the credits' end spawns for <paramref name="reward"/>, in spawn order.</summary>
    /// <exception cref="InvalidOperationException">The reward is undefined.</exception>
    internal static IReadOnlyList<EndingSpriteDefinition> Idle(EndingReward reward) => reward switch
    {
        EndingReward.Suitless => EndingActorDefinitions.SuitlessRewardIdle,
        EndingReward.Helmetless => EndingActorDefinitions.HelmetlessRewardIdle,
        EndingReward.Armored => EndingActorDefinitions.ArmoredRewardIdle,
        _ => throw new InvalidOperationException($"Undefined ending reward {reward}."),
    };

    /// <summary>The gesture actors <paramref name="reward"/> spawns, in allocation order: the suited head first, then body and arm.</summary>
    /// <exception cref="InvalidOperationException">The reward is undefined.</exception>
    internal static IReadOnlyList<EndingRewardActor> Gesture(EndingReward reward) => reward switch
    {
        EndingReward.Suitless => [EndingRewardActor.SuitlessUpper, EndingRewardActor.SuitlessLower],
        EndingReward.Helmetless => [EndingRewardActor.HelmetlessHead, EndingRewardActor.SuitedBody, EndingRewardActor.SuitedArm],
        EndingReward.Armored => [EndingRewardActor.HelmetedHead, EndingRewardActor.SuitedBody, EndingRewardActor.SuitedArm],
        _ => throw new InvalidOperationException($"Undefined ending reward {reward}."),
    };

    /// <summary>The jump's head actor for <paramref name="reward"/>: none for the suitless reward, whose body carries its head.</summary>
    /// <exception cref="InvalidOperationException">The reward is undefined.</exception>
    internal static EndingRewardActor? JumpHead(EndingReward reward) => reward switch
    {
        EndingReward.Suitless => null,
        EndingReward.Helmetless => EndingRewardActor.JumpHelmetlessHead,
        EndingReward.Armored => EndingRewardActor.JumpHelmetedHead,
        _ => throw new InvalidOperationException($"Undefined ending reward {reward}."),
    };

    /// <summary>The jump's body actor for <paramref name="reward"/>.</summary>
    /// <exception cref="InvalidOperationException">The reward is undefined.</exception>
    internal static EndingRewardActor JumpBody(EndingReward reward) => reward switch
    {
        EndingReward.Suitless => EndingRewardActor.JumpSuitlessBody,
        EndingReward.Helmetless or EndingReward.Armored => EndingRewardActor.JumpSuitedBody,
        _ => throw new InvalidOperationException($"Undefined ending reward {reward}."),
    };
}
