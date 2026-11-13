namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>What a cue of the Ceres steam vents' animations does: one per kind of step its lists use.</summary>
internal enum CeresSteamCueKind
{
    /// <summary>Shows the vent and lets it collide with Samus.</summary>
    Appear,
    /// <summary>Hides the vent and stops it colliding with Samus.</summary>
    Vanish,
}
