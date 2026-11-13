using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>
/// Verification's native view of the Hibashi identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class HibashiNativeWords
{
    /// <summary>The routine word naming each <see cref="HibashiAction"/>.</summary>
    internal static readonly NativeWordIdentities<HibashiAction> HibashiActions = new(new Dictionary<HibashiAction, ushort>
    {
        [HibashiAction.Inactive] = 0x902f,
        [HibashiAction.Active] = 0x9062,
    });
}
