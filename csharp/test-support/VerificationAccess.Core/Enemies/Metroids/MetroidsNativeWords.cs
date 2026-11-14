using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>
/// Verification's native view of the Metroid and Mochtroid identities (#1809): the cartridge words naming each member.
/// Core's members carry no values; a native snapshot translates through these tables.
/// </summary>
internal static class MetroidsNativeWords
{
    /// <summary>The movement word naming each <see cref="MetroidMovement"/>.</summary>
    internal static readonly NativeWordIdentities<MetroidMovement> Movements = new(new Dictionary<MetroidMovement, ushort>
    {
        [MetroidMovement.Homing] = 0,
        [MetroidMovement.ClosingOnSamus] = 1,
        [MetroidMovement.AttachedToSamus] = 2,
        [MetroidMovement.PowerBombEscape] = 3,
    });

    /// <summary>The movement word naming each <see cref="MochtroidMovementMode"/>.</summary>
    internal static readonly NativeWordIdentities<MochtroidMovementMode> MochtroidMovementModes = new(new Dictionary<MochtroidMovementMode, ushort>
    {
        [MochtroidMovementMode.NotTouchingSamus] = 0,
        [MochtroidMovementMode.TouchingSamus] = 1,
    });
}
