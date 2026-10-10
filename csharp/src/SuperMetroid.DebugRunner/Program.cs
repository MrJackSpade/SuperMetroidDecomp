using System.Runtime.InteropServices;

// Developer tools that operate on caller-supplied files: asset installation and extraction,
// generated-definition writers, state-fixture exporters, and captured-frame comparisons.
// Keep faults in the CLI. Without this process policy Windows can display a modal "unknown
// software exception" dialog for an unhandled failure, steal desktop focus, and leave the
// build output locked until somebody dismisses it.
if (OperatingSystem.IsWindows())
    NativeConsoleProcess.SetErrorMode(0x0001 | 0x0002 | 0x8000);

try
{
    if (AssetTools.Run(args) is int toolExit)
        return toolExit;
    if (IntegrationTools.Run(args) is int integrationToolExit)
        return integrationToolExit;
    if (RenderTools.Run(args) is int renderToolExit)
        return renderToolExit;
    if (args.Length > 0 && args[0] == "assets")
        return AssetCommands.Run(args[1..]);

    Console.Error.WriteLine($"Unrecognized arguments: {string.Join(' ', args)}");
    return 2;
}
catch (Exception exception)
{
    // An explicit failing exit code preserves automation semantics without allowing the
    // CLR/Windows error reporter to display a modal "unknown software exception" box.
    Console.Error.WriteLine(exception);
    return 1;
}

/// <summary>
/// Makes the command-line tools genuinely non-interactive on Windows. The CLR retains
/// its ordinary stderr stack trace and exit code; only OS-owned modal error boxes are barred.
/// </summary>
static partial class NativeConsoleProcess
{
    /// <summary>Configures Windows to keep system error dialogs from interrupting command-line failures.</summary>
    /// <param name="errorMode">The process error-mode flags passed to the Windows API.</param>
    /// <returns>The process's previous error-mode flags.</returns>
    [LibraryImport("kernel32.dll")]
    internal static partial uint SetErrorMode(uint errorMode);
}
