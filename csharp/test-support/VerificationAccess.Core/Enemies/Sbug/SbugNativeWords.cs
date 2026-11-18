using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>
/// Verification's native view of the Sbug identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class SbugNativeWords
{
    /// <summary>The routine word naming each <see cref="SbugAction"/>.</summary>
    internal static readonly NativeWordIdentities<SbugAction> SbugActions = new(new Dictionary<SbugAction, ushort>
    {
        [SbugAction.WaitForSamus] = 0xa2d7,
        [SbugAction.ActivateMoveForward] = 0xa301,
        [SbugAction.ActivateZigZag] = 0xa30b,
        [SbugAction.ActivateMoveForwardThenWait] = 0xa33b,
        [SbugAction.MoveForward] = 0xa407,
        [SbugAction.ZigZag] = 0xa40e,
        [SbugAction.MoveForwardThenWait] = 0xa44e,
    });
}
