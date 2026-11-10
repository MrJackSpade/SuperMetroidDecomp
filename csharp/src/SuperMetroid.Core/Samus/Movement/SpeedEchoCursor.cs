namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Which Speed Booster echo slot the next capture fills, or whether the echoes are departing. Ordinary boosting
/// alternates the first and second slots; cancelling the boost sends the echoes departing, which stops capture.
/// </summary>
public readonly record struct SpeedEchoCursor
{
    /// <summary>Holds a cursor.</summary>
    private SpeedEchoCursor(SpeedEchoCursorState state) => State = state;

    /// <summary>What the cursor holds.</summary>
    public SpeedEchoCursorState State { get; }

    /// <summary>The next capture fills the first slot; the cursor of a Samus who has not boosted.</summary>
    public static SpeedEchoCursor FirstSlot => default;

    /// <summary>The next capture fills the second slot.</summary>
    public static SpeedEchoCursor SecondSlot { get; } = new(SpeedEchoCursorState.SecondSlot);

    /// <summary>The echoes are departing toward Samus; nothing is captured until they have gone.</summary>
    public static SpeedEchoCursor Departing { get; } = new(SpeedEchoCursorState.Departing);

    /// <summary>Whether the echoes are departing, which stops capture.</summary>
    public bool IsDeparting => State == SpeedEchoCursorState.Departing;

    /// <summary>Whether the next capture fills the first slot; any other cursor that captures fills the second.</summary>
    public bool CapturesFirstSlot => State == SpeedEchoCursorState.FirstSlot;

    /// <summary>The cursor after a capture: the first slot's capture moves it to the second, and the second's to the first.</summary>
    /// <exception cref="InvalidOperationException">The echoes are departing, and nothing is captured.</exception>
    public SpeedEchoCursor AfterCapture() => State switch
    {
        SpeedEchoCursorState.FirstSlot => SecondSlot,
        SpeedEchoCursorState.SecondSlot => FirstSlot,
        SpeedEchoCursorState.Departing => throw new InvalidOperationException("Departing echoes are not captured."),
        _ => throw new InvalidOperationException($"Undefined {nameof(SpeedEchoCursorState)} {State}."),
    };
}
