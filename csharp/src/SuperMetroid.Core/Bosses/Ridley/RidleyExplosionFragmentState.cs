using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The state of one fragment of Norfair Ridley's breakup.</summary>
internal sealed class RidleyExplosionFragmentState
{
    /// <summary>Signed 8.8 velocity per update: X, which drag slows toward zero, and Y, which gravity increases.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>The updates the fragment lives, counted down past zero.</summary>
    public int Lifetime { get; internal set; }

    /// <summary>Where the fragment's typed breakup program resumes (#641), set when it spawns.</summary>
    internal AnimationProgramCursor<RidleyBreakupFrame> Program { get; set; }
}
