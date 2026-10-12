using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Game;

/// <summary>The game, power-bomb, projectile and bomb state that can block X-ray activation (#633).</summary>
public sealed record XrayBeginConditions
{
    /// <summary>Game state.</summary>
    public ushort GameState { get; init; } = 8;

    /// <summary>Power bomb explosion status.</summary>
    public ushort PowerBombExplosionStatus { get; init; } = 0;

    /// <summary>Projectile cooldown timer.</summary>
    public ushort ProjectileCooldownTimer { get; init; } = 0;

    /// <summary>Bomb counter.</summary>
    public ushort BombCounter { get; init; } = 0;
}
