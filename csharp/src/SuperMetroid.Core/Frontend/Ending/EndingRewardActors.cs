using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// Selects the ending reward actors' definitions and initializations.
/// </summary>
internal static class EndingRewardActors
{
    /// <summary>
    /// Resolves one reward actor's initialization and animation. The
    /// selected animation is a typed cinematic sprite animation (#641).
    /// </summary>
    public static EndingRewardActorDefinition Get(EndingRewardActor definition) => definition switch
    {
        EndingRewardActor.SuitlessUpper => new(EndingRewardInitialization.Suitless, EndingRewardAnimation.HairRelease),
        EndingRewardActor.SuitlessLower => new(EndingRewardInitialization.Suitless, EndingRewardAnimation.HairReleaseLegs),
        EndingRewardActor.SuitedBody => new(EndingRewardInitialization.SuitedBody, EndingRewardAnimation.GestureBody),
        EndingRewardActor.SuitedArm => new(EndingRewardInitialization.SuitedBody, EndingRewardAnimation.GestureArm),
        EndingRewardActor.HelmetedHead => new(EndingRewardInitialization.HelmetedHead, EndingRewardAnimation.GestureHelmet),
        EndingRewardActor.HelmetlessHead => new(EndingRewardInitialization.HelmetlessHead, EndingRewardAnimation.GestureHead),
        EndingRewardActor.JumpSuitlessBody => new(EndingRewardInitialization.Suitless, EndingRewardAnimation.SuitlessJump),
        EndingRewardActor.JumpSuitedBody => new(EndingRewardInitialization.SuitedBody, EndingRewardAnimation.SuitedJump),
        EndingRewardActor.JumpHelmetedHead => new(EndingRewardInitialization.HelmetedHead, EndingRewardAnimation.HelmetJump),
        EndingRewardActor.JumpHelmetlessHead => new(EndingRewardInitialization.HelmetlessHead, EndingRewardAnimation.HeadJump),
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
