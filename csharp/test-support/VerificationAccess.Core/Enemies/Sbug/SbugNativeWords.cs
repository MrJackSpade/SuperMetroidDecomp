using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>
/// Verification's native view of the Sbug identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class SbugNativeWords
{
    /// <summary>The routine word naming each <see cref="SbugEnemyFunction"/>.</summary>
    internal static readonly NativeWordIdentities<SbugEnemyFunction> SbugEnemyFunctions = new(new Dictionary<SbugEnemyFunction, ushort>
    {
        [SbugEnemyFunction.WaitForSamus] = 0xa2d7,
        [SbugEnemyFunction.ActivateMoveForward] = 0xa301,
        [SbugEnemyFunction.ActivateZigZag] = 0xa30b,
        [SbugEnemyFunction.ActivateRandomUntilCollision] = 0xa315,
        [SbugEnemyFunction.ActivateRandomAndReverseWhenFar] = 0xa325,
        [SbugEnemyFunction.ActivateMoveForwardThenWait] = 0xa33b,
        [SbugEnemyFunction.ActivateMoveTowardSamus] = 0xa34b,
        [SbugEnemyFunction.ActivateMoveAwayFromSamus] = 0xa380,
        [SbugEnemyFunction.MoveForward] = 0xa407,
        [SbugEnemyFunction.ZigZag] = 0xa40e,
        [SbugEnemyFunction.MoveTowardSamus] = 0xa440,
        [SbugEnemyFunction.MoveAwayFromSamus] = 0xa447,
        [SbugEnemyFunction.MoveForwardThenWait] = 0xa44e,
        [SbugEnemyFunction.MoveRandomlyUntilCollision] = 0xa462,
        [SbugEnemyFunction.MoveStraightAndReverseWhenFar] = 0xa476,
        [SbugEnemyFunction.ChooseRandomDirectionAndReverseWhenFar] = 0xa4b6,
        [SbugEnemyFunction.ChooseRandomDirectionUntilCollision] = 0xa4f0,
    });
}
