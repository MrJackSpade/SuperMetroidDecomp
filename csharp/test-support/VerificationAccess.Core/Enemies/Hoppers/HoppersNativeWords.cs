using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>
/// Verification's native view of the Hoppers identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class HoppersNativeWords
{
    /// <summary>
    /// The routine words naming each <see cref="HopperAction"/>: the original kept a floor and a ceiling routine for the
    /// direction choice and each start, and a floor and a ceiling routine for each direction of the hop, which a
    /// hopper's placement tells apart (#1814).
    /// </summary>
    internal static readonly NativeWordIdentities<HopperAction> HopperActions = new(new Dictionary<ushort, HopperAction>
    {
        [0xabd6] = HopperAction.ChooseHopSize,
        [0xabe6] = HopperAction.PrepareSmallHop,
        [0xac13] = HopperAction.PrepareBigHop,
        [0xac40] = HopperAction.ChooseDirection,
        [0xac56] = HopperAction.ChooseDirection,
        [0xac6c] = HopperAction.StartBackwardHop,
        [0xac8f] = HopperAction.StartForwardHop,
        [0xaca8] = HopperAction.StartBackwardHop,
        [0xaccb] = HopperAction.StartForwardHop,
        [0xace4] = HopperAction.Landed,
        [0xad0e] = HopperAction.Jumping,
        [0xad20] = HopperAction.Jumping,
        [0xad32] = HopperAction.Jumping,
        [0xad44] = HopperAction.Jumping,
        [0xad56] = HopperAction.WaitToHop,
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
