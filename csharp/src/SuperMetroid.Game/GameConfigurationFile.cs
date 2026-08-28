using SuperMetroid.Core.Frontend;
using System.Text;

namespace SuperMetroid.Game;

/// <summary>Loads the playable host's documented settings from beside the private ROM.</summary>
internal sealed record GameConfigurationFile(
    string Path,
    SuperMetroidGameOptions Options)
{
    public const string FileName = "SuperMetroid.ini";

    public static GameConfigurationFile LoadOrCreate(string romPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(romPath);

        // The ROM is the one file every standalone private copy already needs. Anchoring the
        // INI to it makes the setting independent of Visual Studio's working directory and
        // lets the whole playable directory move without changing an absolute path.
        string fullRomPath = System.IO.Path.GetFullPath(romPath);
        string romDirectory = System.IO.Path.GetDirectoryName(fullRomPath)
            ?? throw new InvalidOperationException(
                $"Private ROM path has no containing directory: {fullRomPath}");
        string configurationPath = System.IO.Path.Combine(romDirectory, FileName);

        if (!File.Exists(configurationPath))
        {
            // UTF-8 without a byte-order mark remains readable in ordinary text editors and
            // avoids putting an invisible character in front of the first INI comment.
            File.WriteAllText(
                configurationPath,
                SuperMetroidGameOptionsIni.DefaultFileContents,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        string contents = File.ReadAllText(configurationPath);
        SuperMetroidGameOptions options =
            SuperMetroidGameOptionsIni.Parse(contents, configurationPath);
        return new GameConfigurationFile(configurationPath, options);
    }
}
