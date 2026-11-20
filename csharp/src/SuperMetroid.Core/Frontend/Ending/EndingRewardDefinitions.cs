using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Each ending reward's post-credits presentation (#1513).</summary>
internal static class EndingRewardDefinitions
{
    /// <summary>
    /// Each reward's presentation: the suitless Samus fades in her own colours; the helmetless Samus her suitless colours and
    /// her suit's; the armoured Samus shows at once in hers, over no background.
    /// </summary>
    internal static readonly IReadOnlyDictionary<EndingReward, EndingRewardDefinition> Rewards = new Dictionary<EndingReward, EndingRewardDefinition>
    {
        [EndingReward.Suitless] = new(EndingRewardDress.Suitless, SpriteSheetId.EndingSuitlessRewardSprites,
            [EndingFade.SuitlessSamus],
            EndingActorDefinitions.SuitlessRewardIdle,
            [EndingRewardActor.SuitlessUpper, EndingRewardActor.SuitlessLower],
            JumpHead: null,
            EndingRewardActor.JumpSuitlessBody),
        [EndingReward.Helmetless] = new(EndingRewardDress.Suited, SpriteSheetId.EndingSuitedRewardSprites,
            [EndingFade.SuitlessSamus, EndingFade.SuitedSamus],
            EndingActorDefinitions.HelmetlessRewardIdle,
            [EndingRewardActor.HelmetlessHead, EndingRewardActor.SuitedBody, EndingRewardActor.SuitedArm],
            new(EndingRewardActor.JumpHelmetlessHead, EndingRewardJumpDefinitions.HelmetlessHeadReadied, EndingRewardJumpDefinitions.HelmetlessHeadLaunched),
            EndingRewardActor.JumpSuitedBody),
        [EndingReward.Armored] = new(EndingRewardDress.Suited, SpriteSheetId.EndingSuitedRewardSprites,
            [],
            EndingActorDefinitions.ArmoredRewardIdle,
            [EndingRewardActor.HelmetedHead, EndingRewardActor.SuitedBody, EndingRewardActor.SuitedArm],
            new(EndingRewardActor.JumpHelmetedHead, EndingRewardJumpDefinitions.HelmetedHeadReadied, EndingRewardJumpDefinitions.HelmetedHeadLaunched),
            EndingRewardActor.JumpSuitedBody),
    };
}
