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
        EndingRewardActor.SuitlessUpper => new(EndingRewardStartDefinitions.Suitless, CinematicSpriteSegment.HairRelease),
        EndingRewardActor.SuitlessLower => new(EndingRewardStartDefinitions.Suitless, CinematicSpriteSegment.HairReleaseLegs),
        EndingRewardActor.SuitedBody => new(EndingRewardStartDefinitions.SuitedBody, CinematicSpriteSegment.GestureBody),
        EndingRewardActor.SuitedArm => new(EndingRewardStartDefinitions.SuitedBody, CinematicSpriteSegment.GestureArm),
        EndingRewardActor.HelmetedHead => new(EndingRewardStartDefinitions.HelmetedHead, CinematicSpriteSegment.GestureHelmet),
        EndingRewardActor.HelmetlessHead => new(EndingRewardStartDefinitions.HelmetlessHead, CinematicSpriteSegment.GestureHead),
        EndingRewardActor.JumpSuitlessBody => new(EndingRewardStartDefinitions.Suitless, CinematicSpriteSegment.SuitlessJump),
        EndingRewardActor.JumpSuitedBody => new(EndingRewardStartDefinitions.SuitedBody, CinematicSpriteSegment.SuitedJump),
        EndingRewardActor.JumpHelmetedHead => new(EndingRewardStartDefinitions.HelmetedHead, CinematicSpriteSegment.HelmetJump),
        EndingRewardActor.JumpHelmetlessHead => new(EndingRewardStartDefinitions.HelmetlessHead, CinematicSpriteSegment.HeadJump),
        _ => throw new InvalidOperationException($"Undefined {nameof(EndingRewardActor)} {definition}."),
    };

}
