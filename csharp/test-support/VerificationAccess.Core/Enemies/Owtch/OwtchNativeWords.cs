using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Owtch;

/// <summary>
/// Verification's native view of Owtch's behaviour states (#1481): the cartridge's main-AI table index naming each
/// member. Core's values are arbitrary; a native snapshot translates through this table.
/// </summary>
internal static class OwtchNativeWords
{
    /// <summary>
    /// The state word naming each <see cref="OwtchBehaviorState"/>. The cartridge reaches
    /// <see cref="OwtchBehaviorState.TurningAtLeftBound"/> by decrementing index zero, so its word is all ones.
    /// </summary>
    internal static readonly NativeWordIdentities<OwtchBehaviorState> Behaviors = new(new Dictionary<OwtchBehaviorState, ushort>
    {
        [OwtchBehaviorState.MovingLeft] = 0,
        [OwtchBehaviorState.MovingRight] = 1,
        [OwtchBehaviorState.Underground] = 2,
        [OwtchBehaviorState.Sinking] = 3,
        [OwtchBehaviorState.Rising] = 4,
        [OwtchBehaviorState.TurningAtLeftBound] = 0xffff,
    });
}
