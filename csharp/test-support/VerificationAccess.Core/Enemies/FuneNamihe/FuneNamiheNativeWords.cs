using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// Verification's native view of the FuneNamihe identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class FuneNamiheNativeWords
{
    /// <summary>
    /// The routine words naming each <see cref="FuneNamiheAction"/>: Fune and Namihe spit while running one of two
    /// routines that each return at once (#1814).
    /// </summary>
    internal static readonly NativeWordIdentities<FuneNamiheAction> FuneNamiheActions = new(new Dictionary<ushort, FuneNamiheAction>
    {
        [0x9737] = FuneNamiheAction.FuneWaitForCooldown,
        [0x975c] = FuneNamiheAction.NamiheWaitForSamus,
        [0x978e] = FuneNamiheAction.Spitting,
        [0x978f] = FuneNamiheAction.Spitting,
    });
}
