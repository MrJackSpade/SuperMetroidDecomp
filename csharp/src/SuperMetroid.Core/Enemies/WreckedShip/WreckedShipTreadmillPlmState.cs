using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>Semantic discriminator for the one-frame Wrecked Ship treadmill PLM.</summary>
/// <param name="direction">The belt's push direction.</param>
/// <param name="areaBossDefeated">Whether Phantoon was defeated when the treadmill spawned.</param>
internal sealed class WreckedShipTreadmillPlmState(WreckedShipTreadmillDirection direction, bool areaBossDefeated) : PlmFamilyState
{
    /// <summary>The belt's push direction.</summary>
    internal WreckedShipTreadmillDirection Direction { get; } = direction;

    /// <summary>Whether Phantoon was defeated when the treadmill spawned.</summary>
    internal bool AreaBossDefeated { get; } = areaBossDefeated;

    /// <inheritdoc/>
    internal override PlmFamily Family => PlmFamily.WreckedShipTreadmill;
}
