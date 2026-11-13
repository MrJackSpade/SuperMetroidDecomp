using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>The state of one Fune or Namihe, the two species that share this actor.</summary>
public sealed class FuneNamiheEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<FuneNamiheSegment> Playback { get; set; }

    /// <summary>Whether the spitting animation is selected rather than the idle one; waking selects it and the spit's finishing cue deselects it.</summary>
    public bool ShowsActiveAnimation { get; internal set; }

    /// <summary>Whether the right-facing animations are selected, from its spawn facing.</summary>
    public bool FacingRight { get; internal set; }

    /// <summary>The waiting or spitting behaviour.</summary>
    public FuneNamiheAction? Action { get; internal set; }

    /// <summary>Strict vertical wake distance in pixels used only by Namihe, from the spawn settings.</summary>
    public int YProximity { get; internal set; }

    /// <summary>Whether it behaves as a Fune or a Namihe, from the spawn settings.</summary>
    public FuneNamiheVariant Variant { get; internal set; }

    /// <summary>
    /// Fune's incrementing cooldown count of waiting updates, 0 through 65535, compared with <see cref="CooldownTime"/>
    /// as a signed difference and reset to zero when the wait ends; the waiting step's increment wraps it past
    /// 65535.
    /// </summary>
    public int CooldownTimer { get; internal set; }

    /// <summary>The number of updates Fune waits, from the spawn settings.</summary>
    public int CooldownTime { get; internal set; }

    /// <summary>Whether it behaves as a Namihe.</summary>
    public bool IsNamihe => Variant == FuneNamiheVariant.Namihe;
}
