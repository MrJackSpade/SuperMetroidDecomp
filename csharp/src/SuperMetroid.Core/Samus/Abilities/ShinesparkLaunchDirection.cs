namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// A shinespark's launch direction (#2553): horizontal, vertical or diagonal, with its launch for each facing. A windup's
/// direction input and its countdown's vertical launch choose one, and the windup's facing picks its launch.
/// </summary>
public sealed class ShinesparkLaunchDirection
{
    /// <summary>Only the three directions declared here exist.</summary>
    /// <param name="facingRight">The direction's launch facing right.</param>
    /// <param name="facingLeft">The direction's launch facing left.</param>
    private ShinesparkLaunchDirection(ShinesparkLaunch facingRight, ShinesparkLaunch facingLeft)
    {
        FacingRight = facingRight;
        FacingLeft = facingLeft;
    }

    /// <summary>The direction's launch facing right.</summary>
    public ShinesparkLaunch FacingRight { get; }

    /// <summary>The direction's launch facing left.</summary>
    public ShinesparkLaunch FacingLeft { get; }

    /// <summary>The horizontal spark.</summary>
    public static ShinesparkLaunchDirection Horizontal { get; } = new(ShinesparkLaunch.HorizontalRight, ShinesparkLaunch.HorizontalLeft);

    /// <summary>The vertical spark.</summary>
    public static ShinesparkLaunchDirection Vertical { get; } = new(ShinesparkLaunch.VerticalRight, ShinesparkLaunch.VerticalLeft);

    /// <summary>The diagonal spark.</summary>
    public static ShinesparkLaunchDirection Diagonal { get; } = new(ShinesparkLaunch.DiagonalRight, ShinesparkLaunch.DiagonalLeft);

    /// <summary>The direction's launch for <paramref name="facing"/>: the left-facing launch facing left, otherwise the right-facing one, as the game picks it.</summary>
    /// <param name="facing">The way the windup faces.</param>
    public ShinesparkLaunch For(SamusFacingDirection facing) => facing == SamusFacingDirection.Left ? FacingLeft : FacingRight;
}
