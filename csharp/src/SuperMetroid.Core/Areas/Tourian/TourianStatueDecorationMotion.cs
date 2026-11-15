namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// How one of the Tourian entrance statue's decorations moves itself (#1843).
/// </summary>
internal enum TourianStatueDecorationMotion
{
    /// <summary>Keeps a Tourian entrance statue actor on the statue's line-by-line reveal.</summary>
    FollowingReveal,

    /// <summary>Keeps a finished statue decoration at its place.</summary>
    Settled,
}
