namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The game, power-bomb, projectile and bomb state that can block X-ray activation (#633).</summary>
public sealed record XrayBeginConditions
{
    /// <summary>Live game state; setup rejects every state but main gameplay.</summary>
    public required SuperMetroid.Core.Progression.SuperMetroidGameState GameState { get; init; }

    /// <summary>Whether a power bomb explosion's status is set; any set status rejects X-ray activation.</summary>
    public bool PowerBombExplosionActive { get; init; } = false;

    /// <summary>The shared projectile cooldown, which X-ray reads to tell whether Samus reads as drained (<see cref="XrayRefusedAsDrained"/>).</summary>
    public WrappingCountdown ProjectileCooldownTimer { get; init; } = WrappingCountdown.Start(0);

    /// <summary>The number of live bombs, a count of bomb slots, which X-ray reads to tell whether Samus reads as drained (<see cref="XrayRefusedAsDrained"/>).</summary>
    public int BombCounter { get; init; } = 0;
}
