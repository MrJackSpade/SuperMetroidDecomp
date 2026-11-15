namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>
/// The phase of a jumping Squeept's jump. Its values are arbitrary.
/// </summary>
public enum NorfairLavaJumpingEnemyAction
{
    /// <summary>Choose one of four signed 1/256-pixel launch velocities from the current random number, enable offscreen processing, and request the enemy-library jump sound.</summary>
    BeginJump,
    /// <summary>Integrate Y and add gravity 56/256 pixel per update squared; once the rise has slowed to four pixels an update the flip animation starts while it is still ascending.</summary>
    RiseBeforeAnimationSwitch,
    /// <summary>Continue the signed 1/256-pixel arc until the flip animation's cue signals completion, then clear the handshake and enter final falling.</summary>
    MoveUntilAnimationSignal,
    /// <summary>Continue descent until Y modulo 256 reaches 240, then restore spawn whole coordinates, start the hidden/rising animation, and disable offscreen processing without clearing subpositions.</summary>
    FallBackIntoLava,
}
