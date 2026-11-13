namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres steam vents' typed animations (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="CeresSteamAnimations"/>.
/// </summary>
internal enum CeresSteamSegment
{
    /// <summary>The up animation.</summary>
    Up,
    /// <summary>The loop.</summary>
    UpLoop,
    /// <summary>The loop.</summary>
    UpLoop2,
    /// <summary>The left animation.</summary>
    Left,
    /// <summary>The loop.</summary>
    LeftLoop,
    /// <summary>The loop.</summary>
    LeftLoop2,
    /// <summary>The down animation.</summary>
    Down,
    /// <summary>The loop.</summary>
    DownLoop,
    /// <summary>The loop.</summary>
    DownLoop2,
    /// <summary>The right animation.</summary>
    Right,
    /// <summary>The loop.</summary>
    RightLoop,
    /// <summary>The loop.</summary>
    RightLoop2,
}
