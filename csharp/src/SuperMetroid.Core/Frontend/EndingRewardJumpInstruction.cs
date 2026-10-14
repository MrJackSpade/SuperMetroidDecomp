namespace SuperMetroid.Core.Frontend;

/// <summary>Private instructions of the ending reward jump body and head actors.</summary>
internal enum EndingRewardJumpInstruction : ushort
{
    /// <summary><c>Instruction_CineSpriteObject_PositionSuitedHeadToPrepareJump</c> at $8B:F597.</summary>
    PrepareHead = 0xf597,

    /// <summary><c>Instruction_CinematicSpriteObject_PositionSamusHeadToJump</c> at $8B:F5BA.</summary>
    LaunchHead = 0xf5ba,

    /// <summary><c>Instruction_CinematicSpriteObject_SamusShootsScreen</c> at $8B:F604.</summary>
    Shoot = 0xf604,

    /// <summary><c>Instruction_CinematicSpriteObject_MakeEndingSamusJump</c> at $8B:F651.</summary>
    Launch = 0xf651,
}
