using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>
/// Verification's native view of the LavaJumpers identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class LavaJumpersNativeWords
{
    /// <summary>The routine word naming each <see cref="SqueeptAction"/>.</summary>
    internal static readonly NativeWordIdentities<SqueeptAction> SqueeptActions = new(new Dictionary<SqueeptAction, ushort>
    {
        [SqueeptAction.BeginJump] = 0xbf1a,
        [SqueeptAction.RiseBeforeAnimationSwitch] = 0xbf3e,
        [SqueeptAction.MoveUntilAnimationSignal] = 0xbf7c,
        [SqueeptAction.FallBackIntoLava] = 0xbfbc,
    });
}
