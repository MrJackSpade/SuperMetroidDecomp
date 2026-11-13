using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>
/// Verification's native view of the Kzan identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class KzanNativeWords
{
    /// <summary>The routine word naming each <see cref="KzanAction"/>.</summary>
    internal static readonly NativeWordIdentities<KzanAction> KzanActions = new(new Dictionary<KzanAction, ushort>
    {
        [KzanAction.WaitingToFall] = 0x8bb4,
        [KzanAction.Falling] = 0x8bdc,
        [KzanAction.WaitingToRise] = 0x8c4a,
        [KzanAction.Rising] = 0x8c5d,
    });
}
