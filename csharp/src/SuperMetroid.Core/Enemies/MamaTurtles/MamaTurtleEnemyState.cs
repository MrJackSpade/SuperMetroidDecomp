using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>
/// The state of the Mama Turtle. The function, the horizontal velocity's whole pixels and the wake counter are values
/// the cartridge keeps in the enemy's common record, so they are emptied with that record when the enemy is killed
/// (<see cref="ClearWithRecord"/>); the rest outlive the kill.
/// </summary>
public sealed class MamaTurtleEnemyState
{
    /// <summary>Where the typed animation (#641) resumes.</summary>
    internal AnimationProgramCursor<MamaTurtleProgram> Program { get; set; }

    /// <summary>The current Mama Turtle behaviour; none once a kill has emptied the record.</summary>
    public MamaTurtleAiFunction? Function { get; internal set; }

    /// <summary>Signed 16.16 velocity per AI update.</summary>
    public FixedVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>Wake counter; each Baby touch or shot decrements it, and the Mama wakes once it reaches zero.</summary>
    public NativeCountdown AsleepFlag { get; internal set; }

    /// <summary>Gets the phase-specific countdown used by peak and transition states.</summary>
    public NativeCountdown FunctionTimer { get; internal set; }

    /// <summary>
    /// Signed 16.16 horizontal acceleration per hovering update, toward Samus's side when hovering starts; a wall
    /// reversal negates it with the cartridge's carry quirk.
    /// </summary>
    public SnesSignedSixteenSixteen XAcceleration { get; internal set; }

    /// <summary>
    /// Empties the values the cartridge keeps in the enemy's common record, when a kill empties that record: no
    /// function, a zero wake counter, and no whole pixels of horizontal velocity, its fraction and the vertical
    /// velocity kept.
    /// </summary>
    internal void ClearWithRecord()
    {
        Function = null;
        Velocity = Velocity.WithX(Velocity.X.WithWholePixels(0));
        AsleepFlag = NativeCountdown.Start(0);
    }
}
