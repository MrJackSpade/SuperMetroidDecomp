using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>
/// The state of a Cacatac: its patrol velocities, direction and function, plus its two patrol bounds. The values the
/// cartridge keeps in the enemy's common record are emptied with it when the enemy is killed (<see
/// cref="ClearWithRecord"/>).
/// </summary>
public sealed class CacatacEnemyState
{
    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<CacatacProgram> Program { get; set; }

    /// <summary>
    /// Signed 16.16 room-pixel displacement added to X on every rightward patrol update. A kill empties its whole
    /// pixels with the common record; its fraction lives outside the record and outlives the kill.
    /// </summary>
    public SnesSignedSixteenSixteen RightVelocity { get; internal set; }

    /// <summary>
    /// Signed 16.16 room-pixel displacement added to X on every leftward patrol update (negative for the authored
    /// speeds).
    /// </summary>
    public SnesSignedSixteenSixteen LeftVelocity { get; internal set; }

    /// <summary>Native; zero is left, one is right, two initially stops.</summary>
    public CacatacDirection Direction { get; internal set; }

    /// <summary>Native indirect function pointer at; none once a kill has emptied the record.</summary>
    public CacatacEnemyFunction? Function { get; internal set; }

    /// <summary>
    /// Left patrol limit; only X is meaningful. Leftward patrol turns right once the whole X position lies strictly
    /// left of it, measured as the wrapped signed difference, so a limit that wrapped past the room's left edge still
    /// turns the enemy as the cartridge does.
    /// </summary>
    public Position16<RoomPixels> LeftPatrolLimit { get; internal set; }

    /// <summary>
    /// Right patrol limit; only X is meaningful. Rightward patrol turns left once the whole X position reaches or
    /// passes it, measured as the wrapped signed difference.
    /// </summary>
    public Position16<RoomPixels> RightPatrolLimit { get; internal set; }

    /// <summary>
    /// Parameter-one's high-byte presentation selector. The assembly tests only zero versus nonzero, so this is a
    /// Boolean discriminator rather than a guessed flags enum.
    /// </summary>
    public bool UpsideUp { get; internal set; }

    /// <summary>Empties the values the cartridge keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        RightVelocity = RightVelocity.WithWholePixels(0);
        LeftVelocity = SnesSignedSixteenSixteen.Zero;
        Direction = default;
        Function = null;
    }
}
