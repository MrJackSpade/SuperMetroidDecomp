using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>
/// Verification's native view of the Boulder identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class BoulderNativeWords
{
    /// <summary>The routine word naming each <see cref="BoulderAction"/>.</summary>
    internal static readonly NativeWordIdentities<BoulderAction> BoulderActions = new(new Dictionary<BoulderAction, ushort>
    {
        [BoulderAction.WaitingForSamus] = 0x879a,
        [BoulderAction.InitialFall] = 0x87ed,
        [BoulderAction.Rebound] = 0x8832,
        [BoulderAction.Falling] = 0x888b,
        [BoulderAction.Rolling] = 0x8942,
        [BoulderAction.Inert] = 0x89fc,
    });
}
