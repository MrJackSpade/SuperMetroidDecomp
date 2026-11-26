using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction.Engine;
using SuperMetroid.AssetExtraction.Installation;

namespace SuperMetroid.RebirthAssets;

/// <summary>
/// Makes the assets of Super Metroid Rebirth, the Godot game, from the player's own ROM. The game ships with none:
/// this is run once, beside the game, and writes the folder the game reads them from. For the Android game the folder
/// it writes is copied to the device.
/// </summary>
/// <remarks>
/// Usage: <c>SuperMetroidRebirthAssets &lt;rom&gt; [assets folder]</c>. Without a folder it writes <c>assets</c> beside
/// itself, which is where the Windows game looks. The ROM is read and never changed; what is made on the way is kept
/// in a folder of its own under the temporary files and removed when the assets are written.
/// </remarks>
internal static partial class Program
{
    /// <summary>Keeps Windows from putting up a dialog over an unhandled failure: the failure is printed and returned.</summary>
    [LibraryImport("kernel32.dll")]
    private static partial uint SetErrorMode(uint errorMode);

    /// <summary>Makes the assets behind the no-dialog process boundary and returns a failing exit code on error.</summary>
    private static int Main(string[] args)
    {
        if (OperatingSystem.IsWindows())
            _ = SetErrorMode(0x0001 | 0x0002 | 0x8000);
        try
        {
            if (args.Length is not (1 or 2))
            {
                Console.Error.WriteLine("Usage: SuperMetroidRebirthAssets <rom> [assets folder]");
                return 2;
            }

            string rom = Path.GetFullPath(args[0]);
            string assets = Path.GetFullPath(args.Length == 2 ? args[1] : Path.Combine(AppContext.BaseDirectory, "assets"));
            string installation = Path.Combine(Path.GetTempPath(), "SuperMetroidRebirthAssets-" + Guid.NewGuid().ToString("N"));
            try
            {
                Console.WriteLine($"Reading {rom}");
                GameAssetInstaller.Install(rom, installation, progress: new Progress());
                Console.WriteLine($"Writing the game's assets into {assets}");
                EngineAssetWriter.Write(installation, assets);
            }
            finally
            {
                if (Directory.Exists(installation))
                    Directory.Delete(installation, recursive: true);
            }

            Console.WriteLine($"Done. Super Metroid Rebirth reads its assets from {assets}");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }

    /// <summary>Prints each step of reading the ROM as it starts.</summary>
    private sealed class Progress : IProgress<string>
    {
        /// <inheritdoc />
        public void Report(string value) => Console.WriteLine($"  {value}");
    }
}
