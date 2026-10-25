using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>
/// Verification's native view of the LavaJumpers identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class LavaJumpersNativeWords
{
    /// <summary>The routine word naming each <see cref="NorfairLavaJumpingEnemyFunction"/>.</summary>
    internal static readonly NativeWordIdentities<NorfairLavaJumpingEnemyFunction> NorfairLavaJumpingEnemyFunctions = new(new Dictionary<NorfairLavaJumpingEnemyFunction, ushort>
    {
        [NorfairLavaJumpingEnemyFunction.FollowParent] = 0xbedc,
        [NorfairLavaJumpingEnemyFunction.BeginJump] = 0xbf1a,
        [NorfairLavaJumpingEnemyFunction.RiseBeforeAnimationSwitch] = 0xbf3e,
        [NorfairLavaJumpingEnemyFunction.MoveUntilAnimationSignal] = 0xbf7c,
        [NorfairLavaJumpingEnemyFunction.FallBackIntoLava] = 0xbfbc,
    });
}
