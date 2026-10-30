namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A plane's scroll on one axis that changes from screen line to screen line, described by its shape (#1468): a sine
/// wave (<see cref="ScrollWave"/>), a waveform of samples (<see cref="SampledScrollWave"/>), a stretch
/// (<see cref="ScrollStretch"/>) or a list of bands (<see cref="ScrollBands"/>). The backend's shader evaluates the shape
/// line by line; nothing keeps a table of lines.
/// </summary>
public abstract record LineScroll
{
    /// <summary>The scroll the shape starts from, 0 through 65535; each kind of shape says which lines show it.</summary>
    public abstract int BaseScroll { get; init; }
}
