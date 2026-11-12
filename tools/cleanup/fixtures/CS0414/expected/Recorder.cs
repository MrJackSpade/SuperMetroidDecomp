namespace Fixture;

/// <summary>A recorder whose flags are written with constants and never read.</summary>
public sealed class Recorder
{
    private readonly int _start;

    /// <summary>Creates a recorder starting from <paramref name="start"/>.</summary>
    public Recorder(int start)
    {
        _start = start;
    }

    /// <summary>Records a call and returns the start value.</summary>
    public int Record()
    {
        return _start;
    }
}
