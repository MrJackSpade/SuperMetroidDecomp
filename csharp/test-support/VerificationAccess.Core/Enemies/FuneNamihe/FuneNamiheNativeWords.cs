using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// Verification's native view of the FuneNamihe identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class FuneNamiheNativeWords
{
    /// <summary>The routine word naming each <see cref="FuneNamiheAction"/>.</summary>
    internal static readonly NativeWordIdentities<FuneNamiheAction> FuneNamiheActions = new(new Dictionary<FuneNamiheAction, ushort>
    {
        [FuneNamiheAction.FuneWaitForCooldown] = 0x9737,
        [FuneNamiheAction.NamiheWaitForSamus] = 0x975c,
        [FuneNamiheAction.FuneActivityNoOp] = 0x978e,
        [FuneNamiheAction.NamiheActivityNoOp] = 0x978f,
    });
}
