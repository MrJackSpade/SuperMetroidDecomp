namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>One positioned part of the escape timer: the label, or the tens digit of one time pair.</summary>
public enum EscapeTimerAnchor
{
    /// <summary>The TIME label's origin.</summary>
    Label,
    /// <summary>The minutes pair's tens digit.</summary>
    Minutes,
    /// <summary>The seconds pair's tens digit.</summary>
    Seconds,
    /// <summary>The centiseconds pair's tens digit.</summary>
    Centiseconds,
}
