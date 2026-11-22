namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// A body animation Ridley plays the way he faces (#2175): his owner starts the one for his facing, so no animation
/// redirects itself to its counterpart. A Ridley turning plays the right-facing one, as the game's animations did.
/// </summary>
/// <param name="FacingLeft">The segment the animation starts at while he faces left.</param>
/// <param name="FacingRight">The segment the animation starts at while he faces right or turns.</param>
internal readonly partial record struct RidleyFacedAnimation(RidleyBodySegment FacingLeft, RidleyBodySegment FacingRight)
{
    /// <summary>The segment the animation starts at for <paramref name="facing"/>.</summary>
    internal RidleyBodySegment For(RidleyFacing facing) => facing == RidleyFacing.Left ? FacingLeft : FacingRight;
}
