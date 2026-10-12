using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>Semantic discriminator for the one-frame Wrecked Ship treadmill PLM.</summary>
internal sealed record WreckedShipTreadmillPlmState(
    WreckedShipTreadmillDirection Direction,
    bool AreaBossDefeated);
