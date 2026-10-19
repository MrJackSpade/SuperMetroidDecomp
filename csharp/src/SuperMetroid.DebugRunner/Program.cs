namespace SuperMetroid.DebugRunner;
using SuperMetroid.DebugRunner.Assets;
using SuperMetroid.DebugRunner.Tools;


/// <summary>
/// Developer tools that operate on caller-supplied files: asset installation and extraction,
/// generated-definition writers, state-fixture exporters, and captured-frame comparisons.
/// Keep faults in the CLI. Without this process policy Windows can display a modal "unknown
/// software exception" dialog for an unhandled failure, steal desktop focus, and leave the
/// build output locked until somebody dismisses it.
/// </summary>
internal static class Program
{
    /// <summary>Dispatches tool commands behind the no-dialog process boundary and returns a failing exit code on error.</summary>
    private static int Main(string[] args)
    {
        if (OperatingSystem.IsWindows())
            _ = NativeConsoleProcess.SetErrorMode(0x0001 | 0x0002 | 0x8000);

        try
        {
            if (AssetTools.Run(args) is int toolExit)
                return toolExit;
            if (IntegrationTools.Run(args) is int integrationToolExit)
                return integrationToolExit;
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
    }
}
