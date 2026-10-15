namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>What a cue of the Ceres steam vents' programs does: one per bank-$A6 instruction its lists use.</summary>
internal enum CeresSteamCueKind
{
    /// <summary>Shows the vent and lets it collide with Samus (<c>SetToTangibleAndVisible</c>, $A6:F135).</summary>
    Appear,
    /// <summary>Counts the vent's activation timer down, continuing at the target while it runs and at the alternate, visible and tangible, once it expires (<c>DecrementActivationTimerGotoYOrY2</c>, $A6:F127).</summary>
    CountDownToActivation,
    /// <summary>Hides the vent and stops it colliding with Samus (<c>SetToIntangibleAndInvisible</c>, $A6:F11D).</summary>
    Vanish,
}
