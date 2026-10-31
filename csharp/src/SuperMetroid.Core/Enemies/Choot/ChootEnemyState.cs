using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>
/// The state of one Choot. The function, ascent index, falling cursor, loop counter, pattern and its loop advance
/// are values the cartridge keeps in the enemy's common record, so they are emptied with that record when the enemy
/// is killed (<see cref="ClearWithRecord"/>). The rest are the Choot's own too: setup writes the spawn and falling
/// positions and the starting ascent index, activation writes the jump delay and the apex writes the falling origin,
/// each before it is read, so nothing a previous Choot in the slot left reaches a new one.
/// </summary>
public sealed class ChootEnemyState
{
    /// <summary>Where the typed animation (#641) resumes.</summary>
    internal AnimationProgramCursor<ChootProgram> Program { get; set; }

    /// <summary>The behaviour selected by Choot's main AI dispatcher; none once a kill has emptied the record.</summary>
    public ChootEnemyFunction? Function { get; internal set; }

    /// <summary>
    /// The ascent's quadratic-speed record index, falling by two per jumping update; the update that takes it below
    /// zero is the apex. It starts at the even index whose integrated speeds cover the jump height, at most 94.
    /// </summary>
    public int YSpeedTableIndex { get; internal set; }

    /// <summary>The falling pattern's current frame, advanced by one per falling update and restarted at each terminator.</summary>
    public int FallingPatternFrame { get; internal set; }

    /// <summary>Loop counter initialized to parameter one's low byte minus one, expiring when it decrements below zero.</summary>
    public WrappingCountdown FallingPatternLoopCounter { get; internal set; }

    /// <summary>The authored falling path the Choot follows after its apex; none once a kill has emptied the record.</summary>
    internal ChootFallingPath? FallingPath { get; set; }

    /// <summary>Whole-pixel Y origin advance applied between falling-pattern loops, the pattern's authored 28 to 32 pixels.</summary>
    public int FallingPatternYDistance { get; internal set; }

    /// <summary>The initial room position in whole pixels, restored after the last falling-pattern loop.</summary>
    public Position16<RoomPixels> SpawnPosition { get; internal set; }

    /// <summary>Spawn X and the wrapped whole-pixel spawn Y minus the computed jump height, installed as the position and falling origin at the apex transition.</summary>
    public Position16<RoomPixels> InitialFallingPosition { get; internal set; }

    /// <summary>The whole-pixel base added to each signed falling-pattern offset; initialized at the apex, and its Y advanced by <see cref="FallingPatternYDistance"/> at every stream terminator.</summary>
    public Position16<RoomPixels> FallingOrigin { get; internal set; }

    /// <summary>The ascent record index calculated from the population's loop count and pattern height; copied back to <see cref="YSpeedTableIndex"/> when the jump cycle finishes.</summary>
    public int InitialYSpeedTableIndex { get; internal set; }

    /// <summary>
    /// Parameter-two countdown. The jump begins only after decrementing through zero to
    /// below zero, so a population delay of zero launches on the first preparation frame.
    /// </summary>
    public WrappingCountdown JumpDelayTimer { get; internal set; }

    /// <summary>Empties the values the cartridge keeps in the enemy's common record, when a kill empties that record.</summary>
    internal void ClearWithRecord()
    {
        Function = null;
        YSpeedTableIndex = 0;
        FallingPatternFrame = 0;
        FallingPatternLoopCounter = WrappingCountdown.Start(0);
        FallingPath = null;
        FallingPatternYDistance = 0;
    }
}
