using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// Selects the ending reward actors' definitions.
/// </summary>
internal static class EndingRewardActors
{
    /// <summary>
    /// Resolves one reward actor's start and animation. The selected animation is a typed cinematic sprite animation
    /// (#641); the start is the reward part's, which its idle actor shares (#2205).
    /// </summary>
    public static EndingRewardActorDefinition Get(EndingRewardActor definition) => definition switch
    {
        EndingRewardActor.SuitlessUpper => new(EndingRewardStartDefinitions.Suitless, EndingRewardAnimation.HairRelease),
        EndingRewardActor.SuitlessLower => new(EndingRewardStartDefinitions.Suitless, EndingRewardAnimation.HairReleaseLegs),
        EndingRewardActor.SuitedBody => new(EndingRewardStartDefinitions.SuitedBody, EndingRewardAnimation.GestureBody),
        EndingRewardActor.SuitedArm => new(EndingRewardStartDefinitions.SuitedBody, EndingRewardAnimation.GestureArm),
        EndingRewardActor.HelmetedHead => new(EndingRewardStartDefinitions.HelmetedHead, EndingRewardAnimation.GestureHelmet),
        EndingRewardActor.HelmetlessHead => new(EndingRewardStartDefinitions.HelmetlessHead, EndingRewardAnimation.GestureHead),
        EndingRewardActor.JumpSuitlessBody => new(EndingRewardStartDefinitions.Suitless, EndingRewardAnimation.SuitlessJump),
        EndingRewardActor.JumpSuitedBody => new(EndingRewardStartDefinitions.SuitedBody, EndingRewardAnimation.SuitedJump),
        EndingRewardActor.JumpHelmetedHead => new(EndingRewardStartDefinitions.HelmetedHead, EndingRewardAnimation.HelmetJump),
        EndingRewardActor.JumpHelmetlessHead => new(EndingRewardStartDefinitions.HelmetlessHead, EndingRewardAnimation.HeadJump),
        _ => throw new InvalidOperationException($"Undefined {nameof(EndingRewardActor)} {definition}."),
    };

}
