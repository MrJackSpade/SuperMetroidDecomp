namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// The state of one Dragon body. The body's motion timer and its three-shot counter are separate countdowns here, since
/// every phase starts its own before reading it.
/// </summary>
public sealed class DragonEnemyState : DragonPartState
{
    /// <summary>Starts the body in <paramref name="action"/>.</summary>
    internal DragonEnemyState(DragonAction action, RoomEnemyPlace wing)
    {
        Action = action;
        Wing = Ensure.NotNull(wing);
    }

    /// <summary>The body's cosmetic wing, which its room lists right after it, found when the body was placed.</summary>
    internal RoomEnemyPlace Wing { get; }

    /// <summary>The way the body last turned to face, toward Samus (#2277); the fireball spawns on that side.</summary>
    public HorizontalDirection Facing { get; internal set; }

    /// <summary>
    /// The updates left in the body's wait, rise or sink, the one it ends on included: the step that takes it to zero ends
    /// the phase, and one at zero ends it on its next step (#2281).
    /// </summary>
    public int ActionTimer { get; internal set; }

    /// <summary>The body's shots left while attacking; initialized to three before the first volley.</summary>
    public int AttackCounter { get; internal set; }

    /// <summary>
    /// The body's current behaviour. The attack's finishing cue moves it from <see cref="DragonAction.Attacking"/> to
    /// <see cref="DragonAction.AttackFinished"/>, which the next update leaves (#2476).
    /// </summary>
    public DragonAction Action { get; internal set; }
}
