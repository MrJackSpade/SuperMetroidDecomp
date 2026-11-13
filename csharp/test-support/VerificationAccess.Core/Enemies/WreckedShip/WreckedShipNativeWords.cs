using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>
/// Verification's native view of the WreckedShip identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class WreckedShipNativeWords
{
    /// <summary>The routine word naming each <see cref="WreckedShipGhostAction"/>.</summary>
    internal static readonly NativeWordIdentities<WreckedShipGhostAction> WreckedShipGhostActions = new(new Dictionary<WreckedShipGhostAction, ushort>
    {
        [WreckedShipGhostAction.BrighteningAndFlickering] = 0x9b42,
        [WreckedShipGhostAction.FadingToGhostColours] = 0x9bad,
        [WreckedShipGhostAction.WaitingForWhiteFade] = 0x9c69,
        [WreckedShipGhostAction.BobbingWhileVisible] = 0x9c8a,
        [WreckedShipGhostAction.InitialInvisibleDelay] = 0x9d13,
        [WreckedShipGhostAction.TrackingSamusForSpawn] = 0x9d36,
    });
}
