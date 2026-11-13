using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>The state of one Alcoon.</summary>
public sealed class AlcoonEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<AlcoonSegment> Playback { get; set; }

    /// <summary>The current behaviour.</summary>
    public AlcoonEnemyFunction? Function { get; internal set; }

    /// <summary>
    /// Motion per step. Y is the signed 16.16 vertical velocity: emergence starts at -12 and retreat at -4, and
    /// acceleration carries from the fraction into the whole pixels. X is the signed whole-pixel horizontal velocity
    /// (retail uses -2, 0 or +2); Alcoon has no horizontal fraction, so a velocity with a nonzero X fraction is rejected.
    /// </summary>
    public FixedVector<RoomPixels> Velocity
    {
        get;
        internal set
        {
            if (value.X.Fraction != 0)
                throw new InvalidOperationException(
                    $"Alcoon has no horizontal subvelocity; X fraction {value.X.Fraction} cannot be stored.");
            field = value;
        }
    }

    /// <summary>
    /// Original population whole-pixel position. Y is restored after the initialization floor search and after
    /// retreat, leaving the live Y subposition unchanged; X enforces the 112-pixel walking range and is restored when
    /// hiding completes.
    /// </summary>
    public Position16<RoomPixels> SpawnPosition { get; internal set; }

    /// <summary>Vertical velocity increment per acceleration step; jump setup makes it half a pixel downward.</summary>
    public SignedSixteenSixteen YAcceleration { get; internal set; }

    /// <summary>The floor-aligned position the complete initialization jump lands at: the spawn X and the landing Y.</summary>
    public Position16<RoomPixels> LandingPosition { get; internal set; }

    /// <summary>Number of four-step walking cycles remaining before another fire volley.</summary>
    public int StepCounter { get; internal set; }
}
