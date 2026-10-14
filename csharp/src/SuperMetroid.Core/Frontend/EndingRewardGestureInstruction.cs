namespace SuperMetroid.Core.Frontend;

/// <summary>Private instructions of the ending reward gesture actor.</summary>
internal enum EndingRewardGestureInstruction : ushort
{
    /// <summary><c>Instruction_CinematicSpriteObject_SpawnSuitlessSamusJump</c> at $8B:F51D.</summary>
    SpawnSuitlessJump = 0xf51d,

    /// <summary><c>Instruction_CinematicSpriteObject_SpawnSuitedSamusJump</c> at $8B:F554.</summary>
    SpawnSuitedJump = 0xf554,
}
