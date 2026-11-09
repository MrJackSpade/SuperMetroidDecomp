namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>
/// The routine the Norfair lava-jumping enemy's main AI runs. The retail source is commonly labelled
/// <c>NorfairLavajumpingEnemy</c>; the authored name record spells the internal name <c>SQUEEWPT</c>.
/// Its values are arbitrary.
/// </summary>
public enum NorfairLavaJumpingEnemyFunction
{
    /// <summary>The immediately following cosmetic slot copies parent freeze/Y state, is visible only during unfrozen ascent, and deletes itself when parent health is zero.</summary>
    FollowParent = 1,
    /// <summary>Choose one of four signed 8.8 launch velocities from the current RNG word, enable offscreen processing, and request the enemy-library jump sound.</summary>
    BeginJump,
    /// <summary>Integrate Y and add NTSC gravity 56/256 pixel per update squared; unsigned velocity reaching -1024 starts the flip animation while still ascending.</summary>
    RiseBeforeAnimationSwitch,
    /// <summary>Continue the signed 8.8 arc until the flip animation's cue signals completion, then clear the handshake and enter final falling.</summary>
    MoveUntilAnimationSignal,
    /// <summary>Continue descent until Y bits 4..7 equal $F, then restore spawn whole coordinates, install the hidden/rising list, and disable offscreen processing without clearing subpositions.</summary>
    FallBackIntoLava,
}
