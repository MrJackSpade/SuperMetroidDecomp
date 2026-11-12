namespace BuildPolicy;

/// <summary>Executable entry point used by the violating build-policy fixture.</summary>
internal static class Program
{
    /// <summary>Exercises the fixture types that need a caller, then exits successfully.</summary>
    private static int Main()
    {
        Console.WriteLine(new Ca1822().Twice(1));
        return 0;
    }
}
