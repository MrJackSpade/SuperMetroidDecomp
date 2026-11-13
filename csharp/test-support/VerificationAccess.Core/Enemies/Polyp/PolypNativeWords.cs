using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>
/// Verification's native view of the Polyp identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class PolypNativeWords
{
    /// <summary>The routine word naming each <see cref="PolypAction"/>.</summary>
    internal static readonly NativeWordIdentities<PolypAction> PolypActions = new(new Dictionary<PolypAction, ushort>
    {
        [PolypAction.WaitingForSamus] = 0xb596,
        [PolypAction.ShootingRock] = 0xb5b2,
        [PolypAction.Cooldown] = 0xb5ea,
    });

    /// <summary>The routine word naming each <see cref="PolypRockFunction"/>.</summary>
    internal static readonly NativeWordIdentities<PolypRockFunction> PolypRockFunctions = new(new Dictionary<PolypRockFunction, ushort>
    {
        [PolypRockFunction.Rising] = 0xbc16,
        [PolypRockFunction.Falling] = 0xbc8f,
    });
}
