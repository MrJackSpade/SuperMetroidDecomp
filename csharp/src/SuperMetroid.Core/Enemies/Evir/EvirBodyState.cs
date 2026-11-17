namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>The state of an Evir's body, which bobs up and down and turns to face Samus while its spit is present.</summary>
public sealed class EvirBodyState : EvirPartState
{
    /// <summary>Starts the state of a body whose spit the room lists at <paramref name="spit"/>, two places after it.</summary>
    internal EvirBodyState(RoomEnemyPlace spit) => Spit = Ensure.NotNull(spit);

    /// <summary>The place of the body's spit, which its room lists two places after it, found when the body was placed.</summary>
    internal RoomEnemyPlace Spit { get; }

    /// <summary>
    /// The updates left in the body's current half of the bobbing, the one it turns on included: the step that takes it to
    /// zero reverses the bobbing and restarts it at the body's <see cref="DirectionChangePeriod"/> and the update it runs
    /// out on (<see cref="EnemyWaitDefinitions.RunOutUpdate"/>, #2281).
    /// </summary>
    public int MovementTimer { get; internal set; }

    /// <summary>The updates each later half of the bobbing lasts, from the body's spawn settings.</summary>
    public int DirectionChangePeriod { get; internal set; }

    /// <summary>The way the body bobs at its <see cref="BobbingSpeed"/> (#2369): up from its spawn, reversed each time <see cref="MovementTimer"/> expires.</summary>
    public VerticalDirection BobbingDirection { get; internal set; }

    /// <summary>The distance the body bobs each update, in pixels, toward its <see cref="BobbingDirection"/>.</summary>
    public SignedSixteenSixteen BobbingSpeed { get; internal set; }

    /// <summary>The body's vertical velocity: its <see cref="BobbingSpeed"/> toward its <see cref="BobbingDirection"/>.</summary>
    internal SignedSixteenSixteen BobbingVelocity => BobbingDirection == VerticalDirection.Down ? BobbingSpeed : -BobbingSpeed;
}
