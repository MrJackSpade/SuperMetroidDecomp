namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The Norfair Rio's idle and swoop animations and its flames' animations as typed programs (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="NorfairRioAnimations"/>.
/// </summary>
internal enum NorfairRioSegment
{
    /// <summary>The idle animation.</summary>
    Idle,
    /// <summary>The start descending animation.</summary>
    StartDescending,
    /// <summary>The descending animation.</summary>
    Descending,
    /// <summary>The start ascending animation.</summary>
    StartAscending,
    /// <summary>The ascending animation.</summary>
    Ascending,
    /// <summary>The flames ascending animation.</summary>
    FlamesAscending,
    /// <summary>The flames descending animation.</summary>
    FlamesDescending,
}
