using System.Runtime.InteropServices;
using SuperMetroid.Core;

namespace SuperMetroid.EnsureVerification;

/// <summary>Runs focused runtime and compiler-contract checks for the Ensure API.</summary>
internal static partial class Program
{
    /// <summary>Runs the requested verifier mode and converts failures to a nonzero process exit.</summary>
    /// <param name="args">Optional verifier mode arguments.</param>
    /// <returns>Zero when all selected checks pass; otherwise one.</returns>
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

    /// <summary>Checks Ensure's null, range, and length contracts, including exception parameter names.</summary>
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

    /// <summary>Throws when two verification values differ.</summary>
    /// <typeparam name="T">The compared value type.</typeparam>
    /// <param name="expected">The required value.</param>
    /// <param name="actual">The observed value.</param>
    private static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new InvalidOperationException($"Expected {expected}, got {actual}.");
    }

    /// <summary>Checks that an action throws the exact expected argument exception.</summary>
    /// <typeparam name="T">The required exception type.</typeparam>
    /// <param name="action">The action expected to throw.</param>
    /// <param name="parameterName">The required exception parameter name.</param>
    /// <param name="messageFragment">Optional text required in the exception message.</param>
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

/// <summary>Provides the Windows process error-mode call used to suppress modal crash dialogs.</summary>
internal static partial class NativeConsoleProcess
{
    /// <summary>Sets process error handling flags before verifier work begins.</summary>
    /// <param name="errorMode">The Windows error-mode flags to enable.</param>
    /// <returns>The previous process error mode.</returns>
    [LibraryImport("kernel32.dll")]
    internal static partial uint SetErrorMode(uint errorMode);
}
