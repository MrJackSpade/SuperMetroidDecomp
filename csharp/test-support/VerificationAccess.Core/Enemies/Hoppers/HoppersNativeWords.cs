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
    /// hopper's placement tells apart (#1814). A preparing hop's routine names its size, and a starting hop's the heading
    /// it takes (#2338).
    /// </summary>
    internal static readonly NativeWordIdentities<(HopperAction Action, HopperHopSize? Size, HorizontalDirection? Heading)> HopperActions =
        new(new Dictionary<ushort, (HopperAction Action, HopperHopSize? Size, HorizontalDirection? Heading)>
        {
            [0xabd6] = (HopperAction.ChooseHopSize, null, null),
            [0xabe6] = (HopperAction.PrepareHop, HopperHopSize.Small, null),
            [0xac13] = (HopperAction.PrepareHop, HopperHopSize.Big, null),
            [0xac40] = (HopperAction.ChooseDirection, null, null),
            [0xac56] = (HopperAction.ChooseDirection, null, null),
            [0xac6c] = (HopperAction.StartHop, null, HorizontalDirection.Left),
            [0xac8f] = (HopperAction.StartHop, null, HorizontalDirection.Right),
            [0xaca8] = (HopperAction.StartHop, null, HorizontalDirection.Left),
            [0xaccb] = (HopperAction.StartHop, null, HorizontalDirection.Right),
            [0xace4] = (HopperAction.Landed, null, null),
            [0xad0e] = (HopperAction.Jumping, null, null),
            [0xad20] = (HopperAction.Jumping, null, null),
            [0xad32] = (HopperAction.Jumping, null, null),
            [0xad44] = (HopperAction.Jumping, null, null),
            [0xad56] = (HopperAction.WaitToHop, null, null),
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
