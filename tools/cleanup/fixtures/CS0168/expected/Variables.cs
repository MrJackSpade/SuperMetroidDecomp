using System.Globalization;

namespace Fixture;

/// <summary>Methods that declare variables they never use.</summary>
public static class Variables
{
    /// <summary>Returns <paramref name="value"/> after declaring a variable it never uses.</summary>
    public static int Declared(int value)
    {
        return value;
    }

    /// <summary>Returns one more than <paramref name="value"/> through the one variable of a pair that is used.</summary>
    public static int OneOfTwo(int value)
    {
        int used;
        used = value + 1;
        return used;
    }

    /// <summary>Parses <paramref name="text"/>, or returns zero when it is not a number.</summary>
    public static int Caught(string text)
    {
        try
        {
            return int.Parse(text, CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            return 0;
        }
    }
}
