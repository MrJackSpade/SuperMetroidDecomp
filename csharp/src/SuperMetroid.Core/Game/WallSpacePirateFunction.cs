namespace SuperMetroid.Core.Game;

/// <summary>Literal bank-$B2 function pointers stored in wall Pirate variable A.</summary>
public enum WallSpacePirateFunction : ushort
{
    /// <summary>Watch Samus while continuing the left-wall climbing animation.</summary>
    ClimbingLeftWall = 0xf034,

    /// <summary>The attack list owns presentation until it prepares a rightward jump.</summary>
    AttackAnimationOnLeftWall = 0xf04f,

    /// <summary>Follow the cartridge's sine/cosine arc from the left wall to the right.</summary>
    WallJumpingRight = 0xf050,

    /// <summary>Watch Samus while continuing the right-wall climbing animation.</summary>
    ClimbingRightWall = 0xf0c8,

    /// <summary>The attack list owns presentation until it prepares a leftward jump.</summary>
    AttackAnimationOnRightWall = 0xf0e3,

    /// <summary>Follow the cartridge's sine/cosine arc from the right wall to the left.</summary>
    WallJumpingLeft = 0xf0e4,
}
