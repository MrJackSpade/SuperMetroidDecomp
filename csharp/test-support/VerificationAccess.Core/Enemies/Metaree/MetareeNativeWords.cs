using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Metaree;

/// <summary>
/// Verification's native view of the Metaree identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class MetareeNativeWords
{
    /// <summary>The routine word naming each <see cref="MetareeAction"/>.</summary>
    internal static readonly NativeWordIdentities<MetareeAction> MetareeActions = new(new Dictionary<MetareeAction, ushort>
    {
        [MetareeAction.Idling] = 0x8987,
        [MetareeAction.PreparingAttack] = 0x89d4,
        [MetareeAction.LaunchedAttack] = 0x89f3,
        [MetareeAction.Burrowing] = 0x8a5c,
    });
}
