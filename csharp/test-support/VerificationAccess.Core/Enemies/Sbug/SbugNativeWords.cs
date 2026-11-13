using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>
/// Verification's native view of the Sbug identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class SbugNativeWords
{
    /// <summary>The routine word naming each <see cref="SbugAction"/>.</summary>
    internal static readonly NativeWordIdentities<SbugAction> SbugActions = new(new Dictionary<SbugAction, ushort>
    {
        [SbugAction.WaitForSamus] = 0xa2d7,
        [SbugAction.ActivateMoveForward] = 0xa301,
        [SbugAction.ActivateZigZag] = 0xa30b,
        [SbugAction.ActivateRandomUntilCollision] = 0xa315,
        [SbugAction.ActivateRandomAndReverseWhenFar] = 0xa325,
        [SbugAction.ActivateMoveForwardThenWait] = 0xa33b,
        [SbugAction.ActivateMoveTowardSamus] = 0xa34b,
        [SbugAction.ActivateMoveAwayFromSamus] = 0xa380,
        [SbugAction.MoveForward] = 0xa407,
        [SbugAction.ZigZag] = 0xa40e,
        [SbugAction.MoveTowardSamus] = 0xa440,
        [SbugAction.MoveAwayFromSamus] = 0xa447,
        [SbugAction.MoveForwardThenWait] = 0xa44e,
        [SbugAction.MoveRandomlyUntilCollision] = 0xa462,
        [SbugAction.MoveStraightAndReverseWhenFar] = 0xa476,
        [SbugAction.ChooseRandomDirectionAndReverseWhenFar] = 0xa4b6,
        [SbugAction.ChooseRandomDirectionUntilCollision] = 0xa4f0,
    });
}
