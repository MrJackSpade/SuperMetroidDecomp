using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// Verification's native view of the MorphBallEye identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class MorphBallEyeNativeWords
{
    /// <summary>The routine word naming each <see cref="MorphBallEyeAiFunction"/>.</summary>
    internal static readonly NativeWordIdentities<MorphBallEyeAiFunction> MorphBallEyeAiFunctions = new(new Dictionary<MorphBallEyeAiFunction, ushort>
    {
        [MorphBallEyeAiFunction.WaitForSamus] = 0x90f1,
        [MorphBallEyeAiFunction.Activating] = 0x912e,
        [MorphBallEyeAiFunction.Active] = 0x9160,
        [MorphBallEyeAiFunction.Deactivating] = 0x91ce,
        [MorphBallEyeAiFunction.MountNoOp] = 0x91dc,
    });
}
