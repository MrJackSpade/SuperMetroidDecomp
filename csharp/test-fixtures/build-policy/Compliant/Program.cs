namespace BuildPolicy;

/// <summary>Executable entry point used by the compliant build-policy fixture.</summary>
internal static class Program
{
    /// <summary>Prints a stable marker when the fixture is launched and exits successfully.</summary>
    private static int Main()
    {
        Console.WriteLine("policy fixture");
        return 0;
    }
}
