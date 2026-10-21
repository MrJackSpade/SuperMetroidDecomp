namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>The five Tourian access-floor draw lists.</summary>
internal enum TourianAccessDraw
{
    /// <summary>Clear one row after its crumble frames.</summary>
    EmptyRow,
    /// <summary>First visible crumble row.</summary>
    CrumbleFirst,
    /// <summary>Second visible crumble row.</summary>
    CrumbleSecond,
    /// <summary>Third visible crumble row.</summary>
    CrumbleThird,
    /// <summary>Clear all six rows at once.</summary>
    Clear,
}
