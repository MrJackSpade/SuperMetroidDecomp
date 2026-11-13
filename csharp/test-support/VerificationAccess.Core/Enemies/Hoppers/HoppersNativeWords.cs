using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>
/// Verification's native view of the Hoppers identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class HoppersNativeWords
{
    /// <summary>The routine word naming each <see cref="HopperAction"/>.</summary>
    internal static readonly NativeWordIdentities<HopperAction> HopperActions = new(new Dictionary<HopperAction, ushort>
    {
        [HopperAction.ChooseHopSize] = 0xabd6,
        [HopperAction.PrepareSmallHop] = 0xabe6,
        [HopperAction.PrepareBigHop] = 0xac13,
        [HopperAction.ChooseDirectionUpsideUp] = 0xac40,
        [HopperAction.ChooseDirectionUpsideDown] = 0xac56,
        [HopperAction.StartBackwardHopUpsideUp] = 0xac6c,
        [HopperAction.StartForwardHopUpsideUp] = 0xac8f,
        [HopperAction.StartBackwardHopUpsideDown] = 0xaca8,
        [HopperAction.StartForwardHopUpsideDown] = 0xaccb,
        [HopperAction.Landed] = 0xace4,
        [HopperAction.JumpingUpsideUpBackward] = 0xad0e,
        [HopperAction.JumpingUpsideUpForward] = 0xad20,
        [HopperAction.JumpingUpsideDownBackward] = 0xad32,
        [HopperAction.JumpingUpsideDownForward] = 0xad44,
        [HopperAction.WaitToHop] = 0xad56,
    });

    /// <summary>
    /// The variant word naming each <see cref="HopperKind"/> in a hopper's enemy header, which indexes its four-entry
    /// animation tables.
    /// </summary>
    internal static readonly NativeWordIdentities<HopperKind> HopperKinds = new(new Dictionary<HopperKind, ushort>
    {
        [HopperKind.Sidehopper] = 0,
        [HopperKind.LargeSidehopper] = 1,
        [HopperKind.LargeDessgeega] = 2,
        [HopperKind.Dessgeega] = 3,
    });
}
