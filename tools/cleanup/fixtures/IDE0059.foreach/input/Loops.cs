namespace Fixture;

/// <summary>Loops whose iteration variables nothing reads, for which the code-style fixer offers no fix.</summary>
public static class Loops
{
    private static readonly int[] Kept = [1, 2, 3];

    /// <summary>Adds <paramref name="loaded"/> to <paramref name="cells"/> under a loop header left without its own body
    /// (RoomScrollGrid after #2553): the addition runs once per element, and still does after the fix.</summary>
    public static void Leftover(int[] loaded, List<int> cells)
    {
        foreach (int state in loaded)
        cells.AddRange(loaded);
    }

    /// <summary>Counts <paramref name="values"/> without reading them.</summary>
    public static int Counted(IEnumerable<int> values)
    {
        int count = 0;
        foreach (var value in values)
        {
            count++;
        }
        return count;
    }

    /// <summary>Walks a parameter, a field and a local with nothing to run.</summary>
    public static void Empty(int[] values)
    {
        int[] local = [4];
        foreach (int value in values)
        {
        }
        foreach (int value in Kept) ;
        foreach (int value in local) { }
        Console.WriteLine(values.Length + local.Length + Kept.Length);
    }

    /// <summary>Walks a call's result with nothing to run, and an empty loop that is an if's body.</summary>
    public static void EmptyWithEffect(bool walk)
    {
        foreach (int value in Values())
        {
        }
        if (walk)
            foreach (int value in Kept) { }
    }

    private static int[] Values()
    {
        Console.WriteLine("values");
        return [5];
    }
}
