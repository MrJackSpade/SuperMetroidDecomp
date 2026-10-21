namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The game, power-bomb, projectile and bomb state that can block X-ray activation (#633).</summary>
public sealed record XrayBeginConditions
{
    /// <summary>Live game state at <c>$0998</c>; setup rejects every state but main gameplay.</summary>
    public required SuperMetroid.Core.Progression.SuperMetroidGameState GameState { get; init; }

    /// <summary>Whether a power bomb explosion's status is set; any set status rejects X-ray activation.</summary>
    public bool PowerBombExplosionActive { get; init; } = false;

    /// <summary>The shared projectile cooldown; exactly seven, with five bombs and divisor two, rejects activation.</summary>
    public WrappingCountdown ProjectileCooldownTimer { get; init; } = WrappingCountdown.Start(0);

    /// <summary>The number of live bombs, a count of bomb slots; exactly five, with that cooldown, rejects activation.</summary>
    public int BombCounter { get; init; } = 0;
}
