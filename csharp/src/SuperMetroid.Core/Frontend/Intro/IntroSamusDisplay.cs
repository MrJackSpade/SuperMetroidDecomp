namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// IntroSamusDisplayFlag. The cinematic dispatcher runs Samus's state handlers only while it is
/// nonzero, and the cinematic draw tests its sign to order Samus against the cinematic objects.
/// </summary>
internal enum IntroSamusDisplay
{
    /// <summary>Zero: Samus is neither processed nor drawn.</summary>
    Hidden = 0,

    /// <summary>Positive (1): cinematic objects draw before Samus.</summary>
    ObjectsFirst = 1,

    /// <summary>Negative (65535): Samus and her projectiles draw first.</summary>
    SamusFirst = -1,
}
