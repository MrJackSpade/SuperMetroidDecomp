using SuperMetroid.Core.Frontend;

/// <summary>
/// Private instruction sets of native cinematic lists that verification interprets as an
/// oracle but production does not execute through the shared sprite interpreter.
/// </summary>
internal enum CeresSpawnerOpcode : ushort
{
    /// <summary>$8B:C404, the Ceres explosion spawner's initial burst.</summary>
    SpawnInitial = 0xc404,

    /// <summary>$8B:C50C, the Ceres explosion spawner's final burst.</summary>
    SpawnFinal = 0xc50c,
}

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
