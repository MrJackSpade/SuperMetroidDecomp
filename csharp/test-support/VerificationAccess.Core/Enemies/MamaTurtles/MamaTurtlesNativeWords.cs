using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>
/// Verification's native view of the MamaTurtles identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class MamaTurtlesNativeWords
{
    /// <summary>The routine word naming each <see cref="BabyTurtleAiFunction"/>.</summary>
    internal static readonly NativeWordIdentities<BabyTurtleAiFunction> BabyTurtleAiFunctions = new(new Dictionary<BabyTurtleAiFunction, ushort>
    {
        [BabyTurtleAiFunction.CrawlingNotCarryingSamus] = 0x9142,
        [BabyTurtleAiFunction.HidingCarryingSamus] = 0x916e,
        [BabyTurtleAiFunction.HidingNotCarryingSamus] = 0x9198,
        [BabyTurtleAiFunction.SpinningUnstoppable] = 0x91f8,
        [BabyTurtleAiFunction.SpinningStoppable] = 0x9239,
        [BabyTurtleAiFunction.CrawlingCarryingSamus] = 0x925e,
    });

    /// <summary>The routine word naming each <see cref="MamaTurtleAiFunction"/>.</summary>
    internal static readonly NativeWordIdentities<MamaTurtleAiFunction> MamaTurtleAiFunctions = new(new Dictionary<MamaTurtleAiFunction, ushort>
    {
        [MamaTurtleAiFunction.Initial] = 0x8dd8,
        [MamaTurtleAiFunction.Idle] = 0x8e09,
        [MamaTurtleAiFunction.Asleep] = 0x8e0a,
        [MamaTurtleAiFunction.LeavingShell] = 0x8ee0,
        [MamaTurtleAiFunction.EnteringShell] = 0x8f3f,
        [MamaTurtleAiFunction.RisingToHover] = 0x8f8d,
        [MamaTurtleAiFunction.Hovering] = 0x8feb,
        [MamaTurtleAiFunction.RisingToPeak] = 0x9083,
        [MamaTurtleAiFunction.HoveringAtPeak] = 0x90cc,
        [MamaTurtleAiFunction.Falling] = 0x90e1,
    });
}
