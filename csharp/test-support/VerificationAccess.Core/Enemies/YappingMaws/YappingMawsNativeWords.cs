using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>
/// Verification's native view of the YappingMaws identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class YappingMawsNativeWords
{
    /// <summary>The routine word naming each <see cref="YappingMawAction"/>.</summary>
    internal static readonly NativeWordIdentities<YappingMawAction> YappingMawActions = new(new Dictionary<YappingMawAction, ushort>
    {
        [YappingMawAction.WaitingForSamus] = 0xa235,
        [YappingMawAction.BeginExtension] = 0xa28c,
        [YappingMawAction.ExtendingOrRetracting] = 0xa445,
        [YappingMawAction.RetractedDelay] = 0xa68a,
    });
}
