using System.Globalization;

namespace Fixture;

/// <summary>A counter that stores values it never reads back.</summary>
public sealed class Counter
{
    private int _calls;
    private readonly int _start;

    /// <summary>Creates a counter starting from <paramref name="start"/>.</summary>
    public Counter(int start)
    {
        _start = start;
    }

    private int Latest { get; set; }

    /// <summary>Counts <paramref name="value"/> and returns the start value.</summary>
    public int Count(int value)
    {
        _calls++;
        this._calls += value;
        Latest = value;
        Latest = Twice(value);
        return _start;
    }

    /// <summary>Whether <paramref name="text"/> is a number and the counter started above zero.</summary>
    public bool Parses(string text) => int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out _calls) && _start > 0;

    private static int Twice(int value) => value * 2;
}
