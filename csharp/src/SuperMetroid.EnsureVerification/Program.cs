using System.Runtime.InteropServices;
using SuperMetroid.Core;

namespace SuperMetroid.EnsureVerification;

internal static partial class Program
{
    private static int Main(string[] args)
    {
        try
        {
            if (OperatingSystem.IsWindows())
                NativeConsoleProcess.SetErrorMode(0x0001 | 0x0002 | 0x8000);
            if (args is ["--palette-fx-contract"])
            {
                VerifyPaletteFxDependencyContract();
                return 0;
            }
            if (args.Length != 0)
                throw new ArgumentException("Unknown verification arguments.", nameof(args));
            VerifyEnsure();
            VerifyAnalyzer();
            VerifyPaletteFxDependencyContract();
            Console.WriteLine("Ensure API, analyzer and palette-FX dependency checks passed.");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }

    private static void VerifyEnsure()
    {
        string? text = "okay";
        Equal("okay", Ensure.NotNull(text));
        Throws<ArgumentNullException>(() => Ensure.NotNull((string?)null), "(string?)null");

        int count = 1;
        Equal(1, Ensure.AtLeastZero(count));
        Throws<ArgumentOutOfRangeException>(() => Ensure.AtLeastZero(count - 2), "count - 2");
        Equal(0, Ensure.BetweenInclusive(0, 64, 0));
        Equal(64, Ensure.BetweenInclusive(0, 64, 64));
        Throws<ArgumentOutOfRangeException>(() => Ensure.BetweenInclusive(0, 64, 65), "65");
        Throws<ArgumentException>(() => Ensure.BetweenInclusive(5, 4, 4), "minimum");

        ReadOnlySpan<int> span = [1, 2];
        Equal(2, Ensure.LengthEqual(span, 2).Length);
        try
        {
            Ensure.LengthEqual(span, 3);
            throw new InvalidOperationException("Expected span length rejection.");
        }
        catch (ArgumentException exception)
        {
            Equal("span", exception.ParamName);
        }
    }

    private static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new InvalidOperationException($"Expected {expected}, got {actual}.");
    }

    private static void Throws<T>(Action action, string parameterName, string? messageFragment = null) where T : ArgumentException
    {
        try
        {
            action();
        }
        catch (T exception) when (exception.GetType() == typeof(T))
        {
            Equal(parameterName, exception.ParamName);
            if (messageFragment is not null &&
                !exception.Message.Contains(messageFragment, StringComparison.Ordinal))
                throw new InvalidOperationException($"Expected message to contain '{messageFragment}': {exception.Message}");
            return;
        }
        throw new InvalidOperationException($"Expected {typeof(T).Name} for {parameterName}.");
    }
}

internal static partial class NativeConsoleProcess
{
    [LibraryImport("kernel32.dll")]
    internal static partial uint SetErrorMode(uint errorMode);
}
