using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// Verification's native view of the Chozo identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class ChozoNativeWords
{
    /// <summary>The routine word naming each <see cref="ChozoStatueFrameBehavior"/>.</summary>
    internal static readonly NativeWordIdentities<ChozoStatueFrameBehavior> ChozoStatueFrameBehaviors = new(new Dictionary<ChozoStatueFrameBehavior, ushort>
    {
        [ChozoStatueFrameBehavior.Idle] = 0xe7a6,
        [ChozoStatueFrameBehavior.Cleared] = 0x807b,
        [ChozoStatueFrameBehavior.WaitForLowerNorfairHandTrigger] = 0xe445,
        [ChozoStatueFrameBehavior.WaitForWreckedShipHandTrigger] = 0xe7ae,
        [ChozoStatueFrameBehavior.Walking] = 0xe7da,
    });
}
