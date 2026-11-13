using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>
/// Verification's native view of the Bull identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class BullNativeWords
{
    /// <summary>The routine word naming each <see cref="BullAction"/>.</summary>
    internal static readonly NativeWordIdentities<BullAction> BullActions = new(new Dictionary<BullAction, ushort>
    {
        [BullAction.MovementDelay] = 0xd92b,
        [BullAction.TargetSamus] = 0xd940,
        [BullAction.Accelerating] = 0xd963,
        [BullAction.Decelerating] = 0xd97c,
    });
}
