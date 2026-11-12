namespace Fixture;

/// <summary>Methods that assign constants to variables they never read.</summary>
public static class Locals
{
    /// <summary>Returns <paramref name="value"/> after assigning a constant it never reads.</summary>
    public static int Assigned(int value)
    {
        int unused = 5;
        return value;
    }

    /// <summary>Returns <paramref name="value"/> after assigning a variable twice and never reading it.</summary>
    public static int Reassigned(int value)
    {
        int unused = 5;
        unused = 6;
        return value;
    }

    /// <summary>Returns <paramref name="value"/> plus one, through the one variable of a pair it reads.</summary>
    public static int OneOfTwo(int value)
    {
        int used = 1, unused = 2;
        return value + used;
    }
}
