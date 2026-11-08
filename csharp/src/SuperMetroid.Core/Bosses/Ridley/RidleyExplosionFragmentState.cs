using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The state of one fragment of Norfair Ridley's breakup. Its velocity and lifetime are values the game keeps in
/// the enemy's common record, so they are emptied with the record when the fragment is killed
/// (<see cref="ClearWithRecord"/>).
/// </summary>
internal sealed class RidleyExplosionFragmentState
{
    /// <summary>Signed 8.8 velocity per update: X, which drag slows toward zero, and Y, which gravity increases.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>The updates the fragment lives, counted down past zero.</summary>
    public int Lifetime { get; internal set; }

    /// <summary>Where the fragment's typed breakup program resumes (#641), set when it spawns; a kill leaves it in place.</summary>
    internal AnimationProgramCursor<RidleyBreakupFrame> Program { get; set; }

    /// <summary>Empties the values the game keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        Velocity = default;
        Lifetime = 0;
    }
}
