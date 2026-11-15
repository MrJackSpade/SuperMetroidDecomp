namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// One of the eight pieces Bomb Torizo's statue hand breaks into (#2029), named for its place on the hand's three-by-three
/// block of sixteen-pixel cells, whose top-left corner falls with no piece. The crumble drops them in the order listed
/// (<see cref="BombTorizoHandCrumbleDefinitions.Crumble"/>).
/// </summary>
public enum BombTorizoStatueFragment
{
    /// <summary>The top row's middle piece, the first to fall.</summary>
    TopMiddle,
    /// <summary>The top row's right piece.</summary>
    TopRight,
    /// <summary>The middle row's left piece.</summary>
    MiddleLeft,
    /// <summary>The centre piece.</summary>
    Centre,
    /// <summary>The middle row's right piece.</summary>
    MiddleRight,
    /// <summary>The bottom row's left piece.</summary>
    BottomLeft,
    /// <summary>The bottom row's middle piece.</summary>
    BottomMiddle,
    /// <summary>The bottom row's right piece, the last to fall.</summary>
    BottomRight,
}
