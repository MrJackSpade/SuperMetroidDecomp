using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// Verification's native view of the MorphBallEye identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class MorphBallEyeNativeWords
{
    /// <summary>The routine word naming each <see cref="MorphBallEyeAction"/>.</summary>
    internal static readonly NativeWordIdentities<MorphBallEyeAction> MorphBallEyeActions = new(new Dictionary<MorphBallEyeAction, ushort>
    {
        [MorphBallEyeAction.WaitForSamus] = 0x90f1,
        [MorphBallEyeAction.Activating] = 0x912e,
        [MorphBallEyeAction.Active] = 0x9160,
        [MorphBallEyeAction.Deactivating] = 0x91ce,
        [MorphBallEyeAction.MountNoOp] = 0x91dc,
    });
}
