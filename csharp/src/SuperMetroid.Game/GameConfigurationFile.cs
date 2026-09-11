using SuperMetroid.Core.Frontend;
using System.Text;

namespace SuperMetroid.Game;

/// <summary>Loads player settings from the installation data root (or legacy ROM directory).</summary>
internal sealed record GameConfigurationFile(
    string Path,
    SuperMetroidGameOptions Options,
    string Source)
{
    public const string FileName = "SuperMetroid.ini";
    public const string DefaultsFileName = "SuperMetroid.defaults.ini";

    public static GameConfigurationFile LoadOrCreate(string romPath, string? dataDirectory = null,
        string? defaultsDirectory = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(romPath);

        // The ROM is the one file every standalone private copy already needs. Anchoring the
        // INI to it makes the setting independent of Visual Studio's working directory and
        // lets the whole playable directory move without changing an absolute path.
        string fullRomPath = System.IO.Path.GetFullPath(romPath);
        string romDirectory = System.IO.Path.GetDirectoryName(fullRomPath)
            ?? throw new InvalidOperationException(
                $"Private ROM path has no containing directory: {fullRomPath}");
        string executableDirectory = defaultsDirectory ?? AppContext.BaseDirectory;
        string localPath = System.IO.Path.Combine(executableDirectory, FileName);
        bool localOverride = File.Exists(localPath);
        string configurationPath = System.IO.Path.GetFullPath(localOverride ? localPath :
            System.IO.Path.Combine(dataDirectory ?? romDirectory, FileName));

        if (!File.Exists(configurationPath))
        {
            // The separately named release template cannot overwrite an active INI when
            // a ZIP is extracted over an old application folder. It seeds only first use;
            // existing player settings always win, even if the template later changes.
            string templatePath = System.IO.Path.Combine(defaultsDirectory ?? AppContext.BaseDirectory,
                DefaultsFileName);
            string initialContents = File.Exists(templatePath)
                ? File.ReadAllText(templatePath) : SuperMetroidGameOptionsIni.DefaultFileContents;
            _ = SuperMetroidGameOptionsIni.Parse(initialContents, templatePath);
            // UTF-8 without a byte-order mark remains readable in ordinary text editors and
            // avoids putting an invisible character in front of the first INI comment.
            // CreateNew also prevents a concurrent launch from overwriting a new user file.
            using var stream = new FileStream(configurationPath, FileMode.CreateNew, FileAccess.Write);
            using var writer = new StreamWriter(stream, new UTF8Encoding(false));
            writer.Write(initialContents);
        }

        string contents = File.ReadAllText(configurationPath);
        SuperMetroidGameOptions options =
            SuperMetroidGameOptionsIni.Parse(contents, configurationPath);
        return new GameConfigurationFile(configurationPath, options,
            localOverride ? "executable-directory override" :
            dataDirectory is null ? "legacy ROM-directory settings" : "installation data-directory settings");
    }
}
