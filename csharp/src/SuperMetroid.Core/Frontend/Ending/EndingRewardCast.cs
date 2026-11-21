namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The actors each ending reward's post-credits scenes spawn (#1513): the idle Samus the credits' end shows, the gesture
/// actors and the jump's head and body, in spawn order. The scenes spawn these, and the import pictures the reward
/// sheets' frames from them.
/// </summary>
internal static partial class EndingRewardCast
{
    /// <summary>
    /// Whether <paramref name="reward"/>'s reveal crossfades the waiting backdrop into its colours: the background stays
    /// shown and the sprites glow over it while it does.
    /// </summary>
    /// <exception cref="KeyNotFoundException">The reward is undefined.</exception>
    internal static bool CrossfadesReveal(EndingReward reward) => EndingRewardDefinitions.Rewards[reward].RevealFades.Count > 0;

    /// <summary>The idle actors the credits' end spawns for <paramref name="reward"/>, in spawn order.</summary>
    /// <exception cref="KeyNotFoundException">The reward is undefined.</exception>
    internal static IReadOnlyList<EndingSpriteDefinition> Idle(EndingReward reward) => EndingRewardDefinitions.Rewards[reward].Idle;

    /// <summary>The gesture actors <paramref name="reward"/> spawns, in the order they spawn, step and draw: the suited
    /// head first, then body and arm.</summary>
    /// <exception cref="KeyNotFoundException">The reward is undefined.</exception>
    internal static IReadOnlyList<EndingRewardActor> Gesture(EndingReward reward) => EndingRewardDefinitions.Rewards[reward].Gesture;

    /// <summary>The jump's head actor for <paramref name="reward"/>: none for the suitless reward, whose body carries its head.</summary>
    /// <exception cref="KeyNotFoundException">The reward is undefined.</exception>
    internal static EndingRewardActor? JumpHead(EndingReward reward) => EndingRewardDefinitions.Rewards[reward].JumpHead?.Actor;

    /// <summary>The jump's body actor for <paramref name="reward"/>.</summary>
    /// <exception cref="KeyNotFoundException">The reward is undefined.</exception>
    internal static EndingRewardActor JumpBody(EndingReward reward) => EndingRewardDefinitions.Rewards[reward].JumpBody;
}
