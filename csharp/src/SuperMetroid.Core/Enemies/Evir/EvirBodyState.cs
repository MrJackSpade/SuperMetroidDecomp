namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>The state of an Evir's body, which bobs up and down and turns to face Samus while its spit is present.</summary>
public sealed class EvirBodyState : EvirPartState
{
    /// <summary>Starts the state of a body whose spit the room lists at <paramref name="spit"/>, two places after it.</summary>
    internal EvirBodyState(RoomEnemyPlace spit) => Spit = Ensure.NotNull(spit);

    /// <summary>The place of the body's spit, which its room lists two places after it, found when the body was placed.</summary>
    internal RoomEnemyPlace Spit { get; }

    /// <summary>
    /// The body's bobbing countdown, decremented every update and expiring when it steps below zero, which reverses
    /// the bobbing and restarts it at the body's <see cref="DirectionChangePeriod"/>.
    /// </summary>
    public int MovementTimer { get; internal set; }

    /// <summary>The updates each later half of the bobbing lasts, from the body's spawn settings.</summary>
    public int DirectionChangePeriod { get; internal set; }

    /// <summary>Whether the body is bobbing downward rather than upward at its <see cref="BobbingSpeed"/>; reversed each time <see cref="MovementTimer"/> expires.</summary>
    public bool MovingDown { get; internal set; }

    /// <summary>The distance the body bobs each update, in pixels: added to Y while it bobs down and subtracted while it bobs up.</summary>
    public SignedSixteenSixteen BobbingSpeed { get; internal set; }
}
