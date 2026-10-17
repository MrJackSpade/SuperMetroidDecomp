using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>
/// The state of one Alcoon. The function, the velocity and the spawn Y are values the cartridge keeps in the enemy's
/// common record, so they are emptied with that record when the enemy is killed (<see cref="ClearWithRecord"/>); the
/// rest outlive the kill.
/// </summary>
public sealed class AlcoonEnemyState
{
    /// <summary>The motion per step; see <see cref="Velocity"/>.</summary>
    private FixedVector<RoomPixels> _velocity;

    /// <summary>Where the typed animation (#641) resumes.</summary>
    internal AnimationProgramCursor<AlcoonProgram> Program { get; set; }

    /// <summary>The current behaviour; none once a kill has emptied the record.</summary>
    public AlcoonEnemyFunction? Function { get; internal set; }

    /// <summary>
    /// Motion per step. Y is the signed 16.16 vertical velocity: emergence starts at -12 and retreat at -4, and
    /// acceleration carries from the fraction into the whole pixels. X is the signed whole-pixel horizontal velocity
    /// (retail uses -2, 0 or +2); Alcoon has no horizontal fraction, so a velocity with a nonzero X fraction is rejected.
    /// </summary>
    public FixedVector<RoomPixels> Velocity
    {
        get => _velocity;
        internal set
        {
            if (value.X.Fraction != 0)
                throw new InvalidOperationException(
                    $"Alcoon has no horizontal subvelocity; X fraction ${value.X.Fraction:X4} cannot be stored.");
            _velocity = value;
        }
    }

    /// <summary>
    /// Original population whole-pixel position. Y is restored after the initialization floor search and after
    /// retreat, leaving the live Y subposition unchanged; X enforces the 112-pixel walking range and is restored when
    /// hiding completes.
    /// </summary>
    public Position16<RoomPixels> SpawnPosition { get; internal set; }

    /// <summary>Vertical velocity increment per acceleration step; jump setup makes it half a pixel downward.</summary>
    public SnesSignedSixteenSixteen YAcceleration { get; internal set; }

    /// <summary>The floor-aligned position the complete initialization jump lands at: the spawn X and the landing Y.</summary>
    public Position16<RoomPixels> LandingPosition { get; internal set; }

    /// <summary>Number of four-step walking cycles remaining before another fire volley.</summary>
    public NativeCountdown StepCounter { get; internal set; }

    /// <summary>
    /// Empties the values the cartridge keeps in the enemy's common record, when a kill empties that record: no
    /// function, no velocity, and a zero spawn Y with the spawn X kept.
    /// </summary>
    internal void ClearWithRecord()
    {
        Function = null;
        _velocity = FixedVector<RoomPixels>.Zero;
        SpawnPosition = SpawnPosition.WithY(0);
    }
}
