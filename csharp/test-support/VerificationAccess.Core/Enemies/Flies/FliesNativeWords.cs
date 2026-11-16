using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Flies;

/// <summary>
/// Verification's native view of the Flies identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class FliesNativeWords
{
    /// <summary>
    /// The routine word naming each <see cref="FlyAction"/> and, for circling, the way it circles (#2277), the cartridge
    /// giving each way a routine of its own.
    /// </summary>
    internal static readonly NativeWordIdentities<(FlyAction Action, RotationDirection? Rotation)> FlyActions =
        new(new Dictionary<(FlyAction Action, RotationDirection? Rotation), ushort>
        {
            [(FlyAction.Circling, RotationDirection.Clockwise)] = 0xb14e,
            [(FlyAction.Circling, RotationDirection.Counterclockwise)] = 0xb17c,
            [(FlyAction.AttackSamus, null)] = 0xb1aa,
            [(FlyAction.Retreat, null)] = 0xb1d2,
        });
}
