using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>
/// Verification's native view of the LavaJumpers identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class LavaJumpersNativeWords
{
    /// <summary>The routine word naming each <see cref="NorfairLavaJumpingEnemyAction"/>.</summary>
    internal static readonly NativeWordIdentities<NorfairLavaJumpingEnemyAction> NorfairLavaJumpingEnemyActions = new(new Dictionary<NorfairLavaJumpingEnemyAction, ushort>
    {
        [NorfairLavaJumpingEnemyAction.FollowParent] = 0xbedc,
        [NorfairLavaJumpingEnemyAction.BeginJump] = 0xbf1a,
        [NorfairLavaJumpingEnemyAction.RiseBeforeAnimationSwitch] = 0xbf3e,
        [NorfairLavaJumpingEnemyAction.MoveUntilAnimationSignal] = 0xbf7c,
        [NorfairLavaJumpingEnemyAction.FallBackIntoLava] = 0xbfbc,
    });
}
