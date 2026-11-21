namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Where Kraid's arm's swing stands (#1886, #2301, #2560), one value where the arm kept two flags.</summary>
internal enum KraidArmSwing
{
    /// <summary>Away from the normal swing: rising or sinking, already slow, dying or preparing a lunge.</summary>
    Away,
    /// <summary>In the normal swing, before it shows its last frame.</summary>
    BeforePause,
    /// <summary>In the normal swing at or past its last frame, or through its pause.</summary>
    PastPause,
}
