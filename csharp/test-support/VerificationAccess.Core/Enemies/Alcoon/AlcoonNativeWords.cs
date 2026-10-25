using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>
/// Verification's native view of the Alcoon identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class AlcoonNativeWords
{
    /// <summary>The routine word naming each <see cref="AlcoonEnemyFunction"/>.</summary>
    internal static readonly NativeWordIdentities<AlcoonEnemyFunction> AlcoonEnemyFunctions = new(new Dictionary<AlcoonEnemyFunction, ushort>
    {
        [AlcoonEnemyFunction.WaitingForSamus] = 0xdd71,
        [AlcoonEnemyFunction.EmergingRising] = 0xddc6,
        [AlcoonEnemyFunction.EmergingFalling] = 0xde05,
        [AlcoonEnemyFunction.WalkingAndFiring] = 0xde4b,
        [AlcoonEnemyFunction.WaitingForFireAnimation] = 0xdecc,
        [AlcoonEnemyFunction.HidingRising] = 0xdecd,
        [AlcoonEnemyFunction.HidingFalling] = 0xdeec,
    });
}
