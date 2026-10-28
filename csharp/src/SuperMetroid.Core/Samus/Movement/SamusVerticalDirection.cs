namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Samus's vertical direction at WRAM <c>$0B36</c> (<c>SamusYDirection</c>). The native game
/// stores vertical speed as a magnitude and keeps its sign here. Banks <c>$90</c>, <c>$91</c>,
/// <c>$9B</c> and <c>$A9</c> store only these three values (<c>STZ</c>, <c>LDA #$0001</c>,
/// <c>LDA #$0002</c>).
/// </summary>
public enum SamusVerticalDirection
{
    /// <summary>Zero: no vertical movement (grounded, or a pose change that cleared the motion).</summary>
    None = 0,

    /// <summary>One: moving up (jumping, bouncing, rising).</summary>
    Up = 1,

    /// <summary>Two: moving down (falling).</summary>
    Down = 2,
}
