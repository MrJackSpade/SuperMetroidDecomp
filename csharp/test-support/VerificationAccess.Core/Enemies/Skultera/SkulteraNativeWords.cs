using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>
/// Verification's native view of the Skultera identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class SkulteraNativeWords
{
    /// <summary>The routine word naming each <see cref="SkulteraAction"/>.</summary>
    internal static readonly NativeWordIdentities<SkulteraAction> SkulteraActions = new(new Dictionary<SkulteraAction, ushort>
    {
        [SkulteraAction.SwimmingLeft] = 0x9132,
        [SkulteraAction.SwimmingRight] = 0x91ab,
        [SkulteraAction.TurningRight] = 0x9224,
        [SkulteraAction.TurningLeft] = 0x9256,
    });
}
