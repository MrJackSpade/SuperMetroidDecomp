namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>What a cue of the Ceres steam vents' animations does: show the vent and let it collide with Samus, or hide it and stop it colliding.</summary>
internal enum CeresSteamCueKind
{
    /// <summary>Shows the vent and lets it collide with Samus.</summary>
    Appear,
    /// <summary>Hides the vent and stops it colliding with Samus.</summary>
    Vanish,
}
