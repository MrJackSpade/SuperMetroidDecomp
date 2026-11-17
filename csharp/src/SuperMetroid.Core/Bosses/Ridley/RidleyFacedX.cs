namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// A horizontal place or offset Ridley takes by the way he faces (#2335): one for each facing, the turn toward the screen
/// included.
/// </summary>
/// <param name="FacingLeft">The value while he faces left.</param>
/// <param name="Turning">The value while he turns toward the screen.</param>
/// <param name="FacingRight">The value while he faces right.</param>
internal readonly record struct RidleyFacedX(int FacingLeft, int Turning, int FacingRight)
{
    /// <summary>The value for <paramref name="facing"/>.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="facing"/> is no facing.</exception>
    internal int For(RidleyFacing facing) => facing switch
    {
        RidleyFacing.Left => FacingLeft,
        RidleyFacing.Turning => Turning,
        RidleyFacing.Right => FacingRight,
        _ => throw new InvalidOperationException($"Undefined {nameof(RidleyFacing)} {facing}."),
    };
}
