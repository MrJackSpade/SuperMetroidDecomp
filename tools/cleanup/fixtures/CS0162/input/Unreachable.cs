namespace Fixture;

/// <summary>Methods with code that can never run.</summary>
public static class Unreachable
{
    /// <summary>Returns <paramref name="value"/>; the statements after the return never run.</summary>
    public static int AfterReturn(int value)
    {
        return value;
        Console.WriteLine(value);
        Console.WriteLine(value + 1);
    }

    /// <summary>Returns <paramref name="value"/>; the nested block after the return never runs.</summary>
    public static int NestedBlock(int value)
    {
        return value;
        {
            Console.WriteLine(value);
        }
    }

    /// <summary>Counts <paramref name="value"/> up past ten; only the unreachable jump named the label.</summary>
    public static int Jump(int value)
    {
        while (true)
        {
            if (value > 10)
                break;
            value++;
            continue;
            goto finish;
        }
    finish:
        return value;
    }
}
