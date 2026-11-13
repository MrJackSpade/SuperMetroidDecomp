using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Flies;

/// <summary>
/// Verification's native view of the Flies identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class FliesNativeWords
{
    /// <summary>The routine word naming each <see cref="FlyAction"/>.</summary>
    internal static readonly NativeWordIdentities<FlyAction> FlyActions = new(new Dictionary<FlyAction, ushort>
    {
        [FlyAction.ClockwiseCircle] = 0xb14e,
        [FlyAction.AntiClockwiseCircle] = 0xb17c,
        [FlyAction.AttackSamus] = 0xb1aa,
        [FlyAction.Retreat] = 0xb1d2,
    });
}
