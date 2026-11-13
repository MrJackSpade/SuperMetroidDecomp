namespace SuperMetroid.Core.Animation;

/// <summary>When a phase counts its plays down the owner's repeat counter (#1600).</summary>
internal enum AnimationPhaseCounting
{
    /// <summary>Only when it plays more than once.</summary>
    RepeatsOnly,

    /// <summary>On every play, even when it plays once: it loads one and counts it to zero.</summary>
    EveryPlay,
}
