namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// X-ray's warm-up (#1631): once X-ray starts, its beam spends eight updates of frozen time before its first step reads
/// whether Run is still held, and the room shows the X-ray's blend from the third of those updates on. The game
/// spent the eight updates preparing the reveal's picture, work this simulation does not need, but the count is
/// gameplay-observable: every update of the warm-up is a whole update of frozen gameplay with its own random draw, and the
/// beam's widening, aiming and ending, with Samus's release from X-ray, all start only once it is over.
/// </summary>
/// <remarks>
/// Started when X-ray is admitted, spent once by each of the beam's updates until it is over, and read by the frame's
/// display for whether the room shows the X-ray's blend and whether the beam's window and backdrop show yet. A finished or
/// torn-down beam holds the warm-up that is over. Evidence: the X-ray's instruction list runs eight setup calls, one per
/// update, before installing the beam's main step, and its blending step first runs together with the second call; the
/// X-ray special-state check steps the beam through exactly eight warm-up updates before its first widening step.
/// </remarks>
[CompatibilityService("X-ray's beam waits eight updates of frozen time after X-ray starts before its first step, and the room shows the X-ray's blend from the third of them")]
public readonly record struct XrayWarmUp
{
    /// <summary>The updates the warm-up lasts.</summary>
    public const int Updates = 8;

    /// <summary>The warm-up's updates that pass before the room shows the X-ray's blend.</summary>
    public const int UpdatesBeforeBlend = 2;

    /// <summary>A warm-up with <paramref name="updatesLeft"/> updates still to spend.</summary>
    private XrayWarmUp(int updatesLeft) => UpdatesLeft = updatesLeft;

    /// <summary>The warm-up X-ray starts with, none of its updates spent yet.</summary>
    public static XrayWarmUp Started { get; } = new(Updates);

    /// <summary>The warm-up that is over: the beam steps on every update.</summary>
    public static XrayWarmUp Over => default;

    /// <summary>The warm-up's updates still to spend, from <see cref="Updates"/> down to zero.</summary>
    public int UpdatesLeft { get; }

    /// <summary>The warm-up's updates already spent.</summary>
    public int UpdatesSpent => Updates - UpdatesLeft;

    /// <summary>Whether the warm-up is over, so the beam's own step runs.</summary>
    public bool IsOver => UpdatesLeft == 0;

    /// <summary>Whether the room shows the X-ray's blend: from the warm-up's third update on, and once it is over.</summary>
    public bool ShowsBlendedRoom => UpdatesSpent >= UpdatesBeforeBlend;

    /// <summary>The warm-up after one more of its updates is spent.</summary>
    /// <exception cref="InvalidOperationException">The warm-up is already over.</exception>
    public XrayWarmUp Spend() => IsOver
        ? throw new InvalidOperationException("X-ray's warm-up is already over.")
        : new(UpdatesLeft - 1);
}
