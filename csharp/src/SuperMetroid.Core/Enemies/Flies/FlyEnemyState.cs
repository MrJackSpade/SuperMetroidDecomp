using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Flies;

/// <summary>
/// The state of one Mellow, Mella or Memu. Every value here is one the cartridge keeps in the enemy's common record,
/// so it is emptied with that record when the enemy is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class FlyEnemyState
{
    /// <summary>Where the fly's typed flight loop (#641) resumes.</summary>
    internal AnimationProgramCursor<FlyProgram> Program { get; set; }

    /// <summary>Counts up per attack update, down through signed underflow during retreat, and then down from 24 to gate another idle attack.</summary>
    public NativeCountdown RetreatTimer { get; internal set; }

    /// <summary>
    /// Signed 8.8 attack/retreat velocity in room pixels per actor update: X sampled from Samus's attack-start angle
    /// and retained during retreat; Y, whose sign reverses when the saved target height is crossed.
    /// </summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>Samus's whole room-pixel height captured when the attack begins; only Y is meaningful. It determines the turnaround rather than tracking Samus's later movement.</summary>
    public Position16<RoomPixels> TargetHeight { get; internal set; }

    /// <summary>The circular flight's phase, advanced sixteen table units per circling update and wrapping after a full turn.</summary>
    public SnesAngle Angle { get; internal set; }

    /// <summary>The current circling, attacking or retreating behaviour; none once a kill has emptied the record.</summary>
    public FlyEnemyFunction? Function { get; internal set; }

    /// <summary>One circling update's phase advance: sixteen table units one way or the other, wrapping after a full turn.</summary>
    /// <param name="clockwise">Whether the fly circles clockwise.</param>
    internal void AdvanceCircle(bool clockwise) => Angle = Angle.AddTableUnits(clockwise ? 16 : -16);

    /// <summary>Empties every value, when a kill empties the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        RetreatTimer = NativeCountdown.Start(0);
        Velocity = new EightEightVector<RoomPixels>(0, 0);
        TargetHeight = Position16<RoomPixels>.Zero;
        Angle = SnesAngle.Zero;
        Function = null;
    }
}
