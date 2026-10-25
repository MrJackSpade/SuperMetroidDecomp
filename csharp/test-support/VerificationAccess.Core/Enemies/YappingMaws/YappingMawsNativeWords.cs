using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>
/// Verification's native view of the YappingMaws identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class YappingMawsNativeWords
{
    /// <summary>The routine word naming each <see cref="YappingMawAiFunction"/>.</summary>
    internal static readonly NativeWordIdentities<YappingMawAiFunction> YappingMawAiFunctions = new(new Dictionary<YappingMawAiFunction, ushort>
    {
        [YappingMawAiFunction.WaitingForSamus] = 0xa235,
        [YappingMawAiFunction.BeginExtension] = 0xa28c,
        [YappingMawAiFunction.ExtendingOrRetracting] = 0xa445,
        [YappingMawAiFunction.RetractedDelay] = 0xa68a,
    });
}
