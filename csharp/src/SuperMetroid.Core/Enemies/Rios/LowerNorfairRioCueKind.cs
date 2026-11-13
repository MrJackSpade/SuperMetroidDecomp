namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>What a cue of the Lower Norfair Rio's animations does: tell main AI the animation has finished, or show or hide the flames.</summary>
internal enum LowerNorfairRioCueKind
{
    /// <summary>Tells main AI the animation has finished.</summary>
    AnimationFinished,
    /// <summary>Hides the flames.</summary>
    HideFlames,
    /// <summary>Shows the flames.</summary>
    ShowFlames,
}
