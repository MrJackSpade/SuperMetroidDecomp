using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// Verification's native view of the FuneNamihe identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class FuneNamiheNativeWords
{
    /// <summary>
    /// The routine words naming each <see cref="FuneAction"/>: a Fune spits while running one of two routines that each
    /// return at once (#1814), which the Namihe shares.
    /// </summary>
    internal static readonly NativeWordIdentities<FuneAction> FuneActions = new(new Dictionary<ushort, FuneAction>
    {
        [0x9737] = FuneAction.WaitForCooldown,
        [0x978e] = FuneAction.Spitting,
        [0x978f] = FuneAction.Spitting,
    });

    /// <summary>
    /// The routine words naming each <see cref="NamiheAction"/>: a Namihe spits while running one of two routines that
    /// each return at once (#1814), which the Fune shares.
    /// </summary>
    internal static readonly NativeWordIdentities<NamiheAction> NamiheActions = new(new Dictionary<ushort, NamiheAction>
    {
        [0x975c] = NamiheAction.WaitForSamus,
        [0x978e] = NamiheAction.Spitting,
        [0x978f] = NamiheAction.Spitting,
    });
}
