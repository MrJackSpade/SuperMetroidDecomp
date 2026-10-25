using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>
/// Verification's native view of the Boulder identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class BoulderNativeWords
{
    /// <summary>The routine word naming each <see cref="BoulderAiFunction"/>.</summary>
    internal static readonly NativeWordIdentities<BoulderAiFunction> BoulderAiFunctions = new(new Dictionary<BoulderAiFunction, ushort>
    {
        [BoulderAiFunction.WaitingForSamus] = 0x879a,
        [BoulderAiFunction.InitialFall] = 0x87ed,
        [BoulderAiFunction.Rebound] = 0x8832,
        [BoulderAiFunction.Falling] = 0x888b,
        [BoulderAiFunction.Rolling] = 0x8942,
        [BoulderAiFunction.Inert] = 0x89fc,
    });
}
