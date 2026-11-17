namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The two pictures the Tourian access floor draws besides its rows' crumbling (#2291,
/// <see cref="TourianAccessDraws.Crumble"/>): a crumbled row cleared, and the whole floor cleared at once.
/// </summary>
internal enum TourianAccessDraw
{
    /// <summary>Clear one row after its crumble frames.</summary>
    EmptyRow,
    /// <summary>Clear all six rows at once.</summary>
    Clear,
}
