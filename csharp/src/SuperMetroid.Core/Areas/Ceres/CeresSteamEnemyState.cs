using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The state of one Ceres steam vent.</summary>
internal sealed class CeresSteamEnemyState
{
    /// <summary>Starts the state with the vent's <paramref name="motion"/>.</summary>
    internal CeresSteamEnemyState(CeresSteamMotion motion) => Motion = Ensure.NotNull(motion);

    /// <summary>Where the vent is drawn: where it stands, or turned with the elevator shaft it stands in.</summary>
    public CeresSteamMotion Motion { get; }

    /// <summary>
    /// The updates the vent stays hidden after its dormant animation hides it, before its first plume, the one it appears
    /// on included: 1 through 32, drawn as it initializes. Its owner holds its animation that long (#2480).
    /// </summary>
    public int UpdatesToActivation { get; internal set; }

    /// <summary>The vent's animation and where it stands in it (#1600), set when the vent initializes.</summary>
    internal AnimationPlayback<CeresSteamSegment> Playback { get; set; }
}
