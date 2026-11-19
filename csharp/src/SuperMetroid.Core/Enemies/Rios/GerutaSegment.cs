namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The Geruta's idle and swoop animations and its flames' animations (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="GerutaAnimations"/>.
/// </summary>
internal enum GerutaSegment
{
    /// <summary>Hovering, its wings spread, level and drooping, with its flames behind it, repeating.</summary>
    Idle,
    /// <summary>
    /// Its wings flattening, lifting and sweeping up as it starts to swoop down, its flames moving with them.
    /// </summary>
    StartDescending,
    /// <summary>Diving down, its flames placed to trail it, held in its one pose (#2512).</summary>
    Descending,
    /// <summary>
    /// Its wings sweeping down from swept up to drooping as it starts to rise again, its flames moving with them.
    /// </summary>
    StartAscending,
    /// <summary>Climbing, its flames placed to trail it, held in its one pose (#2512).</summary>
    Ascending,
    /// <summary>Its flames flickering large, medium and small as it rises, repeating.</summary>
    FlamesAscending,
    /// <summary>Its flames flickering large, medium and small as it falls, repeating.</summary>
    FlamesDescending,
}
