namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Whether and where the intro shows Samus. The cinematic dispatcher runs Samus's state handlers only while it is
/// nonzero, and the cinematic draw orders Samus against the cinematic objects by whether it is negative.
/// </summary>
internal enum IntroSamusDisplay
{
    /// <summary>Zero: Samus is neither processed nor drawn.</summary>
    Hidden = 0,

    /// <summary>Positive (1): cinematic objects draw before Samus.</summary>
    ObjectsFirst = 1,

    /// <summary>Negative: Samus and her projectiles draw first.</summary>
    SamusFirst = -1,
}
