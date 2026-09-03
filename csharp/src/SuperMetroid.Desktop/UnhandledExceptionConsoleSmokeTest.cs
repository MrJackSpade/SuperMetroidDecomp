namespace SuperMetroid.Desktop;

/// <summary>Deterministic verification for the fatal console reporting contract.</summary>
public static class UnhandledExceptionConsoleSmokeTest
{
    /// <summary>
    /// Captures one nested exception and verifies that waiting begins only after the complete
    /// diagnostic and acknowledgment prompt have been written.
    /// </summary>
    public static UnhandledExceptionConsoleSmokeTestResult Run()
    {
        using var output = new StringWriter();
        bool waited = false;
        bool reportWasCompleteBeforeWait = false;
        var inner = new InvalidDataException("inner cartridge diagnostic");
        var exception = new InvalidOperationException("outer frame failure", inner);

        UnhandledExceptionConsole.ReportAndWait(
            exception,
            output,
            () =>
            {
                waited = true;
                string reportAtWait = output.ToString();
                reportWasCompleteBeforeWait =
                    reportAtWait.Contains(typeof(InvalidOperationException).FullName!, StringComparison.Ordinal) &&
                    reportAtWait.Contains("outer frame failure", StringComparison.Ordinal) &&
                    reportAtWait.Contains(typeof(InvalidDataException).FullName!, StringComparison.Ordinal) &&
                    reportAtWait.Contains("inner cartridge diagnostic", StringComparison.Ordinal) &&
                    reportAtWait.Contains("Press Enter to exit.", StringComparison.Ordinal);
            });

        if (!waited)
            throw new InvalidOperationException("Fatal console reporter did not wait for acknowledgment.");
        if (!reportWasCompleteBeforeWait)
        {
            throw new InvalidOperationException(
                "Fatal console reporter waited before its complete exception diagnostic was visible.");
        }

        return new UnhandledExceptionConsoleSmokeTestResult(output.ToString().Length);
    }
}

/// <summary>Observable result returned by the fatal console reporting smoke test.</summary>
public readonly record struct UnhandledExceptionConsoleSmokeTestResult(int ReportLength);
