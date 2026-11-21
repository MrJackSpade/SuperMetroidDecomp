namespace SuperMetroid.Core.Fx;

/// <summary>Where a room colour effect stands (#2560): running, ended, or settled on its final frame.</summary>
internal enum RoomColourEffectProgress
{
    /// <summary>The effect takes its turns.</summary>
    Running,

    /// <summary>The effect's condition or its system ended it; it shows nothing.</summary>
    Ended,

    /// <summary>The effect played its animation to the end and keeps showing its final frame as the look it finishes on.</summary>
    Settled,
}
