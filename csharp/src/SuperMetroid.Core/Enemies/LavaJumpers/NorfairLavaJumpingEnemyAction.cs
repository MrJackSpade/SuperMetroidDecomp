namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>
/// The phase of a jumping Squeept's jump. Its values are arbitrary.
/// </summary>
public enum NorfairLavaJumpingEnemyAction
{
    /// <summary>Chooses one of four launch speeds from the current random number, keeps updating off screen, and plays the jump sound.</summary>
    BeginJump,
    /// <summary>Rises under gravity of 56 256ths of a pixel an update each update; once the rise has slowed to four pixels an update the flip animation starts while it is still ascending.</summary>
    RiseBeforeAnimationSwitch,
    /// <summary>Keeps to its arc until the flip animation signals its end, then falls back.</summary>
    MoveUntilAnimationSignal,
    /// <summary>Falls until the jumper reaches the bottom block row of a room screen, back in the lava, then returns to where it was placed, keeping its fractions of a pixel, starts its hidden rising animation and stops updating off screen.</summary>
    FallBackIntoLava,
}
