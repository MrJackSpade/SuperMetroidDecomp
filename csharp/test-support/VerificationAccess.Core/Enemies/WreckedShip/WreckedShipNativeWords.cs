using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>
/// Verification's native view of the WreckedShip identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class WreckedShipNativeWords
{
    /// <summary>The routine word naming each <see cref="WreckedShipGhostAiFunction"/>.</summary>
    internal static readonly NativeWordIdentities<WreckedShipGhostAiFunction> WreckedShipGhostAiFunctions = new(new Dictionary<WreckedShipGhostAiFunction, ushort>
    {
        [WreckedShipGhostAiFunction.BrighteningAndFlickering] = 0x9b42,
        [WreckedShipGhostAiFunction.FadingToGhostPalette] = 0x9bad,
        [WreckedShipGhostAiFunction.WaitingForWhiteFade] = 0x9c69,
        [WreckedShipGhostAiFunction.BobbingWhileVisible] = 0x9c8a,
        [WreckedShipGhostAiFunction.InitialInvisibleDelay] = 0x9d13,
        [WreckedShipGhostAiFunction.TrackingSamusForSpawn] = 0x9d36,
    });
}
