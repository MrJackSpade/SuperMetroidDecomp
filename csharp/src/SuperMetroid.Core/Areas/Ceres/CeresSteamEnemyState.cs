using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The state of one Ceres steam vent.</summary>
internal sealed class CeresSteamEnemyState
{
    /// <summary>The main-AI function the vent's variant selects.</summary>
    public CeresSteamFunction? Function { get; internal set; }

    /// <summary>The staggered updates before the vent first appears, counted down by its program.</summary>
    public WrappingCountdown ActivationDelay { get; internal set; }

    /// <summary>The vent's animation and where it stands in it (#1600), set when the vent initializes.</summary>
    internal AnimationPlayback<CeresSteamSegment> Playback { get; set; }
}
