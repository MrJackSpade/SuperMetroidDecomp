using SuperMetroid.Core.Frontend.Intro;
namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Bank-$8B reward gesture constructors and their jump handoffs.</summary>
internal static class EndingRewardActorDefinitions
{
    /// <summary>$8B:F51C, distinct RTS pre-instruction used by the suitless upper actor.</summary>
    public const ushort SuitlessUpperNoOp = 0xf51c;
    /// <summary>$8B:93D9, shared RTS pre-instruction used by the other reward actors.</summary>
    public const ushort SharedNoOp = 0x93d9;
    /// <summary>
    /// Resolves one complete native initialization/pre-instruction/list record. The
    /// selected program is a typed cinematic sprite program (#641).
    /// </summary>
    public static EndingRewardActorDefinition Get(EndingRewardActor definition) => definition switch
    {
        EndingRewardActor.SuitlessUpper => new(EndingRewardInitialization.Suitless, SuitlessUpperNoOp, CinematicSpriteProgram.HairRelease),
        EndingRewardActor.SuitlessLower => new(EndingRewardInitialization.Suitless, SharedNoOp, CinematicSpriteProgram.HairReleaseLegs),
        EndingRewardActor.SuitedBody => new(EndingRewardInitialization.SuitedBody, SharedNoOp, CinematicSpriteProgram.GestureBody),
        EndingRewardActor.SuitedArm => new(EndingRewardInitialization.SuitedBody, SharedNoOp, CinematicSpriteProgram.GestureArm),
        EndingRewardActor.HelmetedHead => new(EndingRewardInitialization.HelmetedHead, SharedNoOp, CinematicSpriteProgram.GestureHelmet),
        EndingRewardActor.HelmetlessHead => new(EndingRewardInitialization.HelmetlessHead, SharedNoOp, CinematicSpriteProgram.GestureHead),
        EndingRewardActor.JumpSuitlessBody => new(EndingRewardInitialization.Suitless, SharedNoOp, CinematicSpriteProgram.SuitlessJump),
        EndingRewardActor.JumpSuitedBody => new(EndingRewardInitialization.SuitedBody, SharedNoOp, CinematicSpriteProgram.SuitedJump),
        EndingRewardActor.JumpHelmetedHead => new(EndingRewardInitialization.HelmetedHead, SharedNoOp, CinematicSpriteProgram.HelmetJump),
        EndingRewardActor.JumpHelmetlessHead => new(EndingRewardInitialization.HelmetlessHead, SharedNoOp, CinematicSpriteProgram.HeadJump),
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
