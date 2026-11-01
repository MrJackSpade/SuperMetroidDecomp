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
        EndingRewardActor.SuitlessUpper => new(EndingRewardInitialization.Suitless, CinematicSpriteFrameBehavior.RewardSuitlessUpperNoOp, CinematicSpriteProgram.HairRelease),
        EndingRewardActor.SuitlessLower => new(EndingRewardInitialization.Suitless, CinematicSpriteFrameBehavior.NoOp, CinematicSpriteProgram.HairReleaseLegs),
        EndingRewardActor.SuitedBody => new(EndingRewardInitialization.SuitedBody, CinematicSpriteFrameBehavior.NoOp, CinematicSpriteProgram.GestureBody),
        EndingRewardActor.SuitedArm => new(EndingRewardInitialization.SuitedBody, CinematicSpriteFrameBehavior.NoOp, CinematicSpriteProgram.GestureArm),
        EndingRewardActor.HelmetedHead => new(EndingRewardInitialization.HelmetedHead, CinematicSpriteFrameBehavior.NoOp, CinematicSpriteProgram.GestureHelmet),
        EndingRewardActor.HelmetlessHead => new(EndingRewardInitialization.HelmetlessHead, CinematicSpriteFrameBehavior.NoOp, CinematicSpriteProgram.GestureHead),
        EndingRewardActor.JumpSuitlessBody => new(EndingRewardInitialization.Suitless, CinematicSpriteFrameBehavior.NoOp, CinematicSpriteProgram.SuitlessJump),
        EndingRewardActor.JumpSuitedBody => new(EndingRewardInitialization.SuitedBody, CinematicSpriteFrameBehavior.NoOp, CinematicSpriteProgram.SuitedJump),
        EndingRewardActor.JumpHelmetedHead => new(EndingRewardInitialization.HelmetedHead, CinematicSpriteFrameBehavior.NoOp, CinematicSpriteProgram.HelmetJump),
        EndingRewardActor.JumpHelmetlessHead => new(EndingRewardInitialization.HelmetlessHead, CinematicSpriteFrameBehavior.NoOp, CinematicSpriteProgram.HeadJump),
        _ => throw new InvalidOperationException($"Undefined {nameof(EndingRewardActor)} {definition}."),
    };

    /// <summary>The starting position and screen colours each reward initialization gives its actor.</summary>
    public static EndingRewardActorInitializationDefinition GetInitialization(EndingRewardInitialization initialization) => initialization switch
    {
        EndingRewardInitialization.Suitless => new(new(120, 136), ScreenColourGroup.EndingSuitlessSamus),
        EndingRewardInitialization.SuitedBody => new(new(120, 152), ScreenColourGroup.EndingSuitedSamus),
        EndingRewardInitialization.HelmetedHead => new(new(124, 108), ScreenColourGroup.EndingSuitedSamus),
        EndingRewardInitialization.HelmetlessHead => new(new(121, 107), ScreenColourGroup.EndingSuitlessSamus),
        _ => throw new InvalidOperationException($"Undefined {nameof(EndingRewardInitialization)} {initialization}."),
    };
}
