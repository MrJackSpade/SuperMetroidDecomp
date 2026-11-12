namespace Fixture;

/// <summary>Methods that store values they never read.</summary>
public static class Stores
{
    /// <summary>Returns <paramref name="value"/> after computing a value it never reads.</summary>
    public static int Declared(int value)
    {
        _ = Compute(value);
        return value;
    }

    /// <summary>Returns <paramref name="value"/> after overwriting a variable with values it never reads.</summary>
    public static int Overwritten(int value)
    {
        int result = value + 1;
        Console.WriteLine(result);
        _ = Compute(value);
        return value;
    }

    private static int Compute(int value) => value * 2;
}
