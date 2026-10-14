
namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>Semantic discriminator for the one-frame Wrecked Ship treadmill PLM.</summary>
internal sealed record WreckedShipTreadmillPlmState(
    WreckedShipTreadmillDirection Direction,
    bool AreaBossDefeated);
