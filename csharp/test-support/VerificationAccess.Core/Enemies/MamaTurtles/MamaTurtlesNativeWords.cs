using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>
/// Verification's native view of the MamaTurtles identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class MamaTurtlesNativeWords
{
    /// <summary>The routine word naming each <see cref="BabyTurtleAction"/>.</summary>
    internal static readonly NativeWordIdentities<BabyTurtleAction> BabyTurtleActions = new(new Dictionary<BabyTurtleAction, ushort>
    {
        [BabyTurtleAction.CrawlingNotCarryingSamus] = 0x9142,
        [BabyTurtleAction.HidingCarryingSamus] = 0x916e,
        [BabyTurtleAction.HidingNotCarryingSamus] = 0x9198,
        [BabyTurtleAction.SpinningUnstoppable] = 0x91f8,
        [BabyTurtleAction.SpinningStoppable] = 0x9239,
        [BabyTurtleAction.CrawlingCarryingSamus] = 0x925e,
    });

    /// <summary>The routine word naming each <see cref="MamaTurtleAction"/>.</summary>
    internal static readonly NativeWordIdentities<MamaTurtleAction> MamaTurtleActions = new(new Dictionary<MamaTurtleAction, ushort>
    {
        [MamaTurtleAction.Initial] = 0x8dd8,
        [MamaTurtleAction.Idle] = 0x8e09,
        [MamaTurtleAction.Asleep] = 0x8e0a,
        [MamaTurtleAction.LeavingShell] = 0x8ee0,
        [MamaTurtleAction.EnteringShell] = 0x8f3f,
        [MamaTurtleAction.RisingToHover] = 0x8f8d,
        [MamaTurtleAction.Hovering] = 0x8feb,
        [MamaTurtleAction.RisingToPeak] = 0x9083,
        [MamaTurtleAction.HoveringAtPeak] = 0x90cc,
        [MamaTurtleAction.Falling] = 0x90e1,
    });
}
