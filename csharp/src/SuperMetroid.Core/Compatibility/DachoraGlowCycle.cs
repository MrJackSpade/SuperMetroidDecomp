namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// A Dachora's glow cycle (#1544): the frame of its speed or stored-shine glow it shows next and the updates left before
/// it, which the game counts as one number in which each frame is worth 256 updates.
/// </summary>
/// <remarks>
/// <para>
/// Gameplay behaviour reproduced: an update counted off a cycle with no updates left takes one from the frame, so a cleared
/// cycle becomes frame 255 with 255 updates left; a step adds its frames and updates together, so updates past 255 overflow
/// into the frame and a step from the top of the count wraps to the bottom; a cycle's end is tested by the sign of the
/// difference between the count and the end's count. The running Dachora requests its Speed Booster sound on the update its
/// cycle stands at frame zero with one update left, so when the cycle underflows, overflows and restarts decides when that sound
/// is requested and when each glow frame shows.
/// </para>
/// <para>
/// Consulted on every update of the Dachora's run at full speed and of its shinespark's charge and launch, and set when its
/// run, wall, launch and ceiling phases restart or clear it. Evidence: the Dachora's acceleration and shine routines, which
/// keep the frame and its updates together as one number and count down and add to that number as a whole.
/// </para>
/// </remarks>
[CompatibilityService("a Dachora glow's frame and its updates count as one number: counting from a cleared cycle borrows from the frame, and a step's updates carry into it")]
public readonly record struct DachoraGlowCycle
{
    /// <summary>The updates one frame of the count is worth.</summary>
    private const int UpdatesPerFrame = 256;

    /// <summary>The frame times <see cref="UpdatesPerFrame"/>, plus the updates left before it.</summary>
    private readonly int count;

    /// <summary>Keeps <paramref name="value"/> as the cycle's count.</summary>
    private DachoraGlowCycle(int value) => count = value;

    /// <summary>The cycle at <paramref name="frame"/> with <paramref name="updates"/> left before it, each 0 through 255.</summary>
    public static DachoraGlowCycle At(int frame, int updates) =>
        new(Ensure.BetweenInclusive(0, UpdatesPerFrame - 1, frame) * UpdatesPerFrame + Ensure.BetweenInclusive(0, UpdatesPerFrame - 1, updates));

    /// <summary>The glow frame the cycle shows next.</summary>
    public int Frame => count / UpdatesPerFrame;

    /// <summary>The updates left before the cycle shows <see cref="Frame"/>.</summary>
    [IntegerQuantity("updates left before the glow cycle shows its next frame")]
    public int UpdatesLeft => count % UpdatesPerFrame;

    /// <summary>The cycle after counting one update off it, borrowing from the frame when no updates are left.</summary>
    public DachoraGlowCycle CountedDown() => new(WrappingCounter.DecrementCount(count));

    /// <summary>The cycle after a step of <paramref name="step"/>'s frames and updates, carrying updates into the frame.</summary>
    public DachoraGlowCycle Stepped(DachoraGlowCycle step) => new(WrappingCounter.AddCount(count, step.count));

    /// <summary>Whether the cycle is short of <paramref name="end"/>, as the game compares them by the sign of their difference.</summary>
    public bool IsShortOf(DachoraGlowCycle end) => WrappingCounter.IsBehind(count, end.count);
}
