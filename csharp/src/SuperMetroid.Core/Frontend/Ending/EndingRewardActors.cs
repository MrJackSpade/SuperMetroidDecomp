using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// Selects the ending reward actors' definitions and initializations.
/// </summary>
internal static class EndingRewardActors
{
    /// <summary>
    /// Resolves one complete initialization/frame-behaviour/animation record. The
    /// selected program is a typed cinematic sprite program (#641).
    /// </summary>
    public static EndingRewardActorDefinition Get(EndingRewardActor definition) => definition switch
    {
        EndingRewardActor.SuitlessUpper => new(EndingRewardInitialization.Suitless, CinematicSpriteFrameBehavior.RewardSuitlessUpperNoOp, CinematicSpriteSegment.HairRelease),
        EndingRewardActor.SuitlessLower => new(EndingRewardInitialization.Suitless, CinematicSpriteFrameBehavior.NoOp, CinematicSpriteSegment.HairReleaseLegs),
        EndingRewardActor.SuitedBody => new(EndingRewardInitialization.SuitedBody, CinematicSpriteFrameBehavior.NoOp, CinematicSpriteSegment.GestureBody),
        EndingRewardActor.SuitedArm => new(EndingRewardInitialization.SuitedBody, CinematicSpriteFrameBehavior.NoOp, CinematicSpriteSegment.GestureArm),
        EndingRewardActor.HelmetedHead => new(EndingRewardInitialization.HelmetedHead, CinematicSpriteFrameBehavior.NoOp, CinematicSpriteSegment.GestureHelmet),
        EndingRewardActor.HelmetlessHead => new(EndingRewardInitialization.HelmetlessHead, CinematicSpriteFrameBehavior.NoOp, CinematicSpriteSegment.GestureHead),
        EndingRewardActor.JumpSuitlessBody => new(EndingRewardInitialization.Suitless, CinematicSpriteFrameBehavior.NoOp, CinematicSpriteSegment.SuitlessJump),
        EndingRewardActor.JumpSuitedBody => new(EndingRewardInitialization.SuitedBody, CinematicSpriteFrameBehavior.NoOp, CinematicSpriteSegment.SuitedJump),
        EndingRewardActor.JumpHelmetedHead => new(EndingRewardInitialization.HelmetedHead, CinematicSpriteFrameBehavior.NoOp, CinematicSpriteSegment.HelmetJump),
        EndingRewardActor.JumpHelmetlessHead => new(EndingRewardInitialization.HelmetlessHead, CinematicSpriteFrameBehavior.NoOp, CinematicSpriteSegment.HeadJump),
        _ => throw new InvalidOperationException($"Undefined {nameof(EndingRewardActor)} {definition}."),
    };

    /// <summary>The starting position and colouring each reward initialization gives its actor.</summary>
    public static EndingRewardActorInitializationDefinition GetInitialization(EndingRewardInitialization initialization) => initialization switch
    {
        EndingRewardInitialization.Suitless => new(new(120, 136), EndingSpriteColouring.SuitlessSamus),
        EndingRewardInitialization.SuitedBody => new(new(120, 152), EndingSpriteColouring.SuitedSamus),
        EndingRewardInitialization.HelmetedHead => new(new(124, 108), EndingSpriteColouring.SuitedSamus),
        EndingRewardInitialization.HelmetlessHead => new(new(121, 107), EndingSpriteColouring.SuitlessSamus),
        _ => throw new InvalidOperationException($"Undefined {nameof(EndingRewardInitialization)} {initialization}."),
    };
}
