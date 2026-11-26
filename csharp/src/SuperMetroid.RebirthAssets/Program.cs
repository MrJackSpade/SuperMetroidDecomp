using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction.Engine;

namespace SuperMetroid.RebirthAssets;

/// <summary>
/// Makes the assets of Super Metroid Rebirth, the Godot game, from the player's own ROM. The game ships with none:
/// on Windows this is run once, beside the game, and writes the folder the game reads them from. The Android game
/// makes its own on the device, by the same door (<see cref="AssetExtraction.Engine.RebirthAssets"/>).
/// </summary>
/// <remarks>
/// Usage: <c>SuperMetroidRebirthAssets &lt;rom&gt; [assets folder]</c>. Without a folder it writes <c>assets</c> beside
/// itself, which is where the Windows game looks. The ROM is read and never changed.
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
            Console.WriteLine($"Reading {rom}");
            var progress = new Progress();
            AssetExtraction.Engine.RebirthAssets.Make(rom, assets, progress);
            Console.WriteLine($"Done: {progress.Files} files. Super Metroid Rebirth reads its assets from {assets}");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }

    /// <summary>Prints each step of the making as it starts. The reports come one at a time.</summary>
    private sealed class Progress : IProgress<RebirthAssetsProgress>
    {
        private string _printed = "";

        /// <summary>How many files have been written.</summary>
        internal int Files { get; private set; }

        /// <inheritdoc />
        public void Report(RebirthAssetsProgress value)
        {
            Files = value.Files;
            if (value.Doing == _printed)
                return;
            _printed = value.Doing;
            Console.WriteLine($"  {value.Doing}");
        }
    }
}
