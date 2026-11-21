namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>
/// One frame of the AUTO reserve arrow's pulse (#2361): the pause screen's frame counter shows each in turn, one an update,
/// the first following the last.
/// </summary>
public readonly record struct PauseReserveArrowPulseFrame
{
    /// <summary>The frame at <paramref name="frame"/> of the pulse.</summary>
    private PauseReserveArrowPulseFrame(int frame) => Frame = frame;

    /// <summary>The frame's place in the pulse, from zero: the step of its installed shade.</summary>
    public int Frame { get; }

    /// <summary>Every frame of the pulse, in the order the arrow shows them.</summary>
    public static IReadOnlyList<PauseReserveArrowPulseFrame> All { get; } =
        [.. Enumerable.Range(0, PauseReserveArrowDefinitions.ArrowFrames).Select(frame => new PauseReserveArrowPulseFrame(frame))];

    /// <summary>The frame the pause screen's frame counter at <paramref name="frameCycle"/> shows.</summary>
    internal static PauseReserveArrowPulseFrame PacedBy(int frameCycle) =>
        All[PauseCadenceDefinitions.ReserveArrowPulseFrame.StepAt(frameCycle)];
}
