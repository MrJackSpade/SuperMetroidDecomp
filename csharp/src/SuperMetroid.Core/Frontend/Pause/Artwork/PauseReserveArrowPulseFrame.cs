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

    /// <summary>The frame the pulse shows when the game's short frame count stands at <paramref name="frameCount"/>.</summary>
    /// <param name="frameCount">The short frame count of the update the arrow is drawn on.</param>
    internal static PauseReserveArrowPulseFrame PacedBy(Compatibility.ShortFrameCount frameCount) =>
        All[PauseCadenceDefinitions.ReserveArrowPulseFrame.StepAt(frameCount)];
}
