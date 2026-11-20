namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>The state of an Evir's body, which bobs up and down and turns to face Samus while its spit is present.</summary>
public sealed class EvirBodyState : EvirPartState
{
    /// <summary>What the enemy's population entry told its species, which it starts from and reads for its life (#2554).</summary>
    internal EvirSpawnSettings Settings { get; }

    /// <summary>Starts the state of a body whose spit the room lists at <paramref name="spit"/>, two places after it.</summary>
    /// <param name="settings">What the enemy's population entry told its species.</param>
    internal EvirBodyState(RoomEnemyPlace spit, EvirSpawnSettings settings)
    {
        Settings = Ensure.NotNull(settings);
        Spit = Ensure.NotNull(spit);
    }

    /// <summary>The place of the body's spit, which its room lists two places after it, found when the body was placed.</summary>
    internal RoomEnemyPlace Spit { get; }

    /// <summary>
    /// The updates left in the body's current half of the bobbing, the one it turns on included: the step that takes it to
    /// zero reverses the bobbing and restarts it at its spawn settings' direction-change period and the update it runs
    /// out on (<see cref="EnemyWaitDefinitions.RunOutUpdate"/>, #2281, #2476).
    /// </summary>
    public UpdateCountdown MovementTimer { get; internal set; } = UpdateCountdown.Ended();

    /// <summary>
    /// The way the body bobs at its spawn settings' speed (#2369): up from its spawn, reversed each time
    /// <see cref="MovementTimer"/> expires.
    /// </summary>
    public VerticalDirection BobbingDirection { get; internal set; }

    /// <summary>The body's vertical velocity: <paramref name="speed"/>, its spawn settings' bobbing speed, toward its <see cref="BobbingDirection"/>.</summary>
    internal SignedSixteenSixteen BobbingVelocity(SignedSixteenSixteen speed) => speed.Toward(BobbingDirection);
}
