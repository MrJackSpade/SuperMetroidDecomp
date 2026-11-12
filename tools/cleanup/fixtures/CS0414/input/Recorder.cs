namespace Fixture;

/// <summary>A recorder whose flags are written with constants and never read.</summary>
public sealed class Recorder
{
    private int _initialized = 1;
    private bool _flagged;
    private int _reset;
    private readonly int _start;

    /// <summary>Creates a recorder starting from <paramref name="start"/>.</summary>
    public Recorder(int start)
    {
        _start = start;
        _reset = 0;
    }

    /// <summary>Records a call and returns the start value.</summary>
    public int Record()
    {
        _flagged = true;
        _reset = 5;
        return _start;
    }
}
