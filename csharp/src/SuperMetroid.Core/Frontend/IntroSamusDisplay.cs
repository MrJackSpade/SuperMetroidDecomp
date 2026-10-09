namespace SuperMetroid.Core.Frontend;

/// <summary>
/// $1A57 IntroSamusDisplayFlag. $8B:8E0D runs Samus's state handlers only while it is
/// nonzero, and $8B:8E2D tests its sign to order Samus against the cinematic objects.
/// </summary>
internal enum IntroSamusDisplay : short
{
    /// <summary>Zero: Samus is neither processed nor drawn.</summary>
    Hidden = 0,

    /// <summary>Positive ($0001): cinematic objects enter OAM before Samus.</summary>
    ObjectsFirst = 1,

    /// <summary>Negative ($FFFF): Samus and her projectiles enter OAM first.</summary>
    SamusFirst = -1,
}
