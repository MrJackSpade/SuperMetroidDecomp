using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The game, power-bomb, projectile and bomb state that can block X-ray activation (#633).</summary>
public sealed partial record XrayBeginConditions
{
    /// <summary>Live game state; setup rejects every state but main gameplay.</summary>
    public required SuperMetroidGameState GameState { get; init; }

    /// <summary>Whether a power bomb is exploding; X-ray cannot start while one does.</summary>
    public required bool PowerBombExplosionActive { get; init; }

    /// <summary>The shared projectile cooldown, which X-ray reads to tell whether Samus reads as drained (<see cref="XrayRefusedAsDrained"/>).</summary>
    public required int ProjectileCooldownTimer { get; init; }

    /// <summary>The number of live bombs, which X-ray reads to tell whether Samus reads as drained (<see cref="XrayRefusedAsDrained"/>).</summary>
    public required int BombCounter { get; init; }
}
