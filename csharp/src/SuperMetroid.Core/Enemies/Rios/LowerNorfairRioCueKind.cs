namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>What a cue of the Lower Norfair Rio's programs does: one per bank-$A2 instruction its lists use.</summary>
internal enum LowerNorfairRioCueKind
{
    /// <summary>Tells main AI the animation has finished (<c>SetAnimationFinishedFlag</c>, $A2:C6D2).</summary>
    AnimationFinished,
    /// <summary>Hides the flames (<c>HideFlames</c>, $A2:C6DD).</summary>
    HideFlames,
    /// <summary>Shows the flames (<c>ShowFlames</c>, $A2:C6E8).</summary>
    ShowFlames,
}
