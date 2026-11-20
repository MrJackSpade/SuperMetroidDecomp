namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>
/// The phase of a jumping Squeept's jump. Its values are arbitrary.
/// </summary>
public enum SqueeptAction
{
    /// <summary>Chooses one of four launch speeds from the current random number, keeps updating off screen, and plays the jump sound.</summary>
    BeginJump,
    /// <summary>Rises under its gravity (<see cref="NorfairLavaJumpDefinitions.Gravity"/>) each update; once the rise has slowed to four pixels an update the flip animation starts while it is still ascending.</summary>
    RiseBeforeAnimationSwitch,
    /// <summary>Keeps to its arc while the flip animation plays.</summary>
    MoveUntilAnimationSignal,
    /// <summary>
    /// The flip animation has finished, as its cue says (#2439): the next update keeps to the arc once more and then falls
    /// back.
    /// </summary>
    Flipped,
    /// <summary>Falls until the jumper reaches the bottom block row of a room screen, back in the lava, then returns to where it was placed, keeping its fractions of a pixel, starts its hidden rising animation and stops updating off screen.</summary>
    FallBackIntoLava,
}
