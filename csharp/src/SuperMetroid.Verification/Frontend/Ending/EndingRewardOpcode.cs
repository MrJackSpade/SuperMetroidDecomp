using SuperMetroid.Core.Frontend.Ending;

namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>
/// The union of the ending reward gesture and jump instructions, for oracle comparisons that
/// walk every reward list with one recorder.
/// </summary>
internal enum EndingRewardOpcode : ushort
{
    /// <inheritdoc cref="EndingRewardGestureInstruction.SpawnSuitlessJump"/>
    SpawnSuitlessJump = (ushort)EndingRewardGestureInstruction.SpawnSuitlessJump,

    /// <inheritdoc cref="EndingRewardGestureInstruction.SpawnSuitedJump"/>
    SpawnSuitedJump = (ushort)EndingRewardGestureInstruction.SpawnSuitedJump,

    /// <inheritdoc cref="EndingRewardJumpInstruction.PrepareHead"/>
    PrepareHead = (ushort)EndingRewardJumpInstruction.PrepareHead,

    /// <inheritdoc cref="EndingRewardJumpInstruction.LaunchHead"/>
    LaunchHead = (ushort)EndingRewardJumpInstruction.LaunchHead,

    /// <inheritdoc cref="EndingRewardJumpInstruction.Shoot"/>
    Shoot = (ushort)EndingRewardJumpInstruction.Shoot,

    /// <inheritdoc cref="EndingRewardJumpInstruction.Launch"/>
    Launch = (ushort)EndingRewardJumpInstruction.Launch,
}
