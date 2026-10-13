using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Game;

/// <summary>The game, power-bomb, projectile and bomb state that can block X-ray activation (#633).</summary>
public sealed record XrayBeginConditions
{
    /// <summary>Live game state at <c>$0998</c>; setup rejects every state but main gameplay.</summary>
    public required ushort GameState { get; init; }

    /// <summary>Power bomb explosion status.</summary>
    public ushort PowerBombExplosionStatus { get; init; } = 0;

    /// <summary>Projectile cooldown timer.</summary>
    public ushort ProjectileCooldownTimer { get; init; } = 0;

    /// <summary>Bomb counter.</summary>
    public ushort BombCounter { get; init; } = 0;
}
