namespace Fixture;

/// <summary>A recorder that stores values it never reads back.</summary>
public sealed class Recorder
{
    private int _last = 1;
    private int _stamp = Stamp();
    private readonly int _start;

    /// <summary>Creates a recorder starting from <paramref name="start"/>.</summary>
    public Recorder(int start)
    {
        _start = start;
        _last = start;
    }

    /// <summary>Records <paramref name="value"/> and returns the start value.</summary>
    public int Record(int value)
    {
        _last = value;
        _last = Twice(value);
        return _start;
    }

    /// <summary>Records <paramref name="value"/> and returns it plus the start value.</summary>
    public int Echo(int value) => Same(_last = value) + _start;

    private static int Twice(int value) => value * 2;

    private static int Same(int value) => value;

    private static int Stamp() => 7;
}
