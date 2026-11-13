namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>
/// Sbug's eight directional animation loops as typed programs (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="SbugAnimations"/>.
/// </summary>
internal enum SbugSegment
{
    /// <summary>The right animation.</summary>
    Right,
    /// <summary>The up right animation.</summary>
    UpRight,
    /// <summary>The up animation.</summary>
    Up,
    /// <summary>The up left animation.</summary>
    UpLeft,
    /// <summary>The left animation.</summary>
    Left,
    /// <summary>The down left animation.</summary>
    DownLeft,
    /// <summary>The down animation.</summary>
    Down,
    /// <summary>The down right animation.</summary>
    DownRight,
}
