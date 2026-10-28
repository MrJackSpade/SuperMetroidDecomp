namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>
/// The routine the Norfair lava-jumping enemy's main AI runs. The retail source is commonly labelled
/// <c>NorfairLavajumpingEnemy</c>; the ROM name record spells the internal name <c>SQUEEWPT</c>.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum NorfairLavaJumpingEnemyFunction
{
    /// <summary>$A2:BEDC, Function_Squeept_Flame: the immediately following cosmetic slot copies parent freeze/Y state, is visible only during unfrozen ascent, and deletes itself when parent health is zero.</summary>
    FollowParent = 1,
    /// <summary>$A2:BF1A, Function_Squeept_Jump: choose one of four signed 8.8 launch velocities from the current RNG word, enable offscreen processing, and request library-two jump sound $0D.</summary>
    BeginJump,
    /// <summary>$A2:BF3E, Function_Squeept_Rising: integrate Y and add NTSC gravity $0038 (56/256 pixel per update squared); unsigned velocity reaching $FC00 starts the flip list while still ascending.</summary>
    RiseBeforeAnimationSwitch,
    /// <summary>$A2:BF7C, Function_Squeept_Flipping: continue the signed 8.8 arc until animation instruction $BE8E signals completion, then clear the handshake and enter final falling.</summary>
    MoveUntilAnimationSignal,
    /// <summary>$A2:BFBC, Function_Squeept_Falling: continue descent until Y bits 4..7 equal $F, then restore spawn whole coordinates, install the hidden/rising list, and disable offscreen processing without clearing subpositions.</summary>
    FallBackIntoLava,
}
