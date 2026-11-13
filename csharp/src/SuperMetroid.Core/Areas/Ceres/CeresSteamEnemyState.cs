using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The state of one Ceres steam vent.</summary>
internal sealed class CeresSteamEnemyState
{
    /// <summary>The main-AI action the vent's variant selects.</summary>
    public CeresSteamAction? Action { get; internal set; }

    /// <summary>The staggered updates before the vent first appears, counted down by its animation.</summary>
    public WrappingCountdown ActivationDelay { get; internal set; }

    /// <summary>The vent's animation and where it stands in it (#1600), set when the vent initializes.</summary>
    internal AnimationPlayback<CeresSteamSegment> Playback { get; set; }
}
