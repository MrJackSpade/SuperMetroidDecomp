using SuperMetroid.Core.Frontend.Intro;
namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Bank-$8B reward gesture constructors and their jump handoffs.</summary>
internal static class EndingRewardActorDefinitions
{
    /// <summary>
    /// Resolves one complete native initialization/pre-instruction/list record. The
    /// selected program is a typed cinematic sprite program (#641).
    /// </summary>
    public static EndingRewardActorDefinition Get(EndingRewardActor definition) => definition switch
    {
        EndingRewardActor.SuitlessUpper => new(EndingRewardInitialization.Suitless, CinematicSpritePreInstruction.RewardSuitlessUpperNoOp, CinematicSpriteProgram.HairRelease),
        EndingRewardActor.SuitlessLower => new(EndingRewardInitialization.Suitless, CinematicSpritePreInstruction.NoOp, CinematicSpriteProgram.HairReleaseLegs),
        EndingRewardActor.SuitedBody => new(EndingRewardInitialization.SuitedBody, CinematicSpritePreInstruction.NoOp, CinematicSpriteProgram.GestureBody),
        EndingRewardActor.SuitedArm => new(EndingRewardInitialization.SuitedBody, CinematicSpritePreInstruction.NoOp, CinematicSpriteProgram.GestureArm),
        EndingRewardActor.HelmetedHead => new(EndingRewardInitialization.HelmetedHead, CinematicSpritePreInstruction.NoOp, CinematicSpriteProgram.GestureHelmet),
        EndingRewardActor.HelmetlessHead => new(EndingRewardInitialization.HelmetlessHead, CinematicSpritePreInstruction.NoOp, CinematicSpriteProgram.GestureHead),
        EndingRewardActor.JumpSuitlessBody => new(EndingRewardInitialization.Suitless, CinematicSpritePreInstruction.NoOp, CinematicSpriteProgram.SuitlessJump),
        EndingRewardActor.JumpSuitedBody => new(EndingRewardInitialization.SuitedBody, CinematicSpritePreInstruction.NoOp, CinematicSpriteProgram.SuitedJump),
        EndingRewardActor.JumpHelmetedHead => new(EndingRewardInitialization.HelmetedHead, CinematicSpritePreInstruction.NoOp, CinematicSpriteProgram.HelmetJump),
        EndingRewardActor.JumpHelmetlessHead => new(EndingRewardInitialization.HelmetlessHead, CinematicSpritePreInstruction.NoOp, CinematicSpriteProgram.HeadJump),
        _ => throw new InvalidOperationException($"Undefined {nameof(EndingRewardActor)} {(int)definition:X4}."),
    };

    /// <summary>Literal position/palette writes made by the four native reward initialization routines.</summary>
    public static (int X, int Y, int Palette) GetInitialization(EndingRewardInitialization address) => address switch
    {
        EndingRewardInitialization.Suitless => (120, 136, 0x0a00),
        EndingRewardInitialization.SuitedBody => (120, 152, 0x0c00),
        EndingRewardInitialization.HelmetedHead => (124, 108, 0x0c00),
        EndingRewardInitialization.HelmetlessHead => (121, 107, 0x0a00),
        _ => throw new InvalidOperationException($"Undefined {nameof(EndingRewardInitialization)} {(int)address:X4}."),
    };
}
