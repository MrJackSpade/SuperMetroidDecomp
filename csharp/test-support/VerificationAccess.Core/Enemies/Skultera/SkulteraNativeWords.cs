using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>
/// Verification's native view of the Skultera identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class SkulteraNativeWords
{
    /// <summary>
    /// The routine word naming each <see cref="SkulteraAction"/> and the heading it carries (#2165), the cartridge giving
    /// each heading a routine of its own.
    /// </summary>
    internal static readonly NativeWordIdentities<(SkulteraAction Action, HorizontalDirection Heading)> SkulteraActions =
        new(new Dictionary<(SkulteraAction Action, HorizontalDirection Heading), ushort>
        {
            [(SkulteraAction.Swimming, HorizontalDirection.Left)] = 0x9132,
            [(SkulteraAction.Swimming, HorizontalDirection.Right)] = 0x91ab,
            [(SkulteraAction.Turning, HorizontalDirection.Right)] = 0x9224,
            [(SkulteraAction.Turning, HorizontalDirection.Left)] = 0x9256,
        });
}
