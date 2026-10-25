using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>
/// Verification's native view of the Hoppers identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class HoppersNativeWords
{
    /// <summary>The routine word naming each <see cref="HopperEnemyFunction"/>.</summary>
    internal static readonly NativeWordIdentities<HopperEnemyFunction> HopperEnemyFunctions = new(new Dictionary<HopperEnemyFunction, ushort>
    {
        [HopperEnemyFunction.ChooseHopSize] = 0xabd6,
        [HopperEnemyFunction.PrepareSmallHop] = 0xabe6,
        [HopperEnemyFunction.PrepareBigHop] = 0xac13,
        [HopperEnemyFunction.ChooseDirectionUpsideUp] = 0xac40,
        [HopperEnemyFunction.ChooseDirectionUpsideDown] = 0xac56,
        [HopperEnemyFunction.StartBackwardHopUpsideUp] = 0xac6c,
        [HopperEnemyFunction.StartForwardHopUpsideUp] = 0xac8f,
        [HopperEnemyFunction.StartBackwardHopUpsideDown] = 0xaca8,
        [HopperEnemyFunction.StartForwardHopUpsideDown] = 0xaccb,
        [HopperEnemyFunction.Landed] = 0xace4,
        [HopperEnemyFunction.JumpingUpsideUpBackward] = 0xad0e,
        [HopperEnemyFunction.JumpingUpsideUpForward] = 0xad20,
        [HopperEnemyFunction.JumpingUpsideDownBackward] = 0xad32,
        [HopperEnemyFunction.JumpingUpsideDownForward] = 0xad44,
        [HopperEnemyFunction.WaitToHop] = 0xad56,
    });
}
