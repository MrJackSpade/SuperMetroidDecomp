using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>
/// Verification's native view of the Nuclear identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class NuclearNativeWords
{
    /// <summary>The routine word naming each <see cref="NuclearWaffleAction"/>.</summary>
    internal static readonly NativeWordIdentities<NuclearWaffleAction> NuclearWaffleActions = new(new Dictionary<NuclearWaffleAction, ushort>
    {
        [NuclearWaffleAction.Waiting] = 0x9615,
        [NuclearWaffleAction.Sweeping] = 0x9682,
    });
}
