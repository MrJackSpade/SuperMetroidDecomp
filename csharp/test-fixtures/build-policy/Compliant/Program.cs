namespace BuildPolicy;

/// <summary>Executable entry point used by the compliant build-policy fixture.</summary>
internal static class Program
{
    /// <summary>Prints a stable marker and exercises each fixture type when launched, then exits successfully.</summary>
    private static int Main()
    {
        Console.WriteLine("policy fixture");
        Console.WriteLine(Ca1822.Twice(1));
        Console.WriteLine(new Ide0330().Increment());
        Console.WriteLine(new Ide0032 { Value = 1 }.Value);
        return 0;
    }
}
