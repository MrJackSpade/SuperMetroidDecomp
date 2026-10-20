using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// The state of one of Shaktool's seven linked segments. Every value here is one the cartridge keeps in the
/// segment's common enemy record, so it is emptied with that record when the segment is killed
/// (<see cref="ClearWithRecord"/>); the rest of the chain still reads the emptied values for the remainder of that
/// update.
/// </summary>
public sealed class ShaktoolSegmentState
{
    /// <summary>Where the typed animation (#641) resumes.</summary>
    internal AnimationProgramCursor<ShaktoolProgram> Program { get; set; }
    /// <summary>The chain's target heading, also advanced during straightening and used by attack movement cues.</summary>
    public SnesAngle TargetAngle { get; internal set; }
    /// <summary>The angle around the immediately preceding segment; its table index selects the orbit displacement.</summary>
    public SnesAngle OrbitAngle { get; internal set; }
    /// <summary>
    /// The unsigned orbital increment per gameplay update in raw angle units, from 0 through 1020: an authored
    /// curling rate, four times the whole table steps left to a target, or the straightening step. The motion flags
    /// determine its sign.
    /// </summary>
    public int AngularVelocity { get; internal set; }
    /// <summary>
    /// The centre segment's heading bucket: one of eight eighth-turns in table units (0, $20, ... $E0), selected
    /// from the midpoint of its neighbours' angles and choosing its head animation.
    /// </summary>
    public int OrientationBucket { get; internal set; }
    /// <summary>Whether the chain runs reversed, so the centre segment's midpoint flips a half turn; an endpoint swap toggles it.</summary>
    public bool OrientationReversed { get; internal set; }
    /// <summary>
    /// The final saw's curl progress: the angular increments accumulated since the chain last reached its target,
    /// from 0 through $F3FB. Reaching $F000 starts the chain straightening, which stops the accumulation.
    /// </summary>
    public int CurlProgress { get; internal set; }
    /// <summary>The slot of the first segment of the seven-segment chain, whose segments occupy consecutive slots.</summary>
    public int OwnerSlotIndex { get; internal set; }
    /// <summary>The segment motion run by Shaktool's main AI; collision-response animations temporarily replace it with the idle attack motion, and none once a kill has emptied the record.</summary>
    public ShaktoolPreInstruction? PreInstruction { get; internal set; }
    /// <summary>The chain's reversal, straightening and orbit-direction flags, written to every segment together.</summary>
    internal ShaktoolMotionFlags MotionFlags { get; set; }

    /// <summary>Empties the orientation, curl progress and reversal together, as the cartridge does by zeroing their shared record word.</summary>
    internal void ResetOrientation()
    {
        OrientationBucket = 0;
        OrientationReversed = false;
        CurlProgress = 0;
    }

    /// <summary>Empties every value, when a kill empties the segment's common record.</summary>
    internal void ClearWithRecord()
    {
        TargetAngle = SnesAngle.Zero;
        OrbitAngle = SnesAngle.Zero;
        AngularVelocity = 0;
        ResetOrientation();
        OwnerSlotIndex = 0;
        PreInstruction = null;
        MotionFlags = 0;
    }
}
