using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The state of one Ceres door.</summary>
internal sealed class CeresDoorEnemyState
{
    /// <summary>The main-AI action.</summary>
    public CeresDoorAction? Action { get; internal set; }

    /// <summary>The door's animation and where it stands in it (#1600), set when the door initializes.</summary>
    internal AnimationPlayback<CeresDoorSegment> Playback { get; set; }

    /// <summary>
    /// Whether the door's animation has handed its drawing to Ridley's scene and not yet taken it back.
    /// </summary>
    public bool HandsDrawingToRidley { get; internal set; }

    /// <summary>
    /// Calls left in the elevator door's destruction rumble, 48 when it starts and decremented each call; the call that
    /// takes it below zero ends the destruction, which never counts it again.
    /// </summary>
    public int RumbleCallsLeft { get; internal set; }

    /// <summary>Calls left before the next destruction explosion, decremented each call and reloaded once it runs below zero.</summary>
    public int ExplosionIntervalLeft { get; internal set; }

    /// <summary>
    /// The place in <see cref="CeresDoorRumbleGeometryDefinitions.ExplosionOffsets"/> of the last destruction burst; each
    /// burst takes the place before it, and the last place after the first.
    /// </summary>
    public int LastExplosionPlace { get; internal set; }
}
