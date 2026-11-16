namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// The state of one Dragon body. The body's motion timer and its three-shot counter are separate countdowns here, since
/// every phase starts its own before reading it.
/// </summary>
public sealed class DragonEnemyState : DragonPartState
{
    /// <summary>Starts the body in <paramref name="action"/>.</summary>
    internal DragonEnemyState(DragonAction action, RoomEnemySlot wing)
    {
        Action = action;
        Wing = Ensure.NotNull(wing);
    }

    /// <summary>The body's cosmetic wing, which its room lists right after it, found when the body was placed.</summary>
    internal RoomEnemySlot Wing { get; }

    /// <summary>Whether the body last turned to face left, toward a Samus on its left; the fireball spawns on that side.</summary>
    public bool FacingLeft { get; internal set; }

    /// <summary>The body's countdown while waiting, rising, or sinking.</summary>
    public int ActionTimer { get; internal set; }

    /// <summary>The body's shots left while attacking; initialized to three before the first volley.</summary>
    public int AttackCounter { get; internal set; }

    /// <summary>The body's current behaviour.</summary>
    public DragonAction Action { get; internal set; }

    /// <summary>Set by the attack animation's finishing cue.</summary>
    public bool AnimationFinished { get; internal set; }
}
